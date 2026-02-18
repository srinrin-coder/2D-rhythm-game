using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトル画面のボタン操作を管理
/// </summary>
public class TitleNavigation : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("読み込むゲーム本編のシーン名")]
    public string gameSceneName = "GameScene";

    /// <summary>
    /// STARTボタンから呼び出す
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}