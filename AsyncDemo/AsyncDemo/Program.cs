using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AsyncDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            stopwatch.Start();
            var t1 = Delay3();
            var t2 = Delay4();

            stopwatch.Stop();

            await t1;
            await t2;

            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed} ms");
        }

        public static void Delay()
        {
            Console.WriteLine("Delay started...");
            Task.Delay(1000).Wait();
            Console.WriteLine("Delay completed.");
        }

        public static void Delay2()
        {
            Console.WriteLine("Delay2 started...");
            Task.Delay(2000).Wait();
            Console.WriteLine("Delay2 completed.");
        }

        public static async Task Delay3()
        {
            Console.WriteLine("Delay3 started...");
            await Task.Delay(1000);
            Console.WriteLine("Delay3 completed.");
        }

        public static async Task Delay4()
        {
            Console.WriteLine("Delay4 started...");
            await Task.Delay(2000);
            Console.WriteLine("Delay4 completed.");
        }
    }
}
