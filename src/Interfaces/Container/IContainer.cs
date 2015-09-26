using System;

namespace Nybus.Container
{
    public interface IContainer
    {
        IScope BeginScope();
    }

    public interface IScope : IDisposable
    {
        T Resolve<T>();

        void Release<T>(T component);
    }
}