using dsexp.Ast;
using dsexp.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace dsexp
{
    class Program
    {
        public delegate void LookupDelegate(DSContext context);

        static object Run(Statement node, DSContext context)
        {
            DSAst ast = new DSAst(node);
            ast.Bind();

            var expr = ast.ReduceToExpression();
            // uncomment to use interpreter mode
            // var func = Utils.LightLambda<LookupDelegate>(typeof(object), Utils.Convert(expr, typeof(object)), "<uname>", new List<ParameterExpression> { ast.GlobalContext });
            var func = Expression.Lambda<LookupDelegate>(expr, new[] { ast.GlobalContext });
            var funcode = func.Compile();
            funcode(context);
            return null;
        }

        static void Main(string[] args)
        {
            DSContext context = new DSContext();

            NameExpression name = new NameExpression("x");
            Ast.ConstantExpression con = new Ast.ConstantExpression("foo");
            AssignmentStatement assign = new AssignmentStatement(name, con);
            var r1 = Run(assign, context);

            NameExpression name2 = new NameExpression("y");
            NameExpression name3 = new NameExpression("x");
            AssignmentStatement assign2 = new AssignmentStatement(name2, name3);
            var r2 = Run(assign2, context);
        }
    }
}
