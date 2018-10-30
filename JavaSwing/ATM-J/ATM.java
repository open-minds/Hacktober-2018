package atm;


import java.util.Scanner;

public class ATM {
    public static void main (String [] args) {
    //init scanner
Scanner sc = new Scanner(System.in);
//init Bank

Bank theBank = new Bank("bank of darusin");
// add User to the bank an saving it
User aUser= theBank.addUser("youcef","kolli","7101");

// chack the account for the User
Account newAccount= new Account("chaking",aUser,theBank);
aUser.addAccount(newAccount);
theBank.addAccount(newAccount);
User curUser;
while(true){
    //wait for the loging intel he succssful 
    curUser= ATM.mainMenuPrompt(theBank, sc);
    // stay in main menu inte user quite
    ATM.printUserMenu(curUser,sc);
}
}
    /**
     *
     * @param theBank
     * @param sc
     * @return
     */
    public static User mainMenuPrompt(Bank theBank , Scanner sc ){
    //inits
        String userID;
        String pin;
        User authUser;
        // prompt the User for user ID/pin combo intel is crrect 
do{
System.out.printf("\n\nWlcome to %s\n\n", theBank.getName());
System.out.printf("Entre your ID: ");
    userID= sc.nextLine();
    System.out.print("Entre pin : ");
    pin = sc.nextLine();
    // try to get object coresponding to ID and pin 
    authUser= theBank.userLogin(userID,pin);
    if(authUser==null){
        System.out.println("rak galet fel ID/pin combination." +"Try agin.");
    
    }
}while(authUser==null);// continu looping until successful login
return authUser;
    
}
    public static void printUserMenu(User theUser, Scanner sc){
    
    // print a summary of the User's accounts
        theUser.printAccountsSummary();
        //inite
        int choice;
        //user menu
        do{
            System.out.printf("Marhba Bik  %,what would you like to do ?\n",theUser.getFirstName());
            System.out.println("  1) Show account transaction history ");
            System.out.println("  2)   Withdrawl  ");
            System.out.println("  3)  deposit  ");
            System.out.println("  4)   Transfer ");
            System.out.println("  5)    Quit ");
            System.out.println();
            System.out.println("Entre choice : ");
            choice = sc.nextInt();
    if(choice<1||choice>5){
        System.out.println("your choice is invalide "+"plz choose 1-5 ");
    }
        }while(choice<1||choice>5);
    //peoces the choice 
        switch(choice){
        case 1 :
        ATM.showTransHistory(theUser,sc);
        break;
            case 2 :
            ATM.WithdrawFunds(theUser,sc);
        break;
        case 3:
        ATM.depositFunds(theUser,sc);
            break;
        case 4:
            ATM.TransferFunds(theUser,sc);
            break;
        }
        // redsplay this menu unless the user wants to quit
        if(choice != 5){
        ATM.printUserMenu(theUser,sc);
        }
        
    }
    
    /**
     *
     * @param theUser
     * @param sc
     */
    public static void showTransHistory(User theUser , Scanner sc  ){
    int theAcct;
    //get account trabsaction history to look at
    do{
        System.out.printf("Enter the number (1-%d)of the account\n" +"whose transactions you want to see : ",
                          theUser.numAccounts() );
        theAcct =sc.nextInt()-1;
        if(theAcct<0||theAcct>=theUser.numAccounts()){
            System.out.println("invalid account, pleaz try again.");
            
            }
        
    }while(theAcct<0||theAcct>=theUser.numAccounts());
    //print transaction history 
    
    theUser.printAcctTransHistory(theAcct);
    
    }
    
