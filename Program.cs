using Antlr4.Runtime;

Console.WriteLine("==============================================");
Console.WriteLine("           ANALISADOR LÉXICO ELGOL");
Console.WriteLine("==============================================");
Console.WriteLine();

Console.Write("Digite o caminho do arquivo .elgol: ");
string? caminhoArquivo = Console.ReadLine();

Console.WriteLine();

if (string.IsNullOrWhiteSpace(caminhoArquivo))
{
    Console.WriteLine("Nenhum arquivo foi informado.");
    return;
}

if (!File.Exists(caminhoArquivo))
{
    Console.WriteLine($"Arquivo não encontrado: {caminhoArquivo}");
    return;
}

string codigo = File.ReadAllText(caminhoArquivo);

AntlrInputStream input = new AntlrInputStream(codigo);
ElgolLexer lexer = new ElgolLexer(input);

CommonTokenStream tokenStream = new CommonTokenStream(lexer);
tokenStream.Fill();

var simbolos = new HashSet<string>();
var erros = new List<(int linha, int coluna, string lexema, string tipo)>();

Console.WriteLine("==============================================");
Console.WriteLine("                 LISTA DE TOKENS");
Console.WriteLine("==============================================");
Console.WriteLine();

Console.WriteLine(
    $"{"LINHA",-8} {"COLUNA",-8} {"TOKEN",-25} {"LEXEMA"}"
);

Console.WriteLine(new string('-', 70));

foreach (IToken token in tokenStream.GetTokens())
{
    if (token.Type == TokenConstants.EOF)
        continue;

    string nomeToken =
        lexer.Vocabulary.GetSymbolicName(token.Type)
        ?? "DESCONHECIDO";

    // ANTLR começa coluna em 0
    int coluna = token.Column + 1;

    // =============================================
    // ERROS LÉXICOS
    // =============================================

    if (nomeToken == "INTEIRO_INVALIDO")
    {
        erros.Add((
            token.Line,
            coluna,
            token.Text,
            "Número inteiro inválido: não pode iniciar com zero"
        ));

        continue;
    }

    if (nomeToken == "ID_INVALIDO")
    {
        erros.Add((
            token.Line,
            coluna,
            token.Text,
            "Identificador inválido"
        ));

        continue;
    }

    if (nomeToken == "FUNCAO_INVALIDA")
    {
        erros.Add((
            token.Line,
            coluna,
            token.Text,
            "Nome de função inválido"
        ));

        continue;
    }

    if (nomeToken == "CARACTERE_INVALIDO")
    {
        erros.Add((
            token.Line,
            coluna,
            token.Text,
            "Caractere inválido"
        ));

        continue;
    }

    // =============================================
    // TOKEN VÁLIDO
    // =============================================

    Console.WriteLine(
        $"{token.Line,-8} " +
        $"{coluna,-8} " +
        $"{nomeToken,-25} " +
        $"{token.Text}"
    );

    // =============================================
    // TABELA DE SÍMBOLOS
    // =============================================

    if (nomeToken == "IDENTIFICADOR" || nomeToken == "FUNCAO")
    {
        simbolos.Add(token.Text);
    }
}


// =============================================
// TABELA DE SÍMBOLOS
// =============================================

Console.WriteLine();
Console.WriteLine("==============================================");
Console.WriteLine("              TABELA DE SÍMBOLOS");
Console.WriteLine("==============================================");
Console.WriteLine();

Console.WriteLine(
    $"{"ID",-8} {"LEXEMA",-25}"
);

Console.WriteLine(new string('-', 40));

int id = 1;

foreach (string simbolo in simbolos)
{
    Console.WriteLine(
        $"{id,-8} {simbolo,-25}"
    );

    id++;
}

// =============================================
// ERROS LÉXICOS
// =============================================

Console.WriteLine();
Console.WriteLine("==============================================");
Console.WriteLine("                 ERROS LÉXICOS");
Console.WriteLine("==============================================");
Console.WriteLine();

if (erros.Count == 0)
{
    Console.WriteLine("Nenhum erro léxico encontrado.");
}
else
{
    Console.WriteLine(
        $"{"LINHA",-8} {"COLUNA",-8} {"LEXEMA",-20} {"ERRO"}"
    );

    Console.WriteLine(new string('-', 75));

    foreach (var erro in erros)
    {
        Console.WriteLine(
            $"{erro.linha,-8} " +
            $"{erro.coluna,-8} " +
            $"{erro.lexema,-20} " +
            $"{erro.tipo}"
        );
    }
}

Console.WriteLine();
Console.WriteLine("==============================================");
Console.WriteLine("Análise léxica finalizada.");
Console.WriteLine("==============================================");