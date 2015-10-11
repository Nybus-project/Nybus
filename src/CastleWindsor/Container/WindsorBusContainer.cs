using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;

namespace Nybus.Container
{
    public class WindsorBusContainer : IContainer
    {
        private readonly IKernel _kernel;

        public WindsorBusContainer(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            _kernel = kernel;
        }

        public IScope BeginScope()
        {
            var scope = _kernel.BeginScope();
            return new WindsorScope(_kernel, scope);
        }

        public IKernel Kernel => _kernel;
    }

    public class WindsorScope : IScope
    {
        private readonly IKernel _kernel;
        private readonly IDisposable _scope;

        public WindsorScope(IKernel kernel, IDisposable scope)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));
            if (scope == null) throw new ArgumentNullException(nameof(scope));

            _kernel = kernel;
            _scope = scope;
        }

        public T Resolve<T>()
        {
            return _kernel.Resolve<T>();
        }

        public void Release<T>(T component)
        {
            _kernel.ReleaseComponent(component);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
