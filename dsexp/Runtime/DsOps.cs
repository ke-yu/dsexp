using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Runtime
{
    class DSOps
    {
        public static object GetGlobal(DSContext context, string name)
        {
            object res;
            if (context.TryGetVariable(name, out res))
            {
                return res;
            }

            return null;
        }

        public static void SetGlobal(DSContext context, string name, object value)
        {
            context.SetVariable(name, value);
        }
    }
}
