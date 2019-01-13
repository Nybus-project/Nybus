using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Nybus.Utils;

namespace Tests
{
    public class CustomAutoMoqDataAttribute : AutoDataAttribute
    {
        public CustomAutoMoqDataAttribute() : base(CreateFixture)
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
}