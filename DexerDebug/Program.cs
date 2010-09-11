using Dexer.Core;
using System.Diagnostics;
using System;

namespace Dexer.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Dex dex = Dex.Load("classes.dex");
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            dex.Write("output.dex");

            Console.ReadLine();
        }
    }
}
