using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

Titulo("ANALISADOR LÉXICO E SINTÁTICO ELGOL");

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

// criação do lexer
ElgolLexer lexer = new ElgolLexer(new AntlrInputStream(codigo));

CommonTokenStream tokenStream = new CommonTokenStream(lexer);
tokenStream.Fill();

// tabela de símbolos: identificadores, funções e números
// o valor só se aplica a números; _Z_ é registrado com valor 0
var simbolos = new Dictionary<string, (string tipo, int linha, string valor)>();
var errosLexicos = new List<(int linha, int coluna, string lexema, string tipo)>();
var errosSintaticos = new List<ErroSintatico>();

// Tokens de erro do lexer e a mensagem de cada um
var mensagensErroLexico = new Dictionary<int, string>
{
    [ElgolLexer.INTEIRO_INVALIDO] = "Número inteiro inválido: não pode iniciar com zero",
    [ElgolLexer.ID_INVALIDO] = "Identificador inválido",
    [ElgolLexer.FUNCAO_INVALIDA] = "Nome de função inválido",
    [ElgolLexer.CARACTERE_INVALIDO] = "Caractere inválido",
};

// ===========================================
//               LISTA DE TOKENS
// ===========================================

Titulo("LISTA DE TOKENS");

Console.WriteLine($"{"LINHA",-8} {"COLUNA",-8} {"TOKEN",-25} {"LEXEMA"}");
Console.WriteLine(new string('-', 70));

foreach (IToken token in tokenStream.GetTokens())
{
    // Ignora o fim de arquivo e a exibição das quebras de linha
    if (token.Type == TokenConstants.EOF || token.Type == ElgolLexer.NOVA_LINHA)
        continue;

    string nomeToken = lexer.Vocabulary.GetSymbolicName(token.Type) ?? "DESCONHECIDO";

    // ANTLR começa coluna em 0
    int coluna = token.Column + 1;

    if (mensagensErroLexico.TryGetValue(token.Type, out string? erroLexico))
    {
        errosLexicos.Add((token.Line, coluna, token.Text, erroLexico));
        continue;
    }

    Console.WriteLine($"{token.Line,-8} {coluna,-8} {nomeToken,-25} {token.Text}");

    if (token.Type is ElgolLexer.IDENTIFICADOR or ElgolLexer.FUNCAO)
        simbolos.TryAdd(token.Text, (nomeToken, token.Line, "-"));
    else if (token.Type == ElgolLexer.INTEIRO)
        simbolos.TryAdd(token.Text, (nomeToken, token.Line, token.Text));
    else if (token.Type == ElgolLexer.ZERO)
        simbolos.TryAdd(token.Text, (nomeToken, token.Line, "0"));
}

// ===========================================
//             TABELA DE SÍMBOLOS
// ===========================================

Titulo("TABELA DE SÍMBOLOS");

Console.WriteLine($"{"ID",-8} {"TIPO",-20} {"LEXEMA",-20} {"LINHA",-8} {"VALOR"}");
Console.WriteLine(new string('-', 75));

int id = 1;

foreach (var (lexema, (tipo, linha, valor)) in simbolos)
    Console.WriteLine($"{id++,-8} {tipo,-20} {lexema,-20} {linha,-8} {valor}");

// ===========================================
//                ERROS LÉXICOS
// ===========================================

Titulo("ERROS LÉXICOS");

if (errosLexicos.Count == 0)
{
    Console.WriteLine("Nenhum erro léxico encontrado.");
}
else
{
    Console.WriteLine($"{"LINHA",-8} {"COLUNA",-8} {"LEXEMA",-20} {"ERRO"}");
    Console.WriteLine(new string('-', 75));

    foreach (var erro in errosLexicos)
        Console.WriteLine($"{erro.linha,-8} {erro.coluna,-8} {erro.lexema,-20} {erro.tipo}");
}

// ===========================================
//              ANÁLISE SINTÁTICA
// ===========================================

Titulo("ERROS SINTÁTICOS");

