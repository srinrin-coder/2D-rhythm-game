using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 楽曲の再生と、映像フレームレートに依存しない「正確かつ滑らかな時間」を管理するクラス。
/// 途中再生機能（Debug Start Beat）と停止機能（Stop）を追加。
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

    [Header("デバッグ")]
    [Tooltip("指定した拍数からゲームを開始します（0なら最初から）。配置確認に便利です。")]
    public float debugStartBeat = 0f;

    private const int RegressionBufferSize = 15;

    private AudioSource musicSource;
    private double dspStartTime;
    
    // --- 変更: 外部から状態を確認できるようにプロパティ化 ---
    public bool IsPlaying { get; private set; } = false;
    // ----------------------------------------------------

    // 線形回帰用バッファ（滑らかな動きを実現するため）
    private Queue<double> gameTimeHistory = new Queue<double>();
    private Queue<double> dspTimeHistory = new Queue<double>();
    
    private double slope = 1.0;
    private double intercept = 0.0;

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
    }

    void Start()
    {
        Play();
    }

    /// <summary>
    /// 曲を再生する
    /// </summary>
    public void Play()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        
        // 開始位置（秒）の計算
        double seekTime = 0;
        if (debugStartBeat > 0)
        {
            seekTime = debugStartBeat * (60.0 / bpm);
        }

        if (seekTime > 0)
        {
            // --- 途中から再生する場合 ---
            // 待ち時間なしで即時再生します
            musicSource.time = (float)seekTime;
            musicSource.Play();

            // 「今」が「シーク位置」になるように基準時間を逆算して設定
            dspStartTime = AudioSettings.dspTime - seekTime;
        }
        else
        {
            // --- 最初から再生する場合 ---
            // 少し遅延（StartDelay）を入れて再生予約します
            musicSource.time = 0;
            musicSource.PlayScheduled(AudioSettings.dspTime + startDelay);
            
            // 基準時間は「StartDelay秒後」に設定
            dspStartTime = AudioSettings.dspTime + startDelay;
        }

        IsPlaying = true;
        
        // 回帰分析用の履歴をクリア
        gameTimeHistory.Clear();
        dspTimeHistory.Clear();
    }

    /// <summary>
    /// リトライ時などに呼ばれる
    /// </summary>
    public void Restart()
    {
        // デバッグ設定が残っていれば、リトライ時もその場所から再開します
        Play();
    }

    // --- 追加: 曲を停止する機能 ---
    /// <summary>
    /// 曲を停止し、進行を止める
    /// </summary>
    public void Stop()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        IsPlaying = false;
    }
    // ----------------------------

    void Update()
    {
        if (!IsPlaying) return;

        // 現在のオーディオ時間（DSP Time）
        double currentDspTime = AudioSettings.dspTime;
        
        // ゲーム時間（Time.time）との相関を取るためのデータを蓄積
        gameTimeHistory.Enqueue(Time.time);
        dspTimeHistory.Enqueue(currentDspTime);

        if (gameTimeHistory.Count > RegressionBufferSize)
        {
            gameTimeHistory.Dequeue();
            dspTimeHistory.Dequeue();
        }

        CalculateLinearRegression();
    }

    /// <summary>
    /// カクつき防止のため、オーディオ時間とゲーム時間の相関関係を計算する
    /// </summary>
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

    /// <summary>
    /// 現在の曲の位置（秒）を取得
    /// </summary>
    public double GetSongPosition()
    {
        if (!IsPlaying) return -startDelay;

        // 線形回帰を使って、フレーム変動に強い滑らかな時刻を取得
        double smoothDspTime = (slope * Time.time) + intercept;
        
        // データが溜まるまでは生のDSPタイムを使う
        if (gameTimeHistory.Count < 2)
        {
            smoothDspTime = AudioSettings.dspTime;
        }

        // 曲の位置 = (現在時刻 - 再生開始基準時刻) - レイテンシー
        return smoothDspTime - dspStartTime - audioLatency;
    }
}