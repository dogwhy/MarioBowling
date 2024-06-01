using UnityEngine;

public class Pin : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isKnockedOver;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if ((collision.collider.CompareTag("Ball") || collision.collider.CompareTag("Pin")) && !isKnockedOver)
        {
            if (Vector3.Angle(transform.up, Vector3.up) > 60)
            {
                isKnockedOver = true;
                GameManager.Instance.UpdateScore(1);  // Update the score using the GameManager
            }
        }
    }

    public void ResetPin()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        isKnockedOver = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}
