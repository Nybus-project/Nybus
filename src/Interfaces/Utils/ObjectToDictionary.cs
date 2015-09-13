using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Nybus.Utils
{
    /// <summary>
    /// Code inspired by: http://blog.andreloker.de/post/2008/05/03/Anonymous-type-to-dictionary-using-DynamicMethod.aspx
    /// </summary>
    public static class ObjectToDictionary
    {
        /// <summary>
        /// Converts an anonymous object into a IReadOnlyDictionary
        /// </summary>
        /// <returns>An IReadOnlyDictionary that contains the values of the properties as items</returns>
        public static IReadOnlyDictionary<string, object> Convert(object dataObject)
        {
            if (dataObject == null)
            {
                return new Dictionary<string, object>();
            }

            if (dataObject is IReadOnlyDictionary<string, object>)
            {
                return (IReadOnlyDictionary<string, object>)dataObject;
            }

            return (IReadOnlyDictionary<string, object>)GetObjectToDictionaryConverter(dataObject)(dataObject);
        }

        internal static readonly Dictionary<Type, Func<object, IReadOnlyDictionary<string, object>>> Cache = new Dictionary<Type, Func<object, IReadOnlyDictionary<string, object>>>();
        private static readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        private static Func<object, IReadOnlyDictionary<string, object>> CreateObjectToDictionaryConverter(Type itemType)
        {
            var dictType = typeof(Dictionary<string, object>);

            // setup dynamic method
            // Important: make itemType owner of the method to allow access to internal types
            var dm = new DynamicMethod(string.Empty, typeof(IReadOnlyDictionary<string, object>), new[] { typeof(object) }, itemType);
            var il = dm.GetILGenerator();

            // Dictionary.Add(object key, object value)
            var addMethod = dictType.GetMethod("Add");

            // create the Dictionary and store it in a local variable
            il.DeclareLocal(dictType);
            il.Emit(OpCodes.Newobj, dictType.GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);

            var attributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            foreach (var property in itemType.GetProperties(attributes).Where(info => info.CanRead))
            {
                // load Dictionary (prepare for call later)
                il.Emit(OpCodes.Ldloc_0);
                // load key, i.e. name of the property
                il.Emit(OpCodes.Ldstr, property.Name);

                // load value of property to stack
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, property.GetGetMethod(), null);

                if (property.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Box, property.PropertyType);
                }

                // stack at this point
                // 1. string or null (value)
                // 2. string (key)
                // 3. dictionary

                // ready to call dict.Add(key, value)
                il.EmitCall(OpCodes.Callvirt, addMethod, null);
            }
            // finally load Dictionary and return
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return (Func<object, IReadOnlyDictionary<string, object>>)dm.CreateDelegate(typeof(Func<object, IReadOnlyDictionary<string, object>>));
        }

        private static Func<object, IReadOnlyDictionary<string, object>> GetObjectToDictionaryConverter(object item)
        {
            rwLock.EnterUpgradeableReadLock();

            try
            {
                Func<object, IReadOnlyDictionary<string, object>> ft;

                if (!Cache.TryGetValue(item.GetType(), out ft))
                {
                    rwLock.EnterWriteLock();

                    try
                    {
                        if (!Cache.TryGetValue(item.GetType(), out ft))
                        {
                            ft = CreateObjectToDictionaryConverter(item.GetType());
                            Cache[item.GetType()] = ft;
                        }
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }

                return ft;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }
    }
}