using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daem0n.Test.Web.Models
{
    public class Model : IModel
    {
        static int num = 0;
        static object obj = new object();
        public Model()
        {
            lock (obj)
            {
                this._num = num;
                num++;
            }
        }
        int _num;
        public int Num => _num;
    }
}
