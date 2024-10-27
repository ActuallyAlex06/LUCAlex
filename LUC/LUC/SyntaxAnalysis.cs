using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LUC
{
    public class SyntaxAnalysis
    {

        TreeNode<string> node0 = new TreeNode<string>("root0");

        public void Analyse()
        {
            Dictionary<int, List<string>> tokens = Lexer.tokens;
            GoThroughTokens(tokens);
        }

        //Plan is to add a generalized Method to check for any rule given to it if a sequence of tokens applys to it

        /*
         * -> Reduce literals and identifiers only to the token type and not the acutal value
         * -> Start at the bottom of the code and go through it adding each token to a stack
         * -> See if last input is of a certain type of token and check if any tokens on the stack can be converted into a rule and thus a tree
         * 
         */

        List<string> stack = new List<string>();

        private void GoThroughTokens(Dictionary<int, List<string>> tokens)
        {
            foreach(List<string> tokensline in tokens.Values) 
            {
                foreach(string token in tokensline)
                {
                    string tok = token;
                    AddToStack(tok);
                }
            }
        }

        private void AddToStack(string token)
        {
            stack.Add(token);

            if(token.Equals("s, ;"))
            {
                CheckIfRuleApplysInStack("VariableDefi");
            }
        }

        private void CheckIfRuleApplysInStack(string kind)
        {
            switch(kind)
            {
                case "VariableDefi":

                    if (stack[0][0].Equals('i') && stack[1].Equals("o, :=") && stack[2][0].Equals('l')) { AddNode([stack[0], stack[1], stack[2]]); }
              
                    break;
            }
        }

        private void AddNode(List<string> nodes)
        {
            foreach(string node in nodes)
            {
                
            }
        }


        //numba := 12;
    }
}
