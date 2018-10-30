public class Joueur extends Jeu
{
	private String name;
	private int score;

	public Joueur ()
	{
		score = 0;
	}

	public void setName (String name)
	{
		this.name = name;
	}

	public String getName ()
	{
		return name;
	}

	public void setScore (int point)
	{
		score = score + point;
	}

	public int getScore ()
	{
		return score;
	}

}
