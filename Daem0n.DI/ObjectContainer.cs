using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Daem0n.DI
{
    public sealed class ObjectContainer : IServiceProvider, ISupportRequiredService, IDisposable
    {
        /// <summary>
        /// 单例接口关系
        /// </summary>
        private TypeRelationDictionary singleton;

        /// <summary>
        /// 线程单例关系
        /// </summary>
        private TypeRelationDictionary scoped;

        /// <summary>
        /// 新对象关系
        /// </summary>
        private TypeRelationDictionary transient;

        /// <summary>
        /// 单例对象集合
        /// </summary>
        private TargetObjectContainer singletonObjects;

        /// <summary>
        /// 线程单例对象集合
        /// </summary>
        private Dictionary<Type, List<KeyValuePair<Thread, object>>> scopedObjects;

        private TargetObjectContainer transientObjects;

        /// <summary>
        /// 线程对象扫描
        /// </summary>
        private Task scopedTask;
        public CancellationTokenSource ScopedScanCts { get; set; }

        /// <summary>
        /// 私有构造函数确
        /// </summary>
        internal ObjectContainer()
        {
            Init();
        }

        internal ObjectContainer(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            Init();
            foreach (var descriptor in serviceDescriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    // Test if the an open generic type is being registered
                    var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
                    this.Set(descriptor.ServiceType, descriptor.ImplementationType, descriptor.Lifetime);
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    //this.RegistObject(descriptor.ServiceType, descriptor.ImplementationInstance, descriptor.Lifetime);
                    //var registration = RegistrationBuilder.ForDelegate(descriptor.ServiceType, (context, parameters) =>
                    //{
                    //    var serviceProvider = context.Resolve<IServiceProvider>();
                    //    return descriptor.ImplementationFactory(serviceProvider);
                    //})
                    //    .ConfigureLifecycle(descriptor.Lifetime, lifetimeScopeTagForSingletons)
                    //    .CreateRegistration();

                    //builder.RegisterComponent(registration);
                    this.RegistObject(descriptor.ServiceType, () => descriptor.ImplementationFactory(this), descriptor.Lifetime);
                }
                else
                {
                    this.RegistObject(descriptor.ServiceType, descriptor.ImplementationInstance, descriptor.Lifetime);
                }
            }
        }

        /// <summary>
        /// 检查是否存在于容器中
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal CheckResult CheckIllegal(Type t)
        {
            if (singleton.ContainsKey(t) && singletonObjects[singleton[t]] != null)
            {
                return new CheckResult(true, ServiceLifetime.Singleton);
            }
            else if (scoped.ContainsKey(t) && GetScoped(t) != null)
            {
                return new CheckResult(true, ServiceLifetime.Scoped);
            }
            else if (transient.ContainsKey(t))
            {
                return new CheckResult(true, ServiceLifetime.Transient);
            }
            return false;
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            var obj = Get(typeof(T));
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
        /// 获取对象
        /// </summary>
        /// <param name="tSrouce"></param>
        /// <returns></returns>
        public object Get(Type tSource)
        {
            return Get(tSource, null, null);
        }

        /// <summary>
        /// 提供目标类型获取对象
        /// </summary>
        /// <param name="tSource"></param>
        /// <param name="getTTarget"></param>
        /// <returns></returns>
        public object Get(Type tSource, Func<Type, Type> getTTarget)
        {
            return Get(tSource, null, getTTarget);
        }

        /// <summary>
        /// 根据注入类型、目标类型获取对象
        /// </summary>
        /// <param name="tSource"></param>
        /// <param name="getRegistionWay"></param>
        /// <param name="getTTarget"></param>
        /// <returns></returns>
        public object Get(Type tSource, Func<ServiceLifetime> getRegistionWay, Func<Type, Type> getTTarget)
        {
            var tTarget = getTTarget?.Invoke(tSource);
            var registionWay = getRegistionWay?.Invoke();
            if ((getRegistionWay == null || registionWay == ServiceLifetime.Singleton) &&
                ((getTTarget == null && singleton.ContainsKey(tSource)) || singleton.ContainsRelation(tSource, tTarget)))
            {
                if (tTarget == null)
                {
                    tTarget = singleton[tSource];
                }
                return singletonObjects[tTarget];
            }
            if ((getRegistionWay == null || registionWay == ServiceLifetime.Scoped) &&
                ((getTTarget == null && scoped.ContainsKey(tSource)) || scoped.ContainsRelation(tSource, tTarget)))
            {
                if (tTarget == null)
                {
                    tTarget = scoped[tSource];
                }
                return GetScoped(tTarget);
            }
            if ((getRegistionWay == null || registionWay == ServiceLifetime.Transient) &&
                ((getTTarget == null && singleton.ContainsKey(tSource)) || transient.ContainsRelation(tSource, tTarget)))
            {
                if (tTarget == null)
                {
                    tTarget = transient[tSource];
                }
                return this.GetTransinet(tSource);
            }

            if (tSource.IsConstructedGenericType)
            {
                return CreateGenericType(tSource, getRegistionWay);
            }
            return null;
        }


        /// <summary>
        /// 添加单次对象
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        public ObjectContainer AddTransient<TSource, TTarget>() where TTarget : class, TSource
        {
            return AddTransient(typeof(TSource), typeof(TTarget));
        }
        /// <summary>
        /// 添加单次对象
        /// </summary>
        /// <param name="tSource"></param>
        /// <param name="tTarget"></param>
        /// <returns></returns>
        public ObjectContainer AddTransient(Type tSource, Type tTarget)
        {
            transient.Add(tSource, tTarget);
            transientObjects.Add(tSource, () => this.CreateInstance(tTarget));
            return this;
        }

        public ObjectContainer AddTransient(Type tSource, Func<object> func)
        {
            transient.Add(tSource, tSource);
            transientObjects.Add(tSource, func);
            return this;
        }

        /// <summary>
        /// 添加单例
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        public ObjectContainer AddSingleton<TSource, TTarget>() where TTarget : class, TSource
        {
            return AddSingleton(typeof(TSource), typeof(TTarget));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ObjectContainer AddSingleton<TSource, TTarget>(TTarget obj) where TTarget : class, TSource
        {
            return AddSingleton(typeof(TSource), typeof(TTarget), obj);
        }

        /// <summary>
        /// 添加单例
        /// </summary>
        /// <param name="tSource"></param>
        /// <param name="tTarget"></param>
        /// <returns></returns>
        public ObjectContainer AddSingleton(Type tSource, Type tTarget)
        {
            return AddSingleton(tSource, tTarget, () => this.CreateInstance(tTarget));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tSource"></param>
        /// <param name="tTarget"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ObjectContainer AddSingleton(Type tSource, Type tTarget, object obj)
        {
            if (singleton.Add(tSource, tTarget))
            {
                singletonObjects.Add(tTarget, obj);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tSource"></param>
        /// <param name="tTarget"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ObjectContainer AddSingleton(Type tSource, Type tTarget, Func<object> func)
        {
            if (singleton.Add(tSource, tTarget))
            {
                singletonObjects.Add(tTarget, func);
            }
            return this;
        }

        public ObjectContainer AddScope<TSource, TTarget>() where TTarget : class, TSource
        {
            return AddScope(typeof(TSource), typeof(TTarget));
        }
        public ObjectContainer AddScope<TSource, TTarget>(TTarget obj) where TTarget : class, TSource
        {
            return AddScope(typeof(TSource), typeof(TTarget), obj);
        }

        public ObjectContainer AddScope(Type tSource, Type tTarget)
        {
            return AddScope(tSource, tTarget, this.CreateInstance(tTarget));
        }
        public ObjectContainer AddScope(Type tSource, Type tTarget, object obj)
        {
            scoped.Add(tSource, tTarget);
            List<KeyValuePair<Thread, object>> list = null;
            if (scopedObjects.ContainsKey(tTarget) == false)
            {
                list = new List<KeyValuePair<Thread, object>>();
                scopedObjects.Add(tTarget, list);
            }
            else
            {
                list = scopedObjects[tTarget];
            }
            list.Add(new KeyValuePair<Thread, object>(Thread.CurrentThread, obj));
            return this;
        }



        public ObjectContainer Set(Type tSource, Type tTarget, ServiceLifetime lifetime)
        {
            if (lifetime == ServiceLifetime.Scoped)
            {
                this.AddScope(tSource, tTarget);
            }
            else if (lifetime == ServiceLifetime.Singleton)
            {
                this.AddSingleton(tSource, tTarget);
            }
            else if (lifetime == ServiceLifetime.Transient)
            {
                this.AddTransient(tSource, tTarget);
            }
            return this;
        }

        public void RegistObject(Type tSource, object obj, ServiceLifetime lifetime)
        {
            if (lifetime == ServiceLifetime.Scoped)
            {
                this.AddScope(tSource, tSource, obj);
            }
            else if (lifetime == ServiceLifetime.Singleton)
            {
                this.AddSingleton(tSource, tSource, obj);
            }
            else if (lifetime == ServiceLifetime.Transient)
            {
                this.AddTransient(tSource, tSource);
            }

        }
        public void RegistObject(Type tSource, Func<object> func, ServiceLifetime lifetime)
        {
            if (lifetime == ServiceLifetime.Scoped)
            {
                throw new NotImplementedException();
            }
            else if (lifetime == ServiceLifetime.Singleton)
            {
                this.AddSingleton(tSource, tSource, func);
            }
            else if (lifetime == ServiceLifetime.Transient)
            {
                this.AddTransient(tSource, func);
            }

        }

        /// <summary>
        /// 获取Scoped对象
        /// </summary>
        /// <param name="tSource"></param>
        /// <returns></returns>
        private object GetScoped(Type tSource)
        {
            var list = scopedObjects[tSource];
            if (list.Count(p => p.Key == Thread.CurrentThread) == 0)
            {
                var obj = this.CreateInstance(tSource);
                list.Add(new KeyValuePair<Thread, object>(Thread.CurrentThread, obj));
                return obj;
            }
            else
            {
                return list.First(p => p.Key == Thread.CurrentThread).Value;
            }
        }

        private object GetSingleton(Type tSource)
        {
            var tTarget = singleton[tSource];
            return singletonObjects[tTarget];
        }

        private object GetTransinet(Type tSource)
        {
            var tTarget = transient[tSource];
            return transientObjects[tTarget];
        }

        /// <summary>
        /// 创造泛型实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T CreateInstance<T>()
        {
            var obj = this.CreateInstance(typeof(T));
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
            // Console.WriteLine($"{type}\t{type.IsGenericType}\t{type.IsGenericParameter}\t{type.IsGenericTypeDefinition}");
            foreach (var info in type.GetConstructors())
            {
                if (info.GetParameters().Length == 0)
                {
                    return info.Invoke(null);
                }
                if (info.GetParameters().Select(p => this.Get(p.ParameterType)).Count(p => p == null) == 0)
                {
                    return info.Invoke(info.GetParameters().Select(p => this.Get(p.ParameterType)).ToArray());
                }
            }
            return null;
        }

        private object CreateGenericType(Type tSource, Func<ServiceLifetime> getLifetime)
        {
            var genericType = tSource.GetGenericTypeDefinition();
            var paramTypes = tSource.GenericTypeArguments;
            var tTarget = genericType.MakeGenericType(paramTypes);
            ServiceLifetime lifetime;
            if (getLifetime == null)
            {
                if (singleton.ContainsKey(genericType))
                {
                    lifetime = ServiceLifetime.Singleton;
                }
                else if (scoped.ContainsKey(genericType))
                {
                    lifetime = ServiceLifetime.Scoped;
                }
                else if (transient.ContainsKey(genericType))
                {
                    lifetime = ServiceLifetime.Transient;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                lifetime = getLifetime();
            }
            if (lifetime == ServiceLifetime.Singleton && singleton.ContainsKey(genericType))
            {
                singleton.Add(tSource, tTarget);
                singletonObjects.Add(tTarget, CreateInstance(tTarget));
                return GetSingleton(tSource);
            }
            if (lifetime == ServiceLifetime.Scoped && scoped.ContainsKey(genericType))
            {
                scoped.Add(tSource, genericType.MakeGenericType(paramTypes));
                return GetScoped(tSource);
            }
            if (lifetime == ServiceLifetime.Transient && transient.ContainsKey(genericType))
            {
                transient.Add(tSource, genericType.MakeGenericType(paramTypes));
                transientObjects.Add(tTarget, () => CreateInstance(tTarget));
                return GetTransinet(tSource);
            }
            return null;

        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            singleton = new TypeRelationDictionary();
            scoped = new TypeRelationDictionary();
            transient = new TypeRelationDictionary();
            singletonObjects = new TargetObjectContainer();
            scopedObjects = new Dictionary<Type, List<KeyValuePair<Thread, object>>>();
            ScopedScanCts = new CancellationTokenSource();
            transientObjects = new TargetObjectContainer();
            this.AddSingleton<IServiceProvider, ObjectContainer>(this);
            scopedTask = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        for (int i = 0; i < scopedObjects.Keys.Count; i++)
                        {
                            var key = scopedObjects.Keys.ElementAt(i);
                            for (int k = scopedObjects[key].Count - 1; k >= 0; k--)
                            {
                                var kv = scopedObjects[key][k];
                                if (kv.Key.ThreadState != ThreadState.Running)
                                {
                                    scopedObjects[key].Remove(kv);
                                }
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(1);
                }
            }, ScopedScanCts.Token);
        }

        public object GetService(Type serviceType)
        {
            return Get(serviceType);
        }

        public object GetRequiredService(Type serviceType)
        {
            return Get(serviceType);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        internal class CheckResult
        {
            public bool Result { get; }
            public ServiceLifetime IllegalWay { get; }
            public CheckResult(bool result, ServiceLifetime illegalWay = ServiceLifetime.Singleton)
            {
                this.Result = result;
                this.IllegalWay = illegalWay;
            }

            public static implicit operator bool(CheckResult checkResult)
            {
                return checkResult.Result;
            }
            public static implicit operator CheckResult(bool result)
            {
                return new CheckResult(result);
            }
        }
    }
}
