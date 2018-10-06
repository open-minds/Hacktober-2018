
public class Joueur {
 private String Pseudo;
private static   int score  ;
public Joueur(String Pseudo) {
	this.Pseudo=Pseudo;
	this.score=score;
}

public String getPseudo ()
{
	return Pseudo;
}
public void setPseudo(String Pseudo)
{
	this.Pseudo = Pseudo;
}
public int getScore ()
{
	return score;
}
public void setscore (int score)
{
	this.score = score;
}
public static int score (int point) {
	return score+=point;
}
}