// volta o fluxo de tokens para o início
tokenStream.Seek(0);

// criação do parser
ElgolParser parser = new ElgolParser(tokenStream);

// troca o tratamento padrão de erros do ANTLR pelo personalizado
parser.RemoveErrorListeners();
parser.AddErrorListener(new ErroSintaticoListener(errosSintaticos));
parser.ErrorHandler = new EstrategiaRecuperacaoElgol();

var arvore = parser.programa();

ParseTreeWalker.Default.Walk(new VerificadorFuncaoListener(errosSintaticos), arvore);

if (errosSintaticos.Count == 0)
{
    Console.WriteLine("Nenhum erro sintático encontrado.");
}
else
{
    // os erros do parser e os do VerificadorFuncaoListener são coletados
    // em momentos diferentes; ordenar deixa a saída na ordem do arquivo
    foreach (var erro in errosSintaticos.OrderBy(e => e.Linha).ThenBy(e => e.Coluna))
        Console.WriteLine($"Linha {erro.Linha}, coluna {erro.Coluna}: {erro.Mensagem}");
}

// ===========================================
//                FINALIZAÇÃO
// ===========================================

Titulo("Análise finalizada");


// imprime um cabeçalho de seção centralizado
static void Titulo(string texto)
{
    const int largura = 45;
    string borda = new string('=', largura);

    Console.WriteLine();
    Console.WriteLine(borda);
    Console.WriteLine(texto.PadLeft((largura + texto.Length) / 2));
    Console.WriteLine(borda);
    Console.WriteLine();
}


// ===========================================
//          REGISTRO DE ERRO SINTÁTICO
// ===========================================

public record ErroSintatico(int Linha, int Coluna, string Mensagem);


// ===========================================
//    CLASSE PARA CAPTURAR ERROS SINTÁTICOS
// ===========================================

public class ErroSintaticoListener(List<ErroSintatico> erros) : BaseErrorListener
{
    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        erros.Add(new ErroSintatico(line, charPositionInLine + 1, msg));
    }
}


// ===========================================
//   VERIFICAÇÕES FEITAS DEPOIS DA ANÁLISE
// ===========================================

public class VerificadorFuncaoListener(List<ErroSintatico> erros) : ElgolParserBaseListener
{
    private void Erro(IToken t, string msg) =>
        erros.Add(new ErroSintatico(t.Line, t.Column + 1, msg));

    // 'nomeFuncao' só existe dentro de 'declaracaoFuncao'; se o nome
    // veio como IDENTIFICADOR, faltou o '$'
    public override void ExitNomeFuncao(ElgolParser.NomeFuncaoContext context)
    {
        if (context.IDENTIFICADOR() != null)
            Erro(context.Start, "nome de função inválido: deve começar com '$'.");
    }

    public override void ExitBlocoFuncao(ElgolParser.BlocoFuncaoContext context)
    {
        if (context.INICIO() == null)
            Erro(context.Start, "bloco de função deve começar com 'inicio.'.");
    }

    // comando no nível mais externo: a gramática aceita só para não
    // "perder" o resto do arquivo; aqui ele vira erro
    public override void ExitUnidade(ElgolParser.UnidadeContext context)
    {
        if (context.comando() != null)
            Erro(context.Start,
                "comando fora de bloco: todo comando deve estar entre 'inicio.' e 'fim.' " +
                "(verifique se algum bloco foi fechado antes da hora).");
    }
}


// ===========================================
//      ESTRATÉGIA DE RECUPERAÇÃO DE ERROS
// ===========================================

public class EstrategiaRecuperacaoElgol : DefaultErrorStrategy
{
    // palavras reservadas, para avisar quando aparecem com maiúsculas/minúsculas erradas
    private static readonly string[] PalavrasReservadas =
        ["DECIMAL", "inicio", "fim", "se", "entao", "senao", "enquanto", "para", "elgio"];


