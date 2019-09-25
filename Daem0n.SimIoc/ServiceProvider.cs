﻿using Daem0n.SimIoc.Abstractions;
using Daem0n.SimIoc.TypeRelataion;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Daem0n.SimIoc
{
    public class ServiceProvider : IServiceProvider, ISupportRequiredService
    {
        private ImplementationRelation singleton;
        private ImplementationRelation transient;
        private ImplementationRelation scoped;
        private ConcurrentDictionary<Type, object> singletonObjects = new ConcurrentDictionary<Type, object>();
        private ConcurrentDictionary<Tuple<Thread, Type>, object> scopedObjects = new ConcurrentDictionary<Tuple<Thread, Type>, object>();
        private ConcurrentDictionary<Type, Func<object>> transientObjects = new ConcurrentDictionary<Type, Func<object>>();
        internal ServiceProvider(IRelationContainer relationContainer)
        {
            this.singleton = relationContainer.GetSingleton();
            this.transient = relationContainer.GetTransient(); ;
            this.scoped = relationContainer.GetScoped();
            this.singleton.Add(typeof(IServiceProvider), this.GetType(), null, this);
        }
        public object GetRequiredService(Type serviceType)
        {
            return Get(serviceType);
        }

        public object GetService(Type serviceType)
        {
            return Get(serviceType);
        }

        public object Get(Type tSource)
        {
            if (singleton.Contains(tSource))
            {
                return GetSingleton(tSource);
            }
            else if (scoped.Contains(tSource))
            {
                return GetScoped(tSource);
            }
            else if (transient.Contains(tSource))
            {
                return GetTransient(tSource);
            }
            else if (tSource.IsConstructedGenericType)
            {
                var outType = tSource.GetGenericTypeDefinition();
                Func<IServiceProvider, object> func = null;
                Type checkType = null;
                Type newType = null;
                if (outType == typeof(IEnumerable<>))
                {
                    checkType = tSource.GenericTypeArguments.Single();
                    newType = typeof(List<>).MakeGenericType(checkType);
                    func = _ => CreateListObject(checkType);
                }
                else
                {
                    func = _ => CreateGeneric(tSource);
                    checkType = outType;
                    var obj = CreateGeneric(tSource);
                    newType = obj?.GetType();
                }
                if (singleton.Contains(checkType))
                {
                    singleton.Add(tSource, newType, null, func(this));
                    return GetSingleton(tSource);
                }
                else if (scoped.Contains(checkType))
                {
                    singleton.Add(tSource, newType, func, null);
                    return GetScoped(tSource);
                }
                else if (transient.Contains(checkType))
                {
                    transient.Add(tSource, newType, func, null);
                    return GetTransient(tSource);
                }
                else
                {
                    Console.WriteLine(tSource);
                    return func(this);
                }
            }
            else
            {
                return null;
            }
        }
        private object GetSingleton(Type tSource)
        {
            var tTarget = singleton.GetTarget(tSource);
            if (singletonObjects.TryGetValue(tTarget, out var obj) == false)
            {
                obj = this.CreateObject(tSource, tTarget, singleton.GetBuilder(tTarget));
                if (singletonObjects.TryAdd(tTarget, obj) == false)
                {
                    singletonObjects.TryGetValue(tTarget, out obj);
                }
            }
            return obj;
        }
        private object GetTransient(Type tSource)
        {
            var tTarget = transient.GetTarget(tSource);
            return this.CreateObject(tSource, tTarget, transient.GetBuilder(tTarget));
        }
        private object GetScoped(Type tSource)
        {
            var tTarget = scoped.GetTarget(tSource);
            var key = new Tuple<Thread, Type>(Thread.CurrentThread, tSource);
            if (scopedObjects.TryGetValue(key, out var obj) == false)
            {
                obj = this.CreateObject(tSource, tTarget, scoped.GetBuilder(tTarget));
                if (scopedObjects.TryAdd(key, obj) == false)
                {
                    scopedObjects.TryGetValue(key, out obj);
                }
            }
            return obj;
        }

        private object CreateObject(Type tSource, Type tTarget, BuilderInfo builder)
        {
            if (builder.Instance != null)
            {
                return builder.Instance;
            }
            else if (builder.Builder != null)
            {
                return builder.Builder(this);
            }
            else
            {
                return CreateObject(tTarget);
            }
        }
        private object CreateObject(Type t)
        {
            var constructors = t.GetConstructors().Where(p => p.IsPublic == true).ToArray();
            ConstructorInfo constructor = null;
            ParameterInfo[] parms = null;
            if (constructors.Length == 0)
            {
                throw new Exception("No Constructors");
            }
            if (constructors.Length == 1)
            {
                constructor = constructors.First();
                parms = constructor.GetParameters();
            }
            else
            {
                //Array.Sort(constructors,
                //    (c1, c2) => c2.GetParameters().Length.CompareTo(c1.GetParameters().Length));
                //constructor = constructors.First();
                //var hsParms = new HashSet<Type>(constructor.GetParameters().Select(p => p.ParameterType));
                for (var i = 0; i < constructors.Length; i++)
                {
                    constructor = constructors[i];
                    var check = true;
                    foreach (var parm in constructor.GetParameters())
                    {
                        if (Get(parm.ParameterType) == null)
                        {
                            constructor = null;
                            check = false;
                            break;
                        }
                    }
                    if (check)
                    {
                        break;
                    }
                }
                if (constructor != null)
                {
                    parms = constructor.GetParameters();
                }
                else
                {
                    constructor = constructors[0];
                    parms = constructor.GetParameters();
                }
            }
            var ps = parms.Select(_ => Get(_.ParameterType)).ToArray();
            //var constructorParms =;
            return constructor.Invoke(ps);
        }
        private object CreateGeneric(Type tSource)
        {
            var outType = tSource.GetGenericTypeDefinition();
            if (singleton.Contains(outType))
            {
                var target = singleton.GetTarget(outType);
                target = target.MakeGenericType(tSource.GenericTypeArguments);
                singleton.Add(tSource, target, null, null);
                return GetSingleton(tSource);

            }
            else if (scoped.Contains(outType))
            {
                var target = scoped.GetTarget(outType);
                target = target.MakeGenericType(tSource.GenericTypeArguments);
                scoped.Add(tSource, target, null, null);
                scopedObjects.TryAdd(new Tuple<Thread, Type>(Thread.CurrentThread, target), CreateObject(target));
                return GetScoped(tSource);
            }
            else if (transient.Contains(outType))
            {
                var target = transient.GetTarget(outType);
                target = target.MakeGenericType(tSource.GenericTypeArguments);
                transient.Add(tSource, target, (provider) => CreateObject(target), null);
                return GetTransient(tSource);
            }
            else
            {
                Console.WriteLine(tSource);
                return null;
            }
        }

        private object CreateIEnumerable(Type tSource)
        {
            var inType = tSource.GenericTypeArguments.Single();
            var newType = typeof(List<>).MakeGenericType(inType);
            if (singleton.Contains(inType))
            {
                var target = singleton.GetTarget(inType);
                target = target.MakeGenericType(tSource.GenericTypeArguments);
                singleton.Add(tSource, target, null, null);
                return GetSingleton(tSource);

            }
            else if (scoped.Contains(inType))
            {
                var target = scoped.GetTarget(inType);
                target = target.MakeGenericType(tSource.GenericTypeArguments);
                scoped.Add(tSource, target, null, null);
                scopedObjects.TryAdd(new Tuple<Thread, Type>(Thread.CurrentThread, target), CreateObject(target));
                return GetScoped(tSource);
            }
            else if (transient.Contains(inType))
            {
                var target = transient.GetTarget(inType);
                target = target.MakeGenericType(tSource.GenericTypeArguments);
                transient.Add(tSource, target, (provider) => CreateObject(target), null);
                return GetTransient(tSource);
            }
            else
            {
                Console.WriteLine(tSource);
                return null;
            }
        }

        private object CreateListObject(Type tSource)
        {
            var listType = typeof(List<>).MakeGenericType(tSource);
            var listObj = (IList)Activator.CreateInstance(listType);
            var obj = Get(tSource);
            if (obj != null)
            {
                listObj.Add(obj);
            }
            return listObj;
        }

        private bool CheckType(Type tSource)
        {
            if (singleton.Contains(tSource))
            {
                return true;
            }
            else if (scoped.Contains(tSource))
            {
                return true;
            }
            else if (transient.Contains(tSource))
            {
                return true;
            }
            else if (tSource.IsConstructedGenericType)
            {
                var outType = tSource.GetGenericTypeDefinition();
                if (outType == typeof(IEnumerable<>))
                {
                    return CheckType(tSource.GenericTypeArguments.First());
                }
                else
                {
                    return CheckType(outType);
                }
            }
            else
            {
                return false;
            }
        }
    }
}
