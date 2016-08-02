using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Runtime
{
    public class DSType
    {
        public string Name
        {
            get; private set;
        }

        public DSType(string name)
        {
            Name = name;
        }
    }

    public class DSInteger : DSType
    {
        public Int64 Value
        {
            get; set;
        }

        public DSInteger(Int64 value): base("integer")
        {
            Value = value;
        }
    }

    public class DSFloat: DSType
    {
        public Double Value
        {
            get; set;
        }

        public DSFloat(Double value): base("float")
        {
            Value = value;
        }
    }

    public class DSString : DSType
    {
        public String Value
        {
            get; set;
        }

        public DSString(string value): base("string")
        {
            Value = value;
        }
    }

    public class DSArray : DSType 
    {
        private object[] data;

        public DSArray(object[] data): base("_array")
        {
            this.data = data;            
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("array: ");
            builder.Append("{");
            if (data.Any())
            {
                builder.Append(data[0].ToString());
                foreach (var item in data.Skip(1))
                {
                    builder.Append(",");
                    builder.Append(item.ToString());
                }
            }
            builder.Append("}");
            return builder.ToString();
        }
    }
}
