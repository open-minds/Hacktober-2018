fact(1,1).
fact(A,B) :- C is A - 1 , fact(C,D) , B is A * D.
