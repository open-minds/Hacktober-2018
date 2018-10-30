#include <Servo.h> 
Servo myservo;

int trig=11;
int echo=12;
int md1=7;  
int md2=6;
int mg1=5;
int mg2=4;
int pos=0;
int duration;
int distance;

int calculDistance(int trig, int echo) {
  digitalWrite(trig,LOW);
  delayMicroseconds(2);
  digitalWrite(trig,HIGH);delayMicroseconds(10);digitalWrite(trig,LOW);
 duration=pulseIn(echo,HIGH);
 distance=duration*0.034/2;
 return distance;
 }


void setup() {
  // put your setup code here, to run once:
pinMode(md1,OUTPUT);
pinMode(md2,OUTPUT);
pinMode(mg1,OUTPUT);
pinMode(mg2,OUTPUT);
pinMode(echo,INPUT);
pinMode(trig,OUTPUT);
myservo.attach(9);
myservo.write(90);
}

void loop() {
  // put your main code here, to run repeatedly:

distance = calculDistance(trig,echo);
 
 if (distance > 20) { //front
  digitalWrite(md1,HIGH);
  digitalWrite(md2,LOW);
  digitalWrite(mg1,LOW);
  digitalWrite(mg2,HIGH);

  
 }
 
else if (distance < 20){
  //stopp 
 digitalWrite(md1,LOW);
  digitalWrite(md2,LOW);
  digitalWrite(mg1,LOW);
  digitalWrite(mg2,LOW);
 
    
    for (pos = 90; pos >= 180; pos += 2) { 
    myservo.write(pos);              
                        }
    
    distance = calculDistance(trig,echo);

    if ( distance < 50){
     
    // droite
    digitalWrite(md1,HIGH);
    digitalWrite(md2,LOW);
    digitalWrite(mg1,LOW);
    digitalWrite(mg2,LOW);
   delay(500);               
    }
    
 }
}
 


