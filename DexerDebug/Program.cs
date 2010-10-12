using System;
using Dexer.Core;
using Dexer.Instructions;
using System.Linq;

namespace Dexer.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Dex dex = Dex.Read("classes.dex");
            dex.Write("output.dex");
            Console.ReadLine();
        }
    }
}
