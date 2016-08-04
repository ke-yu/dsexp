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

    class DSParameterBinder : DSAstVisitor
    {
        private DSNameBinder nameBinder;

        public DSParameterBinder(DSNameBinder nameBinder)
        {
            this.nameBinder = nameBinder;
        }

        public override bool Visit(Parameter node)
        {
            node.Parent = nameBinder.CurrentScope;
            node.ParameterVariable = nameBinder.DefineName(node.Name);
            return false;
        }
    }

    public class DSNameBinder : DSAstVisitor
    {
        private DSDefineBinder defineBinder;
        private DSParameterBinder parameterBinder;

        public ScopeStatement CurrentScope
        {
            get; set;
        }

        internal static void BindAst(DSAst ast)
        {
            DSNameBinder binder = new DSNameBinder();
            binder.Bind(ast);
        }

        public DSNameBinder()
        {
            defineBinder = new DSDefineBinder(this);
            parameterBinder = new DSParameterBinder(this);
        }

        public DSVariable DefineName(string name)
        {
            var variable = CurrentScope.CreateVariable(name);
            return variable;
        }

        public DSVariable DefineParameter(string name)
        {
            var variable = CurrentScope.CreateVariable(name);
            return variable;
        }

        private void Bind(DSAst ast)
        {
            CurrentScope = ast;
            ast.Visit(this);
            ast.Bind(this);
            ast.PostBind(this);
        }

        public override bool Visit(DSAst ast)
        {
            return true;
        }

        public override bool Visit(NameExpression node)
        {
            node.Parent = CurrentScope;
            node.Reference = CurrentScope.Reference(node.Name);
            return true;
        }

        public override bool Visit(ConstantExpression node)
        {
            node.Parent = CurrentScope;
            return true;
        }

        public override bool Visit(AssignmentStatement node)
        {
            node.Parent = CurrentScope;
            node.Left.Visit(defineBinder);
            return true;
        }

        public override bool Visit(BinaryExpression node)
        {
            node.Parent = CurrentScope;
            return true;
        }

        public override bool Visit(ArrayExpression node)
        {
            node.Parent = CurrentScope;
            return true;
        }

        public override bool Visit(ExpressionStatement node)
        {
            node.Parent = CurrentScope;
            return true;
        }

        public override bool Visit(RangeExpression node)
        {
            node.Parent = CurrentScope;
            return true;
        }

        public override bool Visit(BlockStatement node)
        {
            node.Parent = CurrentScope;
            return true;
        }

        public override bool Visit(FunctionDefinition node)
        {
            node.Parent = CurrentScope;
            node.FunctionVariable = DefineName(node.Name);
            CurrentScope = node;
            return true;
        }

        public override bool Visit(Parameter node)
        {
            node.Parent = CurrentScope;
            return true;
        }
    }
}
