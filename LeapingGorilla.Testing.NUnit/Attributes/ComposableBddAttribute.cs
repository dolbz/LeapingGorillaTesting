using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeapingGorilla.Testing.Core.Attributes;
using LeapingGorilla.Testing.Core.Exceptions;
using LeapingGorilla.Testing.NUnit.Composable;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace LeapingGorilla.Testing.NUnit.Attributes
{
    internal class ComposableBddAttribute : Attribute, IFixtureBuilder
    {
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            var composeTestMethod = typeInfo
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(x => x.Name == "ComposeTest");

            var composedTest = (ComposedTest)composeTestMethod.Invoke(Activator.CreateInstance(typeInfo.Type));

            if (composedTest == null)
            {
                throw new NoComposedTestDefinitionException();
            }
            
            return new[] { new NUnitTestFixtureBuilder().BuildFrom(typeInfo, new ComposedThensFilter(composedTest.ThenMethods)) };
        }
    }
}