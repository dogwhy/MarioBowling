using UnityEngine;
using TMPro;
using System;
using System.Collections.Concurrent;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentRound = 1;
    public int currentPlayer = 1;
    public int[,] scores = new int[2, 5]; // Adjusted for 5 rounds and 2 players
    public int currTries = 1;
    public TextMeshProUGUI scoreText;  // Reference to the combined scoring text UI element (if still needed)
    public TextMeshProUGUI feedbackText;  // Reference to the feedback UI element
    public TextMeshProUGUI roundText;  // Reference to the round text UI element
    public TextMeshProUGUI player1ScoreText;  // Reference to the Player 1 score text UI element
    public TextMeshProUGUI player2ScoreText;  // Reference to the Player 2 score text UI element
    public Ball ball;  // Reference to the Ball script
    public Pin[] pins;  // Array of Pin objects

    public GameObject bulletBillPrefab; // Bullet Bill prefab
    public GameObject bananaPrefab; // Banana prefab
    public GameObject shellPrefab; // Shell prefab
    public GameObject mushroomPrefab; // Mushroom prefab

    public Transform laneTransform; // The transform representing the bowling lane

    private readonly ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

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
        roundText = FindComponentByTag<TextMeshProUGUI>("RoundText");
        player1ScoreText = FindComponentByTag<TextMeshProUGUI>("Player1");
        player2ScoreText = FindComponentByTag<TextMeshProUGUI>("Player2");
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

        UpdateFeedback($"Player {currentPlayer}'s turn.");
        UpdateRoundText();
        UpdateScoreText();
    }

    void Update()
    {
        while (actionQueue.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    public void Enqueue(Action action)
    {
        actionQueue.Enqueue(action);
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
        if (currentRound <= 5) // Ensure the round is within bounds
        {
            scores[currentPlayer - 1, currentRound - 1] += points;
            UpdateScoreText();
            UpdateFeedback($"Player {currentPlayer}'s turn.");
        }
        if (scores[currentPlayer - 1, currentRound - 1] == 10)
        {
            currTries = 2;
        }
        else
        {
            currTries += 1;
        }
    }

    public void UpdateFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    public void UpdateRoundText()
    {
        if (roundText != null)
        {
            roundText.text = $"Round: {currentRound}";
        }
    }

    public void UpdateScoreText()
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.text = BuildScoreString(0);
        }
        if (player2ScoreText != null)
        {
            player2ScoreText.text = BuildScoreString(1);
        }
        if (scoreText != null)
        {
            scoreText.text = $"Player 1: {GetTotalScore(0)} | Player 2: {GetTotalScore(1)}";
        }
    }

    private string BuildScoreString(int playerIndex)
    {
        string scoreString = "";
        for (int round = 0; round < 5; round++) // Loop through all 5 rounds
        {
            if (round > 0)
            {
                scoreString += " | ";
            }
            scoreString += $"{scores[playerIndex, round]}";
        }
        return scoreString;
    }

    private int GetTotalScore(int playerIndex)
    {
        int totalScore = 0;
        for (int round = 0; round < 5; round++) // Loop through all 5 rounds
        {
            totalScore += scores[playerIndex, round];
        }
        return totalScore;
    }

    public void PlayerFinishedTurn()
    {
        if (currTries > 1 || scores[currentPlayer - 1, currentRound - 1] == 10)
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;
            if (currentPlayer == 1)
            {
                currentRound++;
                if (currentRound > 5) // Adjusted to only run for 5 rounds
                {
                    EndGame();
                    return;
                }
                else
                {
                    StartNewRound();
                }
            }
            UpdateFeedback($"Player {currentPlayer}'s turn.");
            UpdateRoundText();
            ResetGameComponents();
        }
        else
        {
            UpdateFeedback($"Player {currentPlayer}'s turn.");
            UpdateRoundText();
            ContinueGame();
        }

        // Place power-ups randomly during the game
        if (UnityEngine.Random.value <= 0.5f)
        {
            PlaceRandomPowerUp();
        }
    }

    private void ContinueGame()
    {
        if (ball != null)
        {
            ball.ResetPosition();
        }
        else
        {
            Debug.LogError("Ball reference is null in ResetGameComponents!");
        }
    }

    private void ResetGameComponents()
    {
        if (ball != null)
        {
            ball.ResetPosition();
            ball.ResetSize(); // Reset the ball size
            // Enable the ball collider
            Collider ballCollider = ball.GetComponent<Collider>();
            if (ballCollider != null)
            {
                ballCollider.enabled = true;
            }
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
                    // Enable the pin collider
                    Collider pinCollider = pin.GetComponent<Collider>();
                    if (pinCollider != null)
                    {
                        pinCollider.enabled = true;
                    }
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

    private void StartNewRound()
    {
        // Enable colliders for ball and pins
        ResetGameComponents();
    }

    private void EndGame()
    {
        int totalScorePlayer1 = GetTotalScore(0);
        int totalScorePlayer2 = GetTotalScore(1);

        string winner;
        if (totalScorePlayer1 > totalScorePlayer2)
        {
            winner = "Player 1 wins!";
        }
        else if (totalScorePlayer2 > totalScorePlayer1)
        {
            winner = "Player 2 wins!";
        }
        else
        {
            winner = "It's a tie!";
        }

        if (feedbackText != null)
        {
            feedbackText.text += "\nGame Over!";
        }

        if (roundText != null)
        {
            roundText.text = winner;
        }
    }

    // Power-Up Placement Methods

    private void PlaceBulletBill()
    {
        Vector3 position = new Vector3(0, 2, -laneTransform.localScale.z / 2); // Starting at the beginning of the lane
        GameObject bulletBill = Instantiate(bulletBillPrefab, position, Quaternion.identity);
        bulletBill.GetComponent<BulletBill>().OnHitEndOfLane += () => StartCoroutine(EndTurnAfterDelay());
        Debug.Log("Placed Bullet Bill");
    }

    private void PlaceBanana()
    {
        Vector3 position = new Vector3(UnityEngine.Random.Range(-laneTransform.localScale.x / 2, laneTransform.localScale.x / 2), 0.1f, UnityEngine.Random.Range(0, laneTransform.localScale.z));
        Instantiate(bananaPrefab, position, Quaternion.identity);
        Debug.Log("Placed Banana");
    }

    private void PlaceShell()
    {
        Vector3 position = new Vector3(UnityEngine.Random.Range(-laneTransform.localScale.x / 2, laneTransform.localScale.x / 2), 0.1f, UnityEngine.Random.Range(0, laneTransform.localScale.z));
        Instantiate(shellPrefab, position, Quaternion.identity);
        Debug.Log("Placed Shell");
    }

    private void DropMushroom()
    {
        Vector3 position = new Vector3(ball.transform.position.x, ball.transform.position.y + 5, ball.transform.position.z);
        Instantiate(mushroomPrefab, position, Quaternion.identity);
        Debug.Log("Dropped Mushroom");
    }

    private void PlaceRandomPowerUp()
    {
        int randomPowerUp = UnityEngine.Random.Range(0, 4);
        switch (randomPowerUp)
        {
            case 0:
                PlaceBulletBill();
                break;
            case 1:
                PlaceBanana();
                break;
            case 2:
                PlaceShell();
                break;
            case 3:
                DropMushroom();
                break;
        }
    }

    private IEnumerator EndTurnAfterDelay()
    {
        yield return new WaitForSeconds(5);
        EndTurnImmediately();
    }

    private void EndTurnImmediately()
    {
        currTries = 2; // Ensures the turn ends immediately
        PlayerFinishedTurn();
    }
}
