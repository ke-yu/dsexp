using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Ast
{
    public class DSAstVisitor
    {
        public virtual bool Visit(AssignmentStatement node)
        {
            return true;
        }

        public virtual bool Visit(ConstantExpression node)
        {
            return true;
        }

        public virtual bool Visit(NameExpression node)
        {
            return true;
        }

        public virtual bool Visit(BinaryExpression node)
        {
            return true;
        } 

        public virtual bool Visit(DSAst ast)
        {
            return true;
        } 

        public virtual bool Visit(ArrayExpression node)
        {
            return true;
        }

        public virtual bool Visit(RangeExpression node)
        {
            return true;
        } 

        public virtual bool Visit(ExpressionStatement node)
        {
            return true;
        }

        public virtual bool Visit(BlockStatement node)
        {
            return true;
        }

        public virtual bool Visit(FunctionDefinition node)
        {
            return true;
        }

        public virtual bool Visit(Parameter node)
        {
            return true;
        }
    }
}
