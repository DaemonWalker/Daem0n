using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.DI
{
    public static class ExtenssionMethods
    {
        public static Hashtable CreateSyncHashtable(this Hashtable hashtable)
        {
            return Hashtable.Synchronized(hashtable);
        }
    }
}
