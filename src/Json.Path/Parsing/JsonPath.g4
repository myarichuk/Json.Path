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

WS : [ \t\r\n]+ -> skip ;
COMMENT : '//' ~[\r\n]* -> skip ;

// grammar

jsonPath: ROOT indexer* segment* EOF;
// note, just "$" means whole doc, that is why "segments*"

segment
    : DOT property = IDENTIFIER indexer*                    #PropertyNamed
    | { !(this.InputStream.LA(-1) == JsonPathLexer.DOT_DOT_STAR) }? LBRACKET property = STRING RBRACKET indexer*  #PropertyBracketed
    | DOT STAR indexer*                                     #Wildcard
    | DOT_DOT IDENTIFIER indexer*                           #Recursive
    | DOT_DOT_STAR                                          #RecursiveWildcard
    ;
    
indexer: 
      LBRACKET index = INTEGER RBRACKET                                                                                #NumberIndex
    | LBRACKET STAR RBRACKET                                                                                               #WildcardIndex
    | LBRACKET (start = INTEGER)? COLON (end = INTEGER)? ({ this.InputStream.LA(1) == JsonPathLexer.COLON && this.InputStream.LA(2) == JsonPathLexer.INTEGER }?COLON step = INTEGER { Int32.Parse(($step)?.Text ?? "0") != 0 }?)? RBRACKET #SliceIndex
    ;