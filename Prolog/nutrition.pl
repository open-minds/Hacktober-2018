entree(salade).
entree(avocat).
entree(soupe).
dessert(raisin).
dessert(melon).
poisson(truite).
poisson(daurade).
viande(steak).
viande(escalope).
calories(salade,15).
calories(raisin,70).
calories(avocat,220).
calories(melon,27).
calories(huitre,70).
calories(soupe,70).
calories(steak,203).
calories(truite,98).
calories(escalope,105).
calories(salade,15).
calories(daurade,90).
plat(X):- viande(X).
plat(X):- poisson(X).
repas(E,P,D):-entree(E),plat(P),dessert(D).
calc(E,P,D,C):-calories(E,X),calories(P,Y),calories(D,Z), C is X+Y+Z.
