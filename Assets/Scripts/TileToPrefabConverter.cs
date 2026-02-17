using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// タイルマップ上の特定のタイルを、実行時にアニメーション付きのPrefabに置き換えるクラス。
/// これにより、タイルを描く感覚でリズムガイドを配置できる。
/// </summary>
public class TileToPrefabConverter : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("スキャン対象のタイルマップ")]
    public Tilemap targetTilemap;

    [Tooltip("置き換え対象のタイル（パレットからドラッグ＆ドロップ）")]
    public TileBase markerTile;

    [Tooltip("置き換え後のアニメーションPrefab（BeatAnimationが付いたもの）")]
    public GameObject animatedPrefab;

    void Awake()
    {
        if (targetTilemap == null || markerTile == null || animatedPrefab == null)
        {
            Debug.LogWarning("TileToPrefabConverterの設定が不足しています。");
            return;
        }

        ReplaceTiles();
    }

    /// <summary>
    /// タイルをスキャンしてPrefabに置き換える
    /// </summary>
    void ReplaceTiles()
    {
        // タイルマップの範囲を取得
        BoundsInt bounds = targetTilemap.cellBounds;
        TileBase[] allTiles = targetTilemap.GetTilesBlock(bounds);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                TileBase tile = targetTilemap.GetTile(new Vector3Int(x, y, 0));

                if (tile == markerTile)
                {
                    // 1. タイルを消去
                    targetTilemap.SetTile(new Vector3Int(x, y, 0), null);

                    // 2. タイルの中心位置を計算
                    Vector3 spawnPos = targetTilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));

                    // 3. アニメーションPrefabを生成
                    GameObject obj = Instantiate(animatedPrefab, spawnPos, Quaternion.identity);
                    
                    // タイルマップと同じ親に入れる
                    obj.transform.SetParent(targetTilemap.transform);
                }
            }
        }
    }
}