using Dexer.Core;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace Dexer.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Dex dex = Dex.Load("classes.dex");
            sw.Stop();
            Console.WriteLine(string.Format("Load {0} ms", sw.ElapsedMilliseconds));

            sw = Stopwatch.StartNew();
            dex.Write("output.dex");
            sw.Stop();
            Console.WriteLine(string.Format("Write {0} ms", sw.ElapsedMilliseconds));

            Console.ReadLine();
        }
    }
}
