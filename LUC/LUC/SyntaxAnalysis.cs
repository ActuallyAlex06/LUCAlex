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
                    ShuntingYardAlgorithm(token, lsttokens);  
                    
                    if(token.Equals("s, ;"))
                    {
                        lsttokens.Add(token);
                    }
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

                case "k, purefunction":
                case "k, pfunc":
                case "k, pf":
                case "k, purefunc": 

                    lookahead = "k, purefunction";
                    SubPureFuncCreate(leaf, true);

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
            Match("k, function", node);
            Match("i", node);
            SubParam(node, true);
        }

        private void SubPureFuncCreate(TreeNode<string> leaf, bool newlayer)
        {
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "CREPUREFUNC");
            Match(lookahead, node);

            switch (lookahead)
            {
                case "k, bool":
                case "k, int":
                case "k, string":
                case "k, double":

                    Match(lookahead, node);

                    break;

                default: Console.WriteLine("SyntaxError"); break;
            }

            Match("i", node);
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

                    Match("s, (", node);
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

                    Match(lookahead, node);

                    break;

                default: Console.WriteLine("Syntax Error"); break;
            }

            Match("i", node);
            SubAdditionalParam(node);
        }

        private void SubAdditionalParam(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "s, ,":

                    Match("s, ,", node);
                    SubMoreParam(node);

                    break;

                case "s, )":

                    Match("s, )", node);
                    SubBody(node, true);

                    break;
            }
        } 
        #endregion

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
                    Match("k, if", elifnode);
                    SubCond(elifnode, true);
                    SubBody(elifnode, true);
                    SubBodyMain(node);

                    return;

                case "k, else":

                    TreeNode<string> elsenode = NewLayerResult(node, true, "ELSE");
                    Match("k, if", elsenode);
                    SubCond(elsenode, true);
                    SubBody(elsenode, true);
                    SubBodyMain(node);

                    return;

                case "s, }":

                    Match("s, }", node);
                    StartDescent(start);

                break;
            }
        }

        private void SubCond(TreeNode<string> leaf, bool newlayer)
        {
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "CON");

            switch (lookahead)
            {
                case "l":
                    Match(lookahead, node);
                    SubCondSign(node);
                    break;

                case "n":
                    Match(lookahead, node);
                    SubCondSign(node);
                    break;

                case "i":
                    Match(lookahead, node);
                    SubCondSign(node);
                    break;

                case "k, true":
                case "k, false":
                    Match(lookahead, node);
                    SubEndCon(node);
                break;

                default: Console.WriteLine("Big Error");  break;
            }
        }

        private void SubCondSign(TreeNode<string> node)
        {
            switch (lookahead)
            {
                case "k, ==":
                case "k, !=":
                case "k, <=":
                case "k, >=":
                case "k, <":
                case "k, >":
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

                    SubCond(node, false);

                break;

                case "s, {":  return;

                default: Console.WriteLine("B Big Error"); break;
            }
        }

        private void SubEndCon(TreeNode<string> node)
        {
            switch(lookahead)
            {
                case "l":
                    Match(lookahead, node);
                    SubMoreCon(node);
                    break;

                case "n":
                    Match(lookahead, node);
                    SubMoreCon(node);
                    break;

                case "i":
                case "k, true":
                case "k, false":

                    Match(lookahead, node);
                    SubMoreCon(node);

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

        private void ShuntingYardAlgorithm(string token, List<string> lsttokens)
        {
            if (token[0].Equals('l') || token[0].Equals('i') || token[0].Equals('k') || token.Equals("s, {") || token.Equals("s, }") || token.Equals("s, ,") || token[0].Equals('n'))
            {
                lsttokens.Add(token);

            }
            else if (token.Equals("s, ;"))
            {
                int i = 0;
                while (operators.Any())
                {
                    lsttokens.Add(operators.Pop());
                    i++;
                }
            }
            else if (token[0].Equals('o'))
            {
                if (operators.Any())
                {
                    while (NotParanthesis(operators.Peek()) && (GetPresenence(operators.Peek()) > GetPresenence(token) || GetPresenence(token) == GetPresenence(operators.Peek())) && GetPresenence(token) != 3)
                    {
                        lsttokens.Add(operators.Pop());
                        if (!operators.Any()) { break; }
                    }
                }

                operators.Push(token);
            }
            else if (token[0].Equals('s') && token[token.Length - 1].Equals('('))
            {
                lsttokens.Add(token);
                operators.Push(token);

            }
            else if (token[0].Equals('s') && token[token.Length - 1].Equals(')'))
            {
                while (operators.Peek() != "s, (")
                {
                    lsttokens.Add(operators.Pop());
                }

                operators.Pop();
                lsttokens.Add(token);
            }
        }

        private bool NotParanthesis(string token)
        {
            if (token.Equals("s, (") || token.Equals("s, )"))
            {
                return false;
            }
            else { return true; }
        }

        private int GetPresenence(string token)
        {
            if (token.Equals("o, +") || token.Equals("o, -")) { return 1; }
            else if (token.Equals("o, *") || token.Equals("o, /")) { return 2; }
            else if (token.Equals("o, ^")) { return 3; }
            else { return 0; }
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
