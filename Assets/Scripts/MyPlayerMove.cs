using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class MyPlayerMove : MonoBehaviour
{
    [Header("移動設定 (BPM同期)")]
    [Tooltip("【重要】BPMに合わせて速度を自動計算するか")]
    [SerializeField] private bool syncSpeedToBpm = true;
    [Tooltip("1拍あたりに進む距離（メートル）")]
    [SerializeField] private float metersPerBeat = 4f;
    [Tooltip("固定速度（syncSpeedToBpmがオフの時のみ有効）")]
    [SerializeField] private float manualMoveSpeed = 5f;

    private float currentMoveSpeed;

    [Header("ジャンプ設定")]
    [SerializeField] private float jumpHeight = 3.0f;
    [SerializeField] private float jumpDuration = 1.0f;

    [Header("攻撃設定")]
    [Tooltip("攻撃のヒット範囲（半径）")]
    [SerializeField] private float attackRadius = 1.0f;
    [Tooltip("攻撃判定が出る位置（プレイヤー中心からのオフセット）")]
    [SerializeField] private Vector2 attackOffset = new Vector2(1.0f, 0f);
    [Tooltip("敵と判定するレイヤー")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("オーディオ設定")]
    [Tooltip("ジャンプ音")]
    [SerializeField] private AudioClip jumpSfx;
    [Tooltip("攻撃ヒット音")]
    [SerializeField] private AudioClip attackHitSfx;

    [Header("その他")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float deathY = -15f;

    public static event Action OnPlayerRespawn;

    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D boxCollider;
    private bool isGrounded;
    
    // オーディオプール（ジャンプと攻撃が重なってもいいように数を4に増やす）
    private AudioSource[] sfxSources;
    private int currentSfxIndex = 0;

    private float startXPosition;
    private Vector3 initialSpawnPosition;
    private float calculatedJumpVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<CapsuleCollider2D>();
        
        // 低遅延再生用スピーカーを4つ用意（ジャンプ、攻撃連打に対応）
        sfxSources = new AudioSource[4];
        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
        }
    }

    void Start()
    {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        initialSpawnPosition = transform.position;
        startXPosition = transform.position.x;
        
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        RecalculatePhysics();
    }

    private void RecalculatePhysics()
    {
        if (Conductor.Instance == null) return;

        double bpm = Conductor.Instance.bpm;
        if (bpm <= 0) return;

        if (syncSpeedToBpm)
        {
            double beatsPerSecond = bpm / 60.0;
            currentMoveSpeed = (float)(beatsPerSecond * metersPerBeat);
        }
        else
        {
            currentMoveSpeed = manualMoveSpeed;
        }

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

        if (Conductor.Instance == null) return;
        double songPosition = Conductor.Instance.GetSongPosition();

        if (songPosition < 0)
        {
            transform.position = new Vector3(startXPosition, transform.position.y, transform.position.z);
            return;
        }

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
        PlayLowLatencySfx(jumpSfx);
    }

    private void Attack()
    {
        animator.SetTrigger("AttackTrigger");

        // --- 攻撃判定の処理 ---
        // プレイヤーの位置 + オフセット（前方）を中心に円形の判定を出す
        Vector2 attackPos = (Vector2)transform.position + attackOffset;
        
        // 指定したレイヤー（Enemy）のみを検出
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRadius, enemyLayer);

        bool hitAny = false;
        foreach (Collider2D hit in hitEnemies)
        {
            // 当たった相手が Enemyスクリプトを持っていれば倒す
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnDefeated();
                hitAny = true;
            }
        }

        // 敵に当たったら音を鳴らす（空振りでは鳴らさない仕様にしています。好みで変えてください）
        if (hitAny)
        {
            PlayLowLatencySfx(attackHitSfx);
        }
    }

    /// <summary>
    /// 低遅延でSEを再生する共通メソッド
    /// </summary>
    private void PlayLowLatencySfx(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource source = sfxSources[currentSfxIndex];
        source.clip = clip;
        source.PlayScheduled(AudioSettings.dspTime);
        
        // インデックスを回す（0 -> 1 -> 2 -> 3 -> 0...）
        currentSfxIndex = (currentSfxIndex + 1) % sfxSources.Length;
    }

    private void CheckIfGrounded()
    {
        Vector2 castOrigin = boxCollider.bounds.center;
        Vector2 castSize = boxCollider.bounds.size;
        castSize.x *= 0.9f;

        RaycastHit2D hit = Physics2D.BoxCast(castOrigin, castSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    // エディタ上で攻撃範囲を可視化（デバッグ用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 attackPos = (Vector2)transform.position + attackOffset;
        Gizmos.DrawWireSphere(attackPos, attackRadius);
    }
}