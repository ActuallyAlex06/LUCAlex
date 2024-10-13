using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUC
{
    public static class Recources
    {
        public static string ReadFile(string filepath)
        {
            try
            {
                string filetext = string.Empty;

                using (StreamReader reader = new StreamReader(filepath, Encoding.UTF8))
                {
                    filetext = reader.ReadToEnd();
                }

                return filetext;
            }
            catch (Exception)
            {
                // COMMENT: Why should the program keep running after this case?
                Console.WriteLine("Could not find File!");
                return "";
            }
        }
    }
}
