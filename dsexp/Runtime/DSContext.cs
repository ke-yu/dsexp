using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Runtime
{
    public class DSContext
    {
        private Dictionary<object, object> globalDict = new Dictionary<object, object>();
        private Dictionary<object, object> dict = new Dictionary<object, object>();

        public DSContext()
        {

        }

        public void SetVariable(string name, object value)
        {
            dict[name] = value;
        }

        public bool TryGetVariable(string name, out object value)
        {
            return dict.TryGetValue(name, out value);
        } 
    }
}
