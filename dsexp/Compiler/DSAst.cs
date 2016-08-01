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

        public Dictionary<string, DSVariable> Variables
        {
            get
            {
                return variables;
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

        protected abstract DSVariable BindVariableReference(DSNameBinder binder, DSVariableReference reference);
    }

    public class DSAst : ScopeStatement
    {
        private Dictionary<DSVariable, Expression> globals = new Dictionary<DSVariable, Expression>(); 
        private Statement body;
        internal static ParameterExpression functionCode = Expression.Variable(typeof(FunctionCode), "$functionCode");
        internal static ParameterExpression globalContext = Expression.Parameter(typeof(DSCodeContext), "$globalContext");

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

        public ScriptCode ToScriptCode()
        {
            throw new NotImplementedException();
        }

        public void PostBind()
        {
            foreach (var variable in Variables.Values)
            {
                globals[variable] = new LookupVariable(variable.Name, globalContext);
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
            return globals[variable];
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
        private DSExpression[] items;

        public ArrayExpression(DSExpression[] items)
        {
            this.items = items;            
        }

        public override Expression Reduce()
        {
            if (items.Count() == 0)
            {
                var method = typeof(DSOps).GetMethod("CreateEmptyArray");
                var call = Expression.Call(method, new Expression[] { });
                return call;
            }
            else
            {
                var method = typeof(DSOps).GetMethod("CreateArray");
                var call = Expression.Call(method, Expression.NewArrayInit(typeof(object), ConvertToObjectArray(items)));
                return call;
            }
        }

        public override void Visit(DSAstVisitor visitor)
        {
            if (visitor.Visit(this))
            {
                if (items != null)
                {
                    for (int i = 0; i < items.Count(); i++)
                    {
                        items[i].Visit(visitor);
                    }
                }
            } 
        }
    }
}
