using UnityEngine;

/// <summary>
/// プレイヤーが接触すると取得されるコインのクラス
/// リスポーン時に復活する機能を追加
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    [Tooltip("取得時のきらめきエフェクトなどがあれば設定（任意）")]
    public GameObject pickupEffect;

    void Start()
    {
        // リスポーンイベントを購読して、リセット時に復活できるようにする
        MyPlayerMove.OnPlayerRespawn += ResetCoin;
    }

    void OnDestroy()
    {
        // シーン遷移やゲーム終了時にエラーにならないよう購読解除
        MyPlayerMove.OnPlayerRespawn -= ResetCoin;
    }

    /// <summary>
    /// リスポーン時に呼ばれる復活処理
    /// </summary>
    private void ResetCoin()
    {
        // 再表示して復活させる
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 当たった相手がプレイヤーかどうか確認する
        MyPlayerMove player = collision.GetComponent<MyPlayerMove>();

        if (player != null)
        {
            // 1. プレイヤーに「コインを拾ったぞ」と伝える
            player.GetCoin();

            // 2. エフェクトがあれば出す（エフェクトは使い捨てなので生成してOK）
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // 3. 【変更点】削除(Destroy)ではなく、非表示(SetActive false)にする
            // これによりオブジェクトはシーンに残るため、後で復活できる
            gameObject.SetActive(false);
        }
    }
}