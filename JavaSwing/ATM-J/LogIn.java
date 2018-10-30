import java.awt.BorderLayout;
import java.awt.EventQueue;

import javax.swing.JFrame;
import javax.swing.JPanel;
import javax.swing.border.EmptyBorder;
import javax.swing.JLabel;
import java.awt.Font;
import javax.swing.SwingConstants;
import javax.swing.JTextField;
import javax.swing.JButton;
import java.awt.Component;
import javax.swing.Box;
import java.awt.event.ActionListener;
import java.awt.event.ActionEvent;

public class LogIn extends JFrame {

	private JPanel contentPane;
	private JTextField textField;
	private JTextField textField_1;
	private JTextField textField_2;
	private JTextField textField_3;
	private JTextField textField_4;

	/**
	 * Launch the application.
	 */
	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				try {
					LogIn frame = new LogIn();
					frame.setVisible(true);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});
	}

	/**
	 * Create the frame.
	 */
	public LogIn() {
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		setBounds(100, 100, 467, 485);
		contentPane = new JPanel();
		contentPane.setBorder(new EmptyBorder(5, 5, 5, 5));
		setContentPane(contentPane);
		contentPane.setLayout(null);
		
		JLabel loginText = new JLabel("LogIn to My ATM");
		loginText.setHorizontalAlignment(SwingConstants.CENTER);
		loginText.setFont(new Font("Arial", Font.BOLD, 18));
		loginText.setBounds(127, 39, 179, 16);
		contentPane.add(loginText);
		
		textField = new JTextField();
		textField.setBounds(103, 166, 116, 22);
		contentPane.add(textField);
		textField.setColumns(10);
		
		JLabel lblFirstName = new JLabel("First Name");
		lblFirstName.setFont(new Font("Arial", Font.PLAIN, 14));
		lblFirstName.setHorizontalAlignment(SwingConstants.CENTER);
		lblFirstName.setBounds(25, 169, 70, 16);
		contentPane.add(lblFirstName);
		
		JLabel lblLastName = new JLabel("Last Name");
		lblLastName.setFont(new Font("Arial", Font.PLAIN, 14));
		lblLastName.setHorizontalAlignment(SwingConstants.CENTER);
		lblLastName.setBounds(25, 221, 70, 16);
		contentPane.add(lblLastName);
		
		textField_1 = new JTextField();
		textField_1.setColumns(10);
		textField_1.setBounds(103, 218, 116, 22);
		contentPane.add(textField_1);
		
		JLabel lblId = new JLabel("Pin");
		lblId.setFont(new Font("Arial", Font.PLAIN, 14));
		lblId.setHorizontalAlignment(SwingConstants.CENTER);
		lblId.setBounds(25, 271, 70, 16);
		contentPane.add(lblId);
		
		textField_2 = new JTextField();
		textField_2.setColumns(10);
		textField_2.setBounds(103, 268, 116, 22);
		contentPane.add(textField_2);
		
		JLabel lblCreateNewUser = new JLabel("Create new User");
		lblCreateNewUser.setFont(new Font("Arial", Font.PLAIN, 16));
		lblCreateNewUser.setHorizontalAlignment(SwingConstants.CENTER);
		lblCreateNewUser.setBounds(53, 123, 130, 16);
		contentPane.add(lblCreateNewUser);
		
		JButton btnSignUp = new JButton("Sign Up");
		btnSignUp.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent arg0) {
			//call your methode here
			}
		});
		btnSignUp.setBounds(71, 328, 97, 25);
		contentPane.add(btnSignUp);
		
		textField_3 = new JTextField();
		textField_3.setColumns(10);
		textField_3.setBounds(321, 166, 116, 22);
		contentPane.add(textField_3);
		
		JLabel label = new JLabel("Id");
		label.setHorizontalAlignment(SwingConstants.CENTER);
		label.setFont(new Font("Arial", Font.PLAIN, 14));
		label.setBounds(249, 169, 70, 16);
		contentPane.add(label);
		
		JButton btnSignIn = new JButton("Sign In");
		btnSignIn.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent arg0) {
				setVisible(false);
				Bank b = new Bank(textField.getText());
				b.setVisible(true);
			}
		});
		btnSignIn.setBounds(301, 328, 97, 25);
		contentPane.add(btnSignIn);
		
		JLabel lblPinCode = new JLabel("Pin Code");
		lblPinCode.setHorizontalAlignment(SwingConstants.CENTER);
		lblPinCode.setFont(new Font("Arial", Font.PLAIN, 14));
		lblPinCode.setBounds(249, 218, 70, 16);
		contentPane.add(lblPinCode);
		
		textField_4 = new JTextField();
		textField_4.setColumns(10);
		textField_4.setBounds(321, 215, 116, 22);
		contentPane.add(textField_4);
		
		JLabel lblAuthentificate = new JLabel("Authentificate");
		lblAuthentificate.setHorizontalAlignment(SwingConstants.CENTER);
		lblAuthentificate.setFont(new Font("Arial", Font.PLAIN, 16));
		lblAuthentificate.setBounds(291, 123, 130, 16);
		contentPane.add(lblAuthentificate);
	}
}
