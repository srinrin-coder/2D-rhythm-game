using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// コード（譜面データ）に基づいて、ステージの地形や敵を自動生成するクラス。
/// 穴の幅を固定し、配置密度を劇的に高めた高難易度・高密度バージョン。
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("PlayerのMeters Per Beatと同じ値にしてください")]
    public float metersPerBeat = 4f; 
    
    [Tooltip("生成の基準となるX座標（プレイヤーのスタート位置など）")]
    public float startXOffset = 0f;

    [Header("高さ設定 (Y座標)")]
    public float groundY = -2.0f; // 地面の高さ
    public float enemyY = -1.0f;  // 地上の敵の高さ
    public float coinY = 1.0f;    // コインの高さ
    public float flyEnemyY = 2.0f;// 空中の敵の高さ

    [Header("参照 (References)")]
    [Tooltip("地面を描画するためのタイルマップ")]
    public Tilemap groundTilemap;
    [Tooltip("表面のタイル（ひび割れたコンクリートなど）")]
    public TileBase groundTile;
    [Tooltip("基礎のタイル（白いコンクリートなど）")]
    public TileBase foundationTile; 

    [Header("プレハブ (Prefabs)")]
    public GameObject enemyPrefab;
    public GameObject flyingEnemyPrefab;
    public GameObject coinPrefab;
    public GameObject goalPrefab;
    public GameObject jumpMarkPrefab;

    private Transform objectParent;

    // 定数：1拍あたりの距離が4m(4タイル)の場合
    // 1タイル = 0.25拍
    private const float ONE_TILE_BEAT = 0.25f;
    private const float PIT_SIZE_BEAT = 0.75f; // 3タイル分

    void Start()
    {
        GameObject parentObj = new GameObject("GeneratedLevel");
        objectParent = parentObj.transform;

        if (groundTilemap != null) groundTilemap.ClearAllTiles();

        BuildLevel();
    }

    /// <summary>
    /// ステージ構成 (高密度版)
    /// </summary>
    void BuildLevel()
    {
        float currentBeat = 0f;

        // --- 1. イントロ (0 - 16拍) ---
        // スタートダッシュ
        AddGround(currentBeat, 16);
        
        // 0.5拍ごとのコイン配置 (密度アップ)
        for (float b = 2; b < 16; b += 0.5f) 
        { 
            AddCoin(b); 
        }
        currentBeat += 16;

        // --- 2. 敵ラッシュ練習 (16 - 48拍) ---
        // 地面は繋がっているが、敵が1拍おきに出現
        AddGround(currentBeat, 32);
        
        for (float b = currentBeat; b < currentBeat + 32; b += 1.0f)
        {
            if (b % 4 == 0) // 4拍に1回は休憩（コイン）
            {
                AddCoin(b);
                AddCoin(b + 0.5f);
            }
            else
            {
                // ランダムで地上か空中
                if (Random.value > 0.3f) 
                {
                    AddEnemy(b); 
                    AddMark(b); // 攻撃タイミングの目印
                }
                else 
                {
                    AddFlyingEnemy(b);
                }
            }
        }
        currentBeat += 32;

        // --- 3. 連続ジャンプ地帯 (48 - 96拍) ---
        // 穴(3タイル固定)と小足場(1タイル〜2タイル)の連続
        // 密度5倍＝休む暇を与えない
        
        float sectionEnd = 96f;
        while (currentBeat < sectionEnd)
        {
            // 地面生成 (1拍〜2拍のランダムな長さ)
            // ただし、必ずタイル1つ分(0.25拍)以上は確保する
            float groundLen = (Random.Range(0, 3) == 0) ? 1.0f : 2.0f;
            
            // ランダムでギリギリジャンプ用の短い足場(1タイル)を混ぜる
            if (Random.value > 0.8f) groundLen = ONE_TILE_BEAT; 

            AddGround(currentBeat, groundLen);

            // 地面の上に敵かコインを置く
            if (groundLen >= 1.0f) 
            {
                AddEnemy(currentBeat + 0.5f);
            }
            else
            {
                AddCoin(currentBeat); // 短い足場にはコイン
            }

            currentBeat += groundLen;

            // 穴を開ける (3タイル固定)
            if (currentBeat + PIT_SIZE_BEAT < sectionEnd)
            {
                AddMark(currentBeat - 0.25f); // ジャンプ目印
                AddCoin(currentBeat + (PIT_SIZE_BEAT / 2)); // 穴の真ん中にコイン（ジャンプ軌道）
                
                // 穴の時間を進める（地面を作らない＝穴）
                currentBeat += PIT_SIZE_BEAT;
            }
        }

        // --- 4. コイン乱舞 (96 - 128拍) ---
        AddGround(currentBeat, 32);
        for (float b = currentBeat; b < currentBeat + 32; b += 0.25f) // 16分音符間隔
        {
            // 波のような配置
            float yOffset = Mathf.Sin(b) * 1.5f;
            SpawnObject(coinPrefab, b, coinY + yOffset);
        }
        currentBeat += 32;

        // --- 5. クライマックス (128 - 192拍) ---
        // 敵、穴、空中敵の複合ラッシュ
        sectionEnd = 192f;
        while (currentBeat < sectionEnd)
        {
            float groundLen = 4.0f;
            AddGround(currentBeat, groundLen);

            // 敵を詰め込む
            AddEnemy(currentBeat + 1.0f);
            AddFlyingEnemy(currentBeat + 1.5f);
            AddEnemy(currentBeat + 2.0f);
            AddCoin(currentBeat + 3.0f);

            currentBeat += groundLen;

            // 穴
            if (currentBeat + PIT_SIZE_BEAT < sectionEnd)
            {
                AddMark(currentBeat - 0.25f);
                // 穴の上に敵を置く（倒して飛ぶ）
                AddFlyingEnemy(currentBeat + (PIT_SIZE_BEAT / 2)); 
                currentBeat += PIT_SIZE_BEAT;
            }
        }

        // --- 6. ラストラン (192 - 210拍) ---
        AddGround(currentBeat, 25); // ゴールまで
        for(float b = currentBeat; b < currentBeat + 15; b += 0.5f)
        {
            AddEnemy(b); // 最後の猛攻
        }
        
        AddGoal(210);
    }

    /// <summary>
    /// 地面を生成（表面1層 + 基礎3層）
    /// </summary>
    void AddGround(float startBeat, float durationInBeats)
    {
        if (groundTilemap == null || groundTile == null) return;

        float startWorldX = BeatToX(startBeat);
        float endWorldX = BeatToX(startBeat + durationInBeats);

        Vector3Int startCell = groundTilemap.WorldToCell(new Vector3(startWorldX, groundY, 0));
        Vector3Int endCell = groundTilemap.WorldToCell(new Vector3(endWorldX, groundY, 0));

        for (int x = startCell.x; x <= endCell.x; x++)
        {
            // 1. 一番上（表面）
            groundTilemap.SetTile(new Vector3Int(x, startCell.y, 0), groundTile);

            // 2. その下（基礎）: FoundationTileが設定されていればそれを使う
            TileBase foundation = (foundationTile != null) ? foundationTile : groundTile;

            // 地面の下に3ブロック分の基礎を敷く
            groundTilemap.SetTile(new Vector3Int(x, startCell.y - 1, 0), foundation);
            groundTilemap.SetTile(new Vector3Int(x, startCell.y - 2, 0), foundation);
            groundTilemap.SetTile(new Vector3Int(x, startCell.y - 3, 0), foundation);
        }
    }

    void AddEnemy(float beat)
    {
        SpawnObject(enemyPrefab, beat, enemyY);
    }

    void AddFlyingEnemy(float beat)
    {
        SpawnObject(flyingEnemyPrefab, beat, flyEnemyY);
    }

    void AddCoin(float beat)
    {
        SpawnObject(coinPrefab, beat, coinY);
    }

    void AddGoal(float beat)
    {
        SpawnObject(goalPrefab, beat, enemyY);
    }

    void AddMark(float beat)
    {
        SpawnObject(jumpMarkPrefab, beat, enemyY - 0.5f);
    }

    void SpawnObject(GameObject prefab, float beat, float yPos)
    {
        if (prefab == null) return;

        float xPos = BeatToX(beat);
        Vector3 pos = new Vector3(xPos, yPos, 0);

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        obj.transform.SetParent(objectParent);
    }

    float BeatToX(float beat)
    {
        return startXOffset + (beat * metersPerBeat);
    }
}