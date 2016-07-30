using Microsoft.Scripting.Ast;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Interpreter;
using dsexp.Runtime;

namespace dsexp.Compiler
{
    public class DSDynamicExpression2 : LightDynamicExpression2
    {
        public DSDynamicExpression2(CallSiteBinder binder, Expression arg0, Expression arg1)
            : base(binder, arg0, arg1)
        {
        }

        public override Expression Reduce()
        {
            return DynamicExpression.Dynamic((DynamicMetaObjectBinder)Binder, Type, Argument0, Argument1);
        }

        public override void AddInstructions(LightCompiler compiler)
        {
            if (Argument0.Type == typeof(DSCodeContext))
            {
                compiler.Compile(Argument0);
                compiler.Compile(Argument1);
                compiler.Instructions.EmitDynamic<DSCodeContext, object, object>(Binder);
            }
            else
            {
                base.AddInstructions(compiler);
            }
        }
    }
}
