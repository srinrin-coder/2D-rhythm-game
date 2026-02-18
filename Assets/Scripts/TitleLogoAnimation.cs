using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトルロゴをUI(Image)上でアニメーションさせるクラス
/// </summary>
[RequireComponent(typeof(Image))]
public class TitleLogoAnimation : MonoBehaviour
{
    [Header("アニメーション設定")]
    [Tooltip("ロゴのアニメーション画像を順番に登録")]
    public Sprite[] logoFrames;

    [Tooltip("1秒間に何枚めくるか（速度）")]
    public float fps = 12f;

    private Image logoImage;
    private float timer;
    private int currentFrame;

    void Awake()
    {
        logoImage = GetComponent<Image>();
    }

    void Update()
    {
        if (logoFrames == null || logoFrames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / fps)
        {
            timer = 0;
            currentFrame = (currentFrame + 1) % logoFrames.Length;
            logoImage.sprite = logoFrames[currentFrame];
        }
    }
}