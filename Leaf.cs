using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treeees
{
    [Obsolete]
    public class Leaf
    {
        public string prop { get; set; }
        public object[] values { get; set; }
        public comparison comp { get; set; }

        public Leaf(string property_name, comparison c, params object[] values)
        {
            prop = property_name;
            this.values = values;
            comp = c;
        }
    }
}
