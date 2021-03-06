﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprTreesTests
{
    public class DataClass
    {
        public int id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public DataClass(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", id, name);
        }
    }
}
