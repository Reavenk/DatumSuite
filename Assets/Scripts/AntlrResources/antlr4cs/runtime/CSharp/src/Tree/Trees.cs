/* Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    /// <summary>A set of utility routines useful for all kinds of ANTLR trees.</summary>
    /// <remarks>A set of utility routines useful for all kinds of ANTLR trees.</remarks>
    public class Trees
    {
        public const int DefaultStringIndent = 4;
        /// <summary>Print out a whole tree in LISP form.</summary>
        /// <remarks>
        /// Print out a whole tree in LISP form.
        /// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)"/>
        /// is used on the
        /// node payloads to get the text for the nodes.  Detect
        /// parse trees and extract data appropriately.
        /// </remarks>
        public static string ToStringTree(ITree t)
        {
            return ToStringTree(t, (IList<string>)null, DefaultStringIndent, 0);
        }

        /// <summary>Print out a whole tree in LISP form.</summary>
        /// <remarks>
        /// Print out a whole tree in LISP form.
        /// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)"/>
        /// is used on the
        /// node payloads to get the text for the nodes.  Detect
        /// parse trees and extract data appropriately.
        /// </remarks>
        public static string ToStringTree(ITree t, Parser recog)
        {
            string[] ruleNames = recog != null ? recog.RuleNames : null;
            IList<string> ruleNamesList = ruleNames != null ? Arrays.AsList(ruleNames) : null;
            return ToStringTree(t, ruleNamesList, DefaultStringIndent, 0);
        }

        /// <summary>Print out a whole tree in LISP form.</summary>
        /// <remarks>
        /// Print out a whole tree in LISP form.
        /// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)"/>
        /// is used on the
        /// node payloads to get the text for the nodes.  Detect
        /// parse trees and extract data appropriately.
        /// </remarks>
        public static string ToStringTree(ITree t, IList<string> ruleNames, int indent, int totalIndent)
        {
            string s = Utils.EscapeWhitespace(GetNodeText(t, ruleNames), false);
            if (t.ChildCount == 0)
            {
                if(t is ITerminalNode terminal)
                {
                    return GetOptionalIndentedString($"{terminal.GetText()} <T {terminal.Symbol.Type}>", indent != 0, totalIndent); 
                }
                else
                {
                    return GetOptionalIndentedString("s<>", indent != 0, totalIndent);
                }
            }
            int ruleIdx = ((RuleContext)t).RuleIndex;
            StringBuilder buf = new StringBuilder();
            buf.Append(GetOptionalIndentedString("(", indent != 0, totalIndent));
            s = Utils.EscapeWhitespace(GetNodeText(t, ruleNames), false);
            s = $"{s}<R {ruleIdx}>";
            buf.Append(s);
            buf.Append(' ');
            for (int i = 0; i < t.ChildCount; i++)
            {
                if (i > 0)
                {
                    buf.Append(' ');
                }
                string toAppend = ToStringTree(t.GetChild(i), ruleNames, indent, totalIndent + indent);
                buf.Append(toAppend);
            }
            buf.Append(GetOptionalIndentedString(")", indent != 0, totalIndent, true));
            return buf.ToString();
        }

        public static string GetOptionalIndentedString(string toFormat, bool shouldIndent, int indentions, bool forceNewline = false)
        { 
            if(shouldIndent == false)
                return toFormat;

            if (indentions == 0)
            {
                if(forceNewline)
                    return "\n" + toFormat;
                else
                    return toFormat;
            }

            return "\n" + new string(' ', indentions) + toFormat;
        }

        public static string GetNodeText(ITree t, Parser recog)
        {
            string[] ruleNames = recog != null ? recog.RuleNames : null;
            IList<string> ruleNamesList = ruleNames != null ? Arrays.AsList(ruleNames) : null;
            return GetNodeText(t, ruleNamesList);
        }

        public static string GetNodeText(ITree t, IList<string> ruleNames)
        {
            if (ruleNames != null)
            {
                if (t is RuleContext)
                {
                    int ruleIndex = ((RuleContext)t).RuleIndex;
                    string ruleName = ruleNames[ruleIndex];
					int altNumber = ((RuleContext)t).getAltNumber();
					if ( altNumber!=Atn.ATN.INVALID_ALT_NUMBER ) {
						return ruleName+":"+altNumber;
					}
                    return ruleName;
                }
                else
                {
                    if (t is IErrorNode)
                    {
                        return t.ToString();
                    }
                    else
                    {
                        if (t is ITerminalNode)
                        {
                            IToken symbol = ((ITerminalNode)t).Symbol;
                            if (symbol != null)
                            {
                                string s = symbol.Text;
                                return s;
                            }
                        }
                    }
                }
            }
            // no recog for rule names
            object payload = t.Payload;
            if (payload is IToken)
            {
                return ((IToken)payload).Text;
            }
            return t.Payload.ToString();
        }

        /// <summary>Return ordered list of all children of this node</summary>
        public static IList<ITree> GetChildren(ITree t)
        {
            IList<ITree> kids = new List<ITree>();
            for (int i = 0; i < t.ChildCount; i++)
            {
                kids.Add(t.GetChild(i));
            }
            return kids;
        }

        /// <summary>Return a list of all ancestors of this node.</summary>
        /// <remarks>
        /// Return a list of all ancestors of this node.  The first node of
        /// list is the root and the last is the parent of this node.
        /// </remarks>
        [return: NotNull]
        public static IList<ITree> GetAncestors(ITree t)
        {
            if (t.Parent == null)
            {
                return Collections.EmptyList<ITree>();
            }
            IList<ITree> ancestors = new List<ITree>();
            t = t.Parent;
            while (t != null)
            {
                ancestors.Insert(0, t);
                // insert at start
                t = t.Parent;
            }
            return ancestors;
        }

        public static ICollection<IParseTree> FindAllTokenNodes(IParseTree t, int ttype)
        {
            return FindAllNodes(t, ttype, true);
        }

        public static ICollection<IParseTree> FindAllRuleNodes(IParseTree t, int ruleIndex)
        {
            return FindAllNodes(t, ruleIndex, false);
        }

        public static IList<IParseTree> FindAllNodes(IParseTree t, int index, bool findTokens)
        {
            IList<IParseTree> nodes = new List<IParseTree>();
            _findAllNodes(t, index, findTokens, nodes);
            return nodes;
        }

        private static void _findAllNodes(IParseTree t, int index, bool findTokens, IList<IParseTree> nodes)
        {
            // check this node (the root) first
            if (findTokens && t is ITerminalNode)
            {
                ITerminalNode tnode = (ITerminalNode)t;
                if (tnode.Symbol.Type == index)
                {
                    nodes.Add(t);
                }
            }
            else
            {
                if (!findTokens && t is ParserRuleContext)
                {
                    ParserRuleContext ctx = (ParserRuleContext)t;
                    if (ctx.RuleIndex == index)
                    {
                        nodes.Add(t);
                    }
                }
            }
            // check children
            for (int i = 0; i < t.ChildCount; i++)
            {
                _findAllNodes(t.GetChild(i), index, findTokens, nodes);
            }
        }

        public static IList<IParseTree> Descendants(IParseTree t)
        {
            List<IParseTree> nodes = new List<IParseTree>();
            nodes.Add(t);
            int n = t.ChildCount;
            for (int i = 0; i < n; i++)
            {
                nodes.AddRange(Descendants(t.GetChild(i)));
            }
            return nodes;
        }

        private Trees()
        {
        }
    }
}
