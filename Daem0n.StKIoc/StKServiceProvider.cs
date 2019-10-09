using Daem0n.StKIoc.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc
{
    class StKServiceProvider : IServiceProvider, IServiceScopeFactory, ISupportRequiredService
    {
        private StKServiceCollection serviceCollection;
        #region 继承接口
        public IServiceScope CreateScope()
        {
            return new StKServiceScope(this);
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object GetRequiredService(Type serviceType)
        {
            return this.GetService(serviceType);
        }
        #endregion
        #region 构造函数
        public StKServiceProvider(StKServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }
        #endregion
        #region 处理单个对象
        private object GetSingleton(TypeRecord record)
        {
            var oc = this.serviceCollection.GetObjectContainer(record.ID);
            return oc.GetInstacne();
        }
        private object GetScoped(Type serviceType)
        {

        }
        private object GetTransient(Type serviceType)
        {

        }
        #endregion
        #region 处理IEnumberable对象
        private object CreateIEnumerable(Type serviceType)
        {

        }
        #endregion
    }
}
