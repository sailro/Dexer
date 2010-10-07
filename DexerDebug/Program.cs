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

            /*MethodDefinition method = dex.GetClass("com.android.vending.licensing.LicenseValidator").GetMethod("verify");

            Instruction sparseSwitch = (from i in method.Body.Instructions where i.OpCode == OpCodes.Sparse_switch select i).First();
            SparseSwitchData data = sparseSwitch.Operand as SparseSwitchData;
            data.Targets[1] = data.Targets[0]; // NOT_LICENSED -> LICENSED*/

            dex.Write("output.dex");
            Console.ReadLine();
        }
    }
}