    public static void TransferFunds(User theUser , Scanner sc ){
    //inite
        int fromAcct;
        int toAcct;
        double amount;
        double acctBal;
        //get the account to tranfer from
        do{
            System.out.printf("Enter the  number (&-%d) of the account\n"+ "to transfor from : ",theUser.numAccounts());
            fromAcct = sc.nextInt()-1;
            if(fromAcct<0|| fromAcct> theUser.numAccount()){
                System.out.println("invalid account, pleaz try again.");
            
            }
        }while(fromAcct<0||fromAcct>=theUser.numAccounts());
        acctBal = theUser.getAcctBalance(fromAcct);
            
            //get the account to transfor to
            do{
        System.out.printf("Enter the  number (&-%d) of the account\n"+ "to transfor to : ",theUser.numAccounts());
                toAcct = sc.nextInt()-1;
                if(toAcct<0|| toAcct> theUser.numAccount()){
                    System.out.println("invalid account, pleaz try again.");
                
                }
            }while(toAcct<0||toAcct>=theUser.numAccounts());
            //get the amount to transfer
            do{
                System.out.printf("Enter the  amount to transfer  (max $%.02f)  : $ ",acctBal);
                amount = sc.nextDouble();
                if(amount<0){
                System.out.println("amount must be kbir 3la O");
                }else if(amount> acctBal)
                {
                    System.out.printf("amount must not be kbir   3la\n "+"L Balance of $%.02f.\n",acctBal);
                }
                
            }while(amount<0|| amount> acctBal);
            //fanally do the transfer 
    theUser.addAcctTransaction(fromAcct,-1*amount,String.format("Transfer to account %s", theYser.getAcctID(toAcct)));
    theUser.addAcctTransaction(toAcct,amount,String.format("Transfer to account %s", theYser.getAcctID(fromAcct)));
    
    

    }
    /**
     * 
     */
    public static void WithdrawFunds(User theUser , Scanner sc){
        //inite
            int fromAcct;
            double amount;
            double acctBal;
            String memo;
            //get the account to tranfer from
            do{
        System.out.printf("Enter the  number (&-%d) of the account\n"+ "to Withdraw from  : ",theUser.numAccounts());
                fromAcct = sc.nextInt()-1;
                if(fromAcct<0|| fromAcct> theUser.numAccount()){
                    System.out.println("invalid account, pleaz try again.");
                
                }
            }while(fromAcct<0||fromAcct>=theUser.numAccounts());
            acctBal = theUser.getAcctBalance(fromAcct);
        //get the amount to transfer
        do{
            System.out.printf("Enter the  amount to transfer  (max $%.02f)  : $ ",acctBal);
            amount = sc.nextDouble();
            if(amount<0){
            System.out.println("amount must be kbir 3la O");
            }else if(amount> acctBal)
            {
                System.out.printf("amount must not be kbir   3la\n "+"L Balance of $%.02f.\n",acctBal);
            }
            
            //gobbel input
            sc.nextLine();
            //get a memo
            System.out.print("Entre the memo");
            sc.nexLine();
            //the withdrawe
            theUser.addAcctTransaction(fromAcct,-1*amount, meomo);
                   }while(toAcct<0||toAcct>=theUser.numAccounts());
    
    
}
    public static void depositFunds(User theUser, Scanner sc ){
        //inite
            int toAcct;
            double amount;
            double acctBal;
            String memo;
            //get the account to tranfer from
            do{
        System.out.printf("Enter the  number (&-%d) of the account\n"+ "to deposit in: ",theUser.numAccounts());
                toAcct = sc.nextInt()-1;
                if(toAcct<0|| toAcct> theUser.numAccount()){
                    System.out.println("invalid account, pleaz try again.");
                
                }
            }while(toAcct<0||toAcct>=theUser.numAccounts());
            acctBal = theUser.getAcctBalance(toAcct);
        //get the amount to transfer
        do{
            System.out.printf("Enter the  amount to transfer  (max $%.02f)  : $ ",acctBal);
            amount = sc.nextDouble();
            if(amount<0){
            System.out.println("amount must be kbir 3la O");
            }else{
            if(amount> acctBal)
            {
                System.out.printf("amount must not be kbir   3la\n "+"L Balance of $%.02f.\n",acctBal);
            }
            }
        }while(amount <0);
            
            //gobbel input
            sc.nextLine();
            //get a memo
            System.out.print("Entre the memo");
            sc.nexLine();
            //the withdrawe
            theUser.addAcctTransaction(toAcct,amount, meomo);
                   }
    
    
    
    }



