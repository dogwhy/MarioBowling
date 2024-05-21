using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody rb;
    public float startSpeed = 40f;
    private Transform _arrow;

    void Start()
    {
        _arrow = GameObject.FindGameObjectWithTag("Arrow").transform;
        rb = GetComponent<Rigidbody>();
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
}
