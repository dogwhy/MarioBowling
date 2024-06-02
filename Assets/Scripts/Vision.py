import cv2
import mediapipe as mp
import numpy as np
import math
import socket
import time


mp_pose = mp.solutions.pose
mp_drawing = mp.solutions.drawing_utils


cap = cv2.VideoCapture(0)


server_ip = "127.0.0.1"
server_port = 8080


client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

one_count = ""

def send_angle(angle):
    try:
        message = f"{angle}\n".encode()
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
   threshold = {
       (-20, 20): "0",
       (20, 40): "20",
       (40, 60): "40",
       (60, 75): "60",
       (75, 105): "90",
       # (105, 135): "120",
       # (135, 165): "150",
       # (165, 180): "180",
       (-40, -20): "-20",
       (-60, -40): "-40",
       (-75, -60): "-60",
       (-105, -75): "-90",
       # (-135, -105): "-120",
       # (-165, -135): "-150",
       # (-180, -165): "-180"
   }
   




   for range_, label in threshold.items():
       start, end = range_
       if start <= angle <= end:
           return int(label)
   return int(180)
      
def main():
   with mp_pose.Pose(min_detection_confidence=0.5, min_tracking_confidence=0.5) as pose:
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
                   # cv2.line(image, shoulder, wrist, (0, 255, 0), 2)


                   # angle_radian = math.atan2(wrist[1] - shoulder[1], wrist[0] - shoulder[0])
                   # angle_degree = math.degrees(angle_radian)


                   # cv2.putText(image, f"Angle: {angle_degree:.2f}", (shoulder[0] + 50, shoulder[1] - 20),
                   #             cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2, cv2.LINE_AA)


                   

                   #this make sure that initial position goes from start to final position of ideal swing
                   if arm_direction.lower() == "down":
                       one_count = "down"


                   #once valid swing is detected send signal: last angle
                   if arm_direction.lower() == "up" and one_count == "down":
                       time.sleep(0.5)
                       send_angle(angle)
                       one_count = "up"

                  
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
       client_socket.close()
       cap.release()
       cv2.destroyAllWindows()