using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treeees
{
    public class Util
    {
        //http://stackoverflow.com/a/606381/1884140
        public static object ParseString(string str)
        {
            int intValue;
            double doubleValue;
            DateTime dateTimeValue;
            bool boolValue;

            // Place checks higher if if-else statement to give higher priority to type.
            if (int.TryParse(str, out intValue))
                return intValue;
            else if (double.TryParse(str, out doubleValue))
                return doubleValue;
            else if (DateTime.TryParse(str, out dateTimeValue))
                return dateTimeValue;
            else if (bool.TryParse(str, out boolValue))
                return boolValue;

            return null;
        }
    }
}
