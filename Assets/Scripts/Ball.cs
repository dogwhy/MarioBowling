using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ball : MonoBehaviour
{

    public Rigidbody rb; // reference to the Rigidbody component of the ball
    public float startSpeed = 40f; // the speed at which the ball starts moving

    private Transform _arrow;

    private bool _ballMoving;

    private Transform _startPosition;

    private List<GameObject> _pins = new();

    private readonly Dictionary<GameObject, Transform> _pinsDefaultTransform = new();

    private int previousPinsDown;

    public int Point { get; set; }



    private TextMeshProUGUI feedBack;


    private void Start()
    {
        Application.targetFrameRate = 60;

        _arrow = GameObject.FindGameObjectWithTag("Arrow").transform;

        rb = GetComponent<Rigidbody>();

        _startPosition = transform;

        _pins = GameObject.FindGameObjectsWithTag("Pin").ToList();

        foreach (var pin in _pins)
        {
            _pinsDefaultTransform.Add(pin, pin.transform);
        }

        feedBack = GameObject.FindGameObjectWithTag("FeedBack").GetComponent<TextMeshProUGUI>();
    }

    private void ResetPosition()
    {
    StopAllCoroutines();
    transform.position = _startPosition.position; // Reset position to the starting position
    transform.rotation = _startPosition.rotation; // Reset rotation to the starting rotation
    rb.velocity = Vector3.zero; // Reset velocity to zero
    rb.angularVelocity = Vector3.zero; // Reset angular velocity to zero
    _arrow.gameObject.SetActive(true); // Activate the arrow again
    _ballMoving = false; // Set ballMoving flag to false
    }
    


    void Update()
    {
        if (_ballMoving)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Shoot());
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R key pressed. Resetting position.");

            ResetPosition();
        }

    }

    public IEnumerator Shoot()
    {

        _ballMoving = true;
        _arrow.gameObject.SetActive(false);
        rb.isKinematic = false;

        // calculate the force vector to apply to the ball
        Vector3 forceVector = _arrow.forward * (startSpeed * _arrow.transform.localScale.z);

        // calculate the position at which to apply the force (in this case, the center of the ball)
        Vector3 forcePosition = transform.position + (transform.right * 0.5f);

        // apply the force at the specified position
        rb.AddForceAtPosition(forceVector, forcePosition, ForceMode.Impulse);


        yield return new WaitForSecondsRealtime(7);

        _ballMoving = false;

        GenerateFeedBack();

        yield return new WaitForSecondsRealtime(2);

        ResetGame();
    }


    private static void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void CalculatePinsDown()
    {
        GameObject[] pins = GameObject.FindGameObjectsWithTag("Pin");
        int pinsDown = 0;

        foreach (var pin in pins)
        {
            if (Vector3.Angle(pin.transform.up, Vector3.up) < 30f)
            {
                pinsDown++;
            }
        }

        int fallenPins = pinsDown - previousPinsDown;
        previousPinsDown = pinsDown;

    }
    
    private void GenerateFeedBack()
    {
        feedBack.text = Point switch
        {
            0 => "YOU SUCK!",
            > 0 and < 3 => "Spare!",
            >= 3 and < 6 => "Spare!",
            >= 6 and < 10 => "Spare!",
            _ => "STRIIIIIIKE"
        };

    }
}
