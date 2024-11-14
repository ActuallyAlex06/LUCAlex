using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
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

        List<string> stack = new List<string>();
        Dictionary<int, TreeNode<string>> trees = new Dictionary<int, TreeNode<string>> { };
        Dictionary<string, Action<string>> rules = new Dictionary<string, Action<string>> { };
        int treeid = 0;

        private void GoThroughTokens(Dictionary<int, List<string>> tokens)
        {
            foreach(List<string> tokensline in tokens.Values) 
            {
                foreach(string token in tokensline)
                {
                    stack.Add(token);
                }
            }

            CheckRules();
        }

        private void CheckRules()
        {
            for (int i  = 0; i < 6; i++)
            {
                int index = 0;

                foreach (string token in stack)
                {
                    if (token.Equals("s, ;") && i == 1)
                    {
                        CheckIfApplysToRule(index, i);
                    }

                    index++;
                }
            }
        }

        private void CheckIfApplysToRule(int stackindex, int rulesdecider)
        {
            switch (rulesdecider)
            {
                case 1:
                    
                        if (GetTokensToCheck(3, stackindex, "i|o, :=|l|s, ;|")) { }
                        else if(GetTokensToCheck(4, stackindex, "i|o, :=|l|s, ;|")) { }
                        

                    break;

                case 2:

                        

                    break;
            }
        }

        private bool GetTokensToCheck(int amount, int index, string rule)
        {
            if(amount > index) { return false; }

            string checkrule = "";

            for(int i = amount; i >= 0; i--)
            {              
                if (stack[index - i][0].Equals('i') || stack[index - i][0].Equals('l'))
                {
                    checkrule = checkrule + stack[index - i][0] + "|";
                }
                else
                {
                    checkrule = checkrule + stack[index - i] + "|";
                }
            }

            if (checkrule.Equals(rule)) 
            {
                return true; 
            } else 
            { 
                return false;
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
            stack.Add(treeid + " | Variable");
        }

        //numba := 12;
    }
}
