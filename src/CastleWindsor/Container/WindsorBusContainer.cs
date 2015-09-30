using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel;

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

        public T Resolve<T>()
        {
            return _kernel.Resolve<T>();
        }

        public void Release<T>(T component)
        {
            _kernel.ReleaseComponent(component);
        }
    }
}
