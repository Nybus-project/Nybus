using System;
using System.Reflection;
using NUnit.Framework;
using Nybus.Utils;

namespace Tests.Utils.Utilities
{

    [TestFixture]
    public class GetUniqueNameForApplicationTests
    {
        [TestFixture]
        public class WhenNoEntryAssembly
        {
            [Test]
            public void Returns_hashed_current_domain_base_path()
            {
                var actual = Nybus.Utils.Utilities.GetUniqueNameForApplication();

                var expected = AppDomain.CurrentDomain.BaseDirectory.Hash();

                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [TestFixture]
        public class WhenHasEntryAssembly
        {
            [TestFixtureSetUp]
            public void FixtureSetUp()
            {
                SetEntryAssembly(Assembly.GetCallingAssembly());
            }

            [TestFixtureTearDown]
            public void FixtureTearDown()
            {
                SetEntryAssembly(null);
            }

            private void SetEntryAssembly(Assembly assembly)
            {
                AppDomainManager manager = new AppDomainManager();
                FieldInfo entryAssemblyfield = manager.GetType().GetField("m_entryAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
                entryAssemblyfield.SetValue(manager, assembly);

                AppDomain domain = AppDomain.CurrentDomain;
                FieldInfo domainManagerField = domain.GetType().GetField("_domainManager", BindingFlags.Instance | BindingFlags.NonPublic);
                domainManagerField.SetValue(domain, manager);
            }

            [Test]
            public void Returns_hashed_entry_assembly_full_name()
            {
                var actual = Nybus.Utils.Utilities.GetUniqueNameForApplication();

                var expected = System.Reflection.Assembly.GetEntryAssembly().FullName.Hash();

                Assert.That(actual, Is.EqualTo(expected));
            }

        }
    }
}
