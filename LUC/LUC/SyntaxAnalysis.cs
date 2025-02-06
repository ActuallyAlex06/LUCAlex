using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Formats.Asn1;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LUC
{
    public class SyntaxAnalysis
    {
        public void Analyse()
        {
            Dictionary<int, List<string>> tokens = Lexer.tokens;
            GoThroughTokens(tokens);
        }

        Stack<string> operators = new Stack<string> { };
        List<TreeNode<string>> trees = new List<TreeNode<string>>();
        string lookahead;
        List<string> lsttokens = new List<string> { };
        TreeNode<string> start = new TreeNode<string>("Start");

        private void GoThroughTokens(Dictionary<int, List<string>> tokens)
        {
            foreach (List<string> tokensline in tokens.Values)
            {
                foreach (string token in tokensline)
                {
                    lsttokens.Add(token);
                }
            }

            trees.Add(start);
            lsttokens.Add("$");
            lookahead = LookAhead(1);
            StartDescent(start);

            Console.WriteLine();
            ReadTree();
        }

        private void StartDescent(TreeNode<string> leaf)
        {
            Console.WriteLine("LookAhead: " + lookahead);

            switch (lookahead)
            {
                case "k, function":
                case "k, func":
                case "k, f":

                    lookahead = "k, function";
                    SubFuncCreate(leaf, true);

                    break;

                case "i":

                    SubMethodCall(leaf);
                    StartDescent(leaf);

                    break;

                case "$":

                    Console.WriteLine("Exit");

                    return;
            }
        }

        #region Functions
        private void SubFuncCreate(TreeNode<string> leaf, bool newlayer)
        {
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "CREFUNC");
            Skip("k, function");

            switch (lookahead)
            {
                case "k, int":
                case "k, bool":
                case "k, string":

                    SubName(node);

                    break;

                case "i":

                    Match("i", node);

                    break;

            }

            SubParam(node, true);
        }
        #endregion

        #region Parameter Logic
        private void SubParam(TreeNode<string> leaf, bool newlayer)
        {
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "PARAM");

            switch (lookahead)
            {
                case "s, (":

                    Skip("s, (");
                    SubMoreParam(node);

                    break;

                default: Console.WriteLine("Error"); break;
            }
        }

        private void SubMoreParam(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "k, int":
                case "k, bool":
                case "k, string":

                    SubName(node);

                    break;

                case "s, )":

                    Skip("s, )");
                    SubBody(node, true);

                    break;
                    
            }

            SubAdditionalParam(node);
        }

        private void SubAdditionalParam(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "s, ,":

                    Skip("s, ,");
                    SubMoreParam(node);

                    break;

                case "s, )":

                    Skip("s, )");
                    SubBody(node, true);

                    break;
            }
        }
        #endregion

        #region Method Call
        private void SubMethodCall(TreeNode<string> leaf)
        {
            TreeNode<string> node = NewLayerResult(leaf, true, "CALL");
            Match("i", node);
            Skip("s, (");
            SubCallParameters(node);
            Skip("s, )");
            Skip("s, ;");
        }

        private void SubCallParameters(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "n":

                    TreeNode<string> expnode = NewLayerResult(node, true, "EXP");
                    Match(lookahead, expnode);
                    SubExpHandle(expnode);
                    SubEndCallParameters(node);

                    break;

                case "s, (":

                    TreeNode<string> brexpnode = NewLayerResult(node, true, "EXP");
                    SpecialBracketsCheck(brexpnode);
                    SubEndCallParameters(node);

                    break;

                case "i":

                    SubIdent(node);
                    SubEndCallParameters(node);

                    break;

                case "l":

                    TreeNode<string> stringnode = NewLayerResult(node, true, "STRING");
                    Match("l", stringnode);
                    SubStringHandle(stringnode);
                    SubEndCallParameters(node);

                    break;

                case "k, true":
                case "k, false":

                    Match(lookahead, node);
                    SubEndCallParameters(node);

                    break;
            }
        }

        private void SubEndCallParameters(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "s, )": return;

                case "s, ,":

                    Skip("s, ,");
                    SubCallParameters(node);

                    break;
            }
        }

        private void SubName(TreeNode<string> leaf)
        {
            TreeNode<string> node = NewLayerResult(leaf, true, "NAME");
            Match(lookahead, node);
            Match("i", node);
        } 
        #endregion

        #region String handling

        private void SubStringHandle(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "o, &":

                    Skip("o, &");
                    SubStringExt(node);

                    break;
            }
        }

        private void SubStringExt(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "n":
                case "l":

                    Match(lookahead, node);
                    SubStringHandle(node);

                    break;

                case "i":

                    SubIdent(node);

                    break;

                default: Console.WriteLine("What"); break;
            }
        }

        #endregion

        #region Expressions

        private void SubExpHandle(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "o, +":
                case "o, *":
                case "o, /":
                case "o, -":
                case "o, %":

                    Match(lookahead, node);
                    SubExpExtend(node);

                    break;
            }
        }

        private void SubExpExtend(TreeNode<string> leaf)
        {
            TreeNode<string> node = NewLayerResult(leaf, true, "EXP");

            switch (lookahead)
            {
                case "n":

                    Match(lookahead, node);
                    SubExpHandle(node);

                    break;

                case "i":

                    SubIdent(node);

                    break;

                case "s, (":

                    SpecialBracketsCheck(node);

                    break;

                default: Console.WriteLine("What"); break;
            }
        }

        private void SpecialBracketsCheck(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "n":

                    Match(lookahead, node);
                    FrontBracketFix(node);

                    break;

                case "i":

                    SubIdent(node);

                    break;

                case "s, (":

                    Match("s, (", node);
                    SpecialBracketsCheck(node);
                    Match("s, )", node);

                    break;

                default: Console.WriteLine("What"); break;
            }
        }

        private void FrontBracketFix(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "o, +":
                case "o, *":
                case "o, /":
                case "o, %":
                case "o, -":

                    Match(lookahead, node);
                    SubExpExtend(node);

                    break;

                default: Console.WriteLine("What"); break;
            }
        }
        #endregion

        #region Body
        private void SubBody(TreeNode<string> leaf, bool newlayer)
        {
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "BODY");

            switch (lookahead)
            {
                case "s, {":

                    Match("s, {", node);
                    SubBodyMain(node);

                    break;
            }
        }

        private void SubBodyMain(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "k, while":

                    TreeNode<string> whilenode = NewLayerResult(node, true, "WHILE");
                    Match("k, while", whilenode);
                    SubCond(whilenode, true);
                    SubBody(whilenode, true);
                    SubBodyMain(node);

                    return;

                case "k, if":

                    TreeNode<string> ifnode = NewLayerResult(node, true, "IF");
                    Match("k, if", ifnode);
                    SubCond(ifnode, true);
                    SubBody(ifnode, true);
                    SubBodyMain(node);

                    return;

                case "k, elif":

                    Console.WriteLine("AAA");
                    TreeNode<string> elifnode = NewLayerResult(node, true, "ELIF");
                    Match("k, elif", elifnode);
                    SubCond(elifnode, true);
                    SubBody(elifnode, true);
                    SubBodyMain(node);

                    return;

                case "k, else":

                    Console.WriteLine("AAA");
                    TreeNode<string> elsenode = NewLayerResult(node, true, "ELSE");
                    Match("k, else", elsenode);
                    SubBody(elsenode, true);
                    SubBodyMain(node);

                    return;

                case "k, for":

                    TreeNode<string> fornode = NewLayerResult(node, true, "FOR");
                    Match("k, for", fornode);
                    SubFor(fornode);
                    SubBody(fornode, true);
                    SubBodyMain(node);

                    return;

                case "k, int":
                case "k, string":
                case "k, bool":

                    TreeNode<string> varnode = NewLayerResult(node, true, "VAR");
                    SubName(varnode);
                    SubVal(varnode);
                    SubBodyMain(node);

                    break;

                case "i":

                    SubIdentifierKind(node);
                    SubBodyMain(node);

                    return;

                case "k, return":
                case "k, r":
                    lookahead = "k, return";

                    TreeNode<string> returnnode = NewLayerResult(node, true, "RET");
                    SubReturn(returnnode);
                    SubBodyMain(node);

                    break;

                case "o, ?":

                    TreeNode<string> errornode = NewLayerResult(node, true, "ERR");
                    SubErrorHandling(errornode);
                    SubBodyMain(node);

                    break;

                case "s, }":

                    Match("s, }", node);
                    StartDescent(start);

                    break;
            }
        }
        #endregion

        private void SubReturn(TreeNode<string> node)
        {
            Console.WriteLine("AAA");
            Match("k, return", node);
            SubValReal(node);
            Skip("s, ;");
        }

        private void SubIdentifierKind(TreeNode<string> leaf)
        {
            string next = LookAhead(2);
            next = next.Remove(0, 1);

            switch(next)
            {
                case "o, :=":
                case "o, =":
                    
                    SubAssign(leaf);

                    break;

                case "s, [":

                    SubListDef(leaf);

                    break;

                case "s, (":

                    SubIdent(leaf);
                    Skip("s, ;");

                    break;
            }
        }

        private void SubListElements(TreeNode<string> leaf)
        {
            Skip("s, [");
            TreeNode<string> elnode = NewLayerResult(leaf, true, "ITEM");
            SubElement(elnode);
            Skip("s, ]");
        }

        private void SubElement(TreeNode<string> node)
        {
            switch(lookahead)
            {
                case "n":
                case "i":
                case "l":
                case "s, (":
                case "k, true":
                case "k, false":

                    SubValReal(node);
                    SubMoreItems(node);

                    break;
            }
        }

        private void SubMoreItems(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "s, ]":
                    break;

                case "s, ,":

                    Skip("s, ,");
                    SubElement(node);

                    break;
            }
        }

        private void SubListDef(TreeNode<string> leaf)
        {
            string next = LookAhead(3);
            next = next.Remove(0, 5);

            switch (next)
            {
                case "k, int":
                case "k, bool":
                case "k, string":

                    TreeNode<string> listnode = NewLayerResult(leaf, true, "VAR");
                    Match("i", listnode);
                    Match("s, [", listnode);
                    SubListDef(listnode);
                    Match(lookahead, listnode);
                    Match("s, ]", listnode);
                    Skip("o, =");
                    SubListElements(listnode);
                    Skip("s, ;");

                    break;

                case "n":
                case "s, (":
                case "i":

                    TreeNode<string> assignnode = NewLayerResult(leaf, true, "ASSIGN");
                    Match("i", assignnode);
                    Match("s, [", assignnode);
                    SubValReal(assignnode);
                    Match("s, ]", assignnode);
                    SubVal(assignnode);

                    break;
            }
        }

        #region Conditions
        private void SubCond(TreeNode<string> leaf, bool newlayer)
        {
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "CON");

            switch (lookahead)
            {
                case "l":

                    TreeNode<string> stringnode = NewLayerResult(node, true, "STRING");
                    Match("l", stringnode);
                    SubStringHandle(stringnode);
                    SubCondSign(node);

                    break;

                case "n":

                    TreeNode<string> expnode = NewLayerResult(node, true, "EXP");
                    Match(lookahead, expnode);
                    SubExpHandle(expnode);
                    SubCondSign(node);

                    break;

                case "s, (":

                    TreeNode<string> brexpnode = NewLayerResult(node, true, "EXP");
                    SpecialBracketsCheck(brexpnode);
                    SubCondSign(node);

                    break;

                case "i":

                    SubIdent(node);

                    switch (lookahead)
                    {
                        case "s, {":

                            SubEndCon(node);
                            
                            break;

                        default: SubCondSign(node); break;
                    }

                    break;

                case "k, true":
                case "k, false":

                    Match(lookahead, node);
                    SubEndCon(node);

                    break;

                default: Console.WriteLine("Big Error"); break;
            }
        }

        private void SubCondSign(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "o, ==":
                case "o, !=":
                case "o, <=":
                case "o, >=":
                case "o, <":
                case "o, >":
                case "k, is":
                case "k, not":

                    Match(lookahead, node);
                    SubEndCon(node);

                    break;

                default:
                    Console.WriteLine("A Big Error");

                    break;
            }
        }

        private void SubMoreCon(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "k, and":
                case "k, or":

                    Match(lookahead, node);
                    SubCond(node, false);

                    break;

                case "s, {": return;
                case "s, ;": return;

                default: Console.WriteLine("B Big Error"); break;
            }
        }

        private void SubEndCon(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "l":

                    TreeNode<string> stringnode = NewLayerResult(node, true, "STRING");
                    Match("l", stringnode);
                    SubStringHandle(stringnode);
                    SubMoreCon(node);

                    break;


                case "n":

                    TreeNode<string> expnode = NewLayerResult(node, true, "EXP");
                    Match(lookahead, expnode);
                    SubExpHandle(expnode);
                    SubMoreCon(node);


                    break;

                case "s, (":

                    TreeNode<string> brexpnode = NewLayerResult(node, true, "EXP");
                    SpecialBracketsCheck(brexpnode);

                    break;


                case "i":

                    SubIdent(node);

                    break;

                case "k, true":
                case "k, false":

                    Match(lookahead, node);
                    SubMoreCon(node);

                    break;
            }
        }
        #endregion

        #region Assign and VAL

        private void SubVal(TreeNode<string> leaf)
        {
            switch (lookahead)
            {
                case "s, ;":

                    Skip("s, ;");

                    break;

                case "o, =":

                    TreeNode<string> node = NewLayerResult(leaf, true, "VAL");
                    Match("o, =", node);
                    SubValReal(node);
                    Skip("s, ;");

                    break;
            }
        }

        private void SubValReal(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "l":

                    TreeNode<string> stringnode = NewLayerResult(node, true, "STR");
                    Match("l", stringnode);
                    SubStringHandle(stringnode);

                    break;

                case "n":

                    TreeNode<string> expnode = NewLayerResult(node, true, "EXP");
                    Match("n", expnode);
                    SubExpHandle(expnode);

                    break;

                case "s, (":

                    TreeNode<string> brexpnode = NewLayerResult(node, true, "EXP");
                    SpecialBracketsCheck(brexpnode);

                    break;

                case "i":

                    SubIdent(node);
                    
                    break;

                case "k, true":
                case "k, false":

                    Match(lookahead, node);

                    break;
            }
        }

        private void SubAssign(TreeNode<string> leaf)
        {
            string next = LookAhead(2);
            next = next.Remove(0, 1);

            switch (next)
            {
                case "o, :=":

                    TreeNode<string> varnode = NewLayerResult(leaf, true, "VAR");
                    Match("i", varnode);
                    Skip("o, :=");
                    SubDecideVar(varnode);
                    Skip("s, ;");

                    break;

                case "o, =":

                    TreeNode<string> asnode = NewLayerResult(leaf, true, "ASSIGN");
                    Match("i", asnode);
                    SubVal(asnode);

                    break;

                default:

                    Environment.Exit(0);

                    break;
            }
        }

        private void SubDecideVar(TreeNode<string> node)
        {
            switch(lookahead)
            {
                case "s, [":

                    SubListElements(node);

                    break;

                case "n":
                case "l":
                case "i":
                case "k, true":
                case "k, false":
                case "s, (":

                    SubValReal(node);

                    break;
            }
        }

        #endregion

        #region ForLoop
        private void SubFor(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "s, (":

                    Skip("s, (");
                    SubFor(node);

                    break;

                case "i":

                    SubForEachOrFor(node);

                    break;

                case "k, int":

                    TreeNode<string> varnode = NewLayerResult(node, true, "VAR");
                    SubName(varnode);
                    SubForCont(node);

                    break;
            }
        }

        private void SubForCont(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "k, in":

                    Match("k, in", node);
                    Match("i", node);
                    
                    break;

                case "o, =":

                    SubVal(node);
                    SubCond(node, true);
                    Skip("s, ;");
                    SubAssign(node);
                    SubForEnd(node);

                    break;
            }
        }

        private void SubForEachOrFor(TreeNode<string> node)
        {
            string next = LookAhead(2);
            next = next.Remove(0, 1);

            switch (next)
            {
                case "s, )":
                case "s, {":

                    Skip(lookahead);

                    break;

                case "o, :=":

                    SubAssign(node);
                    SubCond(node, true);
                    Skip("s, ;");
                    SubAssign(node);
                    SubForEnd(node);

                    break;

                case "o, ==":
                case "o, !=":
                case "o, <=":
                case "o, >=":
                case "o, <":
                case "o, >":
                case "k, is":
                case "k, not":

                    SubCond(node, true);

                    break;

            }
        }

        private void SubForEnd(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "s, )":
                case "s, {":

                    Match(lookahead, node);
                    
                    break;
            }
        }
        #endregion

        #region IdentifierLogic

        private void SubIdent(TreeNode<string> node)
        {
            Match("i", node);
            switch (lookahead)
            {
                case "s, [":

                    TreeNode<string> listnode = NewLayerResult(node, true, "ELEVAL");
                    Match("s, [", listnode);
                    SubListCall(listnode);
                    Match("s, ]", listnode);
                    SubIdentAdd(listnode);

                    break;

                case "s, (":

                    TreeNode<string> callnode = NewLayerResult(node, true, "CALL");
                    Match("s, (", callnode);
                    SubCallParameters(callnode);
                    Match("s, )", callnode);
                    SubIdentAdd(callnode);

                    break;

                case "o, +":
                case "o, -":
                case "o, *":
                case "o, /":
                case "o, %":
                case "o, ^":

                    SubExpHandle(node);

                    break;
            }
        }

        private void SubIdentAdd(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "o, +":
                case "o, -":
                case "o, *":
                case "o, /":
                case "o, %":
                case "o, ^":

                    SubExpHandle(node);

                    break;

                case "s, ;":

                    return;


                case "o, &":

                    TreeNode<string> stringnode = NewLayerResult(node, true, "STR");
                    SubStringHandle(stringnode);

                    break;
            }
        }

        private void SubListCall(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "s, (":

                    TreeNode<string> brexpnode = NewLayerResult(node, true, "EXP");
                    SpecialBracketsCheck(brexpnode);

                    break;

                case "n":

                    TreeNode<string> expnode = NewLayerResult(node, true, "EXP");
                    Match("n", expnode);
                    SubExpHandle(expnode);

                    break;

                case "i":

                    TreeNode<string> identnode = NewLayerResult(node, true, "IDENT");
                    SubIdent(identnode);

                    break;
            }
        } 

        #endregion

        private void SubErrorHandling(TreeNode<string> node)
        {
            Skip("o, ?");
            Match("s, {", node);
            SubBodyMain(node);
        }

        private TreeNode<string> NewLayerResult(TreeNode<string> leaf, bool newlayer, string nonterminal)
        {
            TreeNode<string> node;
           
            if (newlayer)
            {
                node = new TreeNode<string>(nonterminal);
                leaf.AddChild(node);
                trees.Add(node);
            }
            else 
            { 
                node = leaf;
            }
            return node;
        }

        private void Match(string token, TreeNode<string> leaf)
        {
            if (lookahead.Equals(token))
            {
                Console.WriteLine("Match " + token);

                TreeNode<string> node = new TreeNode<string>(lsttokens[0]);
                leaf.AddChild(node);

                lsttokens.RemoveRange(0, 1);
                lookahead = LookAhead(1);
            }
            else 
            {
                Console.WriteLine(token);
                Console.WriteLine(lookahead);
                Environment.Exit(0);
            }
        }

        private void Skip(string token)
        {
            Console.WriteLine("Skip " + token);
            if (lookahead.Equals(token))
            {
                lsttokens.RemoveRange(0, 1);
                lookahead = LookAhead(1);
            } else
            {
                Console.WriteLine(token);
                Console.WriteLine(lookahead);
                Environment.Exit(0);
            }
        }

        private string LookAhead(int lookahead)
        {
            string output = "";

            for(int i = 0; i < lookahead; i++)
            {
                if (lsttokens[i][0].Equals('l') || lsttokens[i][0].Equals('i') || lsttokens[i][0].Equals('n'))
                {
                    output += lsttokens[i].First();
                } 
                else
                {
                    output += lsttokens[i];
                }
            }

            return output;
        }
    
        private void ReadTree()
        {
            foreach(TreeNode<string> a in trees)
            {
                Console.WriteLine("Root:" + a.Data);

                foreach (TreeNode<string> b in a.GetAllChild())
                {
                    Console.WriteLine("Leaf:" + b.Data); 
                }

                Console.WriteLine();
            }
        }
    }
}
