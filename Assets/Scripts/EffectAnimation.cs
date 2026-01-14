using UnityEngine;

/// <summary>
/// 連続した画像をパラパラ漫画のように再生し、
/// 最後まで再生し終わったら自動的に自分自身を削除するクラス。
/// 爆発エフェクトやヒットエフェクト用。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EffectAnimation : MonoBehaviour
{
    [Header("画像設定")]
    [Tooltip("再生する画像のリスト（21枚をここに登録）")]
    public Sprite[] sprites;

    [Header("再生速度")]
    [Tooltip("1秒間に何枚めくるか（FPS）。値が大きいほど速い。\n目安: 21枚を0.3秒で再生したいなら約60〜70")]
    public float fps = 60f;

    [Tooltip("生成された瞬間から再生を開始するか")]
    public bool playOnAwake = true;

    // --- オプション: BPM同期 ---
    [Header("BPM同期設定 (任意)")]
    [Tooltip("これをオンにするとFPS設定を無視して、BPMに合わせて再生速度を自動調整します")]
    public bool syncToBpm = false;
    [Tooltip("何拍で再生を完了させるか（例: 0.5なら半拍でパッと消える）")]
    public float durationInBeats = 1.0f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private double spawnTime; // BPM同期用

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (syncToBpm && Conductor.Instance != null)
        {
            spawnTime = AudioSettings.dspTime;
        }

        // 最初の画像を表示
        if (sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }

    void Update()
    {
        if (sprites.Length == 0) return;

        int frameIndex = 0;

        if (syncToBpm && Conductor.Instance != null)
        {
            // --- BPM同期モード ---
            // 経過時間（秒）
            double timeSinceSpawn = AudioSettings.dspTime - spawnTime;
            
            // 1拍の秒数
            double secPerBeat = 60.0 / Conductor.Instance.bpm;
            
            // 目標とする総再生時間（秒）
            double totalDuration = secPerBeat * durationInBeats;

            // 進捗率 (0.0 ～ 1.0)
            double progress = timeSinceSpawn / totalDuration;

            if (progress >= 1.0)
            {
                Destroy(gameObject);
                return;
            }

            frameIndex = (int)(progress * sprites.Length);
        }
        else
        {
            // --- 通常FPSモード ---
            timer += Time.deltaTime;
            
            // 経過時間 × FPS = 現在のフレーム番号
            frameIndex = (int)(timer * fps);

            if (frameIndex >= sprites.Length)
            {
                // 全枚数再生し終わったら削除
                Destroy(gameObject);
                return;
            }
        }

        // 画像を更新
        if (frameIndex < sprites.Length)
        {
            spriteRenderer.sprite = sprites[frameIndex];
        }
    }
}