using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;

namespace Nybus.Logging
{
    public static class LoggingRegistrationExtensions
    {
        public static ComponentRegistration<ILogger> CreateLoggerForTargetClass(this ComponentRegistration<ILogger> registration)
        {
            return registration.UsingFactoryMethod(CreateLogger);
        }

        private static ILogger CreateLogger(IKernel kernel, ComponentModel model, CreationContext context)
        {
            var loggerFactory = kernel.Resolve<ILoggerFactory>();

            string className = context.Handler.ComponentModel.Implementation.FullName;

            var logger = loggerFactory.CreateLogger(className);

            kernel.ReleaseComponent(loggerFactory);

            return logger;

        }
    }
}
