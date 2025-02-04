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

                default: Console.WriteLine("Method Call Error"); break;

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

                    TreeNode<string> elifnode = NewLayerResult(node, true, "ELIF");
                    Match("k, elif", elifnode);
                    SubCond(elifnode, true);
                    SubBody(elifnode, true);
                    SubBodyMain(node);

                    return;

                case "k, else":

                    TreeNode<string> elsenode = NewLayerResult(node, true, "ELSE");
                    Match("k, else", elsenode);
                    SubCond(elsenode, true);
                    SubBody(elsenode, true);
                    SubBodyMain(node);

                    return;

                case "k, for":



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

                    SubAssign(node);
                    SubBodyMain(node);

                    return;

                case "s, }":

                    Match("s, }", node);
                    StartDescent(start);

                    break;
            }
        }
        #endregion

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

                    break;

                case "i":


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

                default: Console.WriteLine("A Big Error"); break;
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
                    SubCondSign(node);


                    break;

                case "s, (":

                    TreeNode<string> brexpnode = NewLayerResult(node, true, "EXP");
                    SpecialBracketsCheck(brexpnode);

                    break;


                case "i":


                    break;

                case "k, true":
                case "k, false":

                    Match(lookahead, node);
                    SubMoreCon(node);

                    break;
            }
        }
        #endregion

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
            Console.WriteLine("A: " + next);

            switch (next)
            {
                case "o, :=":

                    TreeNode<string> varnode = NewLayerResult(leaf, true, "VAR");
                    Match("i", varnode);
                    Skip("o, :=");
                    SubValReal(varnode);
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
                Console.WriteLine("Match");

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
