namespace LUC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // TOOD Get input from file
            Lexer lex = new Lexer();
            // inconsistent naming should be Lex in my opinion
            lex.DoLexer();

            SyntaxAnalysis analysis = new SyntaxAnalysis();
            analysis.Analyse();
        }
    }
}
