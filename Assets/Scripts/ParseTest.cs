using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PxPre.Datum;

public class ParseTest : MonoBehaviour
{
    [System.Serializable]
    public class TestCategory
    {
        public string title;
        public string description;
        public List<ScriptTest> tests;
    }

    [System.Serializable]
    public class ScriptTest
    { 
        public enum ExpectedType
        { 
            Ignore,
            Bool,
            Int,
            Float,
            String,
            Null
        }
        public string title;
        [Multiline(5)]
        public string script;
        public string resultExpression;
        public string checkExpression;
        public ExpectedType expectedType = ExpectedType.Ignore;

        public bool Test(LogSession session)
        {
            bool ret = true;
            //  COMPILATION
            //////////////////////////////////////////////////
            Val compiledProgram = Compile(script, true, session);
            session.AppendLine($"Executing {this.title}");

            //  EXECUTION
            //////////////////////////////////////////////////
            Ctx ctx = new Ctx(null);
            Val returnVal = compiledProgram.Execute(ctx, out var _);
            session.AppendLine($"\treturn value {returnVal.GetString()}");

            //  RESULT VALIDATION
            //////////////////////////////////////////////////
            if(!string.IsNullOrEmpty(this.resultExpression))
            {
                session.Append("Running result check");
                Val resultCheckProgram = Compile(this.resultExpression, false, session);
                if (resultCheckProgram == null)
                {
                    session.AppendErrorLine($"Invalid result check expression for {this.title}: Could not compile\n {this.resultExpression}");
                    return false;
                }
                else
                {
                    Val checkVal = resultCheckProgram.Execute(ctx, out var _);
                    if(!checkVal.Equivalent(returnVal))
                    {
                        session.AppendErrorLine($"The result for {this.title} was unexpected.");
                    }
                    else
                    {
                        session.AppendLine($"Result check for {this.title} passed.");
                    }
                }
            }

            //  EXPRESSION VALIDATION
            //////////////////////////////////////////////////
            if (!string.IsNullOrEmpty(this.checkExpression))
            {
                session.AppendLine("Running check expression");
                Val checkProgram = Compile(this.checkExpression, false, session);
                if(checkProgram == null)
                {
                    session.AppendErrorLine($"Invalid check expression for {this.title}: Could not compile\n {this.checkExpression}");
                    ret = false;
                }
                else
                { 
                    Val checkVal = checkProgram.Execute(ctx, out var _);
                    if(checkVal.wrapType != Val.Type.Bool)
                    {
                        session.AppendErrorLine($"Invalid check expression for {this.title}: Expression must return a bool.");
                        ret = false;
                    }
                    else if(checkVal.GetBool() == false)
                    {
                        session.AppendErrorLine($"Check test expression for {this.title} failed.");
                        ret = false;
                    }
                    else
                    {
                        session.AppendLine($"Check test expression for {this.title} passed.");
                    }
                }

            }
            //  TYPE CHECK
            //////////////////////////////////////////////////
            if (expectedType != ExpectedType.Ignore)
            { 
                bool typeMatched = true;
                switch(expectedType)
                { 
                    case ExpectedType.Bool:
                        typeMatched = returnVal.wrapType == Val.Type.Bool;
                        break;
                    case ExpectedType.Int:
                        typeMatched = returnVal.wrapType == Val.Type.Int;
                        break;
                    case ExpectedType.Float:
                        typeMatched = returnVal.wrapType == Val.Type.Float;
                        break;
                    case ExpectedType.String:
                        typeMatched = returnVal.wrapType == Val.Type.String;
                        break;
                    case ExpectedType.Null:
                        typeMatched = returnVal.wrapType == Val.Type.None;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if(!typeMatched)
                {
                    session.AppendErrorLine($"Final typecheck test for {this.title} failed, recived {returnVal.wrapType}.");
                    ret = false;
                }
            }
            return ret;
        }
    }

    public List<TestCategory> testCategories = new List<TestCategory>();

