using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;

public class PoseReceiver : MonoBehaviour
{
    TcpListener server;
    TcpClient client;
    NetworkStream stream;
    byte[] buffer = new byte[1024];
    StringBuilder dataBuffer = new StringBuilder();

    void Start()
    {
        // Start TCP server
        server = new TcpListener(IPAddress.Any, 8080);
        server.Start();

        // Start listening for incoming connections in a background thread
        Thread listenerThread = new Thread(ListenForConnections);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    void ListenForConnections()
    {
        try
        {
            // Accept incoming connection
            client = server.AcceptTcpClient();
            Debug.Log("Client connected.");

            // Get stream for reading data
            stream = client.GetStream();

            // Continue listening for data
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) continue;

                string incomingData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                dataBuffer.Append(incomingData);

                while (dataBuffer.ToString().Contains("\n"))
                {
                    string[] dataLines = dataBuffer.ToString().Split('\n');
                    string completeData = dataLines[0].Trim();

                    if (float.TryParse(completeData, out float angle))
                    {
                        Debug.Log("Received angle: " + angle);
                        ArrowController.Instance.Angle = angle;
                    }
                    else
                    {
                        Debug.LogError("Failed to parse angle: " + completeData);
                    }

                    dataBuffer.Remove(0, completeData.Length + 1);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }

    void OnDestroy()
    {
        // Close connections and stop server when the script is destroyed
        stream?.Close();
        client?.Close();
        server?.Stop();
    }
}
