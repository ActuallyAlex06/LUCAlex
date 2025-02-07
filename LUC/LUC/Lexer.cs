using System;
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
        public static List<string> tokens = new List<string>();
        string restline = String.Empty;
        List<string> keywordseperators = new List<string> { " ", "+", "-", "*", "/", "(", ")", "{", "}", "[", "]", '"'.ToString(), "=", ";", ",", "%", "^", "?", ":", ">", "<" };

        public void DoLexer()
        {
            //Get inputcode from file
            string inputcode = Recources.ReadFile("Applications/Code.luc");
            Tokenizer(inputcode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList()); ;
        }

        private void Tokenizer(List<string> lines)
        {
            //Keep track of in which line an error occured
            List<string> tok = new List<string> { };

            foreach (string line in lines)
            {
                restline = line.TrimStart();

                //The Scanner ignores White Spaces and Commas
                if (!line.Equals("") && !line.StartsWith("#"))
                {
                    while (restline.Length > 0)
                    {
                        //Seach for the fitting type of symbol from our alphabet
                        DefineTokens(tokens);
                    }
                    
                    
                }

            }
        }

        private void DefineTokens(List<string> linetokens)
        {
            try
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
                    case '&':

                        AddToken("o, " + restline[0], 1, linetokens);

                        break;

                    case '>': SpecialCases(CheckNext('='), '=', linetokens); break;
                    case '<': SpecialCases(CheckNext('='), '=', linetokens); break;
                    case '=': SpecialCases(CheckNext('='), '=', linetokens); break;
                    case ':': SpecialCases(CheckNext('='), '=', linetokens); break;
                    case '!': SpecialCases(CheckNext('='), '=', linetokens); break;
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
                        
                        string finalnum = CreateIdentifier(curchara => !double.TryParse(restline[curchara].ToString(), out double a) && !restline[curchara].Equals('.'));
                        //numbers get n despite being literals, it makes thing easier in the syntax analysis.
                        AddToken("n, " + finalnum, finalnum.Length, linetokens);

                        break;
                    #endregion

                    #region Seperators
                    case '(':
                    case ')':
                    case ';':
                    case '{':
                    case ',':
                    case '[':
                    case ']':
                    case '}': AddToken("s, " + restline[0], 1, linetokens); break;
                    #endregion

                    case '"':

                        string strterm = CreateIdentifier(curchara => restline[curchara].Equals('"')) + '"';
                        AddToken("l, " + strterm, strterm.Length, linetokens);

                        break;

                    #region Keywords
                    case 'a': CheckForKeyword(["and"], linetokens); break;
                    case 'b': CheckForKeyword(["bool"], linetokens); break;
                    case 'd': CheckForKeyword(["double"], linetokens); break;
                    case 'e': CheckForKeyword(["elif", "else"], linetokens); break;
                    case 'f': CheckForKeyword(["function", "func", "for", "f", "false"], linetokens); break;
                    case 'i': CheckForKeyword(["in", "int", "if", "is"], linetokens); break;
                    case 'n': CheckForKeyword(["not", "null"], linetokens); break;
                    case 'o': CheckForKeyword(["or"], linetokens); break;
                    case 'r': CheckForKeyword(["r", "return"], linetokens); break;
                    case 's': CheckForKeyword(["string"], linetokens); break;
                    case 't': CheckForKeyword(["true"], linetokens); break;
                    case 'w': CheckForKeyword(["while"], linetokens); break;

                    #endregion

                    default:

                        string word = CreateIdentifier(curchara => keywordseperators.Contains(restline[curchara].ToString()));
                        AddToken("i, " + word, word.Length, linetokens);

                        break;
                }
            }  catch (Exception)
            {
                Console.WriteLine("Schwerwiegender Fehler in der Zeichenfolge");
                Environment.Exit(0);
            }
        }
    
        private bool CheckNext(char nextchar)
        {
            //Method for Special Case óperators -> Checks the next Symbol after the current one
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
            //All defined Apecial Cases where an uncommon list of symbols appears in a row (For example negative numbers, or special operators like :=)
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
                    string finalnum = CreateIdentifier(curchara => !double.TryParse(restline[curchara].ToString(), out double a) && !restline[curchara].Equals('.'));
                    AddToken("l, " + finalnum, finalnum.Length, linetokens);
                }
            }
        }

        private string CreateIdentifier(Func<int, bool> condition)
        {
            //Basic Method to create identifiers uses a condition flexibly given to recude code, used for literals alike
            try
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
            catch (Exception)
            {
                Console.WriteLine("Schwerwiegender Fehler in der Zeichenfolge");
                return "null";
            }
        }
        
        private void CheckForKeyword(List<string> keywords, List<string> linetokens)
        {
            try
            {
                //Checks if the a given keyword is a keyword or an identifier, and if their are any special ccharacters between identifiers
                string word = CreateIdentifier(curchara => keywordseperators.Contains(restline[curchara].ToString()));
                bool iskeyword = true;

                foreach (string term in keywords)
                {
                    if (term.Equals(word))
                    {
                        AddToken("k, " + word, word.Length, linetokens);
                        iskeyword = false;
                    }
                }

                if (iskeyword)
                {
                    AddToken("i, " + word, word.Length, linetokens);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Schwerwiegender Fehler in der Zeichenfolge");
                Environment.Exit(0);
            }
        }

        private void AddToken(string token, int remove, List<string> tokens)
        {
            //Add token to list
            tokens.Add(token);
            restline = restline.Remove(0, remove); 
        }
    }
}
