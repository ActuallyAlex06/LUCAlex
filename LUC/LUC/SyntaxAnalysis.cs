using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        List<string> stack = new List<string>();
        Stack<string> operators = new Stack<string> { };
        Dictionary<int, TreeNode<string>> trees = new Dictionary<int, TreeNode<string>> { };
        int treeid = 0;

        private void GoThroughTokens(Dictionary<int, List<string>> tokens)
        {
            foreach (List<string> tokensline in tokens.Values)
            {
                foreach(string token in tokensline)
                {
                    ShuntingYardAlgorithm(token);
                    //CheckReduce();
                }
            }
            ReadStack();
            //Console.WriteLine();
            //ReadTree();
        }

        private void ShuntingYardAlgorithm(string token)
        {
            if (token[0].Equals('l') || token[0].Equals('i') || token[0].Equals('k') || token.Equals("s, {") ||token.Equals("s, }"))
            {
                stack.Add(token);

            } else if(token.Equals("s, ;"))
            {
                int i = 0;
                while (operators.Any())
                {
                    stack.Add(operators.Pop());
                    i++;
                }
            }    
            else if (token[0].Equals('o'))
            {               
                if(operators.Any())
                {
                    while (NotParanthesis(operators.Peek()) && (GetPresenence(operators.Peek()) > GetPresenence(token) || GetPresenence(token) == GetPresenence(operators.Peek())) && GetPresenence(token) != 3)
                    {
                        stack.Add(operators.Pop());
                        if (!operators.Any()) { break; }
                    }
                }

                operators.Push(token);
            }
            else if (token[0].Equals('s') && token[token.Length - 1].Equals('('))
            {
                operators.Push(token);

            }
            else if (token[0].Equals('s') && token[token.Length - 1].Equals(')'))
            {
                while (operators.Peek() != "s, (")
                    {
                        stack.Add(operators.Pop());
                    }

                operators.Pop();
            }
        }

        private bool NotParanthesis(string token)
        {
            if (token.Equals("s, (") || token.Equals("s, )"))
            {
                return false;
            } else { return true; }
        }

        private int GetPresenence(string token)
        {
            if(token.Equals("o, +") || token.Equals("o, -")) { return 1; }
            else if(token.Equals("o, *") || token.Equals("o, /")) { return 2; }
            else if(token.Equals("o, ^")) { return 3; }
            else { return 0; }
        }


        private void CheckReduce()
        {
            int currentindex = stack.Count;

            //Simple Expression with two literals
            if (ReadStack(3, currentindex).Equals("l|o, +|l|")) { ReduceNormal(2, currentindex, "EXP +", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("l|o, -|l|")) { ReduceNormal(2, currentindex, "EXP -", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("l|o, *|l|")) { ReduceNormal(2, currentindex, "EXP *", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("l|o, /|l|")) { ReduceNormal(2, currentindex, "EXP /", [stack[currentindex - 3], stack[currentindex - 1]]); }

            //Combinign literals and expresions (+)
            else if (ReadStack(3, currentindex).Equals("EXP +|o, +|l|")) { ReduceNormal(2, currentindex, "EXP +", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("EXP -|o, +|l|")) { ReduceNormal(2, currentindex, "EXP +", [stack[currentindex - 3], stack[currentindex - 1]]); }
            else if (ReadStack(3, currentindex).Equals("l|o, +|EXP +|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, +|EXP -|")) { Console.WriteLine("ReduceSpecial"); }
            
            //Combinign literals and expressions (-)
            else if (ReadStack(3, currentindex).Equals("l|o, -|EXP +|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP +|o, -|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, -|EXP -|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP -|o, -|l|")) { Console.WriteLine("ReduceSpecial"); }

            //Combinign literals and expressions (*)     
            else if (ReadStack(3, currentindex).Equals("EXP +|o, *|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP +|o, *|EXP +|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, *|EXP +|")) { Console.WriteLine("ReduceSpecial"); }

            else if (ReadStack(3, currentindex).Equals("EXP -|o, *|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP -|o, *|EXP -|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, *|EXP -|")) { Console.WriteLine("ReduceSpecial"); }

            else if (ReadStack(3, currentindex).Equals("EXP *|o, *|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP *|o, *|EXP *|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, *|EXP *|")) { Console.WriteLine("ReduceSpecial"); }

            else if (ReadStack(3, currentindex).Equals("EXP /|o, *|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP /|o, *|EXP /|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, *|EXP /|")) { Console.WriteLine("ReduceSpecial"); }

            //Combinign literals and expressions (/)
            else if (ReadStack(3, currentindex).Equals("EXP +|o, /|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP +|o, /|EXP +|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, /|EXP +|")) { Console.WriteLine("ReduceSpecial"); }

            else if (ReadStack(3, currentindex).Equals("EXP -|o, /|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP -|o, /|EXP -|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, /|EXP -|")) { Console.WriteLine("ReduceSpecial"); }

            else if (ReadStack(3, currentindex).Equals("EXP *|o, /|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP *|o, /|EXP *|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, /|EXP *|")) { Console.WriteLine("ReduceSpecial"); }

            else if (ReadStack(3, currentindex).Equals("EXP /|o, /|l|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("EXP /|o, /|EXP /|")) { Console.WriteLine("ReduceSpecial"); }
            else if (ReadStack(3, currentindex).Equals("l|o, /|EXP /|")) { Console.WriteLine("ReduceSpecial"); }

            //Completing Expressions (End of Expression)
            else if (ReadStack(2, currentindex).Equals("EXP -|s, ;|")) { stack.RemoveAt(stack.Count - 1); }
            else if (ReadStack(2, currentindex).Equals("EXP *|s, ;|")) { stack.RemoveAt(stack.Count - 1); }
            else if (ReadStack(2, currentindex).Equals("EXP /|s, ;|")) { stack.RemoveAt(stack.Count - 1); }
            else if (ReadStack(2, currentindex).Equals("EXP +|s, ;|")) { stack.RemoveAt(stack.Count - 1); }
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
                if (child[0].Equals('l') || child[0].Equals('s') || child[0].Equals('o') || child[0].Equals('i') || child[0].Equals('k'))
                {
                    node.AddChild(child);
                } else
                {
                    TreeNode<string> treenode = trees[int.Parse(child[child.Count() - 1].ToString())];
                    node.AddChild(treenode);
                    //trees.Remove(int.Parse(child[child.Count() - 1].ToString()));
                }
            }

            trees.Add(treeid, node);
            treeid++;
        }

        private void ReadTree()
        {
            foreach(TreeNode<string> a in trees.Values)
            {
                foreach (TreeNode<string> b in a.GetAllChild())
                {
                    Console.WriteLine(b.Data);
                }
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
