using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int currentRound = 1;
    public int currentPlayer = 1;
    public int[,] scores = new int[2, 2]; // Adjusted for 2 rounds and 2 players
    public int currActions = 0;
    public TextMeshProUGUI scoreText;  // Reference to the combined scoring text UI element (if still needed)
    public TextMeshProUGUI feedbackText;  // Reference to the feedback UI element
    public TextMeshProUGUI roundText;  // Reference to the round text UI element
    public TextMeshProUGUI player1ScoreText;  // Reference to the Player 1 score text UI element
    public TextMeshProUGUI player2ScoreText;  // Reference to the Player 2 score text UI element
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
        if (currentRound <= 2) // Ensure the round is within bounds
        {
            scores[currentPlayer - 1, currentRound - 1] += points;
            UpdateScoreText();
            UpdateFeedback($"Player {currentPlayer}'s turn.");
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
        for (int round = 0; round < 2; round++) // Fixed to loop through all rounds
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
        for (int round = 0; round < 2; round++)
        {
            totalScore += scores[playerIndex, round];
        }
        return totalScore;
    }

    public void PlayerFinishedTurn()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        if (currentPlayer == 1)
        {
            currentRound++;
            if (currentRound > 2) // Adjusted to only run for 2 rounds
            {
                EndGame();
                return;
            }
        }
        UpdateFeedback($"Player {currentPlayer}'s turn.");
        UpdateRoundText();
        
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
}
