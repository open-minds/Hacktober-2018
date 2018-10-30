import java.util.ArrayList;
import java.util.Scanner;
import java.util.Stack;

public class reconaissance implements CharSequence{
    //implementations de l'interface
    @Override
    public CharSequence subSequence(int start, int end) {
        return null;
    }

    @Override
    public int length() {
        return 0;
    }

    @Override
    public char charAt(int index) {
        return 0;
    }
    //implementations de l'interface

    //initialisation
    private boolean reessayer=true;
    private static String mot;
    public int ouv;
    Scanner sc=new Scanner(System.in);
    private Stack<Character>pile=new Stack<Character>();
    //initialisation

    public void entrée(){
        System.out.println("tappez le mot que vous voulez verrifier");
        mot=sc.next();
    }

    public void verification(){
int i=0;
        if (mot.charAt(0)==')'){
            System.out.println("mot non reconnue");
        }else{
            while (i<mot.length() && /* &&*/(mot.charAt(i)==')' || mot.charAt(i)=='(')){
                if (mot.charAt(i)=='('){
                    pile.push(')');
                }else if(mot.charAt(i)==')' && !(pile.empty())){
                    pile.pop();
                }else if (mot.charAt(i)==')' && (pile.empty())){
                    System.out.println("mot non reconnue");
                }
                i++;
            }
            if (pile.empty()){
                System.out.println("mot reconnue");
            }else{
                System.out.println("mot non reconnue ");
            }
        }

    }


    public void lancement(){
        while(reessayer==true){
            entrée();
            verification();
            pile.clear();
            System.out.println("recommencer ?  0=oui   1=non ");
            int var=sc.nextInt();
            if (var==1){
                reessayer=false;
            }
        }

    }



 /* ouv=0;
        int i=0;
        if (mot.charAt(0)==')'){
            System.out.println("mot non reconnue");
        }
        else if (mot.length()-1=='(') {
            System.out.println("mot non reconnue");
        }else if (mot.charAt(0)=='('){
            while (i<mot.length() && (ouv>=0) &&(mot.charAt(i)==')' || mot.charAt(i)=='(')){
               if (mot.charAt(i)=='('){
                   ouv++;
               }else if(mot.charAt(i)==')'){
                   ouv--;
               }
            i++;
            }
            if (ouv!=0){
                System.out.println("mot non reconnue");
            }else if (i==mot.length()){
                System.out.println("mot reconnue");
            }else{
                System.out.println("mot non reconnue");
            }
        }*/