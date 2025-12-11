using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class MyPlayerMove : MonoBehaviour
{
    [Header("移動設定 (BPM同期)")]
    [Tooltip("【重要】BPMに合わせて速度を自動計算するか")]
    [SerializeField] private bool syncSpeedToBpm = true;

    [Tooltip("1拍あたりに進む距離（メートル）。syncSpeedToBpmがオンの時はこれが基準になる")]
    [SerializeField] private float metersPerBeat = 4f;

    [Tooltip("固定速度（syncSpeedToBpmがオフの時のみ有効）")]
    [SerializeField] private float manualMoveSpeed = 5f;

    // 内部で実際に使用する速度
    private float currentMoveSpeed;

    [Header("ジャンプ設定 (音楽同期)")]
    [Tooltip("ジャンプの高さ (ワールド単位)")]
    [SerializeField] private float jumpHeight = 3.0f;
    
    [Tooltip("ジャンプの滞空時間 (拍数)。1.0なら1拍で着地")]
    [SerializeField] private float jumpDuration = 1.0f;

    [Header("接地判定")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Header("リスポーン設定")]
    [SerializeField] private float deathY = -15f;

    public static event Action OnPlayerRespawn;

    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D boxCollider;
    private bool isGrounded;
    
    private float startXPosition;
    private Vector3 initialSpawnPosition;
    private float calculatedJumpVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<CapsuleCollider2D>();
    }

    void Start()
    {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        initialSpawnPosition = transform.position;
        startXPosition = transform.position.x;
        
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 物理と移動速度の計算
        RecalculatePhysics();
    }

    private void RecalculatePhysics()
    {
        if (Conductor.Instance == null)
        {
            Debug.LogError("Conductorが見つかりません。");
            return;
        }

        double bpm = Conductor.Instance.bpm;
        if (bpm <= 0) return;

        // --- 1. 移動速度の計算 ---
        if (syncSpeedToBpm)
        {
            // 分速(BPM) ÷ 60 = 秒速(BPS)
            // 秒速(BPS) × 1拍の距離 = 1秒に進む距離(MoveSpeed)
            double beatsPerSecond = bpm / 60.0;
            currentMoveSpeed = (float)(beatsPerSecond * metersPerBeat);
            Debug.Log($"[Auto Speed] BPM:{bpm} => Speed:{currentMoveSpeed:F2} (1拍{metersPerBeat}m)");
        }
        else
        {
            currentMoveSpeed = manualMoveSpeed;
            Debug.Log($"[Manual Speed] Speed:{currentMoveSpeed:F2}");
        }

        // --- 2. ジャンプ物理の計算 ---
        double timePerBeat = 60.0 / bpm;
        double totalJumpTime = timePerBeat * jumpDuration;
        double timeToPeak = totalJumpTime / 2.0;

        double gravity = (2 * jumpHeight) / (timeToPeak * timeToPeak);
        double initialVelocity = (2 * jumpHeight) / timeToPeak;

        float defaultGravityY = Mathf.Abs(Physics2D.gravity.y);
        if (defaultGravityY > 0)
        {
            rb.gravityScale = (float)(gravity / defaultGravityY);
        }
        
        calculatedJumpVelocity = (float)initialVelocity;
    }

    private void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = initialSpawnPosition;

        if (Conductor.Instance != null)
        {
            Conductor.Instance.Restart();
        }

        OnPlayerRespawn?.Invoke();
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

        // --- 移動ロジック ---
        if (Conductor.Instance == null) return;
        double songPosition = Conductor.Instance.GetSongPosition();

        if (songPosition < 0)
        {
            transform.position = new Vector3(startXPosition, transform.position.y, transform.position.z);
            return;
        }

        // ここで currentMoveSpeed を使用
        float newX = startXPosition + (float)(songPosition * currentMoveSpeed);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(0f, calculatedJumpVelocity);
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