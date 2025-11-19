using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 楽曲の再生と、映像フレームレートに依存しない「正確かつ滑らかな時間」を管理するクラス。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Conductor : MonoBehaviour
{
    public static Conductor Instance { get; private set; }

    [Header("設定")]
    [Tooltip("BPM (Beats Per Minute)")]
    public double bpm = 120.0;
    
    [Tooltip("曲の開始前に設ける遅延時間（秒）。ロード直後やリトライ時の「間」を作ります")]
    public double startDelay = 1.0;

    [Tooltip("オーディオのレイテンシー補正（秒）。")]
    public double audioLatency = 0.0;

    private const int RegressionBufferSize = 15;

    private AudioSource musicSource;
    private double dspStartTime;
    private bool isPlaying = false;

    // 線形回帰用バッファ
    private Queue<double> gameTimeHistory = new Queue<double>();
    private Queue<double> dspTimeHistory = new Queue<double>();
    
    private double slope = 1.0;
    private double intercept = 0.0;
    private double lastSmoothedTime = 0.0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
    }

    void Start()
    {
        StartSong();
    }

    /// <summary>
    /// 曲の再生を開始（または予約）する
    /// </summary>
    private void StartSong()
    {
        // 現在の原子時計の時間 + 待機時間 を開始時刻とする
        dspStartTime = AudioSettings.dspTime + startDelay;
        
        // 音声の再生予約
        musicSource.PlayScheduled(dspStartTime);
        
        isPlaying = true;
        lastSmoothedTime = -startDelay; // 時間管理変数のリセット
    }

    /// <summary>
    /// ゲームオーバー時などに曲を最初からやり直す機能
    /// </summary>
    public void Restart()
    {
        // 1. 曲を止める
        musicSource.Stop();

        // 2. 過去の統計データ（回帰分析用）をクリアする
        // これをやらないと「過去の時間」に引きずられて計算がおかしくなる
        gameTimeHistory.Clear();
        dspTimeHistory.Clear();

        // 3. 再度スケジュールして再生
        StartSong();
    }

    void Update()
    {
        if (!isPlaying) return;

        double currentDspTime = AudioSettings.dspTime;
        double currentGameTime = Time.unscaledTimeAsDouble;

        gameTimeHistory.Enqueue(currentGameTime);
        dspTimeHistory.Enqueue(currentDspTime);

        if (gameTimeHistory.Count > RegressionBufferSize)
        {
            gameTimeHistory.Dequeue();
            dspTimeHistory.Dequeue();
        }

        CalculateLinearRegression();
    }

    private void CalculateLinearRegression()
    {
        if (gameTimeHistory.Count < 2) return;

        double meanX = gameTimeHistory.Average();
        double meanY = dspTimeHistory.Average();

        double sumXY = 0;
        double sumX2 = 0;

        var xArray = gameTimeHistory.ToArray();
        var yArray = dspTimeHistory.ToArray();

        for (int i = 0; i < xArray.Length; i++)
        {
            sumXY += (xArray[i] - meanX) * (yArray[i] - meanY);
            sumX2 += (xArray[i] - meanX) * (xArray[i] - meanX);
        }

        if (sumX2 != 0)
        {
            slope = sumXY / sumX2;
            intercept = meanY - (slope * meanX);
        }
    }

    public double GetSongPosition()
    {
        if (!isPlaying) return -startDelay; // 再生前はマイナス時間を返す

        // 予測DSP時間の計算
        double estimatedDspTime = (Time.unscaledTimeAsDouble * slope) + intercept;

        // 時間逆行防止（リスタート時はリセットされているので問題ない）
        if (estimatedDspTime < lastSmoothedTime)
        {
            estimatedDspTime = lastSmoothedTime;
        }
        lastSmoothedTime = estimatedDspTime;

        // 曲位置 = 現在時刻 - 開始時刻 - 補正
        return estimatedDspTime - dspStartTime - audioLatency;
    }
}
