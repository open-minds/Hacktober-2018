suspect(X) :- present(X, L, J), vol(L, J, V), apuvoler(X, V).
apuvoler(X, _) :- sansargent(X).
apuvoler(X, Y) :- jaloux(X, Y).
vol(hipp, lundi, marie).
vol(bar, mardi, jean).
vol(stade, jeudi, luc).
sansargent(max).
jaloux(eve, marie).
present(max, bar, mercredi).
present(eric, bar, mardi).
present(eve, hipp, lundi).