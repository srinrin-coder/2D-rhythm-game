using UnityEngine;

/// <summary>
/// ステージの終端に配置し、プレイヤーの到達を検知するクラス
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突した相手がプレイヤーかどうかをタグ、またはスクリプトの有無で判定
        if (collision.CompareTag("Player") || collision.GetComponent<MyPlayerMove>() != null)
        {
            Debug.Log("Goal Reached!");

            // GameManagerにゲームクリアを通知
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
            
            // プレイヤーの入力を無効化して立ち止まらせる（任意）
            // collision.GetComponent<MyPlayerMove>().enabled = false;
        }
    }

    // エディタ上で判定範囲を見やすくする
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);
        }
    }
}