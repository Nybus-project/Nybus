using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Nybus.Utils;

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

            fixture.Register(() => MessageDescriptor.EqualityComparer);

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

            return fixture;
        }
    }
}