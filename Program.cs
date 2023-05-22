using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Graphs
{
    class Point
    {
        public int Number {get;set;}
        public List<int> Connections { get;set;}

        public Point(int number, List<int> connections)
        {
            Connections = connections;
            Number = number;
        }

        private string ConsStr(List<int> cons)
        {
            string str = "";
            foreach (int i in cons) { str += i; str += ","; }
            return str;
        }
        public override string ToString()
        {

            return ($"Number: {Number} Connected With {ConsStr(Connections)}");
        }
    }
    class Program
    {
        static object locker = new object();
        static List<int> Used = new List<int>();
        static List<Point> ans = new List<Point>();

        static void Check(List<Point> points, int startout)
        {
            var localans = new List<Point>();
            for (int i = startout; i < points.Count; i++)
            {
                if (!Used.Contains(points[i].Number))
                {
                    localans.Add(points[i]);
                }
            }
            if (localans.Count > ans.Count)
            {
                ans = new List<Point>(localans);
            }
        }

        static void FindShuffle_Nested(List<Point> points, int b, int startout)
        {
            int v = b + 1;
            if (v == points.Count)
            {
                Check(points, startout);
            }
            for (int n = v; n < points.Count; n++)
            {
                if (!Used.Contains(points[n].Number))
                {
                    foreach (var l in points[n].Connections)
                    {

                        if(!Used.Contains(l))
                        {
                            Used.Add(l);
                        }
                    }
                }
                FindShuffle_Nested(points, n, startout);
            }
        }
        static void FindShuffle(List<Point> points)
        {
            for(int i = 0; i < points.Count; i++)
            {
                Used = new List<int>();
                Used.AddRange(points[i].Connections);
                FindShuffle_Nested(points, i, i);

            }
        }


        static void ParallelCheck(ref List<Point> points, ref List<int> LocalUsed, int startout, ref List<Point> locals)
        {
            var tempans = new List<Point>();
            for(int i = startout; i < points.Count; i++)
            {
                if (!LocalUsed.Contains(points[i].Number))
                {
                    tempans.Add(points[i]);
                }
            }
            if (tempans.Count > locals.Count)
                locals = new List<Point>(tempans);
        }
        static void FindShuffle_ParallelNested(ref List<Point> points, List<int> LocalUsed, int v, int startout,ref List<Point> locals)
        {
            int b = v + 1;
            if (b == points.Count)
            {
                ParallelCheck(ref points, ref LocalUsed, startout, ref locals);
            }
            for(int n = b; n < points.Count; n++)
            {
                if (!LocalUsed.Contains(points[n].Number))
                {
                    foreach(var l in points[n].Connections) 
                    { 
                        if(!LocalUsed.Contains(l)) { LocalUsed.Add(l); } 
                    
                    }
                }

                FindShuffle_ParallelNested(ref points, LocalUsed, n, startout,ref locals);
            }
        }
        static void FindShuffle_Parallel(List<Point> points)
        {
            Parallel.For(0, points.Count, () => new List<Point>(), (i, loop, localans) =>
            {
                var LocalUsed = new List<int>();
                LocalUsed.AddRange(points[i].Connections);
                FindShuffle_ParallelNested(ref points, LocalUsed, i, i,ref localans);
                return localans;
            },
            (x) =>
            {
                lock(locker)
                {
                    if(ans.Count < x.Count)
                    {
                        ans = new List<Point>(x);
                    }
                }
            }
            );
        }
        static List<Point> Init()
        {
            var ListToFill = new List<Point>
            {
                new Point(1, new List<int>{12,2,13 }),
                new Point(2, new List<int>{1,3,14}),
                new Point(3, new List<int>{2,4,15}),
                new Point(4, new List<int>{3,5,16}),
                new Point(5, new List<int>{4,6,17}),
                new Point(6, new List<int>{5,7,18}),
                new Point(7, new List<int>{6,8,19}),
                new Point(8, new List<int>{7,9,20}),
                new Point(9, new List<int>{8,10,21}),
                new Point(10, new List<int>{9,11,22}),
                new Point(11, new List<int>{10,12,23}),
                new Point(12, new List<int>{11,13,24}),
                new Point(13, new List<int>{1,17,21}),
                new Point(14, new List<int>{2,18,22}),
                new Point(15, new List<int>{3,19,23}),
                new Point(16, new List<int>{4,20,24}),
                new Point(17, new List<int>{5,21,13}),
                new Point(18, new List<int>{6,22,14}),
                new Point(19, new List<int>{7,23,15}),
                new Point(20, new List<int>{8,24,16}),
                new Point(21, new List<int>{9,13,17}),
                new Point(22, new List<int>{10,14,18}),
                new Point(23, new List<int>{11,15,19}),
                new Point(24, new List<int>{12,16,20})
            };
            return ListToFill;
        }
        
        static List<Point> Test()
        {
            var Listt = new List<Point>
            {
                new Point(1, new List<int>{2,4,5}),
                new Point(2, new List<int>{1,3,6}),
                new Point(3, new List<int>{2,4,7}),
                new Point(4, new List<int>{3,1,8}),
                new Point(5, new List<int>{1,6,8}),
                new Point(6, new List<int>{2,5,7}),
                new Point(7, new List<int>{3,6,8}),
                new Point(8, new List<int>{4,5,7})
            };
            return Listt;
        }
        static void Main(string[] args)
        {
            var Points = new List<Point>();
            Points = Test();
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Последовательный");
            FindShuffle(Points);
            foreach (Point p in ans)
            {
                Console.WriteLine(p.ToString());
            }
            var posled = sw.Elapsed;

            sw.Restart();
            ans = new List<Point>();
            Console.WriteLine();
            Console.WriteLine("Параллельный алгоритм");
            FindShuffle_Parallel(Points);
            foreach (Point p in ans)
            {
                Console.WriteLine(p.ToString());
            }
            var parallel = sw.Elapsed;
            Console.WriteLine("Straight: " + posled);
            Console.WriteLine("Parallel: " + parallel);

        }
    }
}
