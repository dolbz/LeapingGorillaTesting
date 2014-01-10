﻿/*    
   Copyright 2014 Leaping Gorilla LTD

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
using LeapingGorilla.Testing.Attributes;
using LeapingGorilla.Testing.Tests.Mocks;
using NSubstitute;
using NUnit.Framework;

namespace LeapingGorilla.Testing.Tests
{
	public class WhenTestingDependencyInjectionWithAField : WhenTestingTheBehaviourOf
	{
		[ItemUnderTest] 
		public SimpleClassToTest SimpleClass;

		[Dependency] 
		public IMockLogger FakeLogger;

		[When]
		protected void MyClassRaisesALoggingEvent()
		{
			SimpleClass.MethodThatGeneratesALogMessage();
		}

		[Then]
		public void TheFakeLoggerShouldBeCreated()
		{
			Assert.That(FakeLogger, Is.Not.Null);
		}

		[Then]
		public void WeShouldReceiveACallToLog()
		{
			FakeLogger.Received().Log(Arg.Any<string>());
		}

		[Then]
		public void TheSimpleClassShouldBeCreated()
		{
			Assert.That(SimpleClass, Is.Not.Null);
		}
	}
}
