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
INTEGER: '-'? NUMBER;
FLOAT : '-'? NUMBER (DOT [0-9]+)? EXP?;

fragment NUMBER : '0' | [1-9] [0-9]* ;
fragment EXP : [Ee] [+\-]? [0-9]+ ;
fragment HEX : [0-9a-fA-F] ;

STRING
  : '"' ( '\\' ( ["'/bfnrt\\] | 'u' HEX HEX HEX HEX ) | ~["\\\r\n] )* '"'
  | '\''( '\\' ( ["'/bfnrt\\] | 'u' HEX HEX HEX HEX ) | ~['\\\r\n] )* '\''
  ;

IDENTIFIER : [_\p{L}] [_\p{L}\p{N}]*;

WS : [ \t\r\n]+ -> channel(HIDDEN);
COMMENT : '//' ~[\r\n]* -> skip;

// grammar

// RFC specifies either "normalized path" or "regular" path with all the bells and whistles
// But - because "normalized path" is a subset we do not treat it as separate grammar
jsonPath: ROOT pathSegment* EOF;

pathSegment
    : memberSegment             #ChildSegment
    | descendantMemberSegment   #DescendantSegment
    ;
memberSegment
    : DOT STAR                                #WildcardChildSelection
    | DOT property = IDENTIFIER               #MemberNameChildSelection
    | bracketedSelector                       #BracketedChildSelection
    | QUESTION /*placeholder for query expr*/ #QueryChildSelection
    ;

descendantMemberSegment
    : DOT_DOT STAR                                       #WildcardSegment
    | DOT_DOT (property=IDENTIFIER | bracketedSelector)  #SelectorSegment
    ;
  
bracketedSelector: LBRACKET selectors += selector (COMMA selectors += selector)* RBRACKET;

selector
    : property = STRING                                                                            #NameSelector
    | startIndex=INTEGER? c1=COLON endIndex=INTEGER? (c2=COLON step=INTEGER)?                      #SliceSelector
    | QUESTION /* implement expressions*/                                                          #FilterSelector
    | INTEGER                                                                                      #IndexSelector
    | STAR                                                                                         #WildcardSelector
    ;