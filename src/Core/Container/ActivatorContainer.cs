using System;
using System.Linq;

namespace Nybus.Container
{
    public class ActivatorContainer : IContainer, IScope
    {
        public T Resolve<T>()
        {
            if (!IsValidType(typeof (T)))
                return default(T);

            return Activator.CreateInstance<T>();
        }

        private bool IsValidType(Type type)
        {
            if (type.IsAbstract)
                return false;

            if (type.GetConstructors().All(c => c.GetParameters().Any()))
                return false;

            return true;
        }

        public void Release<T>(T component)
        {
            IDisposable disposable = component as IDisposable;

            disposable?.Dispose();
        }

        public IScope BeginScope()
        {
            return this;
        }

        void IDisposable.Dispose() { }
    }
}