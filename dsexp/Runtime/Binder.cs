using dsexp.Ast;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Runtime
{
    public class Binders
    {
        private static Dictionary<ExpressionType, DSBinaryOperationBinder> binaryBinders = new Dictionary<ExpressionType, DSBinaryOperationBinder>();

        public static DynamicMetaObjectBinder BinaryOperationBinder(Operator op)
        {
            ExpressionType? type = GetExpressionTypeFromOperator(op);
            if (!type.HasValue)
            {
                return null;
            }

            DSBinaryOperationBinder binder;
            if (!binaryBinders.TryGetValue(type.Value, out binder))
            {
                binder = new DSBinaryOperationBinder(type.Value);
                binaryBinders[type.Value] = binder;
            }

            return binder;
        }

        private static ExpressionType? GetExpressionTypeFromOperator(Operator op)
        {
            switch (op)
            {
                case Operator.Add:
                    return ExpressionType.Add;
                case Operator.Substract:
                    return ExpressionType.Subtract;
                case Operator.Multiply:
                    return ExpressionType.Multiply;
                case Operator.Divide:
                    return ExpressionType.Divide; 
            }
            return null;
        }
    }
    public class DSBinaryOperationBinder : BinaryOperationBinder
    {
        public DSBinaryOperationBinder(ExpressionType operation):
            base(operation)
        {

        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args)
        {
            if (args[0] != null)
            {
                switch (Operation)
                {
                    case ExpressionType.Add:
                        return BindAdd<T>(site, args);
                }
            }

            return base.BindDelegate<T>(site, args);
        }

        private T BindAdd<T>(CallSite<T> site, object[] args) where T : class
        {
            Type t1 = args[0].GetType();
            Type t2 = args[1].GetType();

            if (t1 == typeof(string) || t2 == typeof(string))
            {
                return (T)(object)new Func<CallSite, object, object, object>(StringAdd);
            }

            return base.BindDelegate(site, args);
        }

        private object StringAdd(CallSite site, object lhs, object rhs)
        {
            if (lhs != null && lhs.GetType() == typeof(string) &&
                rhs != null && rhs.GetType() == typeof(string))
            {
                return StringOps.Add((string)lhs, (string)rhs);
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, lhs, rhs);
        }
    }
}
