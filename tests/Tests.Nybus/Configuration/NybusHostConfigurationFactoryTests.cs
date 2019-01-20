using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using Nybus.Filters;

namespace Tests.Configuration
{
    [TestFixture]
    public class NybusHostConfigurationFactoryTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(NybusHostConfigurationFactory).GetConstructors());
        }

        [Test, CustomAutoMoqData]
        public void CreateConfiguration_uses_selected_provider_to_create_Command_filters([Frozen] IEnumerable<IErrorFilterProvider> errorFilterProviders, NybusHostConfigurationFactory sut)
        {
            var providers = errorFilterProviders.ToArray();
            var selectedProviders = providers.Take(providers.Length - 1);

            var settings = new Dictionary<string, string>();

            var i = 0;
            foreach (var provider in selectedProviders)
            {
                settings.Add($"CommandErrorFilters:{i}:type", provider.ProviderName);
                i++;
            }

            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            var options = new NybusHostOptions();

            config.Bind(options);

            var configuration = sut.CreateConfiguration(options);

            Assert.True(selectedProviders.All(p =>
            {
                Mock.Get(p).Verify(o => o.CreateErrorFilter(It.IsAny<IConfigurationSection>()));
                return true;
            }));
        }


        [Test, CustomAutoMoqData]
        public void CreateConfiguration_discards_unused_selected_provider_to_create_Command_filters([Frozen] IEnumerable<IErrorFilterProvider> errorFilterProviders, NybusHostConfigurationFactory sut)
        {
            var providers = errorFilterProviders.ToArray();
            var selectedProviders = providers.Take(providers.Length - 1);
            var unusedProviders = providers.Skip(providers.Length - 1);

            var settings = new Dictionary<string, string>();

            var i = 0;
            foreach (var provider in selectedProviders)
            {
                settings.Add($"CommandErrorFilters:{i}:type", provider.ProviderName);
                i++;
            }

            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            var options = new NybusHostOptions();

            config.Bind(options);

            var configuration = sut.CreateConfiguration(options);

            Assert.True(unusedProviders.All(p =>
            {
                Mock.Get(p).Verify(o => o.CreateErrorFilter(It.IsAny<IConfigurationSection>()), Times.Never);
                return true;
            }));
        }

        [Test, CustomAutoMoqData]
        public void CreateConfiguration_ignores_unregistered_providers_when_creating_command_filters(NybusHostConfigurationFactory sut, NybusHostOptions options)
        {
            var configuration = sut.CreateConfiguration(options);

            Assert.That(configuration.CommandErrorFilters, Is.Empty);
        }

        [Test, CustomAutoMoqData]
        public void CreateConfiguration_uses_selected_provider_to_create_Event_filters([Frozen] IEnumerable<IErrorFilterProvider> errorFilterProviders, NybusHostConfigurationFactory sut)
        {
            var providers = errorFilterProviders.ToArray();
            var selectedProviders = providers.Take(providers.Length - 1);

            var settings = new Dictionary<string, string>();

            var i = 0;
            foreach (var provider in selectedProviders)
            {
                settings.Add($"EventErrorFilters:{i}:type", provider.ProviderName);
                i++;
            }

            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            var options = new NybusHostOptions();

            config.Bind(options);

            var configuration = sut.CreateConfiguration(options);

            Assert.True(selectedProviders.All(p =>
            {
                Mock.Get(p).Verify(o => o.CreateErrorFilter(It.IsAny<IConfigurationSection>()));
                return true;
            }));
        }


        [Test, CustomAutoMoqData]
        public void CreateConfiguration_discards_unused_selected_provider_to_create_Event_filters([Frozen] IEnumerable<IErrorFilterProvider> errorFilterProviders, NybusHostConfigurationFactory sut)
        {
            var providers = errorFilterProviders.ToArray();
            var selectedProviders = providers.Take(providers.Length - 1);
            var unusedProviders = providers.Skip(providers.Length - 1);

            var settings = new Dictionary<string, string>();

            var i = 0;
            foreach (var provider in selectedProviders)
            {
                settings.Add($"EventErrorFilters:{i}:type", provider.ProviderName);
                i++;
            }

            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            var options = new NybusHostOptions();

            config.Bind(options);

            var configuration = sut.CreateConfiguration(options);

            Assert.True(unusedProviders.All(p =>
            {
                Mock.Get(p).Verify(o => o.CreateErrorFilter(It.IsAny<IConfigurationSection>()), Times.Never);
                return true;
            }));
        }

        [Test, CustomAutoMoqData]
        public void CreateConfiguration_ignores_unregistered_providers_when_creating_event_filters(NybusHostConfigurationFactory sut, NybusHostOptions options)
        {
            var configuration = sut.CreateConfiguration(options);

            Assert.That(configuration.EventErrorFilters, Is.Empty);
        }
    }
}
