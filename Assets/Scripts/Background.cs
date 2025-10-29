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
        // cameraRectMin の計算は不要です。
        Debug.Log("【背景】イベント購読を開始します。初期位置: " + startPosition, this.gameObject);

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

        MyPlayerController.OnPlayerRespawn += ResetPosition;
    }

    void OnDestroy()
    {
        Debug.Log("【背景】イベント購読を解除します。", this.gameObject);
        MyPlayerController.OnPlayerRespawn -= ResetPosition;
    }

    public void ResetPosition()
    {
        Debug.Log("【背景】リスポーン通知を受け取りました。位置を " + startPosition + " にリセットします。", this.gameObject);
        transform.position = startPosition;
    }
    void Update()
    {
        Move();
    }

    void Move()
    {
        // 1. 移動 (変更なし)
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);

        // 2. ループ判定とテレポート (修正箇所)

        // 判定条件:
        // 背景オブジェクトのX座標が、現在のカメラの中心よりも
        // 「背景1枚分の幅」だけ左側に出たらテレポートする。
        // backgroundWidth / 2 ではなく backgroundWidth にしているのは、
        // 画面の幅ではなく、隣り合う背景オブジェクトの幅でループさせるためです。
        if (transform.position.x < Camera.main.transform.position.x - backgroundWidth)
        {
            // テレポート先:
            // 現在の位置から、背景2枚分（backgroundWidth * 2）だけ右に瞬間移動させる。
            // これにより、もう1枚の背景オブジェクトの真後ろにテレポートし、シームレスに繋がります。
            transform.position = new Vector2(transform.position.x + backgroundWidth * 2, transform.position.y);
        }
    }
}