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
        public string rpnString { get; private set; }
        private static readonly string EXPR_SEP = new string('\u00B6',1);
        private static readonly string NODE_SEP = new string('\u00A7', 1);

        public PropTester()
        {
            thetype = typeof(T);

            //the name is "p", only for debugging purposes
            tparam = Expression.Parameter(thetype, "p");

            //predicate = PredicateBuilder.True<T>();

            estack = new Stack<Expression<Func<T, bool>>>();
            rpnString = "";
            
        }

        public PropTester<T> Push(string name, Object val, comparison comp)
        {
            //TODO check if type of val matches property type.  
            //if it doesn't, but it's a string, try to parse it to the actual type. (separate method)

            var test_prop = Expression.Property(tparam, thetype.GetProperty(name));
            var test_val = Expression.Constant(val);
            var expr = eFactory(comp, test_prop, test_val);

            //Console.WriteLine(expr);
            write_expr(name, val.ToString(), Enum.GetName(typeof(comparison), comp));

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

            //Console.WriteLine(method_expr);
            write_expr(name, val, meth_name);

            estack.Push(expr);

            return this;
        }

        private void Push(string encoded)
        {
            var elements = encoded.Split(EXPR_SEP[0]);
            if (elements.Count() == 3)
            {
                comparison comp;
                if(Enum.TryParse(elements[2], out comp))
                    Push(elements[0], elements[1], comp);
                else
                {
                    str_comparison scomp;
                    if (Enum.TryParse(elements[2], out scomp))
                        Push(elements[0], elements[1], comp);
                }
            }
        }

        private void decode_rpn(string rpn)
        {
            foreach (var expr_string in rpn.Split(NODE_SEP[0]))
            {
                if(! string.IsNullOrEmpty(expr_string))
                {
                    switch (expr_string)
                    {
                        case "&":
                            And();
                            break;
                        case "|":
                            Or();
                            break;
                        default:
                            Push(expr_string);
                            break;
                    }
                }
            }
        }

        private void write_expr(string name, string val, string comp)
        {
            var strings = new[] { name, val, comp };
            var joined = string.Join(EXPR_SEP, strings) + NODE_SEP;
            rpnString += joined;
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

            rpnString += "&" + NODE_SEP;
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

            rpnString += "|" + NODE_SEP;
        }

        //TODO add negate operator

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
