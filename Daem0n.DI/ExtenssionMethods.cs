using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daem0n.DI
{
    public static class ExtenssionMethods
    {
        public static Hashtable CreateSyncHashtable(this Hashtable hashtable)
        {
            return Hashtable.Synchronized(hashtable);
        }

        public static void Output(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
        }
    }
}
