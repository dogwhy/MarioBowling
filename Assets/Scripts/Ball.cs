using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody rb;
    public float startSpeed = 40f;
    private Transform _arrow;
    private Vector3 initialPosition;  // To store the initial position of the ball
    private Vector3 originalScale; // To store the original scale of the ball

    void Start()
    {
        _arrow = GameObject.FindGameObjectWithTag("Arrow").transform;
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;  // Store the initial position at game start
        originalScale = transform.localScale; // Store the original scale at game start
        Debug.Log("Initial Position Stored: " + initialPosition);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        rb.isKinematic = false;
        Vector3 forceVector = _arrow.forward * (startSpeed * _arrow.transform.localScale.z);
        rb.AddForce(forceVector, ForceMode.Impulse);
        Invoke("CheckPins", 5); // Delay to check pins
    }

    private void CheckPins()
    {
        GameManager.Instance.PlayerFinishedTurn();
    }

    public void ResetPosition()
    {
        Debug.Log("Resetting pin Position to: " + initialPosition);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;  // Stop the ball's physics simulation temporarily
        transform.position = initialPosition;  // Reset to the initial position
        rb.isKinematic = false;  // Re-enable physics if necessary
    }

    public void ResetSize()
    {
        transform.localScale = originalScale; // Reset the ball size to its original scale
    }
}
