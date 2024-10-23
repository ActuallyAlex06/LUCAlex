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