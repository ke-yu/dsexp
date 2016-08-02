using dsexp.Ast;
using Microsoft.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Hosting.Providers;

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

    public class DSConsoleHost : ConsoleHost
    {
        protected override Type Provider
        {
            get
            {
                return typeof(DSContext);
            }
        }

        protected override void ExecuteInternal()
        {
            var context = HostingHelpers.GetLanguageContext(Engine) as DSContext;
            base.ExecuteInternal();
        }

        protected override CommandLine CreateCommandLine()
        {
            return new DSCommandLine();
        }
    }

    public class DSCommandLine : CommandLine
    {
        private DSCodeContext context = new DSCodeContext();
        private delegate void LookupDelegate(DSCodeContext context);

        protected override int Run()
        {
            return base.Run();
        }

        protected override int? TryInteractiveAction()
        {
            bool continueInteraction;
            string statement = ReadStatement(out continueInteraction);

            if (!continueInteraction)
            {
                return 0;
            }

            if (statement.IndexOf(';') < 0)
            {
                statement = statement + ';';
            }

            var astStatement = dsexp.Ast.Parser.Parse(statement);
            Run(astStatement, context);
            return null;
        }

        private object Run(Statement node, DSCodeContext context)
        {
            DSAst ast = new DSAst(node);
            ast.Bind();

            var expr = ast.ReduceToExpression();
            // uncomment to use interpreter mode
            // var func = Utils.LightLambda<LookupDelegate>(typeof(object), Utils.Convert(expr, typeof(object)), "<uname>", new List<ParameterExpression> { ast.GlobalContext });
            var func = System.Linq.Expressions.Expression.Lambda<Action<DSCodeContext>>(expr, new[] { ast.GlobalContext });
            var funcode = func.Compile();
            funcode(context);
            return null;
        }
    }
}
