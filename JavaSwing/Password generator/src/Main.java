import java.util.Random;

import javax.swing.JOptionPane;
// Java code to explain how to generate random 
// password 

// Here we are using random() method of util 
// class in Java 
public class Main {
	public static void main(String Args[]) {
	//	int c = Integer.parseInt(JOptionPane.showInputDialog("A password of how many characters?"));
		String password = "";
		String Capital_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; 
		String Small_chars = "abcdefghijklmnopqrstuvwxyz"; 
		String numbers = "0123456789"; 
				String symbols = "!@#$%^&*_=+-/.?<>;:'}]())"; 


		String values = Capital_chars + Small_chars + numbers + symbols; 
		Random r = new Random();
		for (int i = 0; i < c; i++) {
			password[i] = values.charAt(r.nextInt(values.length())); 
		}

		JOptionPane.showMessageDialog(null, password);
	}
}
