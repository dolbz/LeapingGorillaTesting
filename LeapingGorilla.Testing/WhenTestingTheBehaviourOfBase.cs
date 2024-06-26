﻿/*    
   Copyright 2014-2021 Leaping Gorilla LTD

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LeapingGorilla.Testing.Core.Attributes;
using LeapingGorilla.Testing.Core.Exceptions;
using LeapingGorilla.Testing.Core.Extensions;
using TypeAccessor = LeapingGorilla.Testing.Core.FastMember.TypeAccessor;

namespace LeapingGorilla.Testing.Core
{
	/// <summary>Base class used for writing BDD style Given/When/Then unit tests for a component with simplified mocking semantics</summary>
	public abstract class WhenTestingTheBehaviourOfBase
	{
		/// <summary>
		/// Stores any <see cref="Exception"/> that was thrown whilst calling the method
		/// marked with a <see cref="WhenAttribute"/>.
		/// </summary>
		public Exception ThrownException { get; private protected set; }

		/// <summary>
		/// Creates the manual dependencies. Override this method if you wish to manually specify the values of any field or property that you
		/// wish to use as a dependency. You should use this method to assign the values to the properties or fields with manually
		/// instantiated values.
		/// </summary>
		protected virtual void CreateManualDependencies() { }
		
#pragma warning disable xUnit1013 // XUnit Specific - Public method should be marked as test
		/// <summary>
		/// Obsolete method retained for backwards compatibility.
		/// Invokes SetupAsync() synchronously.
		/// </summary>
		[Obsolete("Use SetupAsync() instead. Synchronous method relies on Task.Wait() which blocks the thread")]
		public void Setup()
		{
			SetupAsync().GetAwaiter().GetResult();
		}
		
		/// <summary>
		/// Performs setup for this instance - this will prepare all mocks, call the [Given] methods (if any)
		/// and then call the [When] methods (if any), ready for your test assertions
		/// </summary>
		public virtual async Task SetupAsync()
		{
			PrepareMocksDependenciesAndItemUnderTest();
			await ExecuteGivenMethods();
			await ExecuteWhenMethod();
		}
#pragma warning restore xUnit1013

		private async Task ExecuteWhenMethod()
		{
			var method = ValidateAndFetchWhenMethod();

			if (method == null)
			{
				return;
			}

			await ExecuteWhenMethod(method);
		}
		
		private protected async Task ExecuteWhenMethod(MethodInfo method) {
			var whenAttribute = (WhenAttribute)Attribute.GetCustomAttribute(method, typeof(WhenAttribute));

			if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
			{
				throw new WhenMethodsMustBeVoidOrTaskException(method.Name);
			}

			if (method.GetParameters().Any())
			{
				throw new WhenMethodMayNotHaveParametersException(method.Name);
			}

			try
			{
				await InvokeMethodAsVoidOrTask(method);
			}
			catch (Exception ex)
			{
				ThrownException = (ex is TargetInvocationException || ex is AggregateException) && ex.InnerException != null 
									? ex.InnerException 
									: ex;

				if (!whenAttribute.DoNotRethrowExceptions)
				{
					throw;
				}
			}
		}

		private MethodInfo ValidateAndFetchWhenMethod()
		{
			// Check we don't have more than one [When] method
			var whenMethods = GetMethodsWithAttribute(typeof(WhenAttribute)).ToList();
			if (whenMethods.Count > 1)
			{
				throw new WhenMethodMayOnlyAppearOnceException(whenMethods.Select(m => m.Name));
			}

			// If we have no [When] methods then return
			if (!whenMethods.Any())
			{
				return null;
			}

			// Single when method - invoke it after checking it's return and parameters
			return whenMethods.Single();
		}

		private async Task ExecuteGivenMethods()
		{
			var givenMethods = GetMethodsWithAttribute(typeof(GivenAttribute));

			foreach (var method in givenMethods.OrderBy(gm => ((GivenAttribute)gm.GetCustomAttributes(typeof(GivenAttribute), true).First()).Order))
			{
				if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
				{
					throw new GivenMethodsMustBeVoidOrTaskException(method.Name);
				}

				if (method.GetParameters().Any())
				{
					throw new GivenMethodMayNotHaveParametersException(method.Name);
				}

				await InvokeMethodAsVoidOrTask(method);
			}
		}

		private protected async Task InvokeMethodAsVoidOrTask(MethodInfo method)
		{
			if (method.ReturnType == typeof(Task))
			{
				var task = (Task)method.Invoke(this, null);
				await task;
			}
			else
			{
				method.Invoke(this, null);
			}
		}

		/// <summary>
		/// Prepares the mocks, dependencies and the item under test.
		/// </summary>
		/// <exception cref="NoItemUnderTestException">Thrown if the user has specified [Dependency]'s but there is no [ItemUnderTest]</exception>
		/// <exception cref="NoMatchingConstructorFoundException">Thrown if no constructor can be found on the [ItemUnderTest] that matches the number of [Dependency] fields/properties</exception>
		/// <exception cref="DependencyMismatchException">Thrown if a [Dependency] cannot be found for a constructor parameter when one is expected.</exception>
		/// <exception cref="NoItemUnderTestException">Thrown if there are fields/properties marked with [Dependency] but none with [ItemUnderTest]</exception>
		private protected void PrepareMocksDependenciesAndItemUnderTest()
		{
				// Create a fast accessor onto the test class
			var accessor = TypeAccessor.Create(GetTestClassType(), true);

				// Get and validate the item under test
			var itemUnderTest = GetPropertiesWithAttribute(typeof(ItemUnderTestAttribute)).Select(pi => new { Type = pi.PropertyType, pi.Name })
								.Concat(GetFieldsWithAttribute(typeof(ItemUnderTestAttribute)).Select(pi => new { Type = pi.FieldType, pi.Name }))
								.FirstOrDefault();

				// Create mocks and dependencies
			CreateManualDependencies();
			var dependencies = CreateAndAssignPropertiesOrFieldsWithAttribute(accessor, typeof(DependencyAttribute));
			dependencies.AddRange(CreateAndAssignPropertiesOrFieldsWithAttribute(accessor, typeof(NullDependencyAttribute)));

			CreateAndAssignPropertiesOrFieldsWithAttribute(accessor, typeof(MockAttribute));

				// It is invalid to have dependencies but no item under test
			if (itemUnderTest == null)
			{
				if (dependencies.Count > 0)
				{
					throw new NoItemUnderTestException();
				}

					// No item under test but no dependencies. We can abort out of the rest of the setup
				return;
			}

				// Determine the correct constructor for the item under test. If we cant match one then error out
			var constructor = GetPreferredConstructor(itemUnderTest.Type, dependencies);
			if (constructor == null)
			{
				throw new NoMatchingConstructorFoundException(dependencies);
			}


			var parameters = constructor.GetParameters();
			var constructorArguments = new object[parameters.Length];

				// Assign parameters. We remove them from the dependencies list as we go so we don't re-use dependencies in case of indirect (type only) matching
			for (int index = 0; index < parameters.Length; index++)
			{
				var param = parameters[index];
				var dep = dependencies.FirstOrDefault(d => d.Name == param.Name && d.Type == param.ParameterType) ?? dependencies.FirstOrDefault(d => d.Type == param.ParameterType);

				if (dep == null)
				{
					throw new DependencyMismatchException(param.ParameterType, param.Name, index, parameters);
				}

				dependencies.Remove(dep);
				constructorArguments[index] = dep.Value;
			}

			// Create the item under test and load it into the test class
			accessor[this, itemUnderTest.Name] = constructor.Invoke(constructorArguments);
		}

		private List<Dependency> CreateAndAssignPropertiesOrFieldsWithAttribute(TypeAccessor accessor, Type attributeType)
		{
			var dependencies = BuildDependenciesForPropertiesOrFieldsWithAttributeType(attributeType);
			foreach (var dep in dependencies)
			{
				accessor[this, dep.Name] = dep.Value;
			}

			return dependencies;
		}

		private List<Dependency> BuildDependenciesForPropertiesOrFieldsWithAttributeType(Type attributeType)
		{
			var dependencies = new List<Dependency>();

			var props = GetPropertiesWithAttribute(attributeType);

            dependencies.AddRange(props.Select(prop => new Dependency(prop.Name, prop.PropertyType, attributeType == typeof(NullDependencyAttribute), prop.GetValue(this, null))));

			var fields = GetFieldsWithAttribute(attributeType);
			dependencies.AddRange(fields.Select(field => new Dependency(field.Name, field.FieldType, attributeType == typeof(NullDependencyAttribute), field.GetValue(this))));

			return dependencies;
		}

		private IEnumerable<PropertyInfo> GetPropertiesWithAttribute(Type attributeType)
		{
			return GetTestClassType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(prop => prop.IsDefined(attributeType, false));
		}

		private Type GetTestClassType()
		{
			var type = GetType();
			
			// Castle dynamic proxies are used in the custom xUnit runner code
			// which uses a test class instance per LG BDD test instead of a separate
			// instance per [Then] method.
			// When using dynamic proxies GetType() returns the dynamic proxy type and this
			// breaks behaviours such as setting private [Mock] properties so we need to return
			// the original test class which is the base class of the proxy
			if (type.Namespace == "Castle.Proxies")
			{
				type = type.BaseType;
			}

			return type;
		}

		private IEnumerable<FieldInfo> GetFieldsWithAttribute(Type attributeType)
		{
			return GetTestClassType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(prop => prop.IsDefined(attributeType, false));
		}

		private IEnumerable<MethodInfo> GetMethodsWithAttribute(Type attributeType)
		{
			return GetTestClassType().GetMethodsWithAttribute(attributeType);
		}

		private static ConstructorInfo GetPreferredConstructor(Type itemUnderTestType, ICollection<Dependency> dependencies)
		{
			if (itemUnderTestType.IsAbstract || itemUnderTestType.IsInterface)
			{
				throw new ItemUnderTestCannotBeInterfaceStaticOrAbstract(itemUnderTestType);
			}

			// Look for Public or Internal Constructors
			var constructors = itemUnderTestType
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => c.IsPublic || c.IsAssembly)
                .ToArray();

			if (constructors.All(c => c.IsPrivate))
			{
				throw new ItemUnderTestMustHavePublicConstructor(itemUnderTestType);
			}

			var preferredConstructor =
				constructors.FirstOrDefault(ci =>
				{
					var pars = ci.GetParameters();
					return pars.Count() == dependencies.Count &&
						   pars.All(p => dependencies.Any(d => d.Name == p.Name && d.Type == p.ParameterType));
				})

				??

				constructors.FirstOrDefault(ci =>
				{
					var pars = ci.GetParameters();
					return pars.Count() == dependencies.Count &&
						   pars.All(p => dependencies.Any(d => d.Type == p.ParameterType));
				});

			return preferredConstructor;
		}
	}
}