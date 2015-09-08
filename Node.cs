using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMatcher
{
    public class Node
    {
        public Node[] nodes { get; set; }
        public NodeData data { get; private set; }
        public Operator? op { get; set; }

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

        }
    }
}
