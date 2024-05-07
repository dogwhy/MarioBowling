using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class ArrowController : MonoBehaviour
{
    private Ball ballScript;
    private MqttClient client;
    public static string message = "";
    public static ArrowController Instance { get; private set; }

    private float angle = 0;

    public float Angle
    {
        get { return angle; }
        set { angle = value; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional, if you want the instance to persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find the GameObject with the Ball script attached
        GameObject ballGameObject = GameObject.FindGameObjectWithTag("Ball");

        // Get the Ball script component attached to the GameObject
        ballScript = ballGameObject.GetComponent<Ball>();

        // Initialize MQTT client
        client = new MqttClient("mqtt.eclipseprojects.io");
        client.MqttMsgPublishReceived += OnMessageReceived;

        // Connect to MQTT broker
        client.Connect("UnityClient");
        
        // Subscribe to MQTT topic
        client.Subscribe(new string[] { "ece/180dw/shafee" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        // Handle received MQTT message
        message = Encoding.UTF8.GetString(e.Message);
        Debug.Log("Received MQTT message: " + message);
        // You can parse the MQTT message and adjust the angle if needed
        // angle = float.Parse(message); // Example of parsing
    }

    void OnDestroy()
    {
        // Disconnect from MQTT broker when the script is destroyed
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        // Use the shared angle value
        if (angle > 0)
        {
            transform.Rotate(Vector3.down, angle * 0.2f);
            angle = 0;
        }
        if (angle < 0)
        {
            transform.Rotate(Vector3.up, angle * -0.2f);
            angle = 0;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.down, Time.deltaTime * 30f);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up, Time.deltaTime * 30f);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (transform.localScale.z < 2)
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z + 1 * Time.deltaTime);
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 2);
            }
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (transform.localScale.z > 0.1f)
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z - 1 * Time.deltaTime);
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 0.1f);
        }
    }
}
}
