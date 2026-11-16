using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private DeathPopup _deathPopup;
    
    private int _playerHP;
    private float _hitInvincibleEndTime;
    private bool IsFaded => (int)(Time.time * 10) % 2 == 0;
    public bool IsDead {get; private set;}
    private bool IsHitInvincible => Time.time < _hitInvincibleEndTime;
    private PlayerController _playerController;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody2D;
    private PlayerAttack _playerAttack;

    private void Awake()
    {
        CacheComponents();
        Init();
    }

    private void Update()
    {
        if (IsDead) return;
        
        Color color = _spriteRenderer.color;
        
        if (IsHitInvincible)
        {
            if (IsFaded)
            {
                color.a = 0.3f;
            }
            else
            { 
                color.a = 1f;
            }
        }
        else
        {
            color.a = 1f;
        }
        _spriteRenderer.color = color;
    }
    
    private void Init()
    {
        _playerHP = _playerData.PlayerMaxHP;
    }

    private void CacheComponents()
    {
        _playerController = GetComponent<PlayerController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerAttack = GetComponent<PlayerAttack>();
    }

    public void TakeDamage(int damage, ElementType element = ElementType.None)
    {
        if (IsDead) return;
        if(_playerController.IsInvincible || IsHitInvincible) return;

        _playerHP -= damage;
        FeedbackManager.Instance.PlayerHit();
        if (_playerHP <= 0)
        {
            Die();
            return;
        }
        _hitInvincibleEndTime = Time.time + _playerData.HitInvincibleDuration;
        
    }

    private void Die()
    {
        IsDead = true;
        _playerController.enabled = false;
        _playerAttack.enabled = false;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _spriteRenderer.color = Color.gray;
        _deathPopup.Show();
    }
}
