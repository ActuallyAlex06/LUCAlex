namespace LUC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Lexer lex = new Lexer();
            lex.DoLexer();

            SyntaxAnalysis analysis = new SyntaxAnalysis();
            analysis.Analyse();
        }
    }
}
