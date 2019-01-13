using System.Linq;
using System.Text;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;
using Nybus.Configuration;
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

            fixture.Customizations.Add(new TypeRelay(typeof(ISerializer), typeof(JsonSerializer)));

            fixture.Customizations.Add(new TypeRelay(typeof(IMessageDescriptorStore), typeof(TestMessageDescriptorStore)));

            fixture.Register(() => TemporaryQueueFactory.Instance as TemporaryQueueFactory);

            fixture.Customizations.Add(new ElementsBuilder<Encoding>(Encoding.GetEncodings().Select(e => e.GetEncoding())));

            fixture.Customize<RabbitMqOptions>(c => c.With(p => p.OutboundEncoding, (Encoding encoding) => encoding.WebName));

            return fixture;
        }
    }

    public class CustomInlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public CustomInlineAutoMoqDataAttribute(params object[] args) : base(CreateFixture, args)
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