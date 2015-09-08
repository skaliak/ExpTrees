using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectMatcher
{
    /// <summary>
    /// Class for generating an expression tree with builder syntax
    /// </summary>
    /// <example>
    /// This example generates an expression tree that will match an object whose "name" property is "steve" OR "jeff"
    /// <code>
    ///    var tester = new TreeBuilder&lt;DataClass&gt;();
    ///    tester.Push("name", "steve", comparison.Equals)
    ///           .Push("name", "jeff", comparison.Equals)
    ///            .Or();
    ///    var lambda = tester.Build();
    /// 
    /// </code>
    /// </example>
    /// <remarks>
    /// This class can be serialized to json via the tree property
    /// 
    /// PL 9/8/15
    /// </remarks>
    public class TreeBuilder<T>
    {
        private Type thetype;
        private ParameterExpression tparam;

        private Stack<Expression<Func<T, bool>>> estack;
        private int stack_pointer;

        private Operation last_operation;

        List<Node> nodes;
        List<Node> data_nodes;
        public Node tree { get; private set; }

        public TreeBuilder()
        {
            thetype = typeof(T);

            //the name is "p", only for debugging purposes
            tparam = Expression.Parameter(thetype, "p");

            estack = new Stack<Expression<Func<T, bool>>>();

            nodes = new List<Node>();
            data_nodes = new List<Node>();

            tree = new Node();
            stack_pointer = 0;

            last_operation = Operation.Grow;
        }


        /// <summary>
        /// Add an expression to the tree
        /// </summary>
        /// <param name="name">the property name</param>
        /// <param name="val">the value to test against</param>
        /// <param name="comp">the comparison type</param>
        public TreeBuilder<T> Push(string name, Object val, comparison comp)
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

        /// <summary>
        /// join all the previously added nodes (expressions) with an AND operator
        /// </summary>
        public TreeBuilder<T> And()
        {
            return Apply_Op(Operator.And);
        }

        /// <summary>
        /// join all the previously added nodes (expressions) with an OR operator
        /// </summary>
        public TreeBuilder<T> Or()
        {
            return Apply_Op(Operator.Or);
        }



        /// <summary>
        /// negate the most recently added expression
        /// </summary>
        public TreeBuilder<T> Not()
        {
            //are there any other operations that this can follow?
            if (last_operation == Operation.Push)
            {
                var negated = Expression.Negate(estack.Pop());
                var expr = Expression.Lambda<Func<T, bool>>(negated, tparam);
                estack.Push(expr);
            }

            return this;
        }

        /// <summary>
        /// compile the expression tree into a lambda that returns a bool indicating if the
        /// <para>supplied object matches</para>
        /// </summary>
        public Func<T, bool> Build()
        {
            if (estack.Count != 1)
                throw new InvalidOperationException(string.Format("not ready to build, {0} expressions on stack", estack.Count));

            if (nodes.Count == 1)
                tree = nodes[0];

            return estack.Pop().Compile();
        }

        public static TreeBuilder<T> Restore(Node tree)
        {
            return Restore(tree, new TreeBuilder<T>());
        }

        private static TreeBuilder<T> Restore(Node tree, TreeBuilder<T> tb)
        {
            if (tree.nodes != null)
            {
                foreach (var node in tree.nodes)
                {
                    Restore(node, tb);
                } 
            }
 
            tb.Push(tree.data);
            if (tree.op != null)
            {
                var op = (Operator)tree.op;
                tb.Apply_Op(op);
            }

            return tb;
        }

        private void Push(NodeData nodeData)
        {
            if(nodeData != null)
                Push(nodeData.prop, nodeData.value, nodeData.comp);
        }

        private TreeBuilder<T> Apply_Op(Operator op)
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

            if (last_operation == Operation.Grow)
                stack_pointer++;
            else
                stack_pointer = 1;

            return this;
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

        /// <summary>
        /// Generate the proper expression based on the two input expressions and the comparison type</summary>
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
            var actual_node = new Node(name, val, comp);
            data_nodes.Add(actual_node);
        }

        //TODO, rework this to make it more tree-like (sorry ¯\_(ツ)_/¯ )
        private void Build_tree(Operator p)
        {
            if (data_nodes.Count > 0)
            {
                var subtree = new Node(p, data_nodes.ToArray());
                data_nodes.Clear();
                nodes.Add(subtree);
            }
            else if (nodes.Count > 0)
            {
                var subtree = new Node(p, nodes.ToArray());
                nodes.Clear();
                nodes.Add(subtree);
            }
        }

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum comparison
    {
        Equals,
        NotEquals,
        Lt,
        Gt,
        Contains,
        StartsWith,
        EndsWith
    }

    public enum str_comparison
    {
        Contains,
        StartsWith,
        EndsWith
    }

    [JsonConverter(typeof(StringEnumConverter))]
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
