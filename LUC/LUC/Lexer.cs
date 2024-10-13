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
                case 'i': CheckForKeyword(["int", "integer"], linetokens); break;
                case 's': CheckForKeyword(["string"], linetokens); break;
                case 'b': CheckForKeyword(["bool"], linetokens); break;
                #endregion

                default:

                    string word = CreateIdentifier(curchara => restline[curchara].Equals(' '));
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
            string word = CreateIdentifier(curchara => restline[curchara].Equals(' '));
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
