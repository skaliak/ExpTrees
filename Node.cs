using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treeees
{
    public class Node
    {
        public string prop { get; set; }
        public object[] values { get; set; }
        public comparison comp { get; set; }

        public Node(string property_name, comparison c, params object[] values)
        {
            prop = property_name;
            this.values = values;
            comp = c;
        }
    }
}
