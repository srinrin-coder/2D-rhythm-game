using UnityEngine;

/// <summary>
/// BPMに完全同期してスプライトを切り替えるクラス。
/// UnityのAnimatorは使わず、計算で絵を差し替える。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BeatAnimation : MonoBehaviour
{
    [Header("アニメーション設定")]
    [Tooltip("パラパラ漫画の画像の順番")]
    public Sprite[] sprites;

    [Tooltip("1拍あたりに進むコマ数 (例: 1なら1拍ごとに画像変更、2なら1拍に2枚)")]
    public float framesPerBeat = 1.0f;

    [Tooltip("ループするかどうか")]
    public bool loop = true;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Conductor.Instance == null || sprites.Length == 0) return;

        // 1. 現在の曲の位置（秒）を取得
        double songPosition = Conductor.Instance.GetSongPosition();

        // 2. 秒数を「拍数」に変換
        // BPM 60なら 1秒=1拍。BPM 120なら 0.5秒=1拍。
        double secPerBeat = 60.0 / Conductor.Instance.bpm;
        double currentBeat = songPosition / secPerBeat;

        // 曲開始前は0フレーム目で待機
        if (currentBeat < 0)
        {
            spriteRenderer.sprite = sprites[0];
            return;
        }

        // 3. 現在の拍数に基づいて、表示すべき画像のインデックスを計算（ここが核心）
        // (現在の拍 × 倍率) を 画像枚数で割った余り ＝ 現在のページ番号
        int totalFrames = (int)(currentBeat * framesPerBeat);
        int frameIndex = 0;

        if (loop)
        {
            frameIndex = totalFrames % sprites.Length;
        }
        else
        {
            frameIndex = Mathf.Min(totalFrames, sprites.Length - 1);
        }

        // 4. 画像を差し替える
        spriteRenderer.sprite = sprites[frameIndex];
    }
}