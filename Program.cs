using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Treeees
{
    class Program
    {
        static void Main(string[] args)
        {
            //var lambda = GetPropTester<DataClass>("name", "steve");
            var tester = new PropTester<DataClass>();

            tester.Push("name", "steve", comparison.Equals)
                   .Push("name", "jeff", comparison.Equals)
                    .Or();
            var lambda = tester.Build();

            Console.WriteLine(tester.rpnString);

            List<DataClass> objs = MakeSomeDataObjs();

            foreach (var item in objs)
            {
                var itemstr = item.ToString();
                if(lambda(item))
                    Console.WriteLine(string.Format("{0} matches", itemstr));
                else
                    Console.WriteLine(string.Format("{0} doesn't matches", itemstr));
            }

            Console.ReadLine();
        }

        private static Func<T, bool> GetPropTester<T>(string name, string value)
        {
            var thetype = typeof(T);
            var param = Expression.Parameter(thetype, "p");
            var test = Expression.Property(param, thetype.GetProperty(name));
            var c = Expression.Constant(value);
            var full_exp = Expression.Lambda<Func<T, bool>>(Expression.Equal(test, c), new[] { param });
            var compiled = full_exp.Compile();
            return compiled;
        }

        private static List<DataClass> MakeSomeDataObjs()
        {
            var names = new[] { "jimbo", "steve", "jeff", "george" };
            var output = new List<DataClass>();

            for (int i = 0; i < names.Length; i++ )
            {
                output.Add(new DataClass(i, names[i]));
            }

            return output;
        }

        private static void Exp1()
        {
            var param = Expression.Parameter(typeof(int), "i");

            var lam = Expression.Lambda<Func<int, bool>>(Expression.IsTrue(Expression.GreaterThan(param, Expression.Constant(3))), new[] { param });
            var compiled = lam.Compile();

            foreach (var i in Enumerable.Range(1, 5))
            {
                if (compiled(i))
                    Console.WriteLine(string.Format("{0} > 3", i));
            }
        }
    }
}
