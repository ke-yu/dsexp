using dsexp.Ast;
using dsexp.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace dsexp
{
    class Program
    {
        static void Main(string[] args)
        {
            // REPL 
            (new DSConsoleHost()).Run(args);
        }
    }
}
