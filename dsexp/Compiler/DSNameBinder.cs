using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Ast
{
    public enum VariableKind
    {
        Global,
        Local,
        Parameter
    }

    public class DSDefineBinder : DSAstVisitor
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

    public class DSParameterBinder : DSAstVisitor
    {
        private DSNameBinder nameBinder;

        public DSParameterBinder(DSNameBinder nameBinder)
        {
            this.nameBinder = nameBinder;
        }

        public override bool Visit(Parameter node)
        {
            node.Parent = nameBinder.CurrentScope;
            node.ParameterVariable = nameBinder.DefineParameter(node.Name);
            return false;
        }
    }

    public class DSNameBinder : DSAstVisitor
    {
        private DSDefineBinder defineBinder;
        private DSParameterBinder parameterBinder;
        private List<ScopeStatement> scopes = new List<ScopeStatement>();

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
            var variable = CurrentScope.CreateVariable(name, VariableKind.Local);
            return variable;
        }

        public DSVariable DefineParameter(string name)
        {
            var variable = CurrentScope.CreateVariable(name, VariableKind.Parameter);
            return variable;
        }

        private void Bind(DSAst ast)
        {
            CurrentScope = ast;
            ast.Visit(this);
            ast.Bind(this);

            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                scopes[i].PostBind(this);
            }

            ast.PostBind(this);
        }

        public override bool Visit(DSAst ast)
        {
            return true;
        }

        public override void PostVisit(DSAst ast)
        {
            CurrentScope = CurrentScope.Parent;
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

            foreach (var parameter in node.Parameters)
            {
                parameter.Visit(parameterBinder);
            }
            return true;
        }

        public override void PostVisit(FunctionDefinition node)
        {
            scopes.Add(CurrentScope);
            CurrentScope = CurrentScope.Parent;
        }

        public override bool Visit(Parameter node)
        {
            node.Parent = CurrentScope;
            return true;
        }
    }
}
