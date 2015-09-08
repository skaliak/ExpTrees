using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ObjectMatcher
{
    public class Serializer<T>
    {
        public string Serialize(TreeBuilder<T> obj)
        {
            throw new NotImplementedException();
        }

        public TreeBuilder<T> Deserialize(string json)
        {
            throw new NotImplementedException();
        }

    }
}
