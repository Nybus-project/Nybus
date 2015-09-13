using System;
using NUnit.Framework;
using Nybus.MassTransit;
using Ploeh.AutoFixture;

namespace Tests.MassTransit
{
    [TestFixture]
    public class MassTransitConnectionDescriptorTests
    {
        private IFixture fixture;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        [Test, ExpectedException]
        public void Host_is_required()
        {
            var username = fixture.Create<string>();
            var password = fixture.Create<string>();

            new MassTransitConnectionDescriptor(null, username, password);
        }

        [Test, ExpectedException]
        public void Username_is_required()
        {
            var host = fixture.Create<Uri>();
            var password = fixture.Create<string>();

            new MassTransitConnectionDescriptor(host, null, password);
        }

        [Test]
        public void Username_can_be_empty()
        {
            var host = fixture.Create<Uri>();
            var password = fixture.Create<string>();

            new MassTransitConnectionDescriptor(host, string.Empty, password);
        }

        [Test, ExpectedException]
        public void Password_is_required()
        {
            var host = fixture.Create<Uri>();
            var username = fixture.Create<string>();

            new MassTransitConnectionDescriptor(host, username, null);
        }

        [Test]
        public void Password_can_be_empty()
        {
            var host = fixture.Create<Uri>();
            var username = fixture.Create<string>();

            new MassTransitConnectionDescriptor(host, username, string.Empty);
        }


        [Test]
        public void MassTransitConnectionDescriptor_can_be_parsed_from_connection_string()
        {
            var host = fixture.Create<Uri>();
            var username = fixture.Create<string>();
            var password = fixture.Create<string>();

            var connectionString = $"host={host};username={username};password={password}";

            var descriptor = MassTransitConnectionDescriptor.Parse(connectionString);

            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor.Host, Is.EqualTo(host));
            Assert.That(descriptor.UserName, Is.EqualTo(username));
            Assert.That(descriptor.Password, Is.EqualTo(password));
        }

        [Test]
        public void MassTransitConnectionDescriptor_can_be_parsed_from_configuration_file()
        {
            var descriptor = MassTransitConnectionDescriptor.FromConfiguration("Test");

            Assert.That(descriptor, Is.Not.Null);
        }
    }
}
