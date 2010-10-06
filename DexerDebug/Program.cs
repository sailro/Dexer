using System;
using Dexer.Core;
using Dexer.Instructions;

namespace Dexer.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Dex dex = Dex.Load("classes.dex");
            MethodDefinition method = dex.GetClass("dexer.poc.MainActivity").GetMethod("onCreate");

            method.Body.Instructions[5].OpCode = OpCodes.Add_int;
            method.Body.Instructions[7].Operand = "Dexer rocks! ";

            int color; unchecked { color = (int)0xFFFF00FF; }

            // Declare a new method reference with prototype
            Prototype prototype = new Prototype(PrimitiveType.Void, new Parameter(PrimitiveType.Int));
            MethodReference setTitleColor = dex.Import(new MethodReference(method.Owner, "setTitleColor", prototype));

            // Load the color in a register (n°1) then invoke the method (register n°5 is 'this' in our case)
            var regs = method.Body.Registers;
            Instruction iconst = new Instruction(OpCodes.Const, color, regs[1]);
            method.Body.Instructions.Insert(14, iconst);

            Instruction iinvoke = new Instruction(OpCodes.Invoke_virtual, setTitleColor, regs[5], regs[1]);
            method.Body.Instructions.Insert(15, iinvoke);

            dex.Write("output.dex");
            Console.ReadLine();
        }
    }
}
