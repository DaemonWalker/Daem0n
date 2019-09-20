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
        private Dictionary<Type, List<KeyValuePair<Thread, ObjectBuilder>>> scopedObjects;

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
                return GetSingleton(tSource);
            }
            if ((getRegistionWay == null || registionWay == ServiceLifetime.Scoped) &&
                ((getTTarget == null && scoped.ContainsKey(tSource)) || scoped.ContainsRelation(tSource, tTarget)))
            {
                if (tTarget == null)
                {
                    tTarget = scoped[tSource];
                }
                return GetScoped(tSource);
            }
            if ((getRegistionWay == null || registionWay == ServiceLifetime.Transient) &&
                ((getTTarget == null && singleton.ContainsKey(tSource)) || transient.ContainsRelation(tSource, tTarget)))
            {
                if (tTarget == null)
                {
                    tTarget = transient[tSource];
                }
                return this.GetTransient(tSource);
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
            List<KeyValuePair<Thread, ObjectBuilder>> list = null;
            if (scopedObjects.ContainsKey(tTarget) == false)
            {
                list = new List<KeyValuePair<Thread, ObjectBuilder>>();
                scopedObjects.Add(tTarget, list);
            }
            else
            {
                list = scopedObjects[tTarget];
            }
            list.Add(new KeyValuePair<Thread, ObjectBuilder>(Thread.CurrentThread, new ObjectBuilder() { Obj = obj }));
            return this;
        }

        public ObjectContainer AddScope(Type tSource, Type tTarget, Func<object> func)
        {
            scoped.Add(tSource, tTarget);
            List<KeyValuePair<Thread, ObjectBuilder>> list = null;
            if (scopedObjects.ContainsKey(tTarget) == false)
            {
                list = new List<KeyValuePair<Thread, ObjectBuilder>>();
                scopedObjects.Add(tTarget, list);
            }
            else
            {
                list = scopedObjects[tTarget];
            }
            list.Add(new KeyValuePair<Thread, ObjectBuilder>(Thread.CurrentThread, new ObjectBuilder() { Func = func }));
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
            lock (Thread.CurrentThread)
            {
                var tTarget = scoped[tSource];
                var list = scopedObjects[tTarget];
                if (list.Count(p => p.Key == Thread.CurrentThread) == 0)
                {
                    var obj = this.CreateInstance(tTarget);
                    list.Add(new KeyValuePair<Thread, ObjectBuilder>(Thread.CurrentThread, new ObjectBuilder() { Obj = obj }));
                    return obj;
                }
                else
                {
                    var builder = list.First(p => p.Key == Thread.CurrentThread).Value;
                    if (builder.Obj == null)
                    {
                        builder.Obj = builder.Func?.Invoke() ?? CreateInstance(tTarget);
                    }
                    return builder.Obj;
                }
            }
        }

        private object GetSingleton(Type tSource)
        {
            var tTarget = singleton[tSource];
            return singletonObjects[tTarget];
        }

        private object GetTransient(Type tSource)
        {
            var tTarget = transient[tSource];
            return transientObjects.CreateObj(tTarget);
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
                if (info.GetParameters().Select(p =>
                {
                    var obj = this.GetTypeLifetime(p.ParameterType);
                    if (obj == null)
                    {
                        ExtenssionMethods.Output($"{type} - {p.ParameterType} - {p.ParameterType.IsConstructedGenericType}", ConsoleColor.Yellow);
                    }
                    return obj;
                }).Count(p => p == null) == 0)
                {
                    var parms = info.GetParameters().Select(p => this.Get(p.ParameterType)).ToArray();
                    return info.Invoke(parms);
                }
            }
            return null;
        }

        private object CreateGenericType(Type tSource, Func<ServiceLifetime> getLifetime)
        {
            var genericType = tSource.GetGenericTypeDefinition();
            var paramTypes = tSource.GenericTypeArguments;
            ServiceLifetime? lifetime;

            if (tSource.IsConstructedGenericType &&
                tSource.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var innerType = tSource.GenericTypeArguments.Single();
                lifetime = GetTypeLifetime(innerType);
                var listType = typeof(List<>).MakeGenericType(tSource.GenericTypeArguments);
                Set(tSource, listType, lifetime.Value);
                return Get(tSource);
            }
            else
            {
                lifetime = getLifetime?.Invoke() ?? GetTypeLifetime(tSource);
                if (lifetime == ServiceLifetime.Singleton && singleton.ContainsKey(genericType))
                {
                    var tTarget = singleton[genericType].MakeGenericType(paramTypes);
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
                    var tTarget = transient[genericType].MakeGenericType(paramTypes);
                    transient.Add(tSource, tTarget);
                    transientObjects.Add(tTarget, () => CreateInstance(tTarget));
                    return GetTransient(tSource);
                }
                ExtenssionMethods.Output($"Create2 {tSource} Generic Failed", ConsoleColor.Green);
                return null;
            }
        }

        private ServiceLifetime? GetTypeLifetime(Type t)
        {
            if (singleton.ContainsKey(t))
            {
                return ServiceLifetime.Singleton;
            }
            if (scoped.ContainsKey(t))
            {
                return ServiceLifetime.Scoped;
            }
            if (transient.ContainsKey(t))
            {
                return ServiceLifetime.Transient;
            }
            if (t.IsConstructedGenericType)
            {
                return GetTypeLifetime(t.GetGenericTypeDefinition());
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
            scopedObjects = new Dictionary<Type, List<KeyValuePair<Thread, ObjectBuilder>>>();
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
