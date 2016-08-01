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

            Ast.ConstantExpression lhs = new Ast.ConstantExpression("foo");
            Ast.ConstantExpression rhs = new Ast.ConstantExpression("bar");
            Ast.BinaryExpression add = new Ast.BinaryExpression(lhs, rhs, Operator.Add);

            NameExpression name4 = new NameExpression("x");
            AssignmentStatement assign = new AssignmentStatement(name, add);
            var r3 = Run(assign, context);
            */

            /*
            Ast.ConstantExpression v1 = new Ast.ConstantExpression("foo");
            Ast.ConstantExpression v2 = new Ast.ConstantExpression(true);
            Ast.ConstantExpression v3 = new Ast.ConstantExpression(21);
            Ast.ArrayExpression arrExp = new ArrayExpression(new DSExpression[] { v1, v2, v3});
            NameExpression arr = new NameExpression("arr");
            AssignmentStatement assign = new AssignmentStatement(arr, arrExp);
            var r4 = Run(assign, context);
            */

            var st1 = dsexp.Ast.Parser.Parse("x = 21");
            var r = Run(st1, context);

            st1 = dsexp.Ast.Parser.Parse("y = x");
            r = Run(st1, context);
        }
    }
}
