using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc
{
    class ToolUtils
    {
        public static string GenerateID()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
