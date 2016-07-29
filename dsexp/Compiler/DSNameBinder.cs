using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Ast
{
    class DSDefineBinder : DSAstVisitor
    {
        private DSNameBinder nameBinder;

        public DSDefineBinder(DSNameBinder nameBinder)
        {
            this.nameBinder = nameBinder;
        }

        public override bool Visit(NameExpression node)
        {
            nameBinder.DefineName(node.Name);
            return true;
        }
    }

    public class DSNameBinder : DSAstVisitor
    {
        private ScopeStatement currentScope;
        private DSDefineBinder defineBinder;

        internal static void BindAst(DSAst ast)
        {
            DSNameBinder binder = new DSNameBinder();
            binder.Bind(ast);
        }

        public DSNameBinder()
        {
            defineBinder = new DSDefineBinder(this);
        }

        public void DefineName(string name)
        {
            currentScope.CreateVariable(name);
        }

        private void Bind(DSAst ast)
        {
            currentScope = ast;
            ast.Visit(this);
            ast.Bind(this);
            ast.PostBind();
        }

        public override bool Visit(DSAst ast)
        {
            return true;
        }

        public override bool Visit(NameExpression node)
        {
            node.Parent = currentScope;
            node.Reference = currentScope.Reference(node.Name);
            return true;
        }

        public override bool Visit(ConstantExpression node)
        {
            node.Parent = currentScope;
            return true;
        }

        public override bool Visit(AssignmentStatement node)
        {
            node.Left.Visit(defineBinder);
            return true;
        }
    }
}
