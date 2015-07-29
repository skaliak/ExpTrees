using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExprTreesTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //should be a list at the second level
            var x = new List<Tuple<Tuple<string, string, string, int>, int>>();

            x.Add(Tuple.Create(Tuple.Create("hey", "ho", "poo", (int)color.blue), (int)color.red));
            x.Add(Tuple.Create(Tuple.Create("hoi", "blah", "foo", (int)ord.one), (int)ord.two));

            var json = JsonConvert.SerializeObject(x);

            Assert.IsNotNull(json);
            Console.WriteLine(json);
        }
    }

    enum color
    {
        red,
        blue
    }

    enum ord
    {
        one = 2,
        two = 3
    }
}
