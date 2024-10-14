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
        List<string> keywordseperators = new List<string> { " ", "+", "-", "*", "/", "(", ")", "{", "}", '"'.ToString(), "=", ";", ",", "%", "^", "?"};

        public void DoLexer()
        {
            string inputcode = Recources.ReadFile("Applications/Code.luc");
            Tokenizer(inputcode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList()); ;

            foreach(List<string> tokena in tokens.Values)
            {
                foreach(string tokenb in tokena)
                {
                    Console.Write(tokenb + " ");
                }
                Console.WriteLine("");
            }
        }

        private void Tokenizer(List<string> lines)
        {
            int linenum = 1;

            foreach(string line in lines)
            {
                List<string> linetokens = new List<string>();
                restline = line.TrimStart();

                while (restline.Length > 0)
                {
                    DefineTokens(linetokens);
                    Console.WriteLine(restline);
                }

                tokens.Add(linenum, linetokens);
                linenum++;
            }
        }
        
        //Fix Bugs with numbers or identifiers at end of line
        private void DefineTokens(List<string> linetokens)
        {        
            switch (restline[0])
            {
                #region MathOperators
                case ' ': restline = restline.Remove(0, 1); break;
                case '-':
                case '*':
                case '/':
                case '+':
                case '=':
                case '%':
                case '^':
                case '\\':
                case '?':
                case ',':
                case '>':
                case '<':

                    AddToken("(operator, " + restline[0] + ")", 1, linetokens);

                break;

                case ':': AddToken("(operator, :=)", 2, linetokens); break;
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

                    string finalnum = CreateIdentifier(curchara => !int.TryParse(restline[curchara].ToString(), out int a));
                    AddToken("(literal, " + finalnum + ")", finalnum.Length, linetokens);

                break;
                #endregion

                #region Seperators

                case ';': 
                case '(': 
                case ')': 
                case '{': 
                case '}': AddToken("(seperator, " + restline[0] + ")", 1, linetokens); break;
                #endregion

                case '"':

                    string strterm = CreateIdentifier(curchara => restline[curchara].Equals('"')) + '"';
                    AddToken("(literal, " + strterm + ")", strterm.Length + 1, linetokens);

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
                    AddToken("(identifier, " + word + ")", word.Length, linetokens);

                    break;
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
                    AddToken("(keyword, " + word + ")", word.Length, linetokens);
                    iskeyword = false;
                }
            }

            if(iskeyword)
            {
                AddToken("(identifier, " + word + ")", word.Length, linetokens);
            }
        }

        private void AddToken(string token, int remove, List<string> tokens)
        {
            tokens.Add(token);
            restline = restline.Remove(0, remove);
        }
    }
}
