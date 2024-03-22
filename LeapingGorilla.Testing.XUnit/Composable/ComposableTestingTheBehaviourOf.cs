using System;
using LeapingGorilla.Testing.Core.Composable;

namespace LeapingGorilla.Testing.XUnit.Composable
{
    /// <summary>
    /// Base class for XUnit tests using the Composable BDD test pattern.
    ///
    /// Implementors should override the ComposeTest() method and perform other test setup as with
    /// <see cref="WhenTestingTheBehaviourOf"/> tests.
    /// </summary>
    public abstract class ComposableTestingTheBehaviourOf : ComposableTestingTheBehaviourOfBase
    {
        // This property is used to disable the base.Setup() invocation during [Then] discovery.
        // Executing this method adds extra run time and if exceptions are thrown during setup
        // it results in no tests being discovered.
        internal static bool ThenDiscoveryInProgress { get; set; } = false; 
        
        /// <summary>
        /// Performs setup for this instance - this will prepare all mocks and request the test composition via the
        /// ComposeTest() abstract method. Following this it will call the [Given] methods (if any) and then call the
        /// [When] methods (if any). On completion the instance will be ready for the [Then] methods defined in the test
        /// composition to be executed.
        /// </summary>
        /// <param name="shouldSetup">
        /// Should we perform the setup step? Pass false to skip setup. If you skip setup you will
        /// need to implement it yourself by calling the <see cref="ComposableTestingTheBehaviourOfBase.Setup">base.Setup()</see>
        /// method.
        /// </param>
        protected ComposableTestingTheBehaviourOf(bool shouldSetup = true)
        {
            if (shouldSetup && !ThenDiscoveryInProgress)
            {
                base.Setup();
            }
        }
    }
}