    // a recuperação padrão do ANTLR descarta tokens até achar qualquertoken que possa vir a seguir
    // e gera erros falsos em cascata
    public override void Recover(Parser recognizer, RecognitionException e)
    {
        ITokenStream tokens = recognizer.TokenStream;
        IToken anterior = tokens.LT(-1);

        if (anterior == null || anterior.Type == ElgolParser.NOVA_LINHA)
        {
            base.Recover(recognizer, e);
            return;
        }

        if (lastErrorIndex == tokens.Index && lastErrorStates?.Contains(recognizer.State) == true)
            recognizer.Consume();

        lastErrorIndex = tokens.Index;
        lastErrorStates ??= new IntervalSet();
        lastErrorStates.Add(recognizer.State);

        while (tokens.LA(1) != ElgolParser.NOVA_LINHA && tokens.LA(1) != TokenConstants.EOF)
            recognizer.Consume();
    }

    // na regra inicial, uma exceção faria o ANTLR descartar tudo até o EOF
    //  informamos e pulamos até um ponto seguro
    public override void Sync(Parser recognizer)
    {
        try
        {
            base.Sync(recognizer);
        }
        catch (RecognitionException) when (recognizer.Context is ElgolParser.ProgramaContext)
        {
            ReportUnwantedToken(recognizer);
            ConsumeUntil(recognizer, GetExpectedTokens(recognizer).Or(GetErrorRecoverySet(recognizer)));
        }
    }

    // CASO ESPECIAL: faltou 'inicio' mas veio "<outra coisa> ."
    public override IToken RecoverInline(Parser recognizer)
    {
        if (!recognizer.GetExpectedTokens().Contains(ElgolParser.INICIO) ||
            recognizer.InputStream.LA(2) != ElgolParser.PONTO)
        {
            return base.RecoverInline(recognizer);
        }

        IToken tokenEncontrado = recognizer.CurrentToken;
        IToken tokenAusente = GetMissingSymbol(recognizer);

        // não reporta enquanto ainda está se recuperando de um erro anterior
        if (!InErrorRecoveryMode(recognizer))
            recognizer.NotifyErrorListeners(
                tokenEncontrado,
                $"esperado 'inicio.', mas foi encontrado '{tokenEncontrado.Text}'.",
                null);

        BeginErrorCondition(recognizer);
        recognizer.Consume();
        EndErrorCondition(recognizer);

        return tokenAusente;
    }

    // MENSAGENS EM PORTUGUÊS

    // "missing X at Y"  (o ANTLR inseriu um token que faltava)
    protected override void ReportMissingToken(Parser recognizer)
    {
        if (InErrorRecoveryMode(recognizer))
            return;

        BeginErrorCondition(recognizer);

        IToken atual = recognizer.CurrentToken;
        IToken anterior = recognizer.TokenStream.LT(-1);

        if (DicaPalavraReservada(anterior) is string dica)
            recognizer.NotifyErrorListeners(anterior, dica, null);
        else
            recognizer.NotifyErrorListeners(
                atual,
                $"faltou {Descrever(GetExpectedTokens(recognizer), recognizer.Vocabulary)} antes de {Encontrado(atual)}.",
                null);
    }

    // "extraneous input X"  (o ANTLR descartou um token sobrando)
    protected override void ReportUnwantedToken(Parser recognizer)
    {
        if (InErrorRecoveryMode(recognizer))
            return;

        BeginErrorCondition(recognizer);

        IToken atual = recognizer.CurrentToken;
        string esperado = Descrever(GetExpectedTokens(recognizer), recognizer.Vocabulary);

        Notificar(recognizer, atual, null, $"{Encontrado(atual)} inesperado; esperado {esperado}.");
    }

