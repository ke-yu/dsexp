using Microsoft.Scripting.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace dsexp.Runtime
{
    public class FunctionCode : IExpressionSerializable
    {
        public Expression CreateExpression()
        {
            throw new NotImplementedException();
        }
    }
}
