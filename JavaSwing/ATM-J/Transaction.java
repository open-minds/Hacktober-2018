package atm;

import java.util.Date;
public class Transaction {
    // the amount of transaction
   private double amount;
   // the time and date of transaction
   private Date timestamp;
   // the memo of transaction
    private String memo;
    // the account in wich transaction was preformed
    private Account inAccount;
    
    
    /**
     *
     * @param amount
     * @param inAccount
     */
    public Transaction(double  amount, Account inAccount){
        this.amount=amount;
        this.inAccount=inAccount;
        this.timestamp= new Date();
        this.memo="";
        
    }
    public Transaction(double amount, String memo, Account inAccount){
        this(amount,inAccount);
        
        //set memo
        this.memo=memo;
    }
    /**
     *
     * @return the amount of the transaction
     */
    public double getAmount(){
    return  this.amount;
    }
    /**
     *
     * @return the summary string
     */
    public String getSummaryLine(){
    if(this.amount>=0){
    return String.format("%s : $%.O2f : %s",this.timestamp.toString(),this.amount,this.memo);
    
    }
    else  {   return String.format("%s : $(%.O2f) : %s",this.timestamp.toString(),-this.amount,this.memo);
        
        }

    
    }
}
