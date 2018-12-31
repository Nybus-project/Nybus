using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            fixture.Customize<RabbitMqOptions>(c => c.With(p => p.OutboundEncoding, OneOf(Encoding.GetEncodings().Select(e => e.GetEncoding().WebName))));

            return fixture;
        }

        private static T OneOf<T>(params T[] options)
        {
            var random = new Random();

            var randomValue = random.Next(0, options.Length);

            return options[randomValue];
        }

        private static T OneOf<T>(IEnumerable<T> options) => OneOf(options.ToArray());
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