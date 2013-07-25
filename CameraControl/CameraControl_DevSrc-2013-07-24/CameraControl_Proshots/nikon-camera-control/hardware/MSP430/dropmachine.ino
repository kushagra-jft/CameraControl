/*
  Serial Event example
 
 When new serial data arrives, this sketch adds it to a String.
 When a newline is received, the loop prints the string and 
 clears it.
 
 A good test for this is to try it with a GPS receiver 
 that sends out NMEA 0183 sentences. 
 
 Created 9 May 2011
 by Tom Igoe
 
 This example code is in the public domain.
 
 http://www.arduino.cc/en/Tutorial/SerialEvent
 
 */

String inputString = "";         // a string to hold incoming data
String command = "";
String value = "" ; 
boolean stringComplete = false;  // whether the string is complete
boolean start = false;
// pins -----------------
const int valvePin =  P2_3;
const int cameraPin =  P2_4;
const int flashPin =  P2_5;
const int ledPin =  RED_LED;
const int buttonPin = PUSH2;
// -----------------------
// timers-----------------
int camera_timer = 1000;
int drop1_time = 45;
int drop_wait_time = 100;
int drop2_time = 45;
int flash_time = 370;
// -----------------------
void setup() {
  // initialize serial:
  Serial.begin(9600);
  // reserve 200 bytes for the inputString:
  inputString.reserve(200);
  pinMode(valvePin, OUTPUT);
  pinMode(cameraPin, OUTPUT);
  pinMode(ledPin, OUTPUT);  
  pinMode(flashPin, OUTPUT);   
  pinMode(buttonPin, INPUT_PULLUP); 
  resetState();
}

void loop() {

  if (digitalRead(buttonPin) == LOW)
  {
    start = true;
    blinkLed();
    delay(500);
  }
  // print the string when a newline arrives:
  if (start) {
    start = false;
    digitalWrite(cameraPin, HIGH);
    delay(camera_timer/2);
    digitalWrite(valvePin, HIGH);
    delay(drop1_time);    
    digitalWrite(valvePin, LOW);    
    delay(drop_wait_time);        
    digitalWrite(valvePin, HIGH);
    delay(drop2_time);    
    digitalWrite(valvePin, LOW);    
    delay(flash_time);    
    digitalWrite(flashPin, HIGH);
    delay(100);    
    digitalWrite(flashPin, LOW);    
    delay(camera_timer/2);

    digitalWrite(cameraPin, LOW);
  }
}

/*
  SerialEvent occurs whenever a new data comes in the
 hardware serial RX.  This routine is run between each
 time loop() runs, so using delay inside loop can delay
 response.  Multiple bytes of data may be available.
 */
void serialEvent() {
    bool shouldBlink = false;  
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read(); 
    inputString += inChar;
    
    if(inChar == ' ')
    {
      command = "";
      inputString = "";
      start = true;
      shouldBlink = true;
    }

    if(inChar == '?')
    {
      command = "";
      inputString = "";
      sendData();
    }

    // check for varieable separator
    if(inChar == '=')
    {
      command = inputString;
      inputString = "";
      Serial.println(command);
    }

    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;      
              Serial.println(inputString);
      if(command=="c=")
      {
        camera_timer = inputString.toInt();
        shouldBlink = true;
      } 
      if(command=="d1=")
      {
        drop1_time = inputString.toInt();
        shouldBlink = true;
      } 
      if(command=="dw=")
      {
        drop_wait_time = inputString.toInt();
        shouldBlink = true;
      } 
      if(command=="d2=")
      {
        drop2_time = inputString.toInt();
        shouldBlink = true;
      } 
      if(command=="f=")
      {
        flash_time = inputString.toInt();
        shouldBlink = true;
      } 
      command = "";
      inputString = "";
    } 
  }
      if(shouldBlink)
      {
        blinkLed();
        shouldBlink = false;
      }
}

void resetState(){
  digitalWrite(valvePin, LOW);
  digitalWrite(cameraPin, LOW);
  digitalWrite(ledPin, LOW);
}

void blinkLed()
{
  for (int i=0; i <= 1; i++){
    digitalWrite(ledPin, HIGH);
    delay(25);
    digitalWrite(ledPin, LOW);
    delay(25);    
  }
}

void sendData()
{
  Serial.print("camera_timer=");
  Serial.println(camera_timer);
  Serial.print("drop1_time=");
  Serial.println(drop1_time);
  Serial.print("drop_wait_time=");
  Serial.println(drop_wait_time);
  Serial.print("drop2_time=");
  Serial.println(drop2_time);
  Serial.print("flash_time=");
  Serial.println(flash_time);
}












