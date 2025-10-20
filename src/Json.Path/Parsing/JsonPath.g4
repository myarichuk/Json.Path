grammar JsonPath;
// used this RFC as a guideline: https://datatracker.ietf.org/doc/html/rfc9535

ROOT : '$' ;
CURRENT : '@' ;
DOT_DOT_STAR: '..*';
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
INTEGER: '-'? INT;
NUMBER : '-'? INT ('.' [0-9]+)? EXP?;

fragment INT : '0' | [1-9] [0-9]* ;
fragment EXP : [Ee] [+\-]? [0-9]+ ;
fragment ESC : '\\' ( ["'\\/bfnrt] | 'u' HEX HEX HEX HEX );
fragment HEX : [0-9a-fA-F] ;

STRING
  : '"' ( '\\' ( ["'/bfnrt\\] | 'u' HEX HEX HEX HEX ) | ~["\\\r\n] )* '"'
  | '\''( '\\' ( ["'/bfnrt\\] | 'u' HEX HEX HEX HEX ) | ~['\\\r\n] )* '\''
  ;

IDENTIFIER : [_\p{L}] [_\p{L}\p{N}]*;

WS : [ \t\r\n]+ -> channel(HIDDEN);
COMMENT : '//' ~[\r\n]* -> skip;

// grammar

jsonPath: ROOT indexer* segment* EOF;
// note, just "$" means whole doc, that is why "segments*"

segment
    : DOT property = IDENTIFIER indexer*                    #PropertyNamed
    | LBRACKET property = STRING RBRACKET indexer*          #PropertyBracketed
    | DOT STAR indexer*                                     #Wildcard
    | DOT_DOT IDENTIFIER indexer*                           #Recursive
    | DOT_DOT_STAR                                          #RecursiveWildcard
    ;
    
indexer 
    : LBRACKET index = INTEGER RBRACKET              #NumericIndex
    | LBRACKET STAR RBRACKET                         #WildcardIndex
    | LBRACKET slice RBRACKET                        #SliceIndex 
    | LBRACKET unionItem (COMMA unionItem)+ RBRACKET #UnionIndex                                                                               
    ;
    
unionItem
    : INTEGER   #IndexSelector
    | STRING    #PropertyNameSelector
    | slice     #SliceSelector
    ;
    
slice
    : startIndex=INTEGER? c1=COLON endIndex=INTEGER? (c2=COLON step=INTEGER)?
    ;
