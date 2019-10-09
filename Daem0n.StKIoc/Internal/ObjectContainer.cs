using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc.Internal
{
    class ObjectContainer
    {
        private object instance;
        private Func<object> factory;
        public ObjectContainer(object instance)
        {
            this.instance = instance;
        }
        public ObjectContainer(Func<object> func)
        {
            this.factory = func;
        }
        public ObjectContainer() { }
        public object GetObject()
        {
            if (this.instance != null)
            {
                return this.instance;
            }
            if (this.factory != null)
            {
                return factory.Invoke();
            }
            return null;
        }
        public object GetInstacne()
        {
            if (this.instance == null)
            {
                if (this.factory != null)
                {
                    this.instance = this.factory.Invoke();
                }
                else
                {
                    return null;
                }
            }
            return this.instance;
        }
        public object CallFactory()
        {
            return this.factory?.Invoke();
        }
    }
}
