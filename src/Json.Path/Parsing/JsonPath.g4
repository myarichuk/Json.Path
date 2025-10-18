grammar JsonPath;
// used this RFC as a guideline: https://datatracker.ietf.org/doc/html/rfc9535

ROOT : '$' ;
CURRENT : '@' ;

DOT_DOT : '..' ; // lexer is greedy so first try to match this
DOT : '.' ;
LBRACKET : '[' ;
RBRACKET : ']' ;
LPAREN : '(' ;
RPAREN : ')' ;
COLON : ':' ;
COMMA : ',' ;
QUESTION : '?' ;
STAR : '*' ;

EQ : '==' ;
NE : '!=' ;
LE : '<=' ;
LT : '<' ;
GE : '>=' ;
GT : '>' ;
AND : '&&' ;
OR : '||' ;
NOT : '!' ;
ADD : '+' ;
SUB : '-' ;
DIV : '/' ;
MOD : '%' ;
TRUE : 'true' ;
FALSE : 'false' ;
NULL : 'null' ;

// numbers --> RFC 8259
NUMBER : '-'? INT ('.' [0-9]+)? EXP?;

fragment INT : '0' | [1-9] [0-9]* ;
fragment EXP : [Ee] [+\-]? [0-9]+ ;
fragment ESC : '\\' ( ["\\/bfnrt] | 'u' HEX HEX HEX HEX );
fragment HEX : [0-9a-fA-F] ;

STRING : 
    '"' ( ESC | ~["\\\r\n] )* '"' 
    | '\'' ( ESC | ~["\\\r\n] )* '\''
    ;

IDENTIFIER : [_\p{L}] [_\p{L}\p{N}]*;

WS : [ \t\r\n]+ -> skip ;
COMMENT : '//' ~[\r\n]* -> skip ;

// grammar

expression: ROOT firstSegment? segment* EOF;
// note, just "$" means whole doc, that is why "segments*"

segment
    : ((DOT? STAR) | identifier | recursiveSegment) indexer*
    ;

recursiveSegment
    : DOT_DOT (identifier | STAR)?
    ;

firstSegment
    : property = IDENTIFIER                         #FirstDotNotation
    | LBRACKET propertyAsString = STRING RBRACKET   #FirstBracketNotation
    ;
    
identifier
    : DOT property = IDENTIFIER                      #DotNotation
    | LBRACKET propertyAsString = STRING RBRACKET    #BracketNotation
    ;

indexer
    : LBRACKET NUMBER RBRACKET            #ArrayIndex
    | LBRACKET STRING RBRACKET            #PropertyIndex
    ;

// TODO: slice/filter grammar