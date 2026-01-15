using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Tooltip("倒された時のエフェクト（パーティクルなど）があればここに設定")]
    public GameObject deathEffect;

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

        // 自分自身を削除（消滅）
        Destroy(gameObject);
    }
}