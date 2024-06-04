using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public static ArrowController Instance; // Singleton instance
    private float angle = 0;
    private float acceleration = 0;
    private Ball ball; // Reference to the Ball class

    void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ball = FindObjectOfType<Ball>(); // Find the Ball instance in the scene
        if (ball == null)
        {
            Debug.LogError("Ball object not found!");
        }
    }

    void Update()
    {
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

    public float Acceleration
    {
        get { return acceleration; }
        set
        {
            acceleration = value;
            Debug.Log("Setting acceleration (length): " + acceleration);
            Vector3 scale = transform.localScale;
            scale.z = Mathf.Clamp(acceleration, 0.1f, 2f); // Adjust the max value as needed
            transform.localScale = scale;
        }
    }

    public float Angle
    {
        get { return angle; }
        set
        {
            angle = (-1) * value;
            Debug.Log("Setting angle: " + angle);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            if (ball != null)
            {
                ball.Shoot(); // Execute Shoot method when angle is set
            }
        }
    }
}
