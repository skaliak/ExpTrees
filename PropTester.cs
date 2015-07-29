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

        List<Node> nodes;
        List<SubTree> subtrees;
        SubTree tree;

        public PropTester()
        {
            thetype = typeof(T);

            //the name is "p", only for debugging purposes
            tparam = Expression.Parameter(thetype, "p");

            //predicate = PredicateBuilder.True<T>();

            estack = new Stack<Expression<Func<T, bool>>>();
            //rpnString = "";

            nodes = new List<Node>();
            subtrees = new List<SubTree>();
            tree = new SubTree();
        }

        public PropTester<T> Push(string name, Object val, comparison comp)
        {
            var prop = thetype.GetProperty(name);
            var converted_val = Convert_val(prop, val);

            var test_prop = Expression.Property(tparam, prop);
            var test_val = Expression.Constant(converted_val);
            var expr = eFactory(comp, test_prop, test_val);

            //Console.WriteLine(expr);
            //write_expr(name, val.ToString(), Enum.GetName(typeof(comparison), comp));

            Build_tree(name, val, comp);

            estack.Push(expr);

            return this;
        }

        private void Build_tree(string name, object val, comparison comp)
        {
            var node = new Node(name, comp, val);
            nodes.Add(node);
        }

        private object Convert_val(PropertyInfo prop, object val)
        {           
            //check if type of val matches property type.  
            //if it doesn't, but it's a string, try to parse it to the actual type.

            var proptype = prop.PropertyType;
            var valtype = val.GetType();
            if (proptype != valtype && valtype == typeof(string))
            {
                val = Util.ParseString(val.ToString());
            }

            return val;
        }

        #region oldstuff

        [Obsolete]
        public PropTester<T> Push(string name, string val, str_comparison comp)
        {
            var test_prop = Expression.Property(tparam, thetype.GetProperty(name));
            var test_val = Expression.Constant(val);

            string meth_name = Enum.GetName(typeof(str_comparison), comp);
            MethodInfo method = typeof(string).GetMethod(meth_name, new[] { typeof(string) });
            var method_expr = Expression.Call(test_prop, method, test_val);
            var expr = Expression.Lambda<Func<T, bool>>(method_expr, tparam);

            //Console.WriteLine(method_expr);
            //write_expr(name, val, meth_name);

            estack.Push(expr);

            return this;
        }

        [Obsolete]
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

        [Obsolete]
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

        //TODO replace this with something that makes tuples, rather than writes strings
        [Obsolete]
        private void write_expr(string name, string val, string comp)
        {
            var strings = new[] { name, val, comp };
            var joined = string.Join(EXPR_SEP, strings) + NODE_SEP;
            rpnString += joined;
        }

        #endregion

        public void And()
        {
            var pred = PredicateBuilder.True<T>();
            while(estack.Count > 0)
            {
                var expr = estack.Pop();
                pred = pred.And<T>(expr);
            }
            estack.Push(pred);

            //rpnString += "&" + NODE_SEP;
            Build_tree(Operator.And);
        }

        private void Build_tree(Operator p)
        {
            if(nodes.Count > 0)
            {
                var subtree = new SubTree(p, nodes.ToArray());
                subtrees.Add(subtree);
                nodes.Clear();
            }
            else if (subtrees.Count > 0)
            {
                var subtree = new SubTree(p, subtrees.ToArray());
                subtrees.Clear();
                subtrees.Add(subtree);
            }
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

            //rpnString += "|" + NODE_SEP;
            Build_tree(Operator.Or);
        }

        //TODO add negate operator
        public void Not()
        {
            throw new NotImplementedException();
        }

        private Expression<Func<T, bool>> eFactory(comparison comp, Expression left, Expression right)
        {
            Expression expr;

            if (comp < comparison.Contains)
            {
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
            }
            else
            {
                string meth_name = Enum.GetName(typeof(str_comparison), comp);
                MethodInfo method = typeof(string).GetMethod(meth_name, new[] { typeof(string) });
                var method_expr = Expression.Call(left, method, right);
                expr = Expression.Lambda<Func<T, bool>>(method_expr, tparam);
            }

            return Expression.Lambda<Func<T, bool>>(expr, tparam) as Expression<Func<T, bool>>;
        }

        public Func<T, bool> Build()
        {
            if (estack.Count != 1)
                throw new InvalidOperationException(string.Format("not ready to build, {0} expressions on stack", estack.Count));

            if (subtrees.Count == 1)
                tree = subtrees[0];

            return estack.Pop().Compile();
        }
    }

    public enum comparison
    {
        Equals,
        NotEquals,
        Lt,
        Gt,
        Contains,
        StartsWith,
        EndsWith,
        Between
    }

    public enum str_comparison
    {
        Contains,
        StartsWith,
        EndsWith
    }

    public enum Operator
    {
        And,
        Or,
        Not
    }
}
