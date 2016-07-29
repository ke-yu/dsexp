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
    }
}
