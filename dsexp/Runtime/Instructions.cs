using Microsoft.Scripting.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsexp.Runtime
{
    public class LookUpGlobalInstruction : Instruction
    {
        private string name;

        public LookUpGlobalInstruction(string name)
        {
            this.name = name;
        }

        public override int ConsumedStack
        {
            get { return 1; }
        }

        public override int ProducedStack
        {
            get { return 1; }
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Push(DSOps.GetGlobal((DSContext)frame.Pop(), name));
            return 1;
        }
    }
}
