using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Container;

namespace Tests.Container
{
    public interface ITestInterface
    {
        
    }

    public class TestGenericClass<T> : TestAbstractClass
    {
        
    }

    public class TestClassWithConstructor : TestAbstractClass
    {
        public TestClassWithConstructor(int aValue) { }
    }

    public class TestClassWithParameterlessConstructor : TestAbstractClass
    {
        public TestClassWithParameterlessConstructor() { }
    }

    public class TestClassWithDefaultConstructor : TestAbstractClass
    {
        
    }

    public abstract class TestAbstractClass : ITestInterface
    {
        
    }

    [TestFixture]
    public class ActivatorContainerTests
    {
        private ActivatorContainer CreateSystemUnderTest()
        {
            return new ActivatorContainer();
        }

        #region Resolve

        [Test]
        public void Resolve_cant_instantiate_interfaces()
        {
            var sut = CreateSystemUnderTest();

            var item = sut.Resolve<ITestInterface>();

            Assert.That(item, Is.Null);
        }

        [Test]
        public void Resolve_cant_instantiate_classes_with_constructor_with_parameters()
        {
            var sut = CreateSystemUnderTest();

            var item = sut.Resolve<TestClassWithConstructor>();

            Assert.That(item, Is.Null);
        }

        [Test]
        public void Resolve_cant_instantiate_abstract_classes()
        {
            var sut = CreateSystemUnderTest();

            var item = sut.Resolve<TestAbstractClass>();

            Assert.That(item, Is.Null);
        }

        [Test]
        public void Resolve_can_instantiate_classes_with_default_constructor()
        {
            var sut = CreateSystemUnderTest();

            var item = sut.Resolve<TestClassWithDefaultConstructor>();

            Assert.That(item, Is.Not.Null);
        }

        [Test]
        public void Resolve_can_instantiate_classes_with_parameterless_constructor()
        {
            var sut = CreateSystemUnderTest();

            var item = sut.Resolve<TestClassWithParameterlessConstructor>();

            Assert.That(item, Is.Not.Null);
        }

        [Test]
        public void Resolve_can_instantiate_closed_generic_classes()
        {
            var sut = CreateSystemUnderTest();

            var item = sut.Resolve<TestGenericClass<int>>();

            Assert.That(item, Is.Not.Null);
        }

        #endregion

        #region Activator theories

        [Theory]
        [ExpectedException]
        public void Activator_cant_instantiate_interfaces()
        {
            Activator.CreateInstance<ITestInterface>();
        }

        [Theory]
        [ExpectedException]
        public void Activator_cant_instantiate_classes_with_constructor_with_parameters()
        {
            Activator.CreateInstance<TestClassWithConstructor>();
        }

        [Theory]
        [ExpectedException]
        public void Activator_cant_instantiate_abstract_classes()
        {
            Activator.CreateInstance<TestAbstractClass>();
        }

        [Theory]
        public void Activator_can_instantiate_classes_with_parameterless_constructor()
        {
            var item = Activator.CreateInstance<TestClassWithParameterlessConstructor>();
            Assume.That(item, Is.Not.Null);
        }

        [Theory]
        public void Activator_can_instantiate_classes_with_default_constructor()
        {
            var item = Activator.CreateInstance<TestClassWithDefaultConstructor>();
            Assume.That(item, Is.Not.Null);
        }

        [Theory]
        [ExpectedException]
        public void Activator_cant_instantiate_generic_definition_classes()
        {
            Activator.CreateInstance(typeof (TestGenericClass<>));
        }

        [Theory]
        public void Activator_can_instantiate_closed_generic_class()
        {
            var item = Activator.CreateInstance<TestGenericClass<int>>();
            Assume.That(item, Is.Not.Null);
        }

        #endregion

        #region Release

        [Test]
        public void Release_disposes_disposable_objects()
        {
            Mock<IDisposable> mockDisposable = new Mock<IDisposable>();

            var sut = CreateSystemUnderTest();

            sut.Release(mockDisposable.Object);
            
            mockDisposable.Verify(p => p.Dispose(), Times.Once);
        }

        [Test]
        public void Release_can_handle_non_disposable_objects()
        {
            var sut = CreateSystemUnderTest();

            var testHandler = Mock.Of<IEventHandler<TestEvent>>();

            sut.Release(testHandler);
        }

        #endregion

        [Test]
        public void BeginScope_returns_self()
        {
            var sut = CreateSystemUnderTest();

            var scope = sut.BeginScope();

            Assert.That(scope, Is.SameAs(sut));
        }

        [Test]
        public void Dispose_has_no_effect()
        {
            var sut = CreateSystemUnderTest() as IDisposable;

            sut.Dispose();
        }
    }
}
