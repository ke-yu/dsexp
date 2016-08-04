using dsexp.Compiler;
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

namespace dsexp.Ast
{
    public class DSVariable
    {
        public string Name
        {
            get; private set;
        }

        public DSVariable(string name)
        {
            Name = name;
        }
    }

    public class DSVariableReference
    {
        public string Name
        {
            get; private set;
        }

        public DSVariable Variable
        {
            get; set;
        }

        public DSVariableReference(string name)
        {
            Name = name;
        }
    }

    public class DSGlobal
    {
        private Expression globalContext;
        public string Name
        {
            get; private set;
        }

        public DSGlobal(Expression globalContext, string name)
        {
            this.globalContext = globalContext;
            Name = name;
        }
    }

    public abstract class DSNode : Expression
    {
        public abstract void Visit(DSAstVisitor visitor);

        public ScopeStatement Parent
        {
            get; set;
        }

        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        public static Expression[] ConvertToObjectArray(IList<Expression> expressions)
        {
            Expression[] objArray = new Expression[expressions.Count()];
            for (int i = 0; i < expressions.Count(); i++)
            {
                objArray[i] = Utils.Convert(expressions[i], typeof(object));
            }
            return objArray;
        }
    }

    public abstract class Statement : DSNode
    {
        public override Type Type
        {
            get
            {
                return typeof(void);
            }
        }
    }

    public abstract class DSExpression : DSNode
    {
        public override Type Type
        {
            get
            {
                return typeof(object);
            }
        }

        public virtual Expression Transform(Expression right)
        {
            throw new NotImplementedException();
        }
    }


    public abstract class ScopeStatement : Statement
    {
        private Dictionary<string, DSVariable> variables = new Dictionary<string, DSVariable>();
        private Dictionary<string, DSVariableReference> references = new Dictionary<string, DSVariableReference>();
        protected Dictionary<DSVariable, Expression> variableMapping = new Dictionary<DSVariable, Expression>(); 
        private static ParameterExpression localCodeContextVariable = Expression.Parameter(typeof(DSCodeContext), "$localContext");

        public Dictionary<string, DSVariable> Variables
        {
            get
            {
                return variables;
            }
        }

        public virtual ParameterExpression LocalContext
        {
            get
            {
                return localCodeContextVariable;
            }
        }

        public DSAst GlobalParent
        {
            get
            {
                DSNode cur = this;
                while (!(cur is DSAst))
                {
                    cur = cur.Parent;
                }
                return (cur as DSAst);
            }
        }

        public DSVariable CreateVariable(string name)
        {
            DSVariable variable;
            if (!variables.TryGetValue(name, out variable))
            {
                variable = new DSVariable(name);
                variables[name] = variable; 
            }

            return variable;
        }

        public DSVariableReference Reference(string name)
        {
            DSVariableReference reference;
            if (!references.TryGetValue(name, out reference))
            {
                reference = new DSVariableReference(name);
                references[name] = reference;
            }
            return reference;
        }

        public Expression GetVariableExpression(DSVariable variable)
        {
            return GlobalParent.LookupExpression(variable);
        }

        public void Bind(DSNameBinder binder)
        {
            foreach (var reference in references.Values)
            {
                reference.Variable = BindVariableReference(binder, reference); 
            }
        }

        public virtual void PostBind(DSNameBinder binder)
        {

        }

        protected abstract DSVariable BindVariableReference(DSNameBinder binder, DSVariableReference reference);
    }

    public class DSAst : ScopeStatement
    {
        private Statement body;

        private static ParameterExpression functionCode = Expression.Variable(typeof(FunctionCode), "$functionCode");
        private static ParameterExpression globalContext = Expression.Parameter(typeof(DSCodeContext), "$globalContext");

        public DSAst(Statement body)
        {
            this.body = body;
        }

        public void Bind()
        {
            DSNameBinder.BindAst(this);
        }

        public ParameterExpression GlobalContext
        {
            get
            {
                return globalContext;
            }
        }

