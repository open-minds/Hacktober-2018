
#define RED 3
#define GREEN 5
#define BLEU 6

void setup() {
  // put your setup code here, to run once:
 pinMode(RED,OUTPUT); // deffinir les pin en out ils donne du courant
 pinMode(GREEN, OUTPUT);
 pinMode(RED,OUTPUT);
 digitalWrite(RED, LOW);
 digitalWrite(GREEN, LOW);
 digitalWrite(BLEU, HIgit GH);
 
}
int redvalue=0; 
int greenvalue=0;
int bleuvalue=255;

void loop() {
  // put your main code here, to run repeatedly:

 for( int i=0;i<255;i++)
 {
  bleuvalue--;
  redvalue++;
  analogWrite(BLEU, bleuvalue);
  analogWrite(RED, redvalue);
  delay(10);
 }

 redvalue= 255;
 greenvalue=0;
 bleuvalue=0;

 for( int i=0;i<255;i++)
 {
  greenvalue--;
  bleuvalue++;
  analogWrite(GREEN, greenvalue);
  analogWrite(BLEU, bleuvalue);
  delay(10);
 }

 redvalue=0;
 greenvalue=0;
 bleuvalue=255;

 
 for ( int i=0;i<255; i++)
 {
  redvalue--;
  greenvalue++;
  analogWrite(RED, redvalue);
  analogWrite(GREEN, greenvalue);
  delay(10);
 }
      
}