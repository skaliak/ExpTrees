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

        private int stack_pointer;
        private bool grow;
        private Operation last_operation;

        List<Node> nodes;
        List<SubTree> subtrees;
        public SubTree tree { get; private set; }

        public PropTester()
        {
            thetype = typeof(T);

            //the name is "p", only for debugging purposes
            tparam = Expression.Parameter(thetype, "p");

            estack = new Stack<Expression<Func<T, bool>>>();

            nodes = new List<Node>();
            subtrees = new List<SubTree>();
            tree = new SubTree();
            stack_pointer = 0;
            grow = false;
            last_operation = Operation.Grow;
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

        public PropTester<T> Push(string name, Object val, comparison comp)
        {
            last_operation = Operation.Push;
            var prop = thetype.GetProperty(name);
            var converted_val = Convert_val(prop, val);

            var test_prop = Expression.Property(tparam, prop);
            var test_val = Expression.Constant(converted_val);
            var expr = eFactory(comp, test_prop, test_val);

            Build_tree(name, val, comp);

            estack.Push(expr);

            return this;
        }

        public PropTester<T> And()
        {
            return Apply_Op(Operator.And);
        }

        public PropTester<T> Or()
        {
            return Apply_Op(Operator.Or);
        }

        private PropTester<T> Apply_Op(Operator op)
        {
            if (last_operation == Operation.Grow)
                return this;

            if (last_operation == Operation.Apply_Operator)
            {
                stack_pointer = 0;
                last_operation = Operation.Grow;
            }
            else
                last_operation = Operation.Apply_Operator;

            var pred = PredicateBuilder.False<T>();

            if(op == Operator.And)
                pred = PredicateBuilder.True<T>();
            
            while (estack.Count > stack_pointer)
            {
                var expr = estack.Pop();
                if(op == Operator.And)
                    pred = pred.And<T>(expr);
                else if (op == Operator.Or)
                    pred = pred.Or<T>(expr);
            }
            estack.Push(pred);

            Build_tree(op);
            grow = true;
            if (last_operation == Operation.Grow)
                stack_pointer++;
            else
                stack_pointer = 1;

            return this;
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

        private void Build_tree(string name, object val, comparison comp)
        {
            var node = new Node(name, comp, val);
            nodes.Add(node);
        }

        private void Build_tree(Operator p)
        {
            if (nodes.Count > 0)
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
        //Between  //TODO add between operator
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

    enum Operation
    {
        Grow,
        Push,
        Apply_Operator
    }
}
