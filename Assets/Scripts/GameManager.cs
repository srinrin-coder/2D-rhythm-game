using UnityEngine;
using UnityEngine.UI; // 従来のText用
using TMPro;          // TextMeshPro用（追加）
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI設定")]
    // 型を TMP_Text に変えることで、従来のTextとTextMeshProの両方を受け入れ可能になります
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject clearUI;
    [SerializeField] private TMP_Text finalScoreText;

    private int currentScore = 0;
    private bool isGameFinished = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreUI();
        if (clearUI != null) clearUI.SetActive(false);
    }

    public void AddScore(int amount)
    {
        if (isGameFinished) return;
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"SCORE: {currentScore:D6}";
    }

    public void WinGame()
    {
        if (isGameFinished) return;
        isGameFinished = true;
        
        if (clearUI != null) clearUI.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = $"FINAL SCORE: {currentScore}";
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}