using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treeees
{
    public class SubTree
    {
        public Node[] nodes { get; set; }
        public SubTree[] subtrees { get; set; }
        public Operator op { get; set; }

        public SubTree(Operator op, params Node[] nodes)
        {
            this.nodes = nodes;
            this.op = op;
        }

        public SubTree(Operator op, params SubTree[] trees)
        {
            this.subtrees = trees;
            this.op = op;
        }

        public SubTree()
        {
            // TODO: Complete member initialization
        }
    }
}
