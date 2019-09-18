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
        private Dictionary<Type, object> singletonObjects;

        /// <summary>
        /// 线程单例对象集合
        /// </summary>
        private Dictionary<Type, List<KeyValuePair<Thread, object>>> scopedObjects;

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
                    this.RegistObject(descriptor.ServiceType, descriptor.ImplementationFactory(this), descriptor.Lifetime);
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
        private CheckResult CheckIllegal(Type t)
        {
            if (singleton.ContainsKey(t))
            {
                return new CheckResult(true, ServiceLifetime.Singleton);
            }
            else if (scoped.ContainsKey(t))
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
            if (singleton.ContainsKey(tSource))
            {
                return singletonObjects[singleton[tSource]];
            }
            if (scoped.ContainsKey(tSource))
            {
                return GetScoped(tSource);
            }
            if (transient.ContainsKey(tSource))
            {
                return CreateInstance(transient[tSource]);
            }
            return null;
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
            var tTarget = getTTarget(tSource);
            var registionWay = getRegistionWay?.Invoke();
            if ((getRegistionWay == null || registionWay == ServiceLifetime.Scoped) &&
                singleton.ContainsRelation(tSource, tTarget))
            {
                return singletonObjects[tTarget];
            }
            if ((getRegistionWay == null || registionWay == ServiceLifetime.Scoped) &&
                (scoped.ContainsRelation(tSource, tTarget)))
            {
                return GetScoped(tTarget);
            }
            if ((getRegistionWay == null || registionWay == ServiceLifetime.Scoped) &&
                (transient.ContainsRelation(tSource, tTarget)))
            {
                return CreateInstance(tTarget);
            }
            return null;
        }

        /// <summary>
        /// 添加单次对象
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        public ObjectContainer AddTransient<TSource, TTarget>() where TTarget : TSource, new()
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
            return this;
        }

        /// <summary>
        /// 添加单例
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        public ObjectContainer AddSingleton<TSource, TTarget>() where TTarget : TSource, new()
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
        public ObjectContainer AddSingleton<TSource, TTarget>(TTarget obj) where TTarget : TSource, new()
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
            return AddSingleton(tSource, tTarget, CreateInstance(tTarget));
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
            singleton.Add(tSource, tTarget);
            singletonObjects.Add(tTarget, obj);
            return this;
        }

        public ObjectContainer AddScope<TSource, TTarget>() where TTarget : TSource, new()
        {
            return AddScope(typeof(TSource), typeof(TTarget));
        }
        public ObjectContainer AddScope<TSource, TTarget>(TTarget obj) where TTarget : TSource, new()
        {
            return AddScope(typeof(TSource), typeof(TTarget), obj);
        }

        public ObjectContainer AddScope(Type tSource, Type tTarget)
        {
            return AddScope(tSource, tTarget, CreateInstance(tTarget));
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
                var obj = CreateInstance(tSource);
                list.Add(new KeyValuePair<Thread, object>(Thread.CurrentThread, obj));
                return obj;
            }
            else
            {
                return list.First(p => p.Key == Thread.CurrentThread).Value;
            }
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
            // Console.WriteLine($"{type}\t{type.IsGenericType}\t{type.IsGenericParameter}\t{type.IsGenericTypeDefinition}");
            if (type.ContainsGenericParameters)
            {
                return null;
            }
            foreach (var info in type.GetConstructors())
            {
                if (info.GetParameters().Length == 0)
                {
                    return info.Invoke(null);
                }
                if (info.GetParameters().Select(p => CheckIllegal(p.ParameterType)).Count(p => p == false) == 0)
                {
                    return info.Invoke(info.GetParameters().Select(p => CreateInstance(p.ParameterType)).ToArray());
                }
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
            singletonObjects = new Dictionary<Type, object>();
            scopedObjects = new Dictionary<Type, List<KeyValuePair<Thread, object>>>();
            ScopedScanCts = new CancellationTokenSource();
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

        private class CheckResult
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
