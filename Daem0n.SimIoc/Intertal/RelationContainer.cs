using Daem0n.SimIoc.Abstractions;
using Daem0n.SimIoc.TypeRelataion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.SimIoc.Intertal
{
    class RelationContainer : IRelationContainer
    {
        private ImplementationRelation scoped;
        private ImplementationRelation transient;
        private ImplementationRelation singleton;

        public RelationContainer(ImplementationRelation scoped, ImplementationRelation singleton, ImplementationRelation transient)
        {
            this.scoped = scoped;
            this.transient = transient;
            this.singleton = singleton;
        }
        public ImplementationRelation GetScoped() => this.scoped;

        public ImplementationRelation GetSingleton() => this.singleton;

        public ImplementationRelation GetTransient() => this.transient;
    }
}
