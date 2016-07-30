using dsexp.Ast;
using Microsoft.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting.Shell;

namespace dsexp.Runtime
{
    public class DSContext : LanguageContext
    {
        public DSContext(ScriptDomainManager domainManager, IDictionary<string, object> options)
            : base(domainManager)
        {

        }

        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
        {
            return new DSScriptCode(sourceUnit);
        }
    }

    public class DSScriptCode : ScriptCode
    {
        public DSScriptCode(SourceUnit sourceUnit): base(sourceUnit)
        {

        }

        public override object Run(Scope scope)
        {
            Console.WriteLine("DesignScript");
            return null;
        }
    }

    public class DSCommandLine : ConsoleHost
    {
        protected override Type Provider
        {
            get
            {
                return typeof(DSContext);
            }
        }
    }
}
