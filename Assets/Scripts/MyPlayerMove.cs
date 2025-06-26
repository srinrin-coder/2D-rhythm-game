using UnityEngine;

/// <summary>
/// プレイヤーを自動で右に走らせ、ジャンプ・攻撃とアニメーションを制御するスクリプト
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class MyPlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("プレイヤーの移動速度")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("ジャンプ設定")]
    [Tooltip("ジャンプの強さ")]
    [SerializeField] private float jumpForce = 15f;

    [Header("接地判定")]
    [Tooltip("地面と判定するレイヤー")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("地面を検出するための足元からの距離")]
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Header("リスポーン設定")]
    [Tooltip("プレイヤーがリスポーンするY座標の高さ")]
    [SerializeField] private float deathY = -10f;

    // --- プライベート変数 ---
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D boxCollider;
    private bool isGrounded;
    private Vector3 startPosition; // スタート地点を記憶する変数

    void Awake()
    {
        // 必要なコンポーネントを事前に取得
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<CapsuleCollider2D>();
    }

    void Start()
    {
        // キャラクターの向きを右に固定するために、X方向のスケールを-1にする
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        // ★ 追加: ゲーム開始時の位置をスタート地点として記憶する
        startPosition = transform.position;
    }

    void Update()
    {
        // 接地判定を毎フレーム実行
        CheckIfGrounded();
        animator.SetBool("Grounded", isGrounded); // ジャンプと落下の判定用にGrounded状態をAnimatorに渡す

        // 接地している時にスペースキーが押されたらジャンプする
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Kキーが押されたら攻撃する
        if (Input.GetKeyDown(KeyCode.K))
        {
            Attack();
        }

        // ★ 追加: もしプレイヤーが一定の高さより下に落ちたらリスポーンする
        if (transform.position.y < deathY)
        {
            Respawn();
        }

    }

    void FixedUpdate()
    {
        // 物理演算の更新はこちら
        // x軸（横）方向は常に moveSpeed で移動し、y軸（縦）方向の速度は物理演算に任せる
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    private void Jump()
    {
        // 上方向に力を加える
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Animatorに「JumpTrigger」の合図を送る
        animator.SetTrigger("JumpTrigger");
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    private void Attack()
    {
        // Animatorに「AttackTrigger」の合図を送る
        animator.SetTrigger("AttackTrigger");
    }

    /// <summary>
    /// BoxCastを使って接地しているか判定する
    /// </summary>
    private void CheckIfGrounded()
    {
        Vector2 castOrigin = boxCollider.bounds.center;
        Vector2 castSize = boxCollider.bounds.size;
        castSize.x *= 0.9f;

        RaycastHit2D hit = Physics2D.BoxCast(castOrigin, castSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }
    
    // ★ 追加: リスポーン処理
    private void Respawn()
    {
        // プレイヤーの位置を記憶しておいたスタート地点に戻す
        transform.position = startPosition;
        // 落下速度をリセットする（重要）
        rb.linearVelocity = Vector2.zero;
    }

}