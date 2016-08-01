using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Runtime
{
    class DSOps
    {
        public static object GetGlobal(DSCodeContext context, string name)
        {
            object res;
            if (context.TryGetVariable(name, out res))
            {
                return res;
            }

            return null;
        }

        public static void SetGlobal(DSCodeContext context, string name, object value)
        {
            context.SetVariable(name, value);
        }

        public static DSArray CreateArray(object[] data)
        {
            return new DSArray(data);
        }

        public static DSArray CreateEmtpyArray()
        {
            return new DSArray(new object[] { });
        }
    }

    public class StringOps
    {
        public static string Add(string lhs, string rhs)
        {
            return lhs + rhs;
        }
    }
}
