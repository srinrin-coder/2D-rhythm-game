using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField]
    float scrollSpeed = -1;

    private float backgroundWidth;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            backgroundWidth = spriteRenderer.bounds.size.x;
        }

        // イベント購読は念のため残しておきますが、基本はUpdateで制御します
        MyPlayerMove.OnPlayerRespawn += ResetPosition;
    }

    void OnDestroy()
    {
        MyPlayerMove.OnPlayerRespawn -= ResetPosition;
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
    }

    void Update()
    {
        MoveSynchronized();
    }

    void MoveSynchronized()
    {
        if (Conductor.Instance == null) return;

        double songPosition = Conductor.Instance.GetSongPosition();
        
        // --- 修正点 ---
        // 時間がマイナス（リスタート直後）の場合、強制的に初期位置へ固定
        // これにより、イベントのResetPositionと二重になっても問題なく、
        // むしろフレームの更新順序によるズレを完全に防げます。
        if (songPosition < 0) 
        {
             transform.position = startPosition;
             return;
        }

        // 通常のスクロール計算
        float totalOffset = (float)(songPosition * scrollSpeed);
        float newX = startPosition.x + totalOffset;
        
        transform.position = new Vector3(newX, startPosition.y, startPosition.z);
    }
}