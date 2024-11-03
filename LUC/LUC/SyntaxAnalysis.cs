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
        Dictionary<int, TreeNode<string>> trees = new Dictionary<int, TreeNode<string>> { };
        int treeid = 0;

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
                CheckIfRuleApplysInStack("VD");
            }
        }

        private void TrySomething(List<string> rules, string token)
        {
            bool rulefound = false;

            int stackindex = 0;

            string fullrule = token;
            while (!rulefound)
            {
               fullrule = fullrule +  "|" + stack[stackindex];
               
            }
        }

        private void CheckIfRuleApplysInStack(string kind)
        {
            switch(kind)
            {
                case "VD":

                    if (stack[0][0].Equals('i') && stack[1].Equals("o, :=") && stack[2][0].Equals('l')) { 
                        CreateTree(4, "VD", [stack[0], stack[1], stack[2]]);
                    }
              
                    break;
            }
        }

        private void CreateTree(int delete, string name, List<string> nodes)
        { 
            TreeNode<string> root = new TreeNode<string>(name);  
            
            foreach(string node in nodes)
            {
                root.AddChild(node);
            }

            trees.Add(treeid, root);
            treeid++;
            stack.RemoveRange(0, delete);
        }

        //numba := 12;
    }
}
