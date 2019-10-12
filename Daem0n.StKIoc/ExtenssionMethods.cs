using Daem0n.StKIoc.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc
{
    /// <summary>
    /// 扩展方法类
    /// </summary>
    static class ExtenssionMethods
    {
        /// <summary>
        /// 是否是根ServiceProvider
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static bool IsRootServiceProvider(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(typeof(IServiceProvider)) == serviceProvider;
        }

        /// <summary>
        /// 获取当前ServiceProvider的ServiceScope
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IServiceScope GetScope(this IServiceProvider serviceProvider)
        {
            if (serviceProvider is StKServiceProvider provider)
            {
                return provider.ServiceScope;
            }
            else
            {
                throw new Exception("ServiceProvider Type Error");
            }
        }

        /// <summary>
        /// 生成实例
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public static object CreateInstance(this IServiceProvider serviceProvider ,Type implementationType)
        {
            if (serviceProvider is StKServiceProvider provider)
            {
                return provider.CreateInstance(implementationType);
            }
            else
            {
                throw new NotSupportedException($"this method doesn't support {serviceProvider.GetType()}");
            }
        }

        /// <summary>
        /// 获取当前ServiceProvider的ServiceScope的ID
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static int GetScopedID(this IServiceProvider serviceProvider)
        {
            if (serviceProvider is StKServiceProvider provider)
            {
                var scope = provider.ServiceScope;
                if (scope == null)
                {
                    return provider.GetHashCode();
                }
                else
                {
                    return scope.GetHashCode();
                }
            }
            else
            {
                throw new NotSupportedException($"this method doesn't support {serviceProvider.GetType()}");
            }
        }
    }
}
