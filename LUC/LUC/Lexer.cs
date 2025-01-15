﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LUC
{
    internal class Lexer
    {
        public static Dictionary<int, List<string>> tokens = new Dictionary<int, List<string>>();
        string restline = String.Empty;
        List<string> keywordseperators = new List<string> { " ", "+", "-", "*", "/", "(", ")", "{", "}", '"'.ToString(), "=", ";", ",", "%", "^", "?", ":", ">", "<"};

        public void DoLexer()
        {
            string inputcode = Recources.ReadFile("Applications/Code.luc");
            Tokenizer(inputcode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList()); ;

            foreach (List<string> tokensline in tokens.Values)
            {
                foreach (string token in tokensline)
                {
                    Console.WriteLine(token);
                }
            }

            Console.WriteLine();
        }

        private void Tokenizer(List<string> lines)
        {
            int linenum = 1;

            foreach(string line in lines)
            {
                List<string> linetokens = new List<string>();
                restline = line.TrimStart();

                if (!line.Equals("") && !line.StartsWith("#"))
                {
                    while (restline.Length > 0)
                    {
                        DefineTokens(linetokens);
                        Console.WriteLine(restline);
                    }

                    tokens.Add(linenum, linetokens);                 
                }

                linenum++;
            }
        }
        
        //-numbers!!!
        private void DefineTokens(List<string> linetokens)
        {        
            switch (restline[0])
            {
                #region MathOperators
                case ' ': restline = restline.Remove(0, 1); break;
                case '-': SpecialCases(CheckNext(' '), '-', linetokens); break;
                case '*':
                case '/':
                case '+':          
                case '%':
                case '^':
                case '\\':
                case '?':

                    AddToken("o, " + restline[0], 1, linetokens);

                break;

                case '>': SpecialCases(CheckNext('='), '=', linetokens); break;
                case '<': SpecialCases(CheckNext('='), '=', linetokens); break;
                case '=': SpecialCases(CheckNext('='), '=', linetokens); break;
                case ':': SpecialCases(CheckNext('='), '=', linetokens); break;
                #endregion

                #region Numbers
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':

                    string finalnum = CreateIdentifier(curchara => !double.TryParse(restline[curchara].ToString(), out double a) && !restline[curchara].Equals('.')) ;
                    AddToken("l, " + finalnum, finalnum.Length, linetokens);

                break;
                #endregion

                #region Seperators


                case '(': 
                case ')':  
                case ';':
                case '{':
                case ',':
                case '}': AddToken("s, " + restline[0], 1, linetokens); break;
                #endregion

                case '"':

                    string strterm = CreateIdentifier(curchara => restline[curchara].Equals('"')) + '"';
                    AddToken("l, " + strterm, strterm.Length, linetokens);

                    break;

                #region Keywords
                case 'a': CheckForKeyword(["and"], linetokens); break;
                case 'b': CheckForKeyword(["bool"], linetokens); break;
                case 'c': CheckForKeyword(["complex", "continue"], linetokens); break;
                case 'd': CheckForKeyword(["double"], linetokens); break;
                case 'e': CheckForKeyword(["elif", "else"], linetokens); break;
                case 'f': CheckForKeyword(["function", "func", "for", "f", "false"], linetokens); break;
                case 'i': CheckForKeyword(["int", "integer", "if", "is"], linetokens); break;
                case 'm': CheckForKeyword(["matrix"], linetokens); break;
                case 'n': CheckForKeyword(["not"], linetokens); break;
                case 'o': CheckForKeyword(["or"], linetokens); break;
                case 'p': CheckForKeyword(["purefunc", "pfunc", "pf"], linetokens); break;
                case 'r': CheckForKeyword(["return", "stop"], linetokens); break;
                case 's': CheckForKeyword(["string", "stop"], linetokens); break;
                case 't': CheckForKeyword(["true"], linetokens); break;
                case 'w': CheckForKeyword(["while"], linetokens); break;


                #endregion

                default:

                    string word = CreateIdentifier(curchara => keywordseperators.Contains(restline[curchara].ToString()));
                    AddToken("i, " + word, word.Length, linetokens);

                    break;
            }
        }

        private bool CheckNext(char nextchar)
        {
            if (nextchar.Equals(restline[1]))
            {
                return true;
            } else 
            {               
                return false;
            }
        }

        private void SpecialCases(bool nextchar, char caseop, List<string> linetokens)
        {
            if(caseop.Equals('='))
            {
                if (nextchar)
                {
                    AddToken("o, " + restline[0] + "" + restline[1], 2, linetokens);
                } else { AddToken("o, " + restline[0], 1, linetokens); }

            } else if(caseop.Equals('-'))
            {
                if(nextchar)
                {
                    AddToken("o, " + restline[0], 1, linetokens);
                } else 
                {
                    string num = CreateIdentifier(curchara => !int.TryParse(restline[curchara].ToString(), out int a));
                    AddToken("l, " + num, num.Length + 1, linetokens);
                }
            }
        }

        private string CreateIdentifier(Func<int, bool> condition)
        {
            string name = restline[0].ToString();

            int nextchara = 1;
            while (!condition(nextchara))
            {
                name = name + "" + restline[nextchara];
                nextchara++;
            }

            return name;
        }
        
        private void CheckForKeyword(List<string> keywords, List<string> linetokens)
        {
            string word = CreateIdentifier(curchara => keywordseperators.Contains(restline[curchara].ToString()));
            bool iskeyword = true;

            foreach(string term in keywords)
            {
                if (term.Equals(word))
                {
                    AddToken("k, " + word, word.Length, linetokens);
                    iskeyword = false;
                }
            }

            if(iskeyword)
            {
                AddToken("i, " + word, word.Length, linetokens);
            }
        }

        private void AddToken(string token, int remove, List<string> tokens)
        {
            tokens.Add(token);
            restline = restline.Remove(0, remove); 
        }
    }
}
