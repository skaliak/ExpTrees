using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Newtonsoft.Json;
using Treeees;
using System.Linq;

namespace ExprTreesTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestTupleSerialize()
        {
            //should be a list at the second level
            var x = new List<Tuple<Tuple<string, string, string, int>, int>>();

            x.Add(Tuple.Create(Tuple.Create("hey", "ho", "poo", (int)color.blue), (int)color.red));
            x.Add(Tuple.Create(Tuple.Create("hoi", "blah", "foo", (int)ord.one), (int)ord.two));

            var json = JsonConvert.SerializeObject(x);

            Assert.IsNotNull(json);
            Console.WriteLine(json);
        }

        [TestMethod]
        public void TestPropTesterBasic()
        {
            var tester = new TreeBuilder<DataClass>();
            var matched = new List<string>();
            var not_matched = new List<string>();
            var should_matched = new List<string> { "steve", "jeff" };
            var should_not_matched = new List<string> { "jimbo", "george" };

            tester.Push("name", "steve", comparison.Equals)
                   .Push("name", "jeff", comparison.Equals)
                    .Or();
            var lambda = tester.Build();

            foreach (var item in MakeSomeDataObjs())
            {
                if (lambda(item))
                    matched.Add(item.name);
                else
                    not_matched.Add(item.name);
            }

            Assert.IsTrue(Enumerable.SequenceEqual(matched.OrderBy(p => p), should_matched.OrderBy(p => p)));
            Assert.IsTrue(Enumerable.SequenceEqual(not_matched.OrderBy(p => p), should_not_matched.OrderBy(p => p)));
        }

        [TestMethod]
        public void TestTesterSerialization1()
        {
            var tester = new TreeBuilder<DataClass>();

            tester.Push("name", "steve", comparison.Equals)
                .Push("name", "jeff", comparison.Equals)
                .Or();
            var lambda = tester.Build();

            var json = JsonConvert.SerializeObject(tester.tree);
            Assert.IsNotNull(json);

            var tree = JsonConvert.DeserializeObject<Node>(json);
            Assert.IsNotNull(tree);
            
            Console.WriteLine(json);
        }

        [TestMethod]
        public void TestTesterSerialization2()
        {
            var tester = new TreeBuilder<DataClass>();

            tester.Push("name", "steve", comparison.Equals)
                .Push("name", "jeff", comparison.Equals)
                .And()
                .Push("name", "foo", comparison.Equals)
                .Push("name", "bar", comparison.Equals)
                .And()
                .Or();
            var lambda = tester.Build();

            var json = JsonConvert.SerializeObject(tester.tree);
            Assert.IsNotNull(json);

            var tree = JsonConvert.DeserializeObject<Node>(json);
            Assert.IsNotNull(tree);

            Console.WriteLine(json);
        }

        private IEnumerable<DataClass> MakeSomeDataObjs()
        {
            var names = new[] { "jimbo", "steve", "jeff", "george" };
            var output = new List<DataClass>();

            for (int i = 0; i < names.Length; i++)
            {
                yield return new DataClass(i, names[i]);
            }
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
