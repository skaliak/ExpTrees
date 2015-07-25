using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

namespace Treeees
{
    public class PropTester<T>
    {
        private Type thetype;
        private ParameterExpression tparam;
        private Expression<Func<T, bool>> predicate;
        private Stack<Expression<Func<T, bool>>> estack;

        public PropTester()
        {
            thetype = typeof(T);

            //the name is "p", only for debugging purposes
            tparam = Expression.Parameter(thetype, "p");

            //predicate = PredicateBuilder.True<T>();

            estack = new Stack<Expression<Func<T, bool>>>();
            
        }

        public PropTester<T> Push(string name, Object val, comparison comp)
        {
            var test_prop = Expression.Property(tparam, thetype.GetProperty(name));
            var test_val = Expression.Constant(val);
            var expr = eFactory(comp, test_prop, test_val);
            //predicate = predicate.And<T>(expr);
            estack.Push(expr);

            return this;
        }

        public PropTester<T> Push(string name, string val, str_comparison comp)
        {
            var test_prop = Expression.Property(tparam, thetype.GetProperty(name));
            var test_val = Expression.Constant(val);
            string meth_name = Enum.GetName(typeof(str_comparison), comp);
            MethodInfo method = typeof(string).GetMethod(meth_name, new[] { typeof(string) });
            var method_expr = Expression.Call(test_prop, method, test_val);
            var expr = Expression.Lambda<Func<T, bool>>(method_expr, tparam);

            estack.Push(expr);

            return this;
        }

        public void Pop()
        {
            //'And' should pop
        }

        public void And()
        {
            var pred = PredicateBuilder.True<T>();
            while(estack.Count > 0)
            {
                var expr = estack.Pop();
                pred = pred.And<T>(expr);
            }
            estack.Push(pred);
        }

        public void Or()
        {
            var pred = PredicateBuilder.False<T>();
            while (estack.Count > 0)
            {
                var expr = estack.Pop();
                pred = pred.Or<T>(expr);
            }
            estack.Push(pred);
        }

        private Expression<Func<T, bool>> eFactory(comparison comp, Expression left, Expression right)
        {
            Expression expr;

            switch (comp)
            {
                case comparison.Equals:
                    expr = Expression.Equal(left, right);
                    break;
                case comparison.NotEquals:
                    expr = Expression.NotEqual(left, right);
                    break;
                case comparison.Lt:
                    expr = Expression.LessThan(left, right);
                    break;
                case comparison.Gt:
                    expr = Expression.GreaterThan(left, right);
                    break;
                default:
                    return null;
            }
            return Expression.Lambda<Func<T, bool>>(expr, tparam) as Expression<Func<T, bool>>;
        }

        public Func<T, bool> Build()
        {
            if (estack.Count != 1)
                throw new InvalidOperationException(string.Format("not ready to build, {0} expressions on stack", estack.Count));

            return estack.Pop().Compile();
        }
    }

    public enum comparison
    {
        Equals,
        NotEquals,
        Lt,
        Gt
    }

    public enum str_comparison
    {
        Contains,
        StartsWith,
        EndsWith,
    }
}
