package atm;
import java.util.ArrayList;
import java.util.Random;

public class Bank {
   private String name;
        private ArrayList<User> users;
        private ArrayList<Account> accounts;
        
        /**
     *
     * @param name
     */
    public Bank(String name){
    this.name=name;
    this.users=new ArrayList<User>();
    this.accounts = new ArrayList<Account> ();
    }
  
  
    public String getNewUserID(){
                                
// inits    
   String id;                          
   Random rng=new Random();   
   int lien = 10;
   boolean nonUnique;
// bocle de contrill intel we have a Unique ID
   do{
// generate the number
       id="";
       for(int c=0;c<=lien;c++){
           id+=((Integer)rng.nextInt(10)).toString();
       }
       // check if it is Unique
       nonUnique=false;
       for(User u: this.users){
       if(id.compareTo(u.getID())==0){
           nonUnique= true;
           break;
       } 
       }
       
 }
   while(nonUnique);
   return id ;                             
         }
    
public  String getNewAccountID(){
                                     
                                     
        // inits    
           String id;                          
           Random rng=new Random();   
           int lien = 12;
           boolean nonUnique;
        // bocle de contrill intel we have a Unique ID
           do{
        // generate the number
               id="";
               for(int c=0;c<=lien;c++){
                   id+=((Integer)rng.nextInt(10)).toString();
               }
               // check if it is Unique
               nonUnique=false;
               for(Account a: this.accounts){
               if(id.compareTo(a.getID())==0){
                   nonUnique= true;
                   break;
               } 
               }
             
         } while(nonUnique);
           return id;
                     
                 }
                                  
    
 
                                      
    
    /**
     *
     * @param anAcct
     */
    public  void addAccount(Account anAcct){
 this.accounts.add(anAcct);
                     }
    /**
     *
     * @param firstName
     * @param lastName
     * @param pin
     * @return new user .
     */
    public User addUser(String firstName,String lastName,String pin){
    
    //set  a new user and add hem to the list
        User newUser= new User( firstName,lastName,pin,this);
    this.users.add(newUser);
    //create a saving for the new user
    Account newAccount= new Account("Savings",newUser,this); 
        newUser.addAccount(newAccount);
        this.addAccount(newAccount);
        return newUser;
    }
    public User userLogin(String userID,String pin){
    //serch for the user on list
        for(User u: this.users){
        
        //check if the user is corect
            if(u.getID().compareTo(userID)==0 && u.validatepin(pin)){
            
            return u;
            
            }
        }
        // we vanen't found the user or incorect pin
        
        return null;
    
    }
    /**
     *
     * @return the name of the bank
     * 
     */
public  String getName()  {
return this.name;

}
    
    
    
    
}


