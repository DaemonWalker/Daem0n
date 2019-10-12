using Daem0n.StKIoc.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Daem0n.StKIoc
{
    /// <summary>
    /// IOC对象容器
    /// </summary>
    class StKObjectCollection : IDisposable
    {
        #region 私有变量
        /// <summary>
        /// 用于Dispose方法
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// 单例对象Key集合
        /// </summary>
        private HashSet<string> singleton = new HashSet<string>();

        /// <summary>
        /// 域单例对象Key集合
        /// </summary>
        private HashSet<string> scopedKeys = new HashSet<string>();

        /// <summary>
        /// 域单例对象字典
        /// </summary>
        private ConcurrentDictionary<string, object> scoped = new ConcurrentDictionary<string, object>();
        
        /// <summary>
        /// IOC所有对象集合
        /// </summary>
        private List<ObjectContainer> objects = new List<ObjectContainer>();
        #endregion
        #region Dispose
        /// <summary>
        /// IDisposable接口实现
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 消除函数
        /// </summary>
        /// <param name="dispoing"></param>
        private void Dispose(bool dispoing)
        {
            if (disposed == false)
            {
                if (dispoing)
                {
                    for (var i = this.objects.Count - 1; i >= 0; i--)
                    {
                        var oc = this.objects[i];
                        if (oc.Instance is IServiceProvider == false)
                        {
                            (oc.Instance as IDisposable)?.Dispose();
                        }
                    }
                }

                this.disposed = true;
            }
        }
        #endregion
        /// <summary>
        /// 获取单例对象
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public object GetSingleton(IServiceProvider serviceProvider, TypeRecord record)
        {
            if (record.BuildFlag)
            {
                var obj = record.GetInstacne(serviceProvider);
                if (singleton.Contains(record.ID) == false)
                {
                    objects.Add(new ObjectContainer(obj, ServiceLifetime.Singleton));
                }
                return obj;
            }
            else
            {
                var key = GenerateTempKey(serviceProvider, record, record.ImplementationType);
                if (singleton.Contains(key))
                {
                    return objects.First(p => p.ID == key).Instance;
                }
                else
                {
                    var obj = record.GetInstacne(serviceProvider);
                    this.singleton.Add(key);
                    this.objects.Add(new ObjectContainer(obj, ServiceLifetime.Singleton, key));
                    return obj;
                }
            }
        }

        /// <summary>
        /// 获取域单例对象
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public object GetScoped(IServiceProvider serviceProvider, TypeRecord record)
        {
            var key = string.Empty;
            if (record.BuildFlag)
            {
                key = serviceProvider.GetScopedID() + record.ID;
            }
            else
            {
                key = GenerateTempKey(serviceProvider, record, record.ImplementationType);
            }
            if (scopedKeys.Contains(key) == false)
            {
                var obj = record.GetObject(serviceProvider);
                scopedKeys.Add(key);
                this.objects.Add(new ObjectContainer(obj, record.Lifetime, serviceProvider.GetScope()));
                if (scoped.TryAdd(key, obj))
                {
                    return obj;
                }
                else
                {
                    scoped.TryGetValue(key, out obj);
                    return obj;
                }
            }
            else
            {
                if (scoped.TryGetValue(key, out var obj))
                {
                    return obj;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取多例对象
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public object GetTransient(IServiceProvider serviceProvider, TypeRecord record)
        {
            var obj = record.CallFactory(serviceProvider);
            this.objects.Add(new ObjectContainer(obj, ServiceLifetime.Transient, serviceProvider.GetScope()));
            return obj;
        }

        /// <summary>
        /// 生成泛型对象
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="outRecord"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public object GetMakedGeneric(IServiceProvider serviceProvider, TypeRecord outRecord, Type newType)
        {
            var key = GenerateTempKey(serviceProvider, outRecord, newType);
            if (ContainsTempKey(outRecord.Lifetime, key))
            {
                return this.SaveTempObject(serviceProvider, outRecord, newType: newType);
            }
            else
            {
                var obj = serviceProvider.CreateInstance(newType);
                return this.SaveTempObject(serviceProvider, outRecord, obj: obj);
            }
        }


        /// <summary>
        /// 清除域对象
        /// </summary>
        /// <param name="serviceScope"></param>
        internal void ClearScoped(IServiceScope serviceScope)
        {
            if (serviceScope == null)
            {
                return;
            }
            for (var i = this.objects.Count - 1; i >= 0; i--)
            {
                var oc = this.objects[i];
                if (oc.ServiceScope == serviceScope && oc.Lifetime != ServiceLifetime.Singleton)
                {
                    (oc.Instance as IDisposable)?.Dispose();
                    this.objects.RemoveAt(i);
                }
            }
        }

       

        /// <summary>
        /// 存储临时对象
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="record"></param>
        /// <param name="obj"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        private object SaveTempObject(IServiceProvider serviceProvider, TypeRecord record, object obj = null, Type newType = null)
        {
            if (record.Lifetime == ServiceLifetime.Singleton)
            {
                var key = GenerateTempKey(serviceProvider, record, obj?.GetType() ?? newType);
                if (this.singleton.Contains(key))
                {
                    return this.objects.First(_ => _.ID == key).Instance;
                }
                else
                {
                    this.singleton.Add(key);
                    this.objects.Add(new ObjectContainer(obj, ServiceLifetime.Singleton, key));
                    return obj;
                }
            }
            else if (record.Lifetime == ServiceLifetime.Scoped)
            {
                var key = GenerateTempKey(serviceProvider, record, newType);
                if (this.scopedKeys.Contains(key) == false)
                {
                    scopedKeys.Add(key);
                    if (scoped.TryAdd(key, obj))
                    {
                        return obj;
                    }
                    else
                    {
                        scoped.TryGetValue(key, out obj);
                        return obj;
                    }
                }
                else
                {
                    if (scoped.TryGetValue(key, out obj))
                    {
                        return obj;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else if (record.Lifetime == ServiceLifetime.Transient)
            {
                this.objects.Add(new ObjectContainer(obj, ServiceLifetime.Transient, serviceProvider.GetScope()));
                return obj;
            }
            else
            {
                throw new NotSupportedException("Doesn't Support ServiceLifetime");
            }
        }

        /// <summary>
        /// 生成临时对象Key
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="record"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        private string GenerateTempKey(IServiceProvider serviceProvider, TypeRecord record, Type newType)
        {
            if (record.Lifetime == ServiceLifetime.Singleton)
            {
                return record.ID + newType.GetHashCode();
            }
            else if (record.Lifetime == ServiceLifetime.Scoped)
            {
                return record.ID + serviceProvider.GetScopedID() + newType.GetHashCode();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 判断临时对象是否存在
        /// </summary>
        /// <param name="lifetime"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool ContainsTempKey(ServiceLifetime lifetime, string key)
        {
            if (lifetime == ServiceLifetime.Singleton)
            {
                return singleton.Contains(key);
            }
            else if (lifetime == ServiceLifetime.Scoped)
            {
                return scopedKeys.Contains(key);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~StKObjectCollection()
        {
            this.Dispose(false);
        }
    }
}