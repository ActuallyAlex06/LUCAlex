using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUC
{
    public class SyntaxAnalysis
    {
        Dictionary<int, List<string>> rules = new Dictionary<int, List<string>> { };

        public void Analyse()
        {
            AddRules();
            AnalyseTokens();
        }

        private void AddRules()
        {
            rules[0] = [
                
                "k|i|o, =|l|s, ;",
                "i|o, :=|l|s, ;",

                ];
        }

        //Plan is to add a generalized Method to check for any rule given to it if a sequence of tokens applys to it

        /*
         * -> Reduce literals and identifiers only to the token type and not the acutal value
         * -> Check if a token is found, that apply to the first  token of a rule all following tokens if they apply
         * -> If yes, make a tree structure out of it and delete it from the input
         * 
         */

        private void AnalyseTokens()
        {

        }
    }
}
