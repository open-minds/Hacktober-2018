package atm;
import java.util.ArrayList;

public class Account {
  //the account name
    private String name;
    //the account Id
    private String id;
    //the user object that own the account
    private User  holder;
    //the liste of the transaction 
    private ArrayList<Transaction> transaction;
    /**
     **   
     * @param name
     * @param holder
     * @param theBank
     */
public Account(String name,User holder,Bank theBank){
    //set the Account nam and holder
    this.name=name;
    this.holder=holder;
    // get new account id
    this.id=theBank.getNewAccountID();
    //init transaction 
    this.transaction= new ArrayList<Transaction>();
  
   //add holder and bank list
    holder.addAccount(this);
    theBank.addAccount(this);
    }
    public String getID() {
        return this.id;
    }
    
    /**
     *
     * @return the String summary
     */
    public String getSummaryLine(){
    //get account balance
        double balance= getBalance();
        
        //format the summary line depand on balance is negative
        if(balance>=0){
        return String.format("%s : $%.02f :%s",this.id,balance,this.name);
        
        }else{
            return String.format("%s : $(%.02f) :%s",this.id,balance,this.name);

        }
    }
    /**
     *
     * @return balance of the account
     */
    public double getBalance(){
    
    double balance = 0;
    for(Transaction t : this.transaction){
        balance += t.getAmount();
    }
    return balance;
    }
 
//printe the transaction of the account
  
    public   void printTransHistory(){
    System.out.printf("\nTransaction history for account %s\n ", this.id);
    for(int t= this.transaction.size()-1;t>=0; t-- ){
        System.out.println(this.transaction.get(t).getSummaryLine());
    }
          System.out.println();
    }
    
    public void addTransaction(double amount , String memo ){
    //creat a new transaction and add it to our list
        Transaction newTrans= new Transaction( amount , memo , this);
        this.Transaction.add(newTranc);
    }
}
