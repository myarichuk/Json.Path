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
REGEX: '~=';
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
NUMBER
    : '-'? INT ('.' [0-9]+)? ([Ee][+\-]?[0-9]+)?
    ;

fragment INT: '0' | [1-9] [0-9]*;
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
    ;

descendantMemberSegment
    : DOT_DOT STAR                                       #WildcardSegment
    | DOT_DOT (property=IDENTIFIER | bracketedSelector)  #SelectorSegment
    ;
  
bracketedSelector: LBRACKET selectors += selector (COMMA selectors += selector)* RBRACKET;

selector
    : property = STRING                                                                            #NameSelector
    | startIndex=NUMBER? c1=COLON endIndex=NUMBER? (c2=COLON step=NUMBER)?                         #SliceSelector
    | query                                                                                        #FilterSelector
    | NUMBER                                                                                       #IndexSelector
    | STAR                                                                                         #WildcardSelector
    ;
    
query: QUESTION LPAREN expression RPAREN;

expression
    : <assoc=right> NOT expression                                               #NotExpression
    | expression REGEX STRING                                                    #RegexExpression
    | expression op=(STAR | DIV | MOD) right=expression                          #MulDivExpression
    | expression op=(ADD | SUB) right=expression                                 #AddSubExpression
    | expression op=(GE | GT | LE | LT) right=expression                         #ComparisonExpression
    | expression op=(EQ | NE) right=expression                                   #EqualityExpression
    | expression op=AND right=expression                                         #AndExpression
    | expression op=OR right=expression                                          #OrExpression
    | (ROOT | CURRENT) pathSegment+                                              #PathExpression
    | function=IDENTIFIER LPAREN (params+=expression (COMMA params+=expression)*)? RPAREN #FunctionExpression
    | TRUE                                                                      #TrueLiteralExpression
    | FALSE                                                                     #FalseLiteralExpression
    | NULL                                                                      #NullLiteralExpression
    | STRING                                                                    #StringLiteralExpression
    | NUMBER                                                                    #NumericLiteralExpression
    | LPAREN expression RPAREN                                                  #ParenthesisExpression
    ;
   
    
