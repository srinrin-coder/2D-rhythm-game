using UnityEngine;

/// <summary>
/// 敵クラス
/// リスポーン時に復活する機能を追加
/// </summary>
public class Enemy : MonoBehaviour
{
    [Tooltip("倒された時のエフェクト（パーティクルなど）があればここに設定")]
    public GameObject deathEffect;

    void Start()
    {
        // リスポーンイベントを購読
        MyPlayerMove.OnPlayerRespawn += ResetEnemy;
    }

    void OnDestroy()
    {
        MyPlayerMove.OnPlayerRespawn -= ResetEnemy;
    }

    /// <summary>
    /// リスポーン時に呼ばれる復活処理
    /// </summary>
    private void ResetEnemy()
    {
        // 再表示して復活させる
        gameObject.SetActive(true);
    }

    /// <summary>
    /// プレイヤーから攻撃された時に呼ばれる関数
    /// </summary>
    public void OnDefeated()
    {
        // エフェクトがあれば生成
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 【変更点】自分自身を削除せず、非表示にする
        gameObject.SetActive(false);
    }
}