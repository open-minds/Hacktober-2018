package sample;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.fxml.Initializable;
import javafx.scene.Node;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.control.Button;
import javafx.scene.control.PasswordField;
import javafx.scene.control.TextField;
import javafx.stage.Stage;

import java.io.IOException;
import java.net.URL;
import java.util.ResourceBundle;

public class Controller implements Initializable {
    @FXML
    private Button zero;
    @FXML
    private Button un;
    @FXML
    private Button deux;
    @FXML
    private Button trois;
    @FXML
    private Button quatre;
    @FXML
    private Button cinq;
    @FXML
    private Button six;
    @FXML
    private Button sept;
    @FXML
    private Button huite;
    @FXML
    private Button neuf;
    @FXML
    private PasswordField mdp;
    @FXML
    private Button v1;
    @FXML
    private Button v2;
    @FXML
    private Button v3;
    @FXML
    private Button v4;
    @FXML
    private int code=2961;
    @FXML
    private int solde=1500;
    @FXML
    private TextField commentairedp;
    @FXML
    private TextField commentaireop;

    public void enter(ActionEvent actionEvent)throws IOException{
        int p =Integer.parseInt(mdp.getText());
        if (p==code){
            ((Node)(actionEvent.getSource())).getScene().getWindow().hide();
            Parent root = FXMLLoader.load(getClass().getResource("operation.fxml"));
            Stage primaryStage=new Stage();
            primaryStage.setTitle("depart");
            primaryStage.setScene(new Scene(root));
            primaryStage.setResizable(false);
            primaryStage.show();
        }
        else commentairedp.setText("Veillez saisir le bn code a 4 chiffres!!");
    }

    public void ButtonAction(ActionEvent a) {
        Button button = (Button)
                a.getSource();
        if (button.getId().equals("zero"))
            mdp.setText(mdp.getText()+"0");

        else if (button.getId().equals("un"))
            mdp.setText(mdp.getText()+"1");

        else if (button.getId().equals("deux"))
            mdp.setText(mdp.getText()+"2");

        else if (button.getId().equals("trois"))
            mdp.setText(mdp.getText()+"3");

        else if (button.getId().equals("quatre"))
            mdp.setText(mdp.getText()+"4");

        else if (button.getId().equals("sinq"))
            mdp.setText(mdp.getText()+"5");

        else if (button.getId().equals("six"))
            mdp.setText(mdp.getText()+"6");

        else if (button.getId().equals("sept"))
            mdp.setText(mdp.getText()+"7");

        else if (button.getId().equals("huite"))
            mdp.setText(mdp.getText()+"8");

        else if (button.getId().equals("neuf"))
            mdp.setText(mdp.getText()+"9");
    }
    public void clear(){
        mdp.setText("");
    }

    public void consulter(){
        commentaireop.setText("Votre soldes est:"+String.valueOf(solde));
    }

    public void retirer(ActionEvent b){
        Button button = (Button)
                b.getSource();
        commentaireop.setText("Veuillez choisir");
        if(button.getId().equals("v1")){
            solde=solde-100;}
        else if (button.getId().equals("v2")){
            solde=solde-500;}
        else if (button.getId().equals("v3")){
            solde=solde-1000;}
        else if (button.getId().equals("v4")){
                solde=solde-2000;}
        compare();
    }

    public void compare(){
        if (solde>=0)
          commentaireop.setText("transactions réussite!!");
        else
          commentaireop.setText("Pas assez de soldes");

    }

    public void verser(ActionEvent c){
        Button button = (Button)
                c.getSource();
        commentaireop.setText("Veuillez choisir");
        if (button.getId().equals("v1"))
            commentaireop.setText("transactions réussite!!");
        else
            commentaireop.setText("transactions réussite!!");
    }

    public void cancel(ActionEvent cancel)throws IOException{
        ((Node)(cancel.getSource())).getScene().getWindow().hide();
        Parent root = FXMLLoader.load(getClass().getResource("depart.fxml"));
        System.exit(0);
    }

    @Override
    public void initialize(URL location, ResourceBundle resources)
    {

    }
}