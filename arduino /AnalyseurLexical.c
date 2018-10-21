#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include <string.h>

bool automatEntier(char mot[10]);
bool automateClef(char mot[10]);
bool automateIdentifiant(char mot[10]);
bool automatessinon(char mot [10]);
bool autamatalors(char mot [10]);
bool automatevar(char mot[10]);
bool automateint(char mot[10]);
bool automatefsi(char mot[10]);

int main() {
    FILE *fichier = NULL;
    int carActuel = 0;
    bool  temporaire=false;
    fichier = fopen("/home/djawed/Documents/essaiefile", "r");   // pour l'acces au fichier
    char verrifier[100];
    int i = 0;

    if (fichier != NULL) { // verrifier l'acces au fichier




        do {                            // boucle de lecture du fichier
            carActuel = fgetc(fichier); // lecture du  caractère
            printf("%c", carActuel); // On l'affiche

                while (carActuel !=' '){
                    verrifier[i]==carActuel;
                    i++;
                }
                 if (automatEntier(verrifier)==true){
                     printf("  entier\n");
                 } else if(automateClef(verrifier)==true){
                     printf("  mot clef\n");
                 } else if(automateIdentifiant(verrifier)==true){
                     printf("  identifiant\n");
                 } else printf("   erreur lexicale\n");







        } while (carActuel != EOF);

    } else {
        printf("Impossible d'ouvrir le fichier ");
    }


}


bool automatEntier(char mot[10]){  // chaque mot ce termine avec un point pour determiner la fin de chaine
    int j=0;
    bool result=true;
    while (mot[j]!='.' && result==true){
        if(mot[j] == '0' || mot[j] == '1' || mot[j] == '2' || mot[j] == '3' || mot[j] == '4' || mot[j] == '5' ||
           mot[j] == '6' || mot[j] == '7' || mot[j] == '8' || mot[j] == '9'){
            j++;
        } else{
            result=false;
        }
        return result;
    }


}
bool automateIdentifiant(char mot[10]){
    int j=0;
    bool result=true;
    bool result1=true;

    while (result==true && result1==true){


        if(mot[0] == 'A' || mot[0] == 'B' || mot[0] == 'C' || mot[0] == 'D' || mot[0] == 'E' || mot[0] == 'F' ||
           mot[0] == 'G' || mot[0] == 'H' || mot[0] == 'I' || mot[0] == 'J' || mot[0] == 'K' ||
           mot[0] == 'L' || mot[0] == 'M' || mot[0] == 'N' || mot[0] == 'O' || mot[0] == 'P' || mot[0] == 'Q' ||
           mot[0] == 'R' || mot[0] == 'S' || mot[0] == 'T' || mot[0] == 'U' || mot[0] == 'V' || mot[0] == 'W' ||
           mot[0] == 'X' ||
           mot[0] == 'Y' || mot[0] == 'Z' || mot[0] == 'a' || mot[0] == 'b' || mot[0] == 'c' || mot[0] == 'd' || mot[0] == 'e' || mot[0] == 'f' ||
           mot[0] == 'g' || mot[0] == 'h' || mot[0] == 'i' || mot[0] == 'j' || mot[0] == 'k' || mot[0] == 'l' ||
           mot[0] == 'm' ||
           mot[0] == 'n' || mot[0] == 'o' || mot[0] == 'p' || mot[0] == 'q' || mot[0] == 'r' || mot[0] == 's' ||
           mot[0] == 't' || mot[0] == 'u' || mot[0] == 'v' || mot[0] == 'w' || mot[0] == 'x' || mot[0] == 'y' ||
           mot[0] == 'z'){
            result=true;
        } else{result=false;
        }
        while (mot[j] != '.' && result1==true){

            if (mot[j] == 'A' || mot[j] == 'B' || mot[j] == 'C' || mot[j] == 'D' || mot[j] == 'E' || mot[j] == 'F' ||
                mot[j] == 'G' || mot[j] == 'H' || mot[j] == 'I' || mot[j] == 'J' || mot[j] == 'K' ||
                mot[j] == 'L' || mot[j] == 'M' || mot[j] == 'N' || mot[j] == 'O' || mot[j] == 'P' || mot[j] == 'Q' ||
                mot[j] == 'R' || mot[j] == 'S' || mot[j] == 'T' || mot[j] == 'U' || mot[j] == 'V' || mot[j] == 'W' ||
                mot[j] == 'X' ||
                mot[j] == 'Y' || mot[j] == 'Z' || mot[j] == 'a' || mot[j] == 'b' || mot[j] == 'c' || mot[j] == 'd' || mot[j] == 'e' || mot[j] == 'f' ||
                mot[j] == 'g' || mot[j] == 'h' || mot[j] == 'i' || mot[j] == 'j' || mot[j] == 'k' || mot[j] == 'l' ||
                mot[j] == 'm' ||
                mot[j] == 'n' || mot[j] == 'o' || mot[j] == 'p' || mot[j] == 'q' || mot[j] == 'r' || mot[j] == 's' ||
                mot[j] == 't' || mot[j] == 'u' || mot[j] == 'v' || mot[j] == 'w' || mot[j] == 'x' || mot[j] == 'y' ||
                mot[j] == 'z'|| mot[j] == '1' || mot[j] == '2' || mot[j] == '3' || mot[j] == '4' ||
                mot[j] == '4' || mot[j] == '6' || mot[j] == '7' || mot[j] == '8' || mot[j] == '9' || mot[j] == '0' ){
                result1=true;
            } else{result1=false;}
        }

    }
    if (result==false || result1==false){
        return  false;
    } else {                //   si beug tenter la negation du if
        return true;
    }



}
bool automateClef(char mot[10]){

    if (autamatalors(mot)==true || automatefsi(mot)== true || automateint(mot)==true || automatessinon(mot)==true || automatevar(mot)==true){
      return true;
    } else return false;

}

bool automatessinon(char mot [10]){      // verrifier le si et sinon

    if(mot[0]=='s' && mot[1]=='i' && mot[2]=='.'){
        return true;
    } else if(mot[0]=='s' && mot[1]=='i' && mot[2]=='n' && mot[3]=='o' && mot[4]=='n' && mot[5]=='.' ){
        return true;
    } else return false;

}

bool autamatalors(char mot [10]){
    if (mot[0]=='a' && mot[1]=='l' && mot[2]=='o' && mot[3]=='r' && mot[4]=='s' && mot[5]=='.' ){
        return true;
    } else return false;
}


bool automatevar(char mot[10]){
    if (mot[0]=='v' && mot[1]=='a' && mot[2]=='r' && mot[3]=='.'){
        return true;
    } else return false;
}

bool automateint(char mot[10]){
    if (mot[0]=='i' && mot[1]=='n' && mot[2]=='t' && mot[3]=='.'){
        return true;
    } else return false;
}


bool automatefsi(char mot[10]){
    if (mot[0]=='f' && mot[1]=='s' && mot[2]=='i' && mot[3]=='.'){
        return true;
    } else return false;
}

