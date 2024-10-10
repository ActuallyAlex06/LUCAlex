using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUC
{
    internal class Lexer
    {
        Dictionary<int, List<string>> tokens = new Dictionary<int, List<string>>();

        public void StartLexer()
        {
            string inputcode = Recources.ReadFile("Applications/Code.lug");
            Tokenizer(inputcode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList()); ;

            foreach(List<string> tokens in tokens.Values)
            {
                foreach(string token in tokens)
                {
                    Console.Write(token);
                }
                Console.WriteLine("");
            }
        }

        int currentpos = 0;


        private void Tokenizer(List<string> lines)
        {
            int linenum = 1;

            foreach(string line in lines)
            {
                List<string> linetokens = new List<string>();

                while (currentpos < line.Length)
                {
                    char currentoken = line[currentpos];

                    DefineToken(linetokens, currentoken, line);
                }

                tokens.Add(linenum, linetokens);
                linenum++;
                currentpos = 0;
            }
        }
        
        private void DefineToken(List<string> linetokens, char current, string restline)
        {
            currentpos++;

            switch (current)
            {
                #region BasicMath

                    case ' ': break; 
                    case '+': linetokens.Add("(operator, +)"); break;
                    case '-': linetokens.Add("(operator, -)"); break;
                    case '*': linetokens.Add("(operator, *)"); break;
                    case '/': linetokens.Add("(operator, /)"); break;
                    
                #endregion

                #region VariableDef
                case ':':
                    currentpos++;
                    linetokens.Add("(operator, :=)"); break;

                case '=': linetokens.Add("(operator, =)"); break;
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

                    string enterednum = restline[currentpos - 1].ToString();
                    int nextpos = currentpos;
                    while (int.TryParse(restline[nextpos].ToString(), out int a))
                    {                    
                        enterednum = enterednum + "" + a;
                        nextpos++;
                    }

                    currentpos = currentpos + nextpos - currentpos;
                    linetokens.Add("(literal, " + enterednum + ")");

                    break;
                #endregion

                #region Seperators
                case ';': linetokens.Add("(seperator, ;)"); break;
                case '(': linetokens.Add("(seperator, ()"); break;
                case ')': linetokens.Add("(seperator, ))"); break;
                case '{': linetokens.Add("(seperator, {)"); break;
                case '}': linetokens.Add("(seperator, ])"); break;
                #endregion

                case 'i':

                    CheckIfKeyword(linetokens, "int", restline);
                 
                    break;

                case 's':

                    CheckIfKeyword(linetokens, "string", restline);

                    break;

                default:
                    
                    string name = restline[currentpos - 1].ToString();
                    int nextchara = currentpos;
                    
                    while(!restline[nextchara].Equals(' '))
                    {

                        name = name + "" + restline[nextchara];
                        nextchara++;
                    }

                    currentpos = currentpos + nextchara - currentpos;
                    linetokens.Add("(identifier, " + name + ")");

                    break;
            }
        }

        private void CheckIfKeyword(List<string> linetokens, string word, string restline)
        {
            int index = 0;
            bool check = true;

            for(int i = currentpos - 1; i < currentpos + word.Length - 1; i++)
            {
                if (!word[index].Equals(restline[i]))
                {
                    check = false;
                }

                index++;
            }

            if(check ) 
            {
                linetokens.Add("(Keyword, " + word + ")");
                currentpos = currentpos + word.Length - 1;
            }   
        }
    }
}
