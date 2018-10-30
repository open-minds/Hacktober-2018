package atm;

import java.util.ArrayList;

import java.security.MessageDigest;


import java.security.NoSuchAlgorithmException;



public class User {
    //the first Name of the User
   private String firstName;
   //the Last Name of the User
   private String lastName;
   // the Id nomber of the user
   private String id;
   // pin nomber
   private byte pinHach[];
   // the list of accounts of thes user
    private ArrayList<Account> accounts;
    /**
     **creat a new User
     * @param firstName
     * @param lastName
     * @param pin
     * @param theBank
     */
    public User(String firstName,String lastName,String pin,Bank theBank){
        // set the user name
        this.firstName=firstName;
        this.lastName=lastName;
        //security of the User /// sliman yfahamhali
        try{
        MessageDigest md =MessageDigest.getInstance("MD5");
           this.pinHach =md.digest(pin.getBytes());
            }
       
            
        
     catch (NoSuchAlgorithmException e) {
            System.err.println("error NoSuchAlgorithmException");   
            e.printStackTrace();
            System.exit(1);
        }

// get the new ID for  the user
        this.id=theBank.getNewUserID();
        //creat ampty List of account
        this.accounts= new ArrayList<Account>();
        // print a message
        System.out.printf("New User %s,%s with ID %s created.\n",lastName,firstName,this.id);
        
    }
    /**
     *
     * @param anAcct
     * @return
     */
    public void addAccount(Account anAcct){
        this.accounts.add(anAcct);
    }
    /**
     *
     * @return
     */
    public String getID() {
        return this.id;
    }
    /**
     *
     * @param aPin
     * @return
     */
    public boolean validatepin(String aPin){
        try {
            MessageDigest md = MessageDigest.getInstance("MD5");
            return MessageDigest.isEqual(md.digest(aPin.getBytes()),this.pinHach) ;
        } catch (NoSuchAlgorithmException e) {
            System.err.println("error NoSuchAlgorithmException");   
            e.printStackTrace();
            System.exit(1);
        }
    return false;
}
    public String getFirstName(){
    return this.firstName;
    
    }

    public void printAccountsSummary(){
        System.out.printf("\n\n%s's accounts summary\n",this.firstName);
        for(int a=0; a< this.accounts.size();a++){
        System.out.printf("  %d) %s\n", a+1, this.accounts.get(a).getSummaryLine());
        }
        System.out.println();
    }
    /**
     *
     * @return the number of accounts
     */
    public int numAccounts(){
    return this.accounts.size();
    }
    /**
     *
     * @param acctIdx
     */
    public void  printAcctTransHistory(int acctIdx){
    this.accounts.get(acctIdx).printTransHistory();
    
    }
    public double getAcctBalance(int accIdx){
    return this.accounts.get( accIdx).geBalance();
    }
    public String getAcctID(int acctIdx){
    return this.accounts.get(acctIdx).getID();
    
    }
    
    public void  addAcctTransaction(int accIdx,double amount , String memo){
    
    this.accounts.get(accIdx).addTransaction(amount , memo);
    }
}