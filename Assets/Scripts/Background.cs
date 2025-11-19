using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField]
    float scrollSpeed = -1;

    // 新規追加: 背景1枚のワールド空間での幅を保持します。
    private float backgroundWidth;
    private Vector3 startPosition;


    void Start()
    {

        startPosition = transform.position;
        //Debug.Log("【背景】イベント購読を開始します。初期位置: " + startPosition, this.gameObject);

        // 【重要】アタッチされているSpriteRendererから、背景画像の実際のワールド幅を取得します。
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            backgroundWidth = spriteRenderer.bounds.size.x;
        }
        else
        {
            Debug.LogError("背景オブジェクトにSpriteRendererがアタッチされていません。");
        }

        MyPlayerMove.OnPlayerRespawn += ResetPosition;
    }

    void OnDestroy()
    {
        //Debug.Log("【背景】イベント購読を解除します。", this.gameObject);
        MyPlayerMove.OnPlayerRespawn -= ResetPosition;
    }

    public void ResetPosition()
    {
        //Debug.Log("【背景】リスポーン通知を受け取りました。位置を " + startPosition + " にリセットします。", this.gameObject);
        transform.position = startPosition;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);
    }
}