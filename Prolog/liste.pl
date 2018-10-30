elts_com(L,M):- member(X,L),member(X,M).
disjoint(Z,Y):- not(elts_com(Z,Y)).