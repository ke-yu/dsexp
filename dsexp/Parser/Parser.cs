
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
	public const int _openbracket = 7;
	public const int _closebracket = 8;
	public const int _openparen = 9;
	public const int _closeparen = 10;
	public const int _not = 11;
	public const int _neg = 12;
	public const int _pipe = 13;
	public const int _lessthan = 14;
	public const int _greaterthan = 15;
	public const int _lessequal = 16;
	public const int _greaterequal = 17;
	public const int _equal = 18;
	public const int _notequal = 19;
	public const int _endline = 20;
	public const int _rangeop = 21;
	public const int _kw_def = 22;
	public const int _kw_if = 23;
	public const int _kw_elseif = 24;
	public const int _kw_else = 25;
	public const int _kw_while = 26;
	public const int _kw_for = 27;
	public const int _kw_import = 28;
	public const int _kw_from = 29;
	public const int _kw_break = 30;
	public const int _kw_continue = 31;
	public const int _kw_return = 32;
	public const int _literal_true = 33;
	public const int _literal_false = 34;
	public const int _literal_null = 35;
	public const int maxT = 43;
	public const int _inlinecomment = 44;
	public const int _blockcomment = 45;

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

private bool IsAssignment()
    {
        Token pt = la;
        while (pt.kind != _EOF)
        {
            if (pt.val == ";")
            {
                scanner.ResetPeek();
                return false;
            }
            else if (pt.val == "=")
            {
                scanner.ResetPeek();
                return true;
            }

            pt = scanner.Peek();
        }

        scanner.ResetPeek();
        return false;
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
				if (la.kind == 44) {
				}
				if (la.kind == 45) {
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
		
		if (StartOf(1)) {
			Statement(out statement);
		} else if (la.kind == 22) {
			FunctionDefinition(out statement);
		} else SynErr(44);
	}

	void Statement(out Statement statement) {
		statement = null;
		
		if (IsAssignment()) {
			AssignmentStatement(out statement);
		} else if (StartOf(2)) {
			DSExpression exp;
			
			Expression(out exp);
			statement = new ExpressionStatement(exp);
			
			Expect(20);
		} else if (la.kind == 32) {
			Get();
			DSExpression exp;
			
			Expression(out exp);
			statement = new ReturnStatement(exp);
			
			Expect(20);
		} else SynErr(45);
	}

	void FunctionDefinition(out Statement statement) {
		Expect(22);
		string functionName;
		List<string> parameters;
		
		MethodSignature(out functionName, out parameters);
		List<Statement> statements;
		
		BlockStatement(out statements);
		BlockStatement blockStatement = new BlockStatement(statements.ToArray()); 
		statement = new FunctionDefinition(functionName, parameters, blockStatement);
		
	}

	void MethodSignature(out string functionName, out List<string> parameters) {
		Expect(1);
		functionName = t.val;
		
		Parameters(out parameters);
	}

	void BlockStatement(out List<Statement> statements) {
		statements = new List<Statement>();
		
		Expect(37);
		while (StartOf(1)) {
			Statement statement;
			
			Statement(out statement);
			Expect(20);
			if (statement != null)
			{
			   statements.Add(statement);
			}
			
		}
		Expect(38);
	}

	void Parameters(out List<string> parameters) {
		parameters = new List<string>();
		
		Expect(9);
		if (la.kind == 1) {
			Get();
			parameters.Add(t.val);
			
			while (la.kind == 36) {
				Get();
				Expect(1);
				parameters.Add(t.val);
				
			}
		}
		Expect(10);
	}

	void AssignmentStatement(out Statement assign) {
		Expect(1);
		NameExpression lhs = new NameExpression(t.val);
		
		Expect(39);
		DSExpression rhs;
		
		Expression(out rhs);
		assign = new AssignmentStatement(lhs, rhs);
		
		Expect(20);
	}

	void Expression(out DSExpression expression) {
		expression = null;
		
		if (StartOf(3)) {
			ArithmeticExpression(out expression);
			if (la.kind == 21) {
				Get();
				DSExpression end;
				
				ArithmeticExpression(out end);
				expression = new RangeExpression(expression, end);
				
			}
		} else if (la.kind == 37) {
			ArrayExpression(out expression);
		} else SynErr(46);
	}

	void ArithmeticExpression(out DSExpression expression) {
		MultiplicativeExpression(out expression);
		while (la.kind == 12 || la.kind == 40) {
			DSExpression rhs;
			Operator op;
			
			AdditiveOperator(out op);
			MultiplicativeExpression(out rhs);
			expression = new BinaryExpression(expression, rhs, op);
			
		}
	}

	void ArrayExpression(out DSExpression expression) {
		List<DSExpression> expressions = new List<DSExpression>();
		
		Expect(37);
		if (StartOf(2)) {
			DSExpression exp;
			
			Expression(out exp);
			expressions.Add(exp);
			
			while (la.kind == 36) {
				Get();
				DSExpression exp2;
				
				Expression(out exp2);
				expressions.Add(exp2);
				
			}
		}
		Expect(38);
		expression = new ArrayExpression(expressions.ToArray());
		
	}

	void MultiplicativeExpression(out DSExpression expression) {
		PostfixExpression(out expression);
		while (la.kind == 41 || la.kind == 42) {
			DSExpression rhs;
			Operator op;
			
			MultiplicativeOperator(out op);
			PostfixExpression(out rhs);
			expression = new BinaryExpression(expression, rhs, op);
			
		}
	}

	void AdditiveOperator(out Operator op) {
		op = Operator.Add;
		
		if (la.kind == 40) {
			Get();
		} else if (la.kind == 12) {
			Get();
			op = Operator.Substract;
			
		} else SynErr(47);
	}

	void PostfixExpression(out DSExpression exp) {
		exp = null;
		
		PrimaryExpression(out exp);
		if (la.kind == 7 || la.kind == 9) {
			if (la.kind == 7) {
				DSExpression exp1;
				
				Get();
				Expression(out exp1);
				Expect(8);
				while (la.kind == 7) {
					Get();
					DSExpression exp2;
					
					Expression(out exp2);
					Expect(8);
				}
			} else {
				List<DSExpression> expressions = new List<DSExpression>();
				
				Get();
				ArgumentList(out expressions);
				Expect(10);
			}
		}
	}

	void MultiplicativeOperator(out Operator op) {
		op = Operator.Multiply;
		
		if (la.kind == 41) {
			Get();
		} else if (la.kind == 42) {
			Get();
			op = Operator.Divide;
			
		} else SynErr(48);
	}

	void PrimaryExpression(out DSExpression exp) {
		exp = null;
		
		switch (la.kind) {
		case 9: {
			Get();
			Expression(out exp);
			Expect(10);
			break;
		}
		case 1: {
			Get();
			exp = new NameExpression(t.val);
			
			break;
		}
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
		case 33: {
			Get();
			exp = new ConstantExpression(true);
			
			break;
		}
		case 34: {
			Get();
			exp = new ConstantExpression(false);
			
			break;
		}
		case 35: {
			Get();
			exp = new ConstantExpression(null);
			
			break;
		}
		case 4: {
			Get();
			exp = new ConstantExpression(t.val);
			
			break;
		}
		default: SynErr(49); break;
		}
	}

	void ArgumentList(out List<DSExpression> expressions) {
		expressions = new List<DSExpression>();
		
		while (StartOf(2)) {
			DSExpression arg;
			
			Expression(out arg);
			expressions.Add(arg);
			
			while (la.kind == 36) {
				Get();
				DSExpression arg2;
				
				Expression(out arg2);
				expressions.Add(arg2);
				
			}
		}
	}


    public void Parse() {
        la = new Token();
        la.val = "";        
        Get();
		SimpleDSParser();
		Expect(0);

    }
    
    static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,T,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,T,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x}

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
			case 7: s = "openbracket expected"; break;
			case 8: s = "closebracket expected"; break;
			case 9: s = "openparen expected"; break;
			case 10: s = "closeparen expected"; break;
			case 11: s = "not expected"; break;
			case 12: s = "neg expected"; break;
			case 13: s = "pipe expected"; break;
			case 14: s = "lessthan expected"; break;
			case 15: s = "greaterthan expected"; break;
			case 16: s = "lessequal expected"; break;
			case 17: s = "greaterequal expected"; break;
			case 18: s = "equal expected"; break;
			case 19: s = "notequal expected"; break;
			case 20: s = "endline expected"; break;
			case 21: s = "rangeop expected"; break;
			case 22: s = "kw_def expected"; break;
			case 23: s = "kw_if expected"; break;
			case 24: s = "kw_elseif expected"; break;
			case 25: s = "kw_else expected"; break;
			case 26: s = "kw_while expected"; break;
			case 27: s = "kw_for expected"; break;
			case 28: s = "kw_import expected"; break;
			case 29: s = "kw_from expected"; break;
			case 30: s = "kw_break expected"; break;
			case 31: s = "kw_continue expected"; break;
			case 32: s = "kw_return expected"; break;
			case 33: s = "literal_true expected"; break;
			case 34: s = "literal_false expected"; break;
			case 35: s = "literal_null expected"; break;
			case 36: s = "\",\" expected"; break;
			case 37: s = "\"{\" expected"; break;
			case 38: s = "\"}\" expected"; break;
			case 39: s = "\"=\" expected"; break;
			case 40: s = "\"+\" expected"; break;
			case 41: s = "\"*\" expected"; break;
			case 42: s = "\"/\" expected"; break;
			case 43: s = "??? expected"; break;
			case 44: s = "invalid EntryPoint"; break;
			case 45: s = "invalid Statement"; break;
			case 46: s = "invalid Expression"; break;
			case 47: s = "invalid AdditiveOperator"; break;
			case 48: s = "invalid MultiplicativeOperator"; break;
			case 49: s = "invalid PrimaryExpression"; break;

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