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
        //This is the syntax analysis of our programming language. A recursive descent parser is used. All Subroutines and their corresponding grammatical rule can be lookd up in the thesis paper.

        List<TreeNode<string>> trees = new List<TreeNode<string>>();
        string lookahead;
        List<string> lsttokens = new List<string> { };
        TreeNode<string> start = new TreeNode<string>("Start");

        public void Analyse()
        {
            List<string> tokens = Lexer.tokens;
            lsttokens = tokens;
            GoThroughTokens();
        }

        private void GoThroughTokens()
        {
            //Simple Implementation of a Recursive Descent Parser
            trees.Add(start);
            lsttokens.Add("$");
            lookahead = LookAhead(1);
            StartDescent(start);

            ReadTree(trees);
        }

        private void StartDescent(TreeNode<string> leaf)
        {
            //Subroutine reprsenting the outer shell of the programm, only function calls and function definitions possible
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
            //Creation of Functions, with or without return value
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "CREFUNC");
            Skip("k, function");

            switch (lookahead)
            {
                case "k, int":
                case "k, bool":
                case "k, string":
                case "k, double":

                    SubName(node);

                    break;

                case "i":

                    Match("i", node);

                    break;

                default:

                    Console.WriteLine("FUNC DEF Error");
                    Environment.Exit(0);

                    break;
            }

            SubParam(node, true);
        }
        #endregion

        #region Parameter Logic
        private void SubParam(TreeNode<string> leaf, bool newlayer)
        {
            //Parameters of a defined method. These and the two subroutines below handle any parameter combination possible.
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "PARAM");

            switch (lookahead)
            {
                case "s, (":

                    Skip("s, (");
                    SubMoreParam(node);

                    break;

                default:

                    Console.WriteLine("Parameter Error");
                    Environment.Exit(0);

                    break; 
            }
        }

        private void SubMoreParam(TreeNode<string> node)
        {
            //Adds the Parameter and checks if we want to add even more parameters.
            switch (lookahead)
            {
                case "k, int":
                case "k, bool":
                case "k, string":
                case "k, double":

                    SubName(node);

                    break;

                case "s, )":

                    Skip("s, )");
                    SubBody(node, true);

                    break;

                default:

                    Console.WriteLine("PARAM Error");
                    Environment.Exit(0);

                    break;
            }

            SubAdditionalParam(node);
        }

        private void SubAdditionalParam(TreeNode<string> node)
        {
            //Ckecks if more parameters need to be added.
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
            //Code to call some Method.
            TreeNode<string> node = NewLayerResult(leaf, true, "CALL");
            Match("i", node);
            Skip("s, (");
            SubCallParameters(node);
            Skip("s, )");
            Skip("s, ;");
        }

        private void SubCallParameters(TreeNode<string> node)
        {
            //Parameters to be used in a method call.
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

                case "s, )": return;
            }
        }

        private void SubEndCallParameters(TreeNode<string> node)
        {
            //Checks if more parameters want to be added.
            switch (lookahead)
            {
                case "s, )":

                    return;

                case "s, ,":

                    Skip("s, ,");
                    SubCallParameters(node);

                    break;

                default:

                    Console.WriteLine("PARAM Error");
                    Environment.Exit(0);

                    break;
            }
        }

        private void SubName(TreeNode<string> leaf)
        {
            //Any kind of given name. Paramaters, Variables etc. are included
            TreeNode<string> node = NewLayerResult(leaf, true, "NAME");
            Match(lookahead, node);
            Match("i", node);
        } 
        #endregion

        #region String handling

        private void SubStringHandle(TreeNode<string> node)
        {
            //Stringhandling in LUC is done with the & operator. Checks if a string has an added variable, number etc.
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
            //Handles the case that a string does have an extension.

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


                default:

                    Console.WriteLine("STRING Error");
                    Environment.Exit(0);

                    break;
            }
        }

        #endregion

        #region Expressions

        private void SubExpHandle(TreeNode<string> node)
        {
            //Entrance to an expression
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
            //The term after the operator in an expression is handled in this Subroutine
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

                default:

                    Console.WriteLine("EXP Error");
                    Environment.Exit(0);

                    break;
            }
        }

        private void SpecialBracketsCheck(TreeNode<string> node)
        {
           //Oping and closing brackets simple handling
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

                default:

                    Console.WriteLine("EXP Error");
                    Environment.Exit(0);

                    break;
            }
        }

        private void FrontBracketFix(TreeNode<string> node)
        {
            //Special method to ensure grammitcal correctness in the case of expressions starting with a left paranthesis
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

                default:

                    Console.WriteLine("EXP Error");
                    Environment.Exit(0);

                    break;
            }
        }
        #endregion

        #region Body
        private void SubBody(TreeNode<string> leaf, bool newlayer)
        {
            //Body Subroutine, any codeblock enclosed by a { and } uses this subroutine
            TreeNode<string> node = NewLayerResult(leaf, newlayer, "BODY");

            switch (lookahead)
            {
                case "s, {":

                    Match("s, {", node);
                    SubBodyMain(node);

                    break;

                default:

                    Console.WriteLine("BODY Error");
                    Environment.Exit(0);

                    break;
            }
        }

        private void SubBodyMain(TreeNode<string> node)
        {
            //Any kind of possible grammtical construction that can appear in a funcion or loop is handled in this method. Furthermore error handling and return values also use this subroutine
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
                case "k, double":

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

        #region Other concepts
        private void SubReturn(TreeNode<string> node)
        {
            //Return values are handled in this subroutine
            Match("k, return", node);
            SubValReal(node);
            Skip("s, ;");
        }

        private void SubErrorHandling(TreeNode<string> node)
        {
            //Defines the grammatical construction needed for proper error handling
            Skip("o, ?");
            Match("s, {", node);
            SubBodyMain(node);
        } 
        #endregion

        #region List
        private void SubListElements(TreeNode<string> leaf)
        {
            //Subroutine to add elements to a defined list
            Skip("s, [");
            TreeNode<string> elnode = NewLayerResult(leaf, true, "ITEM");
            SubElement(elnode);
            Skip("s, ]");
        }

        private void SubElement(TreeNode<string> node)
        {
            //Subroutine creating the elements that are to be added to a list. Handled in SubListElements
            switch (lookahead)
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
            //Decides if more elements are part of the list or not.
            switch (lookahead)
            {
                case "s, ]":
                    break;

                case "s, ,":

                    Skip("s, ,");
                    SubElement(node);

                    break;

                default:

                    Console.WriteLine("LIST Error");
                    Environment.Exit(0);

                    break;
            }
        }

        private void SubListDef(TreeNode<string> leaf)
        {
            //Subroutine for the grammatical concept of a list.
            string next = LookAhead(3);
            next = next.Remove(0, 5);

            switch (next)
            {
                case "k, int":
                case "k, bool":
                case "k, string":
                case "k, double":

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

                default:

                    Console.WriteLine("CON Error");
                    Environment.Exit(0);

                    ; break;
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

                    Console.WriteLine("CON Error");
                    Environment.Exit(0);

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

                case "s, {": 
                case "s, ;": 


                    return;

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
                    SubMoreCon(node);

                    break;

                case "k, null":

                    Match("k, null", node);
                    SubMoreCon(node);

                    break;

                case "i":

                    SubIdent(node);
                    SubMoreCon(node);

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
                case "k, null":

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

                    Console.WriteLine("ASSIGN Error");
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
                case "k, null":

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

        private void SubIdentifierKind(TreeNode<string> leaf)
        {
            //Identifiers can be numerous grammatically sound constructs, this subroutine differentiates between the different ones.
            //Sometimes next is used to loop ahead in the tokenslist. This doesn't help the process, it is merly used to make the syntax tree more organized.
            string next = LookAhead(2);
            next = next.Remove(0, 1);

            switch (next)
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

                default:

                    Console.WriteLine("IDENT Error");
                    Environment.Exit(0);

                    break;
            }
        }

        private void SubIdent(TreeNode<string> node)
        {
            //Dependent on what token is next the identifier is converted to a different grammatical constant

            Match("i", node);
            switch (lookahead)
            {
                case "s, [":

                    TreeNode<string> listnode = NewLayerResult(node, true, "LISTVAL");
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
            //Consturctions with identifiers often have some kind of constant or another identifier directly following them. This subroutine handles expressions, etc. that are preceded by identifier constructions.
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
            //A list call is a value that is stored in a list, called by defining the index of the value in the list.
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

                default:

                    Console.WriteLine("IDENT Error");
                    Environment.Exit(0);

                    break;
            }
        }

        #endregion

        #region RecursiveDescentParser Managment
        private TreeNode<string> NewLayerResult(TreeNode<string> leaf, bool newlayer, string nonterminal)
        {
            //Used to create  a new layer in the syntax tree.
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
            //Match is an essential method of the recursice descent parser. It checks if an given element on top of the list has the expected value. This is especially important when handling 
            //long complex grammar rules

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
                Console.WriteLine("Error!");
                Environment.Exit(0);
            }
        }

        private void Skip(string token)
        {
            //Method used to skip certain tokens on the tokenslist. Makes the final syntax tree smaller and easier to use.
            Console.WriteLine("Skip " + token);
            if (lookahead.Equals(token))
            {
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
            //Essential method of any recursive descent parser. Lookahead peeks onto the tokenlist and uses the information to call the proper subroutine.
            string output = "";

            for (int i = 0; i < lookahead; i++)
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
        #endregion

        private void ReadTree(List<TreeNode<string>> tokens)
        {
            foreach (TreeNode<string> a in trees)
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
