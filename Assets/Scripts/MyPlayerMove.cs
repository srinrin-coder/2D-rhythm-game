using UnityEngine;
using System;


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
    [SerializeField] private float deathY = -15f;

    public static event Action OnPlayerRespawn;
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D boxCollider;
    private bool isGrounded;
    private Vector3 startPosition;

    private void Respawn()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("【プレイヤー】リスポーンします。イベントを発行（通知）します。");

        OnPlayerRespawn?.Invoke();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<CapsuleCollider2D>();
    }

    void Start()
    {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        startPosition = transform.position;
    }

    void Update()
    {
        CheckIfGrounded();
        animator.SetBool("Ground", isGrounded);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Attack();
        }

        if (transform.position.y < deathY)
        {
            Respawn();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        animator.SetTrigger("JumpTrigger");
    }

    private void Attack()
    {
        animator.SetTrigger("AttackTrigger");
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
