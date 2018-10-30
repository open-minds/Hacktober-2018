package sample;

import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.scene.Node;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.control.Button;
import javafx.scene.control.TextField;
import javafx.stage.Stage;

import java.io.IOException;

public class Controller {
    @FXML
    private Button j1;
    @FXML
    private Button j2;
    @FXML
    private Button j3;
    @FXML
    private Button j4;
    @FXML
    private Button j5;
    @FXML
    private Button j6;
    @FXML
    private Button j7;
    @FXML
    private Button j8;
    @FXML
    private Button solution;
    @FXML
    private int s = 4;
    @FXML
    private TextField s1;
    @FXML
    private TextField s2;
    @FXML
    private TextField s3;
    @FXML
    private TextField s4;
    @FXML
    private TextField s5;
    @FXML
    private TextField s6;
    @FXML
    private TextField s7;
    @FXML
    private TextField s8;
    @FXML
    private TextField sol;
    @FXML
    private TextField com;

    public void depart (ActionEvent actionEvent) throws IOException
    {
        ((Node)(actionEvent.getSource())).getScene().getWindow().hide();
        Parent root = FXMLLoader.load(getClass().getResource("getme.fxml"));
        Stage primaryStage=new Stage();
        primaryStage.setTitle("Get Me if You can!!");
        primaryStage.setScene(new Scene(root));
        primaryStage.setResizable(false);
        primaryStage.show();
    }

    public void ButtonAction(ActionEvent a) {
        Button button = (Button)
                a.getSource();
        if (button.getId().equals("solution"))
            sol.setText(String.valueOf(s));
        // joueur 1
        if (button.getId().equals("j1")) {
            int jo1 = (int) (Math.random() * 8 + 1);
            s1.setText(String.valueOf(jo1));
            if (s1.getText().equals(String.valueOf(s))) {
                com.setText("BRAVO");
            } else if (Integer.valueOf(s1.getText()) < s) {
                com.setText("PLUS!!");
            } else if (Integer.valueOf(s1.getText()) > s)
                com.setText("MOINS!!");
        }
        // joueur 2
        if (button.getId().equals("j2")) {
            int jo2 = (int) (Math.random() * 8 + 1);
            s2.setText(String.valueOf(jo2));
            if (s2.getText().equals(String.valueOf(s))) {
                com.setText("BRAVO");
            } else if (Integer.valueOf(s2.getText()) < s) {
                com.setText("PLUS!!");
            } else if (Integer.valueOf(s2.getText()) > s)
                com.setText("MOINS!!");
        }
        // joueur 3
        if (button.getId().equals("j3")) {
            int jo3 = (int) (Math.random() * 8 + 1);
            s3.setText(String.valueOf(jo3));
            if (s3.getText().equals(String.valueOf(s))) {
                com.setText("BRAVO");
            } else if (Integer.valueOf(s3.getText()) < s) {
                com.setText("PLUS!!");
            } else if (Integer.valueOf(s3.getText()) > s)
                com.setText("MOINS!!");
        }
        // joueur 4
        if (button.getId().equals("j4")) {
            int jo4 = (int) (Math.random() * 8 + 1);
            s4.setText(String.valueOf(jo4));
            if (s4.getText().equals(String.valueOf(s))) {
                com.setText("BRAVO");
            } else if (Integer.valueOf(s4.getText()) < s) {
                com.setText("PLUS!!");
            } else if (Integer.valueOf(s4.getText()) > s)
                com.setText("MOINS!!");
        }
        // joueur 5
        if (button.getId().equals("j5")) {
            int jo5 = (int) (Math.random() * 8 + 1);
            s5.setText(String.valueOf(jo5));
            if (s5.getText().equals(String.valueOf(s))) {
                com.setText("BRAVO");
            } else if (Integer.valueOf(s5.getText()) < s) {
                com.setText("PLUS!!");
            } else if (Integer.valueOf(s5.getText()) > s)
                com.setText("MOINS!!");
        }
        // joueur 6
        if (button.getId().equals("j6")) {
            int jo6 = (int) (Math.random() * 8 + 1);
            s6.setText(String.valueOf(jo6));
            if (s6.getText().equals(String.valueOf(s))) {
                com.setText("BRAVO");
            } else if (Integer.valueOf(s6.getText()) < s) {
                com.setText("PLUS!!");
            } else if (Integer.valueOf(s6.getText()) > s)
                com.setText("MOINS!!");
        }
        // joueur 7
        if (button.getId().equals("j7")) {
            int jo7 = (int) (Math.random() * 8 + 1);
            s7.setText(String.valueOf(jo7));
            if (s7.getText().equals(String.valueOf(s))) {
                com.setText("BRAVO");
            } else if (Integer.valueOf(s7.getText()) < s) {
                com.setText("PLUS!!");
            } else if (Integer.valueOf(s7.getText()) > s)
                com.setText("MOINS!!");
        }
        // joueur 8
        if (button.getId().equals("j8")) {
            int jo8 = (int) (Math.random() * 8 + 1);
            s8.setText(String.valueOf(jo8));
            if (s8.getText().equals(String.valueOf(s))) {
                com.setText("BRAVO");
            } else if (Integer.valueOf(s8.getText()) < s) {
                com.setText("PLUS!!");
            } else if (Integer.valueOf(s8.getText()) > s)
                com.setText("MOINS!!");
        }
        //solution
        if (button.getId().equals("solution"))
            sol.setText(String.valueOf(s));
    }
}


