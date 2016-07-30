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
            // (new DSCommandLine()).Run(args);

            DSCodeContext context = new DSCodeContext();

            /*
            NameExpression name = new NameExpression("x");
            Ast.ConstantExpression con = new Ast.ConstantExpression("foo");
            AssignmentStatement assign = new AssignmentStatement(name, con);
            var r1 = Run(assign, context);

            NameExpression name2 = new NameExpression("y");
            NameExpression name3 = new NameExpression("x");
            AssignmentStatement assign2 = new AssignmentStatement(name2, name3);
            var r2 = Run(assign2, context);
            */

            Ast.ConstantExpression lhs = new Ast.ConstantExpression("foo");
            Ast.ConstantExpression rhs = new Ast.ConstantExpression("bar");
            Ast.BinaryExpression add = new Ast.BinaryExpression(lhs, rhs, Operator.Add);

            NameExpression name = new NameExpression("x");
            AssignmentStatement assign = new AssignmentStatement(name, add);
            var r1 = Run(assign, context);
        }
    }
}
