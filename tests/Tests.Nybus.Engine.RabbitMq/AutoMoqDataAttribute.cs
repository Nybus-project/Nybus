using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;
using Nybus;
using Nybus.Configuration;

namespace Tests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(CreateFixture)
        {
            
        }

        private static IFixture CreateFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true,
                GenerateDelegates = true
            });

            fixture.Customizations.Add(new TypeRelay(typeof(ISerializer), typeof(JsonSerializer)));

            fixture.Register(() => TemporaryQueueFactory.Instance as TemporaryQueueFactory);

            return fixture;
        }
    }

    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] args) : base(CreateFixture, args)
        {
            
        }

        private static IFixture CreateFixture()
        {
            IFixture fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true,
                GenerateDelegates = true
            });

            fixture.Customizations.Add(new TypeRelay(typeof(ISerializer), typeof(JsonSerializer)));

            return fixture;
        }
    }
}