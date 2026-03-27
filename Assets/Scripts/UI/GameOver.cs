using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance { get; private set; }

    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI statsText;
    public Button restartButton;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Make sure panel is hidden at start
        gameOverPanel.SetActive(false);

        // Hook up restart button
        restartButton.onClick.AddListener(RestartGame);
    }

    public void ShowGameOver()
    {
        // Pause the turn queue
        Time.timeScale = 0f;

        // Build stats string
        statsText.text =
            "Floor Reached: " +
            GameManager.Instance.currentFloor +
            "\nMobs Killed: " +
            GameManager.Instance.mobsKilled +
            "\nGold Collected: " +
            GameManager.Instance.currentGold +
            "\nLevel Reached: " +
            GameManager.Instance.playerLevel;

        // Show the panel
        gameOverPanel.SetActive(true);
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }
}