    // Start is called before the first frame update
    void Start()
    {
        //Compile("true");
        //Compile("false");
        //Compile("true;false");
        //Compile("\"hello jello\"");
        //Compile("'hello jello'");
        //Compile("0");
        //Compile("20");
        //Compile("20.55");
        //Compile("null");
        //Compile("var x = true");
        //Compile("var x = false");
        //Compile("var x = true; x == true;");
        //Compile("var x = false; x == false;");
        //Compile("var x = 10; x == 10;");
        //Compile("var x = 10; x == 20;");
        //Compile("var x = \"hello jello\"");
        //Compile("var x = 0");
        //Compile("var x = 20");
        //Compile("var x = 20.55");
        //Compile("5 == 5");
        //Compile("5 == 6");
        //Compile("5 != 5");
        //Compile("5 != 6");
        //Compile("5 > 6");
        //Compile("5 >= 6");
        //Compile("5 < 6");
        //Compile("5 <= 6");
        //Compile( "7 + 5");
        //Compile("7 + 5 * 3");
        //Compile("(7 + 5) * 3");
        //Compile("x = (7 + 5) * 3");
        //Compile("\"hello \" + \"jello\"");
        //Compile("x = 5;");
        //Compile("x = 5; x = 7");
        //Compile("x = 5; y = 8;");
        //Compile("x = 5; y = 8; x = 7;");
        //Compile("x = 5; x += 5;");
        //Compile("x = 5; x -= 5;");
        //Compile("x = 5; x *= 5;");
        //Compile("x = 5; x /= 5;");
        //Compile("\"hello jello\"[5]");
        //Compile("var x = 5; if(true){x = 6;}");
        //Compile("var x = 5; if(false){x = 6;}");
        //Compile("var x = 5; if(true)x = 6;");
        //Compile("var x = 5; if(false)x = 6;");
        //Compile("//Comment\nx=5");
        //Compile("var x /*Comment*/ = 5;");
        //Compile("var x = []");
        //Compile("var x; x = []");
        //Compile("x.y.z[5] = 4;");
        //Compile("x[5] = 4 + 5");
        //Compile("x[5][2 + 3] = 4 + 5");
        //Compile("x[5][2 + 3]().y = 4 + 5");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Val Compile(string code, bool log, LogSession logSession)
    {
        string jsCode = code;
        AntlrInputStream inputStream = new AntlrInputStream(jsCode);
        JavaScriptLexer lexer = new JavaScriptLexer(inputStream);
        CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
        JavaScriptParser parser = new JavaScriptParser(commonTokenStream);
        JavaScriptParser.ProgramContext programContext = parser.program();

        if(log)
        {
            string treeString = programContext.ToStringTree(parser);
            logSession.AppendLine(code);
            logSession.AppendLine(treeString);
        }

        Visitor visitor = new Visitor();
        Val valProgram = visitor.Visit(programContext);
        Debug.Assert(visitor.program != null);
        return valProgram;
    }
}

class Visitor : Antlr4.Runtime.Tree.AbstractParseTreeVisitor<Val>
{
    public enum EvalType
    { 
        RVal,
        LVal
    }

    public Stack<EvalType> eval = new Stack<EvalType>();

    public Stack<IParseDst> parseInsertion = new Stack<IParseDst>();
    public ScopeProgram program = null;

    public Visitor()
    { 
        eval.Push(EvalType.RVal);
    }


    public override Val Visit(IParseTree tree)
    {
        return tree.Accept(this);
    }

    public Val VisitVariableDeclarationList(IRuleNode node)
    {
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_variableStatement); //20
        Debug.Assert(node.ChildCount == 2);
        Debug.Assert(((IRuleNode)node.GetChild(1)).RuleContext.RuleIndex == JavaScriptParser.RULE_eos);

        IRuleNode varDeclList = (IRuleNode)node.GetChild(0);
        Debug.Assert(varDeclList.RuleContext.RuleIndex == JavaScriptParser.RULE_variableDeclarationList); //21
        Debug.Assert(varDeclList.ChildCount == 2); // Second child will be eos

        // The first one will be the var (or let)
        // The second will be the actual statement
        string mod = varDeclList.GetChild(0).GetText();
        Debug.Assert(mod == "var");
        AstSetFromName.Where where = AstSetFromName.Where.TopContext;

        IRuleNode varDecl = (IRuleNode)varDeclList.GetChild(1);
        Debug.Assert(varDecl.RuleContext.RuleIndex == JavaScriptParser.RULE_variableDeclaration); //22
        // The variable declaration rule is expected to have 3 children
        //  - Assignable
        //      - Variable name
        //  - = Sign
        //  - Expression
        string varEq = varDecl.GetChild(1).GetText();
        Debug.Assert(varEq == "=");
        IRuleNode rvalExpression = (IRuleNode)varDecl.GetChild(2);
        Debug.Assert(rvalExpression.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
        Val expr = GetASTAsmSingleExpression(rvalExpression);
        Val setFn = GetASTAsmVarSetter((IRuleNode)varDecl.GetChild(0), expr, where);
        return setFn;
    }

    public Val GetASTAsmVarSetter(IRuleNode node, Val setValue, AstSetFromName.Where where)
    {
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_assignable); //67
        Debug.Assert(node.ChildCount == 1);

        IRuleNode nodeIdentifier = (IRuleNode)node.GetChild(0);
        Debug.Assert(nodeIdentifier.RuleContext.RuleIndex == JavaScriptParser.RULE_identifier);
        Debug.Assert(nodeIdentifier.ChildCount == 1); // The token with the string variable name
        string varName = nodeIdentifier.GetChild(0).GetText();

