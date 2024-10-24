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
            //CheckBasicProperties();

            ApplyTokensToRules(tokens);
        }

        //Plan is to add a generalized Method to check for any rule given to it if a sequence of tokens applys to it

        /*
         * -> Reduce literals and identifiers only to the token type and not the acutal value
         * -> Check if a token is found, that apply to the first  token of a rule all following tokens if they apply
         * -> If yes, make a tree structure out of it and delete it from the input
         * 
         */

        private void ApplyTokensToRules(Dictionary<int, List<string>> tokens)
        {
            foreach (List<string> line in tokens.Values)
            {
                for (int i = 0; i < line.Count; i++)
                {
                    string token = line[i];

                    if (line[i].First().Equals('i') || line[i].First().Equals('l'))
                    {
                        token = line[i].First().ToString();
                    }

                    CheckIfRuleApplys(token, line);
                }
            }
        }

        private void CheckIfRuleApplys(string token, List<string> line)
        {
            switch(token)
            {
                case "i":

                    CheckNextTokens([["o, :=", "l", "s, ;"]], line);

                    break;
            }
        }

        private void CheckNextTokens(List<List<string>> lst, List<string> line)
        {
            foreach(List<string> rules in lst)
            {
                foreach(string rule in rules)
                {
                    if(rule)
                }
            }
        }
    }
}
