using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LUC
{
    internal class Lexer
    {
        Dictionary<int, List<string>> tokens = new Dictionary<int, List<string>>();

        public void StartLexer()
        {
            string inputcode = Recources.ReadFile("Applications/Code.lug");
            Tokenizer(inputcode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList()); ;

            foreach (List<string> tokens in tokens.Values)
            {
                foreach (string token in tokens)
                {
                    Console.Write(token);
                }
                Console.WriteLine("");
            }
        }

        string restline = String.Empty;

        private void Tokenizer(List<string> lines)
        {
            int linenum = 1;

            foreach(string line in lines)
            {
                List<string> linetokens = new List<string>();
                restline = line;

                while (restline.Length > 0)
                {
                    DefineTokens(linetokens);
                }

                tokens.Add(linenum, linetokens);
                linenum++;
            }
        }
        
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

                    string finalnum = restline[0].ToString();
                    int nextchar = 1;
                    while (int.TryParse(restline[nextchar].ToString(), out int a))
                    {
                        finalnum = finalnum + "" + a;
                        nextchar++;
                    }

                    AddToken("(literal, " + finalnum + ")", nextchar, linetokens);

                break;
                #endregion

                #region Seperators

                case ';': 
                case '(': 
                case ')': 
                case '{': 
                case '}': AddToken("(seperator, " + restline[0] + ")", 1, linetokens); break;
                #endregion

                #region Keywords
                case 'i': CheckIfKeyword(linetokens, "int"); break; 
                #endregion

                default:

                    string name = restline[0].ToString();

                    int nextchara = 1;
                    while (!restline[nextchara].Equals(' '))
                    {
                        name = name + "" + restline[nextchara];
                        nextchara++;
                    }

                    AddToken("(identifier, " +  name + ")", nextchara, linetokens);

                    break;
            }
        }

        private void AddToken(string token, int remove, List<string> tokens)
        {
            tokens.Add(token);
            restline = restline.Remove(0, remove);
        }

        private void CheckIfKeyword(List<string> linetokens, string word)
        {
            bool check = true;

            for (int i = 0; i < word.Length; i++)
            {
                if (!word[i].Equals(restline[i]))
                {
                    check = false;
                }
            }

            if (check)
            {
                AddToken("(keyword, " + word + ")", word.Length, linetokens);
            }
        }
    }
}
