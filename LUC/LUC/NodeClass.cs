using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUC
{
    class TokenClass
    {
        public string token; 
        public TokenClass parenttoken { get; set; }
        public List<TokenClass> childtoken { get; set; }

        public TokenClass(string token)
        {
            this.token = token;
            this.childtoken = new List<TokenClass>();
        }

        public TokenClass AddChild(string child)
        {
            TokenClass tokenchild = new TokenClass(child)
            {
                parenttoken = this 
            };

            this.childtoken.Add(tokenchild);
            return tokenchild;
        }

    }
}
