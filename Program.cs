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
        static List<Point> ans = new List<Point>();

        static void Check(ref List<Point> points,  ref List<Point> locused)
        {
            if(ans.Count < locused.Count)
            {
                ans = new List<Point>(locused);
            }
        }
        static void FindShuffle_nested(ref List<Point> points, int tostart, List<Point> localusedpoints)
        {
            int n = tostart + 1;
            if (n == points.Count)
            {
                Check(ref points, ref localusedpoints);
            }
            for (int i = n; i < points.Count; i++)
            {
                var tempcon = new List<int>(points[i].Connections);
                bool check = true;
                foreach (var p in localusedpoints)
                    if (tempcon.Contains(p.Number))
                    {
                        check = false; break;
                    }
                var newused= new List<Point>(localusedpoints);
                if(check)
                {
                    newused.Add(points[i]);
                }
                FindShuffle_nested(ref points, i, newused);
            }
        }

        static void FindShuffle(List<Point> points)
        {
            for(int i = 0; i < points.Count; i++)
            {
                var localusedpoints = new List<Point>();
                localusedpoints.Add(points[i]);
                FindShuffle_nested(ref points, i, localusedpoints);
            }
        }


        static void ParallelCheck(ref List<Point> points, ref List<Point> LocalUsed, ref List<Point> locals)
        {
            if(locals.Count < LocalUsed.Count)
            {
                locals = new List<Point>(LocalUsed);
            }
        }
        static void FindShuffle_ParallelNested(ref List<Point> points, List<Point> LocalUsed, int v,ref List<Point> locals)
        {
            int b = v + 1;
            if (b == points.Count)
            {
                ParallelCheck(ref points, ref LocalUsed,ref locals);
            }
            for(int n = b; n < points.Count; n++)
            {
                var tempcons = new List<int>(points[n].Connections);
                bool check = true;
                foreach(var p in LocalUsed)
                {
                    if (tempcons.Contains(p.Number))
                    {
                        check = false; break;
                    }
                }
                var newlocal = new List<Point>(LocalUsed);
                if (check)
                {
                    newlocal.Add(points[n]);
                }
                FindShuffle_ParallelNested(ref points, newlocal, n,ref locals);
            }
        }
        static void FindShuffle_Parallel(List<Point> points)
        {
            Parallel.For(0, points.Count, () => new List<Point>(), (i, loop, localans) =>
            {
                var LocalUsedPoints = new List<Point>();
                LocalUsedPoints.Add(points[i]);
                FindShuffle_ParallelNested(ref points, LocalUsedPoints, i,ref localans);
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
                new Point(12, new List<int>{11,1,24}),
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
            //ответ здесь должен включать вершины: 2,4,7,10,12,13,15,18,20
            //                                ИЛИ: 1,3,5,7 ,10,14,16,21,23
            //                                ИЛИ: 3,5,7,9, 12,13,14,16,23
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
            Points = Init();
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
