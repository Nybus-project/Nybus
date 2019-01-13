using NUnit.Framework;

namespace Tests
{
    public class ExternalTestFixtureAttribute : TestFixtureAttribute
    {
        private const string External = "External";

        public ExternalTestFixtureAttribute()
        {
            Category = External;
            Description = "These tests require an external dependency, should not be ran on the build server";
        }
    }
}