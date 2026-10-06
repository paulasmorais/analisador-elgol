lexer grammar ElgolLexer;


// --- PALAVRAS RESERVADAS ---

ELGIO
    : 'elgio'
    ;

DECIMAL
    : 'DECIMAL'
    ;

ZERO
    : '_Z_'
    ;

NEGATIVO
    : '_NEG_'
    ;

EXPONENCIACAO
    : 'EXP'
    ;

RESTO
    : 'RESTO'
    ;

ENQUANTO
    : 'enquanto'
    ;

SE
    : 'se'
    ;

ENTAO
    : 'entao'
    ;

SENAO
    : 'senao'
    ;

PARA
    : 'para'
    ;

INICIO
    : 'inicio'
    ;

FIM
    : 'fim'
    ;

MAIOR
    : 'maior'
    ;

MENOR
    : 'menor'
    ;

IGUAL
    : 'igual'
    ;

DIFERENTE
    : 'diferente'
    ;

MENOR_IGUAL
    : 'migual'
    ;

MAIOR_IGUAL
    : 'MIgual'
    ;


// --- IDENTIFICADORES ---

IDENTIFICADOR
    : ID_VALIDO
    ;


// --- FUNÇÕES ---

FUNCAO
    : '$' ID_VALIDO
    ;


// --- NÚMEROS ---

INTEIRO
    : [1-9] [0-9]*
    ;


// --- OPERADORES ---

ATRIBUICAO
    : '='
    ;

SOMA
    : '+'
    ;

SUBTRACAO
    : '-'
    ;

DIVISAO
    : '/'
    ;

MULTIPLICACAO
    : 'x'
    ;


// --- DELIMITADORES ---

PAREN_ABRE
    : '('
    ;

PAREN_FECHA
    : ')'
    ;
  
VIRGULA 
    : ','
    ;

PONTO
    : '.'
    ;


// --- COMENTÁRIOS ---

COMENTARIO
    : '*' ~[\r\n]* -> skip
    ;


// --- ESPAÇOS E QUEBRAS DE LINHA ---

ESPACOS
    : [ \t\r\n]+ -> skip
    ;


// --- ERROS LEXICOS ---

// Inteiro Inválido
// Números que iniciam com zero
// Ex: 
// 0
// 03
// 034
INTEIRO_INVALIDO 
    : '0' [0-9]*
    ;

// Identificador Inválido
// Ex:
// Vim
// teste
// teste2
// Teste39
// Tes_Te
// LetrA
// Ateras
ID_INVALIDO
    : [A-Za-z0-9_]+
    ;

// Funções Inválidas
// Ex:
// $teste
// $Te34
// $Teste39
// $Te
FUNCAO_INVALIDA
    : '$' [A-Za-z0-9_]+
    ;


// --- CARACTERES NÃO RECONHECIDOS ---

// Ex: @, #, %, &, ?, !
CARACTERE_INVALIDO
    : .
    ;


// --- FRAGMENTOS AUXILIARES ---

// Identificador:
// - começa com consoante maiúscula
// - somente letras
// - mínimo de 4 caracteres
// - termina com letra minúscula

fragment ID_VALIDO
    : [B-DF-HJ-NP-TV-Z]
      [A-Za-z]
      [A-Za-z]+
      [a-z]
    ;