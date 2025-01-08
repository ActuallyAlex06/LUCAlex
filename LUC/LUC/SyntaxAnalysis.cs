using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        int treeid = 0;

        private void GoThroughTokens(Dictionary<int, List<string>> tokens)
        {
            foreach (List<string> tokensline in tokens.Values)
            {
                foreach(string token in tokensline)
                {
                    stack.Add(token);
                    CheckReduce();
                    ReadStack();
                }
            }
        }

        private void CheckReduce()
        {
            int currentindex = stack.Count;
            if (ReadStack(3, currentindex).Equals("l|o, +|l|")) { ReduceNormal(2, currentindex, "EXP +", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("i|o, +|l|")) { ReduceNormal(2, currentindex, "EXP +", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("l|o, +|i|")) { ReduceNormal(2, currentindex, "EXP +", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("i|o, +|i|")) { ReduceNormal(2, currentindex, "EXP +", [stack[currentindex - 3], stack[currentindex - 1]]); }

            else if (ReadStack(3, currentindex).Equals("l|o, -|l|")) { ReduceNormal(2, currentindex, "EXP -", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("i|o, -|l|")) { ReduceNormal(2, currentindex, "EXP -", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("l|o, -|i|")) { ReduceNormal(2, currentindex, "EXP -", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("i|o, -|i|")) { ReduceNormal(2, currentindex, "EXP -", [stack[currentindex - 3], stack[currentindex - 1]]); }

            else if (ReadStack(3, currentindex).Equals("EXP +|o, +|i|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("i|o, +|EXP +|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP +|o, +|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, +|EXP +|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP +|o, +|EXP +|")) { Console.WriteLine("ReduceSpecial"); }
        }

        private string ReadStack(int lookbehind, int currentindex)
        {
            string stackoutput = "";

            try
            {
                for (int i = lookbehind; i > 0; i--)
                {
                    if (stack[currentindex - i][0].Equals('l') || stack[currentindex - i][0].Equals('i'))
                    {
                        stackoutput = stackoutput += stack[currentindex - i][0] + "|";
                    }
                    else if (stack[currentindex - i][0].Equals('k') || stack[currentindex - i][0].Equals('s') || stack[currentindex - i][0].Equals('o'))
                    {
                        stackoutput = stackoutput += stack[currentindex - i] + "|";
                    }
                    else
                    {
                        stackoutput = stackoutput += stack[currentindex - i].Remove(stack[currentindex - i].Length - 2) + "|";
                    }
                }

                return stackoutput;
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        private void ReduceNormal(int reduce, int currentindex, string root, List<string> leafs)
        {
            stack.RemoveRange(currentindex - reduce, reduce);
            stack[currentindex - reduce - 1] = root + "|" + treeid;

            TreeNode<string> node = new TreeNode<string>(root + "|" + treeid);
            foreach(string child in leafs)
            {
                node.AddChild(child);
            }

            trees.Add(treeid, node);
            treeid++;
        }

        private void ReadTree()
        {
            foreach(TreeNode<string> node in trees.Values) 
            {
                Console.WriteLine(node.GetChild(1).Data);
                Console.WriteLine(node.GetChild(2).Data);
            }
        }

        private void ReadStack()
        {
            foreach(string s in stack)
            {
                Console.WriteLine(s);
            }
        }
    }
}
