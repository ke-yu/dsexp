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
        public delegate void LookupDelegate(DSCodeContext context);

        static object Run(Statement node, DSCodeContext context)
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
            // REPL 
            (new DSConsoleHost()).Run(args);

            DSCodeContext context = new DSCodeContext();

            var st = dsexp.Ast.Parser.Parse("x = 21;");
            var r = Run(st, context);

            st = dsexp.Ast.Parser.Parse("y = 33;");
            r = Run(st, context);

            st = dsexp.Ast.Parser.Parse("z = x + y;");
            r = Run(st, context);
            ;
        }
    }
}
