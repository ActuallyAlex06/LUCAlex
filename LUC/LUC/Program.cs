namespace LUC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Lexer lex = new Lexer();
            // COMMENT: StartLexer is bad naming because it does the entire lexinig and is not just the start.
            lex.StartLexer();
        }
    }
}