        public override ParameterExpression LocalContext
        {
            get
            {
                return globalContext;
            }
        }

        public ScriptCode ToScriptCode()
        {
            throw new NotImplementedException();
        }

        public override void PostBind(DSNameBinder binder)
        {
            foreach (var variable in Variables.Values)
            {
                variableMapping[variable] = new LookupVariable(variable.Name, globalContext);
            }
        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
                body.Visit(visitor);
            }
        }

        protected override DSVariable BindVariableReference(DSNameBinder binder, DSVariableReference reference)
        {
            return CreateVariable(reference.Name); 
        }

        public Expression LookupExpression(DSVariable variable)
        {
            return variableMapping[variable];
        }

        public Expression ReduceToExpression()
        {
            List<Expression> block = new List<Expression> { body };
            var expr = Expression.Block(block);
            return expr; 
        }

        public override Type Type
        {
            get
            {
                return typeof(Expression<Action>);
            }
        }
    }

    public class AssignmentStatement : Statement
    {
        private DSExpression left, right;

        public DSExpression Left
        {
            get
            {
                return left;
            }
        }

        public DSExpression Right
        {
            get
            {
                return right;
            }
        }

        public AssignmentStatement(DSExpression left, DSExpression right)
        {
            this.left = left;
            this.right = right;
        }

        public override Expression Reduce()
        {
            return left.Transform(right);
        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
                left.Visit(visitor);
                right.Visit(visitor);
            }
        }
    }

    public class NameExpression : DSExpression
    {
        private string name;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public DSVariableReference Reference
        {
            get; set;
        }

        public NameExpression(string name)
        {
            this.name = name;
        }

        public override void Visit(DSAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override Expression Reduce()
        {
            Expression read = Parent.GetVariableExpression(Reference.Variable);
            return read;
        }

        public override Expression Transform(Expression right)
        {
            var variable = Parent.GetVariableExpression(Reference.Variable);

            if (variable is LookupVariable)
            {
                var assignment = (variable as LookupVariable).Assign(right);
                return assignment;
            }
            else
            {
                var assignment = Expression.Assign(variable, right);
                return assignment;
            }
        }
    }

    // uncomment to use interpreter mode
    public class ConstantExpression : DSExpression /*, IInstructionProvider */
    {
        private readonly object value;

        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

        public override Expression Reduce()
        {
            var expr = Utils.Convert(Expression.Constant(value), typeof(object));
            return expr;
        }

        public override void Visit(DSAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public ConstantExpression(object v)
        {
            value = v;
        }

        // uncomment to use interpreter mode
        /*
        public void AddInstructions(LightCompiler compiler)
        {
            compiler.Instructions.EmitLoad(value);
        }
        */
    }

    // uncomment to use interpreter mode
    public class LookupVariable : Expression /*, IInstructionProvider */
    {
        private string name;
        private Expression context;

        public LookupVariable(string variable, Expression context)
        {
            this.name = variable;
            this.context = context;
        }

        public sealed override Type Type
        {
            get
            {
                return typeof(object);
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

        public Expression Assign(Expression value)
        {
            return Expression.Call(typeof(DSOps).GetMethod("SetGlobal"), context, Utils.Constant(name), value); 
        }

        public override Expression Reduce()
        {
            return Expression.Call(typeof(DSOps).GetMethod("GetGlobal"), context, Utils.Constant(name)); 
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        // uncomment to use interpreter mode
        /*
        public void AddInstructions(LightCompiler compiler)
        {
            compiler.Compile(context);
            compiler.Instructions.Emit(new LookUpGlobalInstruction(name));
        }
        */
    }

    public enum Operator
    {
        None,
        Add,
        Substract,
        Multiply,
        Divide
    }

    public class BinaryExpression : DSExpression , IInstructionProvider 
    {
        public DSExpression Left
        {
            get; private set;
        }

        public DSExpression Right
        {
            get; private set;
        }

        public Operator Operator
        {
            get; private set;
        } 

        public BinaryExpression(DSExpression left, DSExpression right, Operator op)
        {
            Left = left;
            Right = right;
            Operator = op;
        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
                Left.Visit(visitor);
                Right.Visit(visitor);
            }
        }

        public void AddInstructions(LightCompiler compiler)
        {
            compiler.Compile(Reduce());
        }

        public override Expression Reduce()
        {
            return MakeBinaryOperation();
        }

        private Expression MakeBinaryOperation()
        {
            return new DSDynamicExpression2(Binders.BinaryOperationBinder(Operator), Left, Right);
        }
    }

    public class ArrayExpression : DSExpression
    {
        public DSExpression[] Items
        {
            get; private set;
        }
        

        public ArrayExpression(DSExpression[] items)
        {
            this.Items = items;            
        }

        public override Expression Reduce()
        {
            if (Items.Count() == 0)
            {
                var method = typeof(DSOps).GetMethod("CreateEmptyArray");
                var call = Expression.Call(method, new Expression[] { });
                return call;
            }
            else
            {
                var method = typeof(DSOps).GetMethod("CreateArray");
                var call = Expression.Call(method, Expression.NewArrayInit(typeof(object), ConvertToObjectArray(Items)));
                return call;
            }
        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
                for (int i = 0; i < Items.Count(); i++)
                {
                    Items[i].Visit(visitor);
                }
            }
        }
    }

    public class RangeExpression : DSExpression
    {
        public DSExpression Start
        {
            get; private set;
        }

        public DSExpression End
        {
            get; private set;
        }

        public RangeExpression(DSExpression start, DSExpression end)
        {
            Start = start;
            End = end;

        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
                Start.Visit(visitor);
                End.Visit(visitor);
            }
        }
    }

    public class ExpressionStatement : Statement
    {
        public DSExpression Expression
        {
            get; private set;
        }

        public ExpressionStatement(DSExpression expression)
        {
            Expression = expression;
        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
                Expression.Visit(visitor);
            }
        }

        public override Expression Reduce()
        {
            var method = typeof(DSOps).GetMethod("Print");
            return Call(method, Parent.LocalContext, Expression);
        }
    }

    public class BlockStatement : Statement
    {
        public Statement[] Statements
        {
            get; private set;
        }

        public BlockStatement(Statement[] statements)
        {
            Statements = statements;
        }

        public override void Visit(DSAstVisitor visitor)
        {
            foreach (var statement in Statements)
            {
                statement.Visit(visitor);
            }
        }
    }

    public class Parameter : DSNode
    {
        public string Name
        {
            get; private set;
        }

        public DSVariable ParameterVariable
        {
            get; set;
        }

        public ParameterExpression ParameterExpression
        {
            get; private set;
        }

        public Parameter(string name)
        {
            Name = name;
        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
            }
        }

        public Expression PostBind()
        {
            ParameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(object), Name);
            return ParameterExpression;
        }
    }

    public class FunctionDefinition : ScopeStatement
    {
        public string Name
        {
            get; private set;
        }

        public IEnumerable<Parameter> Parameters
        {
            get; private set;
        }

        public BlockStatement Body
        {
            get; private set;
        }

        public DSVariable FunctionVariable
        {
            get; set;
        }

        public FunctionDefinition(string functionName, List<string> parameters, BlockStatement blockStatement)
        {
            Name = functionName;
            Parameters = parameters.Select(p => new Parameter(p));
            Body = blockStatement;
        }

        public override Expression Reduce()
        {
            return base.Reduce();
        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
                foreach (var p in Parameters)
                {
                    p.Visit(visitor);
                }

                Body.Visit(visitor);
            }
        }

        public override void PostBind(DSNameBinder binder)
        {
            foreach (var parameter in Parameters)
            {
                variableMapping[parameter.ParameterVariable] = parameter.PostBind();
            }

            base.PostBind(binder);
        }

        protected override DSVariable BindVariableReference(DSNameBinder binder, DSVariableReference reference)
        {
            throw new NotImplementedException();
        }
    }
}
