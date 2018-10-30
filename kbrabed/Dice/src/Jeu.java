import java.util.Scanner;

public class Jeu
{

	Scanner sc = new Scanner(System.in);

	static int nombreJoueurs = 2;                      				//Ici modifie le nombre de joueurs, g pa su comment le faire avec un scan :(
	static Joueur liste[] = new Joueur[nombreJoueurs];
	int nombreParties;

	public void jouer()
	{
		for (int i = 0; i < nombreJoueurs; i++)
		{
			liste[i] = new Joueur();
		}
		for (int i = 0; i < nombreJoueurs; i++)
		{
			System.out.println("Nom du joueur " + (i + 1));
			liste[i].setName(sc.nextLine());
		}

		System.out.print("Nombre de tour : ");
		nombreParties = sc.nextInt();

		for (int i2 = 0; i2 < nombreParties; i2++)
		{
			System.out.println("-----------------------------------------------------");

			for (int j = 0; j < nombreJoueurs; j++)
			{
				int x = De.lancerDe();
				liste[j].setScore(x);

				System.out.println(liste[j].getName() + " a marqué " + x + " points, pour un total de " + liste[j].getScore() + " points !");
			}
		}

		calculGagnant();
	}

	public void calculGagnant ()
	{
		int max = liste[0].getScore();
		int p=0;
		for (int k = 1; k < nombreJoueurs; k++)
		{
			if (liste[k].getScore() > max)
			{
				max = liste[k].getScore();
				p=k;
			}
		}

		System.out.println();
		System.out.println();
		System.out.println(liste[p].getName() + " Gagne la partie :)");
		System.out.println();
		System.out.println();

	}

}
