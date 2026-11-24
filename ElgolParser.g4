parser grammar ElgolParser;

options {
    tokenVocab = ElgolLexer;
}


// --- PROGRAMA E BLOCOS ---

// o programa é uma sequência de "unidades"
// um erro em uma unidade nunca derruba a regra inicial, o laço continua na próxima unidade
programa
    : unidade* EOF
    ;

// 'comando' aparece aqui apenas para recuperação de erros
unidade
    : declaracaoFuncao
    | bloco
    | comando
    | NOVA_LINHA
    ;

bloco
    : INICIO PONTO quebraLinha
      (comando | NOVA_LINHA)*
      FIM PONTO quebraLinha?
    ;

comando
    : declaracaoVariavel
    | atribuicao
    | valorNegativo
    | condicional
    | enquanto
    | para
    ;


// --- QUEBRA DE LINHA ---
// uma ou mais quebras consecutivas

quebraLinha
    : NOVA_LINHA+
    ;

// --- REGRAS AUXILIARES PARA RECUPERAÇÃO ---
// aceitam os tokens inválidos do léxico (ID_INVALIDO, INTEIRO_INVALIDO, FUNCAO_INVALIDA)
// um erro léxico é reportado uma vez só, pelo léxico, sem gerar também um erro sintático

identificador
    : IDENTIFICADOR
    | ID_INVALIDO
    ;

inteiro
    : INTEIRO
    | INTEIRO_INVALIDO
    ;

nomeFuncao
    : FUNCAO
    | FUNCAO_INVALIDA
    | IDENTIFICADOR
    ;

// --- DECLARAÇÃO DE VARIÁVEIS ---

declaracaoVariavel
    : DECIMAL identificador PONTO quebraLinha
    ;


// --- ATRIBUIÇÕES E EXPRESSÕES MATEMÁTICAS ---

atribuicao
    : IDENTIFICADOR ATRIBUICAO expressaoMatematica PONTO quebraLinha
    ;

expressaoMatematica
    : operando (operadorMatematico operando)*
    ;

operando
    : identificador
    | inteiro
    | ZERO
    | chamadaFuncao
    ;

operadorMatematico
    : SOMA
    | SUBTRACAO
    | MULTIPLICACAO
    | DIVISAO
    | EXPONENCIACAO
    | RESTO
    ;


// --- DECLARAÇÃO DE FUNÇÕES ---

declaracaoFuncao
    : DECIMAL nomeFuncao
      PAREN_ABRE listaParametros PAREN_FECHA
      PONTO quebraLinha blocoFuncao
    ;

listaParametros
    : (parametro (VIRGULA parametro)*)?
    ;

parametro
    : DECIMAL identificador
    ;

blocoFuncao
    : (INICIO PONTO quebraLinha)?
      (comando | NOVA_LINHA)*
      retorno?
      FIM PONTO quebraLinha?
    ;


// --- CHAMADA DE FUNÇÕES ---

chamadaFuncao
    : FUNCAO PAREN_ABRE listaArgumentos PAREN_FECHA
    ;

listaArgumentos
    : (argumento (VIRGULA argumento)*)?
    ;

argumento
    : identificador
    | inteiro
    | ZERO
    ;


// --- RETORNO DA FUNÇAO: ELGIO ---

retorno
    : ELGIO ATRIBUICAO expressaoSemChamada PONTO quebraLinha
    ;

expressaoSemChamada
    : operandoSimples (operadorMatematico operandoSimples)*
    ;

operandoSimples
    : identificador
    | inteiro
    | ZERO
    ;


// --- ESTRUTURA CONDICIONAL: SE, ENTAO, SENAO ---

condicional
    : SE condicao PONTO quebraLinha
      ENTAO PONTO quebraLinha
      bloco
      SENAO PONTO quebraLinha
      bloco
    ;

condicao
    : operandoCondicional operadorRelacional operandoCondicional
    ;

operandoCondicional
    : IDENTIFICADOR
    | INTEIRO
    | ZERO
    ;

operadorRelacional
    : MAIOR
    | MENOR
    | IGUAL
    | DIFERENTE
    | MENOR_IGUAL
    | MAIOR_IGUAL
    ;


// --- ESTRUTURA DE REPETIÇÃO: ENQUANTO ---

enquanto
    : ENQUANTO condicao PONTO quebraLinha bloco
    ;


// --- ESTRUTURA DE REPETIÇÃO: PARA

para
    : PARA IDENTIFICADOR (INTEIRO | IDENTIFICADOR)
      PONTO quebraLinha bloco
    ;


// --- INSTRUÇÃO ESPECIAL: _NEG_ ---

valorNegativo
    : NEGATIVO IDENTIFICADOR PONTO quebraLinha
    ;