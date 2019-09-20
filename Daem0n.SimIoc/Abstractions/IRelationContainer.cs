using Daem0n.SimIoc.TypeRelataion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.SimIoc.Abstractions
{
    interface IRelationContainer
    {
        ImplementationRelation GetScoped();
        ImplementationRelation GetSingleton();
        ImplementationRelation GetTransient();
    }
}
