using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<MyPlayerMove>() != null)
        {
            Debug.Log("Goal Reached!");

            // 音楽を停止
            if (Conductor.Instance != null)
            {
                Conductor.Instance.Stop();
            }

            // --- 変更: プレイヤーを非表示にする ---
            collision.gameObject.SetActive(false); 
            // ------------------------------------

            // GameManagerに通知（クリア画面表示）
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }
    }

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