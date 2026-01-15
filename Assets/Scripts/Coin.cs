using UnityEngine;

/// <summary>
/// プレイヤーが接触すると取得されるコインのクラス
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    [Tooltip("取得時のきらめきエフェクトなどがあれば設定（任意）")]
    public GameObject pickupEffect;

    /// <summary>
    /// 何かが触れた時に呼ばれるUnity標準の関数
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 当たった相手がプレイヤーかどうか確認する
        MyPlayerMove player = collision.GetComponent<MyPlayerMove>();

        if (player != null)
        {
            // 1. プレイヤーに「コインを拾ったぞ」と伝える（音を鳴らすため）
            player.GetCoin();

            // 2. エフェクトがあれば出す
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // 3. コイン自身を消滅させる
            Destroy(gameObject);
        }
    }
}