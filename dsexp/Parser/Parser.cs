
//#define ENABLE_INC_DEC_FIX
using System;
using System.Collections.Generic;
using System.IO;
using dsexp.Ast;

namespace dsexp.Ast {


public class Parser {
	public const int _EOF = 0;
	public const int _identifier = 1;
	public const int _number = 2;
	public const int _float = 3;
	public const int _textstring = 4;
	public const int _char = 5;
	public const int _period = 6;
	public const int _postfixed_replicationguide = 7;
	public const int _list_at = 8;
	public const int _dominant_list_at = 9;
	public const int _openbracket = 10;
	public const int _closebracket = 11;
	public const int _openparen = 12;
	public const int _closeparen = 13;
	public const int _not = 14;
	public const int _neg = 15;
	public const int _pipe = 16;
	public const int _lessthan = 17;
	public const int _greaterthan = 18;
	public const int _lessequal = 19;
	public const int _greaterequal = 20;
	public const int _equal = 21;
	public const int _notequal = 22;
	public const int _endline = 23;
	public const int _rangeop = 24;
	public const int _kw_def = 25;
	public const int _kw_if = 26;
	public const int _kw_elseif = 27;
	public const int _kw_else = 28;
	public const int _kw_while = 29;
	public const int _kw_for = 30;
	public const int _kw_import = 31;
	public const int _kw_from = 32;
	public const int _kw_break = 33;
	public const int _kw_continue = 34;
	public const int _literal_true = 35;
	public const int _literal_false = 36;
	public const int _literal_null = 37;
	public const int maxT = 45;
	public const int _inlinecomment = 46;
	public const int _blockcomment = 47;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

    public Statement RootStatement
    {
        get; set;
    }


    public static Statement Parse(string code)
    {
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(code);
        byte[] utf8Buffer = new byte[buffer.Length + 3];

        // Add UTF-8 BOM - Coco/R requires UTF-8 stream should contain BOM
        utf8Buffer[0] = (byte)0xEF;
        utf8Buffer[1] = (byte)0xBB;
        utf8Buffer[2] = (byte)0xBF;
        Array.Copy(buffer, 0, utf8Buffer, 3, buffer.Length);

        System.IO.MemoryStream memstream = new System.IO.MemoryStream(utf8Buffer);
        Scanner scanner = new Scanner(memstream);
        Parser parser = new Parser(scanner);
        parser.Parse();
        return parser.RootStatement;
    }

	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }
				if (la.kind == 46) {
				}
				if (la.kind == 47) {
				}

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}

	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}
	
	void SimpleDSParser() {
		Statement statement;
		
		EntryPoint(out statement);
		RootStatement = statement;
		
	}

	void EntryPoint(out Statement statement) {
		statement = null;
		
		if (la.kind == 1) {
			Statement(out statement);
		} else if (la.kind == 25) {
			FunctionDefinition();
		} else SynErr(46);
	}

	void Statement(out Statement statement) {
		AssignmentStatement(out statement);
	}

	void FunctionDefinition() {
		Expect(25);
		string functionName;
		List<string> parameters;
		
		MethodSignature(out functionName, out parameters);
		List<Statement> statements;
		
		BlockStatement(out statements);
	}

	void MethodSignature(out string functionName, out List<string> parameters) {
		Expect(1);
		functionName = t.val;
		
		Parameters(out parameters);
	}

	void BlockStatement(out List<Statement> statements) {
		statements = new List<Statement>();
		
		Expect(39);
		while (la.kind == 1) {
			Statement statement;
			
			Statement(out statement);
			if (statement != null)
			{
			   statements.Add(statement);
			}
			
		}
		Expect(40);
	}

	void Parameters(out List<string> parameters) {
		parameters = new List<string>();
		
		Expect(12);
		if (la.kind == 1) {
			Get();
			parameters.Add(t.val);
			
			while (la.kind == 38) {
				Get();
				Expect(1);
				parameters.Add(t.val);
				
			}
		}
	}

	void AssignmentStatement(out Statement assign) {
		Expect(1);
		NameExpression lhs = new NameExpression(t.val);
		
		Expect(41);
		DSExpression rhs;
		
		Expression(out rhs);
		assign = new AssignmentStatement(lhs, rhs);
		
	}

	void Expression(out DSExpression expression) {
		ArithmeticExpression(out expression);
		if (la.kind == 24) {
			Get();
			DSExpression end;
			
			ArithmeticExpression(out end);
			expression = new RangeExpression(expression, end);
			
		}
	}

	void ArithmeticExpression(out DSExpression expression) {
		MultiplicativeExpression(out expression);
		while (la.kind == 15 || la.kind == 42) {
			DSExpression rhs;
			Operator op;
			
			AdditiveOperator(out op);
			MultiplicativeExpression(out rhs);
			expression = new BinaryExpression(expression, rhs, op);
			
		}
	}

	void MultiplicativeExpression(out DSExpression expression) {
		Factor(out expression);
		while (la.kind == 43 || la.kind == 44) {
			DSExpression rhs;
			Operator op;
			
			MultiplicativeOperator(out op);
			Factor(out rhs);
			expression = new BinaryExpression(expression, rhs, op);
			
		}
	}

	void AdditiveOperator(out Operator op) {
		op = Operator.Add;
		
		if (la.kind == 42) {
			Get();
		} else if (la.kind == 15) {
			Get();
			op = Operator.Substract;
			
		} else SynErr(47);
	}

	void Factor(out DSExpression exp) {
		exp = null;
		
		switch (la.kind) {
		case 2: {
			Get();
			Int64 value;
			if (Int64.TryParse(t.val, out value))
			{
			   exp = new ConstantExpression(value);
			}
			else
			{
			   exp = new ConstantExpression(null);
			}
			
			break;
		}
		case 3: {
			Get();
			double value;
			if (Double.TryParse(t.val, out value))
			{
			   exp = new ConstantExpression(value);
			}
			else
			{
			   exp = new ConstantExpression(null);
			}
			
			break;
		}
		case 35: {
			Get();
			exp = new ConstantExpression(true);
			
			break;
		}
		case 36: {
			Get();
			exp = new ConstantExpression(false);
			
			break;
		}
		case 37: {
			Get();
			exp = new ConstantExpression(null);
			
			break;
		}
		case 4: {
			Get();
			exp = new ConstantExpression(t.val);
			
			break;
		}
		default: SynErr(48); break;
		}
	}

	void MultiplicativeOperator(out Operator op) {
		op = Operator.Multiply;
		
		if (la.kind == 43) {
			Get();
		} else if (la.kind == 44) {
			Get();
			op = Operator.Divide;
			
		} else SynErr(49);
	}


	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		SimpleDSParser();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x}

	};
} // end Parser

public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public System.IO.TextWriter warningStream = Console.Out;
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "identifier expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "float expected"; break;
			case 4: s = "textstring expected"; break;
			case 5: s = "char expected"; break;
			case 6: s = "period expected"; break;
			case 7: s = "postfixed_replicationguide expected"; break;
			case 8: s = "list_at expected"; break;
			case 9: s = "dominant_list_at expected"; break;
			case 10: s = "openbracket expected"; break;
			case 11: s = "closebracket expected"; break;
			case 12: s = "openparen expected"; break;
			case 13: s = "closeparen expected"; break;
			case 14: s = "not expected"; break;
			case 15: s = "neg expected"; break;
			case 16: s = "pipe expected"; break;
			case 17: s = "lessthan expected"; break;
			case 18: s = "greaterthan expected"; break;
			case 19: s = "lessequal expected"; break;
			case 20: s = "greaterequal expected"; break;
			case 21: s = "equal expected"; break;
			case 22: s = "notequal expected"; break;
			case 23: s = "endline expected"; break;
			case 24: s = "rangeop expected"; break;
			case 25: s = "kw_def expected"; break;
			case 26: s = "kw_if expected"; break;
			case 27: s = "kw_elseif expected"; break;
			case 28: s = "kw_else expected"; break;
			case 29: s = "kw_while expected"; break;
			case 30: s = "kw_for expected"; break;
			case 31: s = "kw_import expected"; break;
			case 32: s = "kw_from expected"; break;
			case 33: s = "kw_break expected"; break;
			case 34: s = "kw_continue expected"; break;
			case 35: s = "literal_true expected"; break;
			case 36: s = "literal_false expected"; break;
			case 37: s = "literal_null expected"; break;
			case 38: s = "\",\" expected"; break;
			case 39: s = "\"{\" expected"; break;
			case 40: s = "\"}\" expected"; break;
			case 41: s = "\"=\" expected"; break;
			case 42: s = "\"+\" expected"; break;
			case 43: s = "\"*\" expected"; break;
			case 44: s = "\"/\" expected"; break;
			case 45: s = "??? expected"; break;
			case 46: s = "invalid EntryPoint"; break;
			case 47: s = "invalid AdditiveOperator"; break;
			case 48: s = "invalid Factor"; break;
			case 49: s = "invalid MultiplicativeOperator"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		warningStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		warningStream.WriteLine(String.Format("Warning: {0}",s));
	}
} // Errors

public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}


}