using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;
using Nybus.Container;

namespace Tests.Container
{
    [TestFixture]
    public class ScopeTests
    {

        private IWindsorContainer CreateContainer()
        {
            IWindsorContainer container = new WindsorContainer();

            container.Register(Component.For<InnerDependency>().LifestyleScoped());
            container.Register(Component.For<OuterDependency>().LifestyleTransient());
            container.Register(Component.For<SharedDependency>().LifestyleSingleton());

            return container;
        }

        [Test]
        public void A_test()
        {
            var container = CreateContainer();

            using (container.BeginScope())
            {
                var outer1 = container.Resolve<OuterDependency>();
                var outer2 = container.Resolve<OuterDependency>();

                Assert.That(outer1, Is.Not.SameAs(outer2));
                Assert.That(outer1.Inner, Is.SameAs(outer2.Inner));
                Assert.That(outer1.Shared, Is.SameAs(outer2.Shared));

                var inner = outer1.Inner;
                var shared = outer1.Shared;

                container.Release(outer1);
                container.Release(outer2);

                Assert.That(inner, Is.Not.Null);

                var newInner = container.Resolve<InnerDependency>();
                var newShared = container.Resolve<SharedDependency>();

                Assert.That(inner, Is.SameAs(newInner));

                Assert.That(shared, Is.SameAs(newShared));

                container.Release(inner);
                container.Release(newInner);

                container.Release(shared);
                container.Release(newShared);
            }
        }

        [Test]
        public void B_test()
        {
            var container = CreateContainer();

            OuterDependency outer1, outer2;
            InnerDependency inner1, inner2;
            SharedDependency shared1, shared2;

            using (container.BeginScope())
            {
                outer1 = container.Resolve<OuterDependency>();
                inner1 = outer1.Inner;
                shared1 = outer1.Shared;
            }

            using (container.BeginScope())
            {
                outer2 = container.Resolve<OuterDependency>();
                inner2 = outer2.Inner;
                shared2 = outer2.Shared;
            }

            Assert.That(outer1, Is.Not.SameAs(outer2));
            Assert.That(inner1, Is.Not.SameAs(inner2));
            Assert.That(shared1, Is.SameAs(shared2));
        }

        [Test]
        public async Task C_test()
        {
            var container = CreateContainer();

            OuterDependency outer1 = null, outer2 = null;
            InnerDependency inner1 = null, inner2 = null;
            SharedDependency shared1 = null, shared2 = null;


            var t1 = Task.Run(() =>
            {
                using (container.BeginScope())
                {
                    outer1 = container.Resolve<OuterDependency>();
                    inner1 = outer1.Inner;
                    shared1 = outer1.Shared;
                }
            });

            var t2 = Task.Run(() =>
            {
                using (container.BeginScope())
                {
                    outer2 = container.Resolve<OuterDependency>();
                    inner2 = outer2.Inner;
                    shared2 = outer2.Shared;
                }
            });

            await Task.WhenAll(t1, t2);

            Assert.That(outer1, Is.Not.SameAs(outer2));
            Assert.That(inner1, Is.Not.SameAs(inner2));
            Assert.That(shared1, Is.SameAs(shared2));

        }

        public class SharedDependency
        {
            
        }

        public class InnerDependency
        {
            
        }

        public class OuterDependency
        {
            public InnerDependency Inner { get; }

            public SharedDependency Shared { get; }

            public OuterDependency(InnerDependency inner, SharedDependency shared)
            {
                Inner = inner;
                Shared = shared;
            }
        }
    }
}
