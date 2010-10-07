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

            MethodDefinition method = dex.GetClass("com.android.vending.licensing.LicenseValidator").GetMethod("verify");
            method.Body.Instructions.Clear();
            method.Body.Exceptions.Clear();
            method.Body.Instructions.Add(new Instruction(OpCodes.Return_void));

            dex.Write("output.dex");
            Console.ReadLine();
        }
    }
}
