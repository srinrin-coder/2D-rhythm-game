using UnityEngine;

/// <summary>
/// プレイヤーを自動で右に動かし、接地時のみジャンプさせるスクリプト（診断機能付き）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerAutoMove : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("プレイヤーの移動速度")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("ジャンプ設定")]
    [Tooltip("ジャンプの強さ（初速）。Rigidbody 2DのGravity Scaleと合わせて調整してください。")]
    [SerializeField] private float jumpForce = 8f;

    [Header("接地判定")]
    [Tooltip("地面と判定するレイヤー")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("地面を検出するための距離")]
    [SerializeField] private float groundCheckDistance = 0.1f;

    // --- プライベート変数 ---
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isGrounded;
    private bool jumpRequested = false;

    void Awake()
    {
        // --- 診断機能：スクリプトの重複チェック ---
        // このゲームオブジェクトにPlayerAutoMoveスクリプトが複数アタッチされていないか確認
        if (GetComponents<PlayerAutoMove>().Length > 1)
        {
            Debug.LogError("PlayerAutoMove スクリプトが同じオブジェクトに複数アタッチされています！一つにしてください。");
        }

        // 必要なコンポーネントを事前に取得
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        CheckIfGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            // --- 診断機能：ジャンプ情報のログ出力 ---
            Debug.Log("--- ジャンプ診断開始 ---");
            Debug.Log($"Jump Force: {jumpForce}");
            Debug.Log($"Gravity Scale: {rb.gravityScale}");
            Debug.Log($"Player Mass: {rb.mass}");

            // 実際のジャンプ処理
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // 力を加えた直後の速度をログに出力
            Debug.Log($"ジャンプ直後の速度 (Y): {rb.linearVelocity.y}");
            Debug.Log("--- 診断終了 ---");
            
            jumpRequested = false;
        }
    }

    private void CheckIfGrounded()
    {
        Vector2 castOrigin = boxCollider.bounds.center;
        Vector2 castSize = boxCollider.bounds.size;
        castSize.x *= 0.9f;
        RaycastHit2D hit = Physics2D.BoxCast(castOrigin, castSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }
}