    // "mismatched input X expecting Y"
    protected override void ReportInputMismatch(Parser recognizer, InputMismatchException e)
    {
        IToken t = e.OffendingToken;
        IntervalSet esperados = e.GetExpectedTokens();

        string? msgContexto = recognizer.Context switch
        {
            ElgolParser.AtribuicaoContext when EhRelacional(t.Type) =>
                "atribuição aceita somente expressões matemáticas; " +
                $"o operador relacional '{t.Text}' não é permitido.",

            ElgolParser.CondicionalContext when esperados.Contains(ElgolParser.SENAO) =>
                $"estrutura 'se' sem 'senao': esperado 'senao.', mas foi encontrado {Encontrado(t)}.",

            _ => null
        };

        string msg = MensagemOperando(recognizer, t)
                     ?? msgContexto
                     ?? DicaPalavraReservada(t)
                     ?? $"esperado {Descrever(esperados, recognizer.Vocabulary)}, mas foi encontrado {Encontrado(t)}.";

        recognizer.NotifyErrorListeners(t, msg, e);
    }

    // "no viable alternative at input X"
    protected override void ReportNoViableAlternative(Parser recognizer, NoViableAltException e)
    {
        Notificar(recognizer, e.OffendingToken, e, $"construção inválida a partir de {Encontrado(e.OffendingToken)}.");
    }

    // ---------------- auxiliares ----------------

    // usa a mensagem mais específica disponível
    // senão usa a padrão
    private static void Notificar(Parser recognizer, IToken t, RecognitionException? e, string padrao)
    {
        string msg = MensagemOperando(recognizer, t) ?? DicaPalavraReservada(t) ?? padrao;
        recognizer.NotifyErrorListeners(t, msg, e);
    }

    // erros onde se esperava um operando
    private static string? MensagemOperando(Parser recognizer, IToken t)
    {
        bool emOperandoSimples = recognizer.Context is ElgolParser.OperandoSimplesContext;

        if (emOperandoSimples && t.Type == ElgolParser.FUNCAO)
            return "o retorno 'elgio' não pode conter chamada de função.";

        if (emOperandoSimples || recognizer.Context is ElgolParser.OperandoContext)
            return $"faltou um operando antes de {Encontrado(t)}: " +
                   "todo operador matemático precisa de dois operandos.";

        return null;
    }

    private static bool EhRelacional(int tipo) =>
        tipo is ElgolParser.MAIOR or ElgolParser.MENOR or ElgolParser.IGUAL or
                ElgolParser.DIFERENTE or ElgolParser.MENOR_IGUAL or ElgolParser.MAIOR_IGUAL;

    private static string? DicaPalavraReservada(IToken? t)
    {
        if (t == null || (t.Type != ElgolParser.IDENTIFICADOR && t.Type != ElgolParser.ID_INVALIDO))
            return null;

        string? correta = PalavrasReservadas.FirstOrDefault(
            p => p.Equals(t.Text, StringComparison.OrdinalIgnoreCase));

        return correta != null && t.Text != correta
            ? $"'{t.Text}' não é palavra reservada da Elgol; você quis dizer '{correta}'?"
            : null;
    }

    private static string Encontrado(IToken t) => t.Type switch
    {
        TokenConstants.EOF => "fim do arquivo",
        ElgolParser.NOVA_LINHA => "quebra de linha",
        _ => $"'{t.Text}'"
    };

    private static string NomeToken(int tipo, IVocabulary vocabulario) => tipo switch
    {
        TokenConstants.EOF => "fim do arquivo",
        ElgolParser.NOVA_LINHA => "quebra de linha",
        ElgolParser.IDENTIFICADOR or ElgolParser.ID_INVALIDO => "identificador",
        ElgolParser.INTEIRO or ElgolParser.INTEIRO_INVALIDO => "número inteiro",
        ElgolParser.FUNCAO or ElgolParser.FUNCAO_INVALIDA => "nome de função",
        _ => vocabulario.GetDisplayName(tipo)
    };

    private static string Descrever(IntervalSet conjunto, IVocabulary vocabulario)
    {
        var nomes = conjunto.ToList().Select(t => NomeToken(t, vocabulario)).Distinct().ToList();

        return nomes.Count switch
        {
            0 => "outro token",
            1 => nomes[0],
            _ => string.Join(", ", nomes.Take(nomes.Count - 1)) + " ou " + nomes[^1]
        };
    }
}