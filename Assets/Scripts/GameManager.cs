using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro用
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI設定")]
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

    // --- 追加: キー入力の監視 ---
    void Update()
    {
        // ESCキーが押されたらタイトルに戻る
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 必要に応じて確認ダイアログなどを出すこともできますが、
            // 今回は即座にタイトルへ戻る挙動にします。
            BackToTitle();
        }
    }
    // -------------------------

    public void AddScore(int amount)
    {
        if (isGameFinished) return;
        currentScore += amount;
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        isGameFinished = false;
        UpdateScoreUI();
        
        if (clearUI != null) clearUI.SetActive(false);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"SCORE: {currentScore:D6}";
    }

    public void WinGame()
    {
        if (isGameFinished) return;
        isGameFinished = true;
        
        // 音楽を停止
        if (Conductor.Instance != null)
        {
            AudioSource music = Conductor.Instance.GetComponent<AudioSource>();
            if (music != null) music.Stop();
        }

        if (clearUI != null) clearUI.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = $"FINAL SCORE: {currentScore}";
    }

    public void BackToTitle()
    {
        // Build Settings で "TitleScene" が登録されている必要があります
        SceneManager.LoadScene("TitleScene");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}