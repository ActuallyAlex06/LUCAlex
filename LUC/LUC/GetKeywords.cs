using System.CodeDom.Compiler;
using System.Drawing;
using System.Text.Json;

public class GetKeywords
{
    public Dictionary<string, List<string>> keywords;
    public GetKeywords()
    {
        this.keywords = ReadKeywords();
    }


    public Dictionary<string, List<string>> ReadKeywords()
    {
        string filePath = "keywords.json";
        string fileContent = File.ReadAllText(filePath);
        Dictionary<string, List<string>> keywords = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(fileContent);
        return keywords;
    }

    // if the user accidentially deleted the keywords file a new one gets created
    public void GenerateStandardKeywordsFile()
    {
        string filePath = "keywords.json";
        if(!File.Exists(filePath))
        {
            Dictionary<string, List<string>> jsonData = new Dictionary<string, List<string>>(){
                // functions
                {"function", new List<string>{"func", "f", "fu"}},
                {"pure", new List<string>{"p", "purefunction", "purefunc"}},
                {"return", new List<string>{"r", "ret", "give_back"}},
                {"filter", new List<string>{""}}, 
                {"map", new List<string>{""}},


                // Datastructures
                {"int", new List<string>{"number", "num"}},
                {"string", new List<string>{"chars"}},
                {"bool", new List<string>{"b", "boolean", "trueOrFalse"}},
                {"decimal", new List<string>{"double", "float", "dec"}},
                {"[string]", new List<string>{"strings"}},
                {"[int]", new List<string>{"ints", "numbers", "[num]"}},
                {"[bool]", new List<string>{"bools"}},
                
                // Logical and comparision operator
                {"==", new List<string>{"=", "equal", "equ", "eq"}},
                {"!=", new List<string>{"n_eq", "not_equal", "no_eq", "no_equ", "n_e"}},
                {">", new List<string>{"greater", "larger"}},
                {"<", new List<string>{"smaller"}},
                {">=", new List<string>{"greater_or_equal", "goe", "g_o_e"}},
                {"<=", new List<string>{"smaller_or_equal", "soe"}},
                {"and", new List<string>{"too"}},
                {"or", new List<string>{""}},
                
                // Variables
                {":=", new List<string>{"is"}},

                // Loops
                {"for", new List<string>{""}},
                {"while", new List<string>{"wh", "whil", "during"}},

                // Builtin functions
                {"print", new List<string>{"write_line"}},
            };
            File.WriteAllText(filePath, JsonSerializer.Serialize(jsonData, new JsonSerializerOptions {WriteIndented = true}));
        }
    }

    public List<string> GetSynonyms(string keyword)
    {
        return keywords[keyword];
    }

    // checks if string is a keyword (synonyms also count)
    public bool IsKeyword(string word)
    {
        try
        {
            foreach(string keyWord in keywords.Keys)
            {
                if(keyWord.Equals(word))
                {
                    return true;
                }
                foreach(string synonym in keywords[keyWord])
                {
                    Console.WriteLine(synonym);
                    if(synonym.Equals(word))
                    {
                        return true;
                    }
                }
            } 
        }
        catch(Exception ex)
        {
            Console.WriteLine("this is an exception");
            return false;
        }
        return false;
    }

    public string GetKeywordFromSynonym(string synonym)
    {
        foreach(string word in keywords.Keys)
        {
            foreach(string syn in keywords[word])
            {
                if(syn.Equals(synonym))
                {
                    return word;
                }
            }
        }
        return "";
    }
}
