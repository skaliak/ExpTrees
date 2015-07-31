using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treeees
{
    public class Node
    {
        [Obsolete]
        public Leaf[] leaves { get; set; }
        public Node[] nodes { get; set; }
        public NodeData data { get; private set; }
        public Operator? op { get; set; }

        [Obsolete]
        public Node(Operator op, params Leaf[] leaves)
        {
            this.leaves = leaves;
            this.op = op;
        }

        public Node(string prop_name, object val, comparison c)
        {
            data = new NodeData()
            {
                value = val,
                prop = prop_name,
                comp = c
            };
        }

        public Node(Operator op, params Node[] nodes)
        {
            this.nodes = nodes;
            this.op = op;
        }

        public Node()
        {
            // TODO: Complete member initialization
        }
    }
}
