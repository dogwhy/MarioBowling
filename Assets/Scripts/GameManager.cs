using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int currentRound = 1;
    public int currentPlayer = 1;
    public int[] scores = new int[2];
    public TextMeshProUGUI scoreText;  // Reference to the scoring text UI element
    public TextMeshProUGUI feedbackText;  // Reference to the feedback UI element
    public Ball ball;  // Reference to the Ball script
    public Pin[] pins;  // Array of Pin objects

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        scoreText = FindComponentByTag<TextMeshProUGUI>("ScoreText");
        feedbackText = FindComponentByTag<TextMeshProUGUI>("Feedback");
    }

    void Start()
    {
        if (ball == null)
        {
            ball = FindObjectOfType<Ball>();
            if (ball == null)
            {
                Debug.LogError("Ball object not found!");
            }
        }

        if (pins == null || pins.Length == 0)
        {
            pins = FindObjectsOfType<Pin>();
            if (pins == null || pins.Length == 0)
            {
                Debug.LogError("Pins not found!");
            }
        }
    }

    private T FindComponentByTag<T>(string tag) where T : Component
    {
        GameObject obj = GameObject.FindGameObjectWithTag(tag);
        if (obj != null)
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            else
            {
                Debug.LogError("The object with tag '" + tag + "' does not have a component of type " + typeof(T));
            }
        }
        else
        {
            Debug.LogError("No object with tag '" + tag + "' found.");
        }
        return null;
    }

    public void UpdateScore(int points)
    {
        scores[currentPlayer - 1] += points;
        if (scoreText != null)
        {
            scoreText.text = $"Player 1: {scores[0]} | Player 2: {scores[1]}";
        }
    }

    public void UpdateFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    public void PlayerFinishedTurn()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        if (currentPlayer == 1)
        {
            currentRound++;
            if (currentRound > 10)
            {
                EndGame();
                return;
            }
        }
        ResetGameComponents();
    }

    private void ResetGameComponents()
    {
        if (ball != null)
        {
            ball.ResetPosition();
        }
        else
        {
            Debug.LogError("Ball reference is null in ResetGameComponents!");
        }

        if (pins != null && pins.Length > 0)
        {
            foreach (var pin in pins)
            {
                if (pin != null)
                {
                    pin.ResetPin();
                }
                else
                {
                    Debug.LogError("One of the pins is null in ResetGameComponents!");
                }
            }
        }
        else
        {
            Debug.LogError("Pins reference is null or empty in ResetGameComponents!");
        }
    }

    private void EndGame()
    {
        if (feedbackText != null)
        {
            feedbackText.text += "\nGame Over!";
        }
    }
}
