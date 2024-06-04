import cv2
import mediapipe as mp
import numpy as np
import math
import socket
import time
import paho.mqtt.client as mqtt

mp_pose = mp.solutions.pose
mp_drawing = mp.solutions.drawing_utils


cap = cv2.VideoCapture(0)

#websocket server info
server_ip = "127.0.0.1"
server_port = 8080

# MQTT broker information
broker = "mqtt.eclipseprojects.io"
port = 1883
topic = "180da/mario"

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
one_count = ""
acceleration = None

# Callback when the client receives a CONNACK response from the server.
def on_connect(client, userdata, flags, rc):
    print(f"Connected with result code {rc}")
    # Subscribing in on_connect() means that if we lose the connection and
    # reconnect then subscriptions will be renewed.
    client.subscribe(topic)

# Callback when a PUBLISH message is received from the server.
def on_message(client, userdata, msg):
    global acceleration
    acceleration = msg.payload.decode()
    # print(f"{msg.topic} {acceleration}")


# Create an MQTT client and attach our routines to it.
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message
client.connect(broker, port, 60)

#runs MQTT loop in a separate thread -> this means it wont break the code
client.loop_start()

def send_acc_angle(acceleration, angle):
    try:
        message = f"{acceleration},{angle}\n".encode()
        client_socket.sendall(message)
    except Exception as e:
        print("Error sending angle:", e)


def connect_to_server():
   while True:
       try:
           client_socket.connect((server_ip, server_port))
           print("Connected to the server.")
           break
       except Exception as e:
           print("Connection failed:", e)
           time.sleep(1)


def calculate_angle(a, b, c):
   """
   Function to calculate the angle between three points (a, b, c)
   """
   radians = math.atan2(c[0]-b[0], c[1]-b[1]) - math.atan2(a[0]-b[0], a[1]-b[1])
   angle = math.degrees(radians)
   angle = (angle + 360) % 360  # Ensure angle is within [0, 360) range
   if angle > 180:
       angle -= 360  # Map angle to (-180, 180] range
   return angle


def print_arm_position(angle):
   """
   Function to print arm position
   """
   if angle > 0:
       position = "To the left"
   elif angle < 0:
       position = "To the right"
   else:
       position = "Straight up or down"
   return position


def print_arm_direction(angle):
   """
   Function to print arm direction
   """
   if angle < 90 and angle > -90 :
       direction = "Up"
   else:
       direction = "Down"
   return direction


def map_angle(angle):
    if angle < -90:
        return (angle + 180)
    elif angle > 90:
        return (angle - 180)
    else:
        return angle
      
def main():
   global acceleration
   with mp_pose.Pose(min_detection_confidence=0.5, min_tracking_confidence=0.5) as pose:
       #connects to websocket
       connect_to_server()


       while cap.isOpened():
           ret, frame = cap.read()
           frame = cv2.flip(frame, 1)
           image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
           results = pose.process(image)
           image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)


           try:
               landmarks = results.pose_landmarks.landmark
               for connection in [(mp_pose.PoseLandmark.LEFT_SHOULDER, mp_pose.PoseLandmark.LEFT_WRIST)]:
                   shoulder = mp_drawing._normalized_to_pixel_coordinates(landmarks[connection[0].value].x,
                                                                         landmarks[connection[0].value].y,
                                                                         frame.shape[1], frame.shape[0])
                   wrist = mp_drawing._normalized_to_pixel_coordinates(landmarks[connection[1].value].x,
                                                                       landmarks[connection[1].value].y,
                                                                       frame.shape[1], frame.shape[0])
                  
               # Check if both shoulder and wrist landmarks are detected
               if shoulder and wrist:
                   cv2.line(image, shoulder, wrist, (0, 255, 0), 2)  # Green color


                   # Calculate angle relative to a straight line pointing up
                   angle = calculate_angle((shoulder[0], shoulder[1] - 100), shoulder, wrist)
                   angle = map_angle(angle)
                   # Print arm position on one corner
                   arm_position = print_arm_position(angle)
                   cv2.putText(image, arm_position, (30, 50),
                               cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)


                   # Print arm direction on the other corner
                   arm_direction = print_arm_direction(angle)
                   cv2.putText(image, arm_direction, (image.shape[1] - 180, 50),
                               cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)


                   # Print angle
                   cv2.putText(image, f"Angle: {angle:.2f} degrees", (30, 100),
                               cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)

                   #this make sure that initial position goes from start to final position of ideal swing
                   if arm_direction.lower() == "down":
                       one_count = "down"

                   if acceleration is not None:
                           print("Angle: ", angle)
                           print("Acceleration: ", acceleration)
                           send_acc_angle(angle, acceleration)
                           acceleration = None
                    


                   #once valid swing is detected send signal: last angle
                #    if acceleration is not None:
                #        if arm_direction.lower() == "up" and one_count == "down":
                #            print("Angle: ", angle)
                #            print("Acceleration: ", acceleration)
                #     #    sends angle to server
                #     #    send_angle(angle)
                #            one_count = "up"


                  
           except Exception as e:
               print("Error processing frame:", e)


           cv2.imshow('Mediapipe Feed', image)


           if cv2.waitKey(10) & 0xFF == ord('q'):
               break


if __name__ == "__main__":
   try:
       main()
   except KeyboardInterrupt:
       pass
   finally:
    #    disconnectes from websocket
       client_socket.close()
       cap.release()
       cv2.destroyAllWindows()
       client.loop_stop()