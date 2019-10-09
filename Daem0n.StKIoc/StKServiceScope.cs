using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc
{
    public class StKServiceScope : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; }
        public string ID { get; private set; }
        private bool disposed = false;
        public StKServiceScope(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            this.ID = Guid.NewGuid().ToString("N");
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null && this != null)
            {
                return false;
            }
            if (obj is StKServiceScope scope)
            {
                return scope.ID == this.ID;
            }
            else
            {
                return false;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed == false)
            {
                if (disposing)
                {
                    this.ID = null;
                }
                disposed = true;
            }
        }
        ~StKServiceScope()
        {
            this.Dispose(false);
        }
    }
}