        return new AstSetFromName(varName, true, setValue, where, Ctx.Rule.CannotExist);
    }

    public Val GetASTAsmSetter(IRuleNode node, Val setValue, AstSetFromName.Where ? where, Ctx.Rule ? rule)
    { 
        switch(node.ChildCount)
        { 
            case 1:
                { 
                    IRuleNode childNode = (IRuleNode)node.GetChild(0);
                    switch(childNode.RuleContext.RuleIndex)
                    {
                    case JavaScriptParser.RULE_identifier:
                            Debug.Assert(childNode.ChildCount == 1);
                            Debug.Assert(childNode.GetChild(0).TreeType == TreeType.End);

                            string idName = childNode.GetChild(0).GetText();
                            return new AstSetFromName(
                                idName, 
                                true, 
                                setValue, 
                                where ?? AstSetFromName.Where.TopContext, 
                                rule ?? Ctx.Rule.MustExist);
                    default:
                        throw new NotImplementedException();
                    }
                }
            case 2:
                break;
            case 3:
                if(node.GetChild(1).GetText() == "[")
                { 
                }
                else if(node.GetChild(1).GetText() == ".")
                { 
                    IRuleNode lNode = (IRuleNode)node.GetChild(0);
                    IRuleNode rNode = (IRuleNode)node.GetChild(2);
                    Debug.Assert(lNode.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                    Debug.Assert(rNode.RuleContext.RuleIndex == JavaScriptParser.RULE_identifierName);

                    Val lVal = GetASTAsmSingleExpression(lNode);
                    return new AstSetMember(lVal, rNode.GetText(), setValue);
                }
                break;
        }
        return null;
    }

    public Val GetASTAsmLiteral(IRuleNode node)
    {
        int n = node.ChildCount;
        Debug.Assert(node.ChildCount == 1);

        IParseTree literalChild = node.GetChild(0);
        if(literalChild.TreeType == TreeType.End)
        {
            ITerminalNode tnode = (ITerminalNode)literalChild;
            switch (tnode.Symbol.Type)
            {
                case JavaScriptParser.RegularExpressionLiteral:
                    break;

                case JavaScriptParser.StringLiteral:
                    string quotedString = tnode.Symbol.Text;
                    return Val.Make(quotedString.Substring(1, quotedString.Length - 2));

                case JavaScriptParser.NullLiteral:
                    return ValNone.Inst;

                case JavaScriptParser.BooleanLiteral:
                    return Val.Make(tnode.Symbol.Text == "true");

                case JavaScriptParser.DecimalLiteral:
                    if (int.TryParse(tnode.Symbol.Text, out int parsedInt))
                        return Val.Make(parsedInt);
                    if (float.TryParse(tnode.Symbol.Text, out float parsedFloat))
                        return Val.Make(parsedFloat);
                    throw new Exception("Unknown decimal literal pattern");

                case JavaScriptParser.HexIntegerLiteral:
                    break;
                case JavaScriptParser.OctalIntegerLiteral:
                    break;
                case JavaScriptParser.OctalIntegerLiteral2:
                    break;
                case JavaScriptParser.BinaryIntegerLiteral:
                    break;
                case JavaScriptParser.BigHexIntegerLiteral:
                    break;
                case JavaScriptParser.BigOctalIntegerLiteral:
                    break;
                case JavaScriptParser.BigBinaryIntegerLiteral:
                    break;
                case JavaScriptParser.BigDecimalIntegerLiteral:
                    break;
                case JavaScriptParser.Var:
                    break;
                default:
                    throw new NotImplementedException($"Failed to find implementation for symbol type {tnode.Symbol.Type}");

            }
        }

        Debug.Assert(literalChild.TreeType == TreeType.Rule);
        IRuleNode literalRule = (IRuleNode)literalChild;
        switch (literalRule.RuleContext.RuleIndex)
        {
            case JavaScriptParser.RULE_numericLiteral:
                Debug.Assert(n == 1);
                // No idea why this is handled for zeros (or if anything else handled it) instead
                // of as a leaf. For now we'll hope it's just for 0.
                string literalText = literalRule.GetText();
                if (int.TryParse(literalText, out int parsedInt))
                    return Val.Make(parsedInt);
                if(long.TryParse(literalText, out long parsedLong))
                    return Val.Make(parsedLong);
                if (float.TryParse(literalText, out float parsedFloat))
                    return Val.Make(parsedFloat);
                if(double.TryParse(literalText, out double parsedDouble))
                    return Val.Make(parsedDouble);
                throw new NotImplementedException($"Encountered unknown parsed numeric literal {literalRule.GetText()}");
            case JavaScriptParser.RULE_bigintLiteral:
                break;

        }
        return null;
    }

    public Val GetASTAsmExpressionSequence(IRuleNode node)
    {
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_expressionSequence);
        Debug.Assert(node.ChildCount == 1); // Second one will be the eos
        
        IRuleNode childNode = (IRuleNode)node.GetChild(0);
        Debug.Assert(childNode.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
        return GetASTAsmSingleExpression(childNode);
    }

    public Val GetASTAsmSingleExpression(IRuleNode node)
    { 
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);

        if (node.ChildCount == 1)
        {
            IRuleNode childNode = (IRuleNode)node.GetChild(0);

            switch (childNode.RuleContext.RuleIndex)
            {
                case JavaScriptParser.RULE_expressionStatement: // 24
                    return GetASTAsmExpressionStatement(childNode);
                case JavaScriptParser.RULE_expressionSequence:
                    return GetASTAsmExpressionSequence(childNode);
                case JavaScriptParser.RULE_arrayLiteral: // 57
                    return GetASTAsmArrayLiteral(childNode);
                case JavaScriptParser.RULE_singleExpression: // 65
                    return GetASTAsmSingleExpression(childNode);
                case JavaScriptParser.RULE_literal: // 73
                    return GetASTAsmLiteral(childNode);
                case JavaScriptParser.RULE_identifier: // 81
                    Debug.Assert(node.ChildCount == 1); // The token with the string variable name
                    string literalIdentifier = node.GetChild(0).GetText();
                    return new AstGetFromName(literalIdentifier);
                default:
                    throw new NotImplementedException();
            }
        }
        else if(node.ChildCount == 2)
        {
            IParseTree lSide = node.GetChild(0);
            IParseTree rSide = node.GetChild(1);
            if(lSide.TreeType == TreeType.End)
            {
                Debug.Assert(rSide.TreeType == TreeType.Rule);
                if (lSide.GetText() == "--") // --x
                {
                    IRuleNode rRule = (IRuleNode)rSide;
                    Debug.Assert(rRule.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                    Val getExpr = GetASTAsmSingleExpression(rRule);
                    AstMathSub subOp = new AstMathSub(getExpr, Val.Make(1));
                    Val setExpr = GetASTAsmSetter(rRule, subOp, AstSetFromName.Where.Existing, Ctx.Rule.MustExist);
                    return setExpr;
                }
                else if (lSide.GetText() == "++") // ++x
                {
                    IRuleNode rRule = (IRuleNode)rSide;
                    Debug.Assert(rRule.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                    Val getExpr = GetASTAsmSingleExpression(rRule);
                    AstMathAdd addOp = new AstMathAdd(getExpr, Val.Make(1));
                    Val setExpr = GetASTAsmSetter(rRule, addOp, AstSetFromName.Where.Existing, Ctx.Rule.MustExist);
                    return setExpr;
                }
                else
                    throw new NotImplementedException();
            }
            else
            { 
                if(rSide.TreeType == TreeType.Rule)
                { 
                    IRuleNode lRule = (IRuleNode)lSide;
                    IRuleNode rRule = (IRuleNode)rSide;
                    if(rRule.RuleContext.RuleIndex == JavaScriptParser.RULE_arguments)
                    { 
                        Debug.Assert(lRule.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                        Val getFunction = GetASTAsmSingleExpression(lRule);
                        AstCallFn retCall = new AstCallFn();
                        retCall.function = getFunction;

                        Debug.Assert(rRule.GetChild(0).GetText() == "(");
                        Debug.Assert(rRule.GetChild(rRule.ChildCount - 1).GetText() == ")");
                        for(int iArg = 1; iArg < rRule.ChildCount - 1; ++iArg)
                        { 
                            IParseTree argToken = rRule.GetChild(iArg);
                            if(argToken.TreeType == TreeType.End)
                            { 
                                Debug.Assert(argToken.GetText() == ",");
                                continue;
                            }

                            IRuleNode argRule = (IRuleNode)argToken;
                            Debug.Assert(argRule.RuleContext.RuleIndex == JavaScriptParser.RULE_argument);
                            Debug.Assert(argRule.ChildCount == 1);

                            IRuleNode argExpr = (IRuleNode)argRule.GetChild(0);
                            Debug.Assert(argExpr.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);

                            Val arg = GetASTAsmSingleExpression(argExpr);
                            retCall.args.Add(arg);
                        }

                        return retCall;
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (rSide.GetText() == "--") // x--
                    {
                        IRuleNode lRule = (IRuleNode)lSide;
                        Debug.Assert(lRule.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                        Val getExpr = GetASTAsmSingleExpression(lRule);
                        AstStackPush pushVal = new AstStackPush(getExpr);
                        AstMathSub subOp = new AstMathSub(pushVal, Val.Make(1));
                        Val setExpr = GetASTAsmSetter(lRule, subOp, AstSetFromName.Where.Existing, Ctx.Rule.MustExist);
                        AstStackPop popVal = new AstStackPop(setExpr);
                        return popVal;
                    }
                    else if (rSide.GetText() == "++") // x++
                    {
                        IRuleNode lRule = (IRuleNode)lSide;
                        Debug.Assert(lRule.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                        Val getExpr = GetASTAsmSingleExpression(lRule);
                        AstStackPush pushVal = new AstStackPush(getExpr);
                        AstMathAdd addOp = new AstMathAdd(pushVal, Val.Make(1));
                        Val setExpr = GetASTAsmSetter(lRule, addOp, AstSetFromName.Where.Existing, Ctx.Rule.MustExist);
                        AstStackPop popVal = new AstStackPop(setExpr);
                        return popVal;
                    }
                    else
                        throw new NotImplementedException();
                }
            }
        }
        if (node.ChildCount == 3)
        {
            if (node.GetChild(0).TreeType == TreeType.End)
            {
                IParseTree childOpen = node.GetChild(0);
                IParseTree childClose = node.GetChild(2);
                Debug.Assert(childOpen.GetText() == "(");
                Debug.Assert(childClose.GetText() == ")");

                IRuleNode innerChild = (IRuleNode)node.GetChild(1);
                Debug.Assert(innerChild.RuleContext.RuleIndex == JavaScriptParser.RULE_expressionSequence);
                return GetASTAsmExpressionSequence(innerChild);
            }
            else
            {
                IRuleNode nodeLeft = (IRuleNode)node.GetChild(0);
                IRuleNode nodeRight = (IRuleNode)node.GetChild(2);
                string op = node.GetChild(1).GetText();


                if(op == ".")
                { 
                    Debug.Assert(nodeLeft.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                    Debug.Assert(nodeRight.RuleContext.RuleIndex == JavaScriptParser.RULE_identifierName);
                    Val obj = GetASTAsmSingleExpression(nodeLeft);
                    return new AstGetMember(obj, nodeRight.GetText());
                }

                Val rval = GetASTAsmSingleExpression(nodeRight);
                Debug.Assert(nodeRight.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                if (op == "=")
                {
                    eval.Push(EvalType.LVal);
                    Val setter = GetASTAsmSetter((IRuleNode)node.GetChild(0), rval, null, null);
                    eval.Pop();
                    return setter;
                }
                if (op == "+=")
                {
                }
                if (op == "-=")
                {
                }
                if (op == "*=")
                {
                }
                if (op == "/=")
                {
                }
                if (op == "==")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstCompareEq(lval, rval);
                }
                if (op == "!=")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstCompareNotEq(lval, rval);
                }
                if (op == "<")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstCompareLs(lval, rval);
                }
                if (op == ">")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstCompareGr(lval, rval);
                }
                if (op == "<=")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstCompareLsEq(lval, rval);
                }
                if (op == ">=")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstCompareGrEq(lval, rval);
                }
                if (op == "+")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstMathAdd(lval, rval);
                }
                if (op == "-")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstMathSub(lval, rval);
                }
                if (op == "*")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstMathMul(lval, rval);
                }
                if (op == "/")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstMathDiv(lval, rval);
                }
                if (op == "%")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstMathMod(lval, rval);
                }
                if (op == "**")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstMathPow(lval, rval);
                }
                if( op == "&&")
                { 
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstBoolAnd(lval, rval);
                }
                if( op == "||")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstBoolOr(lval, rval);
                }
                if( op == "|")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstBitOr(lval, rval);
                }
                if( op == "&")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstBitAnd(lval, rval);
                }
                if( op == "^")
                {
                    Val lval = GetASTAsmSingleExpression(nodeLeft);
                    return new AstBitAnd(lval, rval);
                }
                throw new NotImplementedException("Unknown binary operator");
            }

            throw new NotImplementedException("Unknown 3 child combination");

            //lvar = node.GetChild(0).Accept(this);
        }
        else if(node.ChildCount == 4)
        {
            IParseTree arrayExpression = node.GetChild(0);
            IParseTree openBracket = node.GetChild(1);
            IParseTree indexingExpression = node.GetChild(2);
            IParseTree closeBracket = node.GetChild(3);

            Debug.Assert(openBracket.TreeType == TreeType.End);
            Debug.Assert(openBracket.GetText() == "[");
            Debug.Assert(closeBracket.TreeType == TreeType.End);
            Debug.Assert(closeBracket.GetText() == "]");

            IRuleNode exprRule = (IRuleNode)arrayExpression;
            Debug.Assert(exprRule.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
            Val vArrayExpr = GetASTAsmSingleExpression(exprRule);

            IRuleNode indexRule = (IRuleNode)indexingExpression;
            Debug.Assert(indexRule.RuleContext.RuleIndex == JavaScriptParser.RULE_expressionSequence);
            Val vIndexExpr = GetASTAsmExpressionSequence(indexRule);

            return new AstGetIndex(vArrayExpr, vIndexExpr);
        }
        
        throw new NotImplementedException("Unknown child count");

        //IRuleNode childNode = (IRuleNode)node.GetChild(0);
        //switch(childNode.RuleContext.RuleIndex)
        //{ 
        //    case JavaScriptParser.RULE_literal:
        //        return GetASTAsmLiteral(childNode);
        //
        //    case JavaScriptParser.RULE_singleExpression:
        //        return GetASTAsmSingleExpression(childNode);
        //}
        //return null;
    }

    public Val GetASTAsmArrayLiteral(IRuleNode node)
    { 
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_arrayLiteral);
        Debug.Assert(node.ChildCount == 3);
        Debug.Assert(node.GetChild(0).GetText() == "[");
        Debug.Assert(node.GetChild(2).GetText() == "]");

        AstCreateList ret = new AstCreateList();

        IRuleNode innerContents = (IRuleNode)node.GetChild(1);
        int nContentsChildren = innerContents.ChildCount;
        switch(innerContents.RuleContext.RuleIndex)
        { 
            case JavaScriptParser.RULE_elementList:
                for(int i = 0; i < nContentsChildren; ++i)
                { 
                    IParseTree tokenNode = innerContents.GetChild(i);

                    if(tokenNode.TreeType == TreeType.End)
                    { 
                        Debug.Assert(tokenNode.GetText() == ",");
                        continue;
                    }

                    IRuleNode arrayEleNode = (IRuleNode)tokenNode;
                    Debug.Assert(arrayEleNode.RuleContext.RuleIndex == JavaScriptParser.RULE_arrayElement);
                    Debug.Assert(arrayEleNode.ChildCount == 1);
                    IRuleNode arrayExpr = (IRuleNode)arrayEleNode.GetChild(0);
                    Debug.Assert(arrayExpr.RuleContext.RuleIndex == JavaScriptParser.RULE_singleExpression);
                    Val vEle = GetASTAsmSingleExpression(arrayExpr);
                    ret.entryInitializers.Add(vEle);
                }
                break;
        }

        return ret;
    }

    public Val GetASTAsmProgram(IRuleNode node)
    {
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_program);
        Debug.Assert(this.parseInsertion.Count == 0);
        Debug.Assert(node.ChildCount == 2); // Second one should be an <EOF>
        Debug.Assert(this.program == null);

        this.program = new PxPre.Datum.ScopeProgram();
        this.parseInsertion.Push(new ParseDstUtil((x) => { this.program.instrs.Add(x); }));

        IRuleNode childNode = (IRuleNode)node.GetChild(0);
        Debug.Assert(childNode.RuleContext.RuleIndex == JavaScriptParser.RULE_sourceElements);
        GetASTAsmSourceElements(childNode);
        this.parseInsertion.Pop();
        return this.program;
    }

    public Val GetASTAsmSourceElements(IRuleNode node)
    {
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_sourceElements);
        int n = node.ChildCount;
        Val lastEle = null;
        for(int i = 0; i < n; ++i)
        { 
            IRuleNode childNode = (IRuleNode)node.GetChild(i);
            Debug.Assert(childNode.RuleContext.RuleIndex == JavaScriptParser.RULE_sourceElement);
            lastEle = GetASTAsmSourceElement(childNode);
        }
        return lastEle;
    }

    public Val GetASTAsmSourceElement(IRuleNode node)
    {
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_sourceElement);
        Debug.Assert(node.ChildCount == 1);
        
        IRuleNode childNode = (IRuleNode)node.GetChild(0);
        switch(childNode.RuleContext.RuleIndex)
        { 
            case JavaScriptParser.RULE_statement:
                return GetASTAsmStatement(childNode);
        }
        return null;
    }

    public Val GetASTAsmStatement(IRuleNode node)
    {
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_statement);
        Debug.Assert(node.ChildCount == 1);

        IRuleNode childNode = (IRuleNode)node.GetChild(0);
        Val statement = null;
        switch(childNode.RuleContext.RuleIndex)
        { 
            case JavaScriptParser.RULE_variableStatement: //20
                statement = VisitVariableDeclarationList(childNode);
                break;
            case JavaScriptParser.RULE_expressionStatement: // 24
                statement = GetASTAsmExpressionStatement(childNode);
                break;
            case JavaScriptParser.RULE_returnStatement:
                { 
                    Debug.Assert(childNode.ChildCount == 3);
                    Debug.Assert(childNode.GetChild(0).GetText() == "return");
                    Debug.Assert(childNode.GetChild(2).GetText() == ";");

                    IRuleNode ruleRetExpr = (IRuleNode)childNode.GetChild(1);
                    Debug.Assert(ruleRetExpr.RuleContext.RuleIndex == JavaScriptParser.RULE_expressionSequence);
                    statement = new AstReturn(GetASTAsmExpressionSequence(ruleRetExpr));
                }
                break;
            case JavaScriptParser.RULE_functionDeclaration: //44
                statement = GetASTAsmFunctionDeclaration(childNode);
                break;
        }

        Debug.Assert(statement != null);
        parseInsertion.Peek().Insert(statement);
        return statement;
    }

    public Val GetASTAsmExpressionStatement(IRuleNode node)
    {
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_expressionStatement);
        Debug.Assert(node.ChildCount == 2);

        IRuleNode terminatorNode = (IRuleNode)node.GetChild(1);
        Debug.Assert(terminatorNode.RuleContext.RuleIndex == JavaScriptParser.RULE_eos);

        IRuleNode childNode = (IRuleNode)node.GetChild(0);
        Debug.Assert(childNode.RuleContext.RuleIndex == JavaScriptParser.RULE_expressionSequence);
        return GetASTAsmExpressionSequence(childNode);
    }

    public Val GetASTAsmFunctionDeclaration(IRuleNode node)
    { 
        Debug.Assert(node.RuleContext.RuleIndex == JavaScriptParser.RULE_functionDeclaration);
        Debug.Assert(node.GetChild(0).TreeType == TreeType.End);
        Debug.Assert(node.GetChild(0).GetText() == "function");
        Debug.Assert(node.GetChild(2).GetText() == "(");

        ScopeFunction functionDef = new ScopeFunction();
        if (node.GetChild(3).TreeType == TreeType.Rule)
        {
            // We have argument declarations, gather there names so we know what to name them
            // during execution.
            Debug.Assert(node.ChildCount == 6);
            Debug.Assert(node.GetChild(4).GetText() == ")");

            IRuleNode paramListing = (IRuleNode)node.GetChild(3);
            Debug.Assert(paramListing.RuleContext.RuleIndex == JavaScriptParser.RULE_formalParameterList);
            for(int i = 0; i < paramListing.ChildCount; ++i)
            {
                IParseTree paramArg = paramListing.GetChild(i);
                if(paramArg.TreeType == TreeType.End)
                {
                    Debug.Assert(paramArg.GetText() == ",");
                    continue;
                }
                IRuleNode ruleArg = (IRuleNode)paramArg;
                Debug.Assert(ruleArg.RuleContext.RuleIndex == JavaScriptParser.RULE_formalParameterArg);
                Debug.Assert(ruleArg.ChildCount == 1);
                IRuleNode ruleArgAssignable = (IRuleNode)ruleArg.GetChild(0);
                Debug.Assert(ruleArgAssignable.RuleContext.RuleIndex == JavaScriptParser.RULE_assignable);
                Debug.Assert(ruleArgAssignable.ChildCount == 1);
                IRuleNode ruleArgId = (IRuleNode)ruleArgAssignable.GetChild(0);
                Debug.Assert(ruleArgId.RuleContext.RuleIndex == JavaScriptParser.RULE_identifier);
                functionDef.argumentNameRemaps.Add(ruleArgId.GetText());
            }
        }
        else
        {
            Debug.Assert(node.ChildCount == 5);
            Debug.Assert(node.GetChild(3).GetText() == ")");
        }

        IRuleNode ruleFName = (IRuleNode)node.GetChild(1);
        Debug.Assert(ruleFName.RuleContext.RuleIndex == JavaScriptParser.RULE_identifier);
        string functionName = ruleFName.GetText();

        IRuleNode ruleBody = (IRuleNode)node.GetChild(node.ChildCount - 1);
        Debug.Assert(ruleBody.RuleContext.RuleIndex == JavaScriptParser.RULE_functionBody);
        Debug.Assert(ruleBody.ChildCount == 3);
        Debug.Assert(ruleBody.GetChild(0).GetText() == "{");
        Debug.Assert(ruleBody.GetChild(2).GetText() == "}");

        this.parseInsertion.Push(new ParseDstUtil((x) => { functionDef.instrs.Add(x); }));
        IRuleNode functionSource = (IRuleNode)ruleBody.GetChild(1);
        Debug.Assert(functionSource.RuleContext.RuleIndex == JavaScriptParser.RULE_sourceElements);
        GetASTAsmSourceElements(functionSource);
        this.parseInsertion.Pop();

        // Set the function as a variable to make it accessible
        return new AstSetFromName(functionName, false, functionDef, AstSetFromName.Where.TopContext, Ctx.Rule.None);
    }

    public override Val VisitChildren(IRuleNode node)
    {
        Val result = DefaultResult;
        int n = node.ChildCount;

        int nodeRuleIndex = node.RuleContext.RuleIndex;
        switch (nodeRuleIndex)
        {
            case JavaScriptParser.RULE_program:
                return GetASTAsmProgram(node);                

            case JavaScriptParser.RULE_block:
		    case JavaScriptParser.RULE_statementList:
            case JavaScriptParser.RULE_importStatement:
            case JavaScriptParser.RULE_importFromBlock:
		    case JavaScriptParser.RULE_importModuleItems:
            case JavaScriptParser.RULE_importAliasName:
            case JavaScriptParser.RULE_moduleExportName:
		    case JavaScriptParser.RULE_importedBinding:
            case JavaScriptParser.RULE_importDefault:
            case JavaScriptParser.RULE_importNamespace:
		    case JavaScriptParser.RULE_importFrom:
            case JavaScriptParser.RULE_aliasName:
            case JavaScriptParser.RULE_exportStatement:
		    case JavaScriptParser.RULE_exportFromBlock:
            case JavaScriptParser.RULE_exportModuleItems:
            case JavaScriptParser.RULE_exportAliasName:
		    case JavaScriptParser.RULE_declaration:
                break;
                
            case JavaScriptParser.RULE_emptyStatement_:
            case JavaScriptParser.RULE_expressionStatement:
            case JavaScriptParser.RULE_ifStatement:
            case JavaScriptParser.RULE_iterationStatement:
            case JavaScriptParser.RULE_varModifier:
		    case JavaScriptParser.RULE_continueStatement:
            case JavaScriptParser.RULE_breakStatement:
            case JavaScriptParser.RULE_returnStatement:
		    case JavaScriptParser.RULE_yieldStatement:
            case JavaScriptParser.RULE_withStatement:
            case JavaScriptParser.RULE_switchStatement:
		    case JavaScriptParser.RULE_caseBlock:
            case JavaScriptParser.RULE_caseClauses:
            case JavaScriptParser.RULE_caseClause:
            case JavaScriptParser.RULE_defaultClause:
		    case JavaScriptParser.RULE_labelledStatement:
            case JavaScriptParser.RULE_throwStatement:
            case JavaScriptParser.RULE_tryStatement:
		    case JavaScriptParser.RULE_catchProduction:
            case JavaScriptParser.RULE_finallyProduction:
            case JavaScriptParser.RULE_debuggerStatement:
		    case JavaScriptParser.RULE_functionDeclaration:
            case JavaScriptParser.RULE_classDeclaration:
            case JavaScriptParser.RULE_classTail:
		    case JavaScriptParser.RULE_classElement:
            case JavaScriptParser.RULE_methodDefinition:
            case JavaScriptParser.RULE_fieldDefinition:
		    case JavaScriptParser.RULE_classElementName:
            case JavaScriptParser.RULE_privateIdentifier:
            case JavaScriptParser.RULE_formalParameterList:
		    case JavaScriptParser.RULE_formalParameterArg:
            case JavaScriptParser.RULE_lastFormalParameterArg:
            case JavaScriptParser.RULE_functionBody:
		    case JavaScriptParser.RULE_sourceElements:
            case JavaScriptParser.RULE_arrayLiteral:
            case JavaScriptParser.RULE_elementList:
		    case JavaScriptParser.RULE_arrayElement:
            case JavaScriptParser.RULE_propertyAssignment:
            case JavaScriptParser.RULE_propertyName:
		    case JavaScriptParser.RULE_arguments:
            case JavaScriptParser.RULE_argument:
            case JavaScriptParser.RULE_initializer:
            case JavaScriptParser.RULE_assignable:
		    case JavaScriptParser.RULE_objectLiteral:
            case JavaScriptParser.RULE_anonymousFunction:
            case JavaScriptParser.RULE_arrowFunctionParameters:
		    case JavaScriptParser.RULE_arrowFunctionBody:
            case JavaScriptParser.RULE_assignmentOperator:
            case JavaScriptParser.RULE_literal:
            case JavaScriptParser.RULE_templateStringLiteral:
            case JavaScriptParser.RULE_templateStringAtom:
            case JavaScriptParser.RULE_getter:
            case JavaScriptParser.RULE_setter:
            case JavaScriptParser.RULE_identifierName: // 80
                break;

		    

            case JavaScriptParser.RULE_reservedWord:
            case JavaScriptParser.RULE_keyword:
            case JavaScriptParser.RULE_let_:
                break;

		    case JavaScriptParser.RULE_eos:
                return null;
        }
        throw new NotImplementedException($"Rule index {node.RuleContext.RuleIndex} not implemented.");
    }
    
    public override Val VisitErrorNode(IErrorNode node)
    {
        return DefaultResult;
    }


    protected internal override Val AggregateResult(Val aggregate, Val nextResult)
    {
        return nextResult;
    }

    protected internal override bool ShouldVisitNextChild(IRuleNode node, Val currentResult)
    {
        return true;
    }
}