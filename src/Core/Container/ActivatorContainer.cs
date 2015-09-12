﻿using System;

namespace Nybus.Container
{
    public class ActivatorContainer : IContainer
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

            if (type.IsInterface)
                return false;

            if (type.IsGenericTypeDefinition)
                return false;

            return true;
        }

        public void Release<T>(T component)
        {
            IDisposable disposable = component as IDisposable;

            disposable?.Dispose();
        }
    }
}