using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Daem0n.DI
{
    public class ObjectContainer
    {
        private Dictionary<Type, Type> singleton;
        private Dictionary<Type, Type> scoped;
        private Dictionary<Type, Type> transient;
        private Dictionary<Type, object> singletonObjects;
        private Dictionary<Type, object> scopedObjects;
        public ObjectContainer()
        {
            singleton = new Dictionary<Type, Type>();
            scoped = new Dictionary<Type, Type>();
            transient = new Dictionary<Type, Type>();
            singletonObjects = new Dictionary<Type, object>();
        }
        private bool CheckIllegal<T>()
        {
            return CheckIllegal(typeof(T));
        }
        public bool CheckIllegal(Type t)
        {
            if (singleton.ContainsKey(t) ||
               scoped.ContainsKey(t) ||
               transient.ContainsKey(t))
            {
                return false;
            }
            return true;
        }
        public ObjectContainer AddSingleton<TSource, TTarget>()
        {
            if (CheckIllegal<TSource>())
            {

            }
            else
            {

            }
            return this;
        }

        /// <summary>
        /// 直接获取对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private object Get(Type type)
        {
            if (singleton.ContainsKey(type))
            {
                return singletonObjects[singleton[type]];
            }
            if (scoped.ContainsKey(type))
            {
                return GetScoped(type);
            }
            if (transient.ContainsKey(type))
            {
                return CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// 获取Scoped对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private object GetScoped(Type type)
        {
            return scopedObjects[type];
        }

        /// <summary>
        /// 创造泛型实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T CreateInstance<T>()
        {
            var obj = CreateInstance(typeof(T));
            if (obj == null)
            {
                return default(T);
            }
            else
            {
                return (T)obj;
            }
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private object CreateInstance(Type type)
        {
            foreach (var info in type.GetConstructors())
            {
                if (info.GetParameters().Length == 0)
                {
                    return info.Invoke(null);
                }
                if (info.GetParameters().Select(p => CheckIllegal(p.ParameterType)).Count(p => p == true) == 0)
                {
                    return info.Invoke(info.GetParameters().Select(p => CreateInstance(p.ParameterType)).ToArray());
                }
            }
            return null;
        }
    }
}
