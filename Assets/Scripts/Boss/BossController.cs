using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private BossData _bossData;
    [SerializeField] private ProjectilePool _projectilePool;
    [SerializeField] private SpriteRenderer _coreRenderer;
    
    private IBossState _currentState;
    private BossIdleState _idleState;
    private BossRadialBurstState _radialBurstState;
    private Rigidbody2D _rigidbody;
    
    public Transform Player => _player;
    public BossData BossData => _bossData;
    public BossIdleState IdleState => _idleState;
    public BossRadialBurstState RadialBurstState => _radialBurstState;
    public ProjectilePool ProjectilePool => _projectilePool;
    public SpriteRenderer CoreRenderer => _coreRenderer;
    
    private void Awake()
    {
        _idleState = new BossIdleState(this);
        _radialBurstState = new BossRadialBurstState(this);
        _rigidbody = GetComponent<Rigidbody2D>();
        ChangeState(_idleState);
    }
    
    private void Update()
    { 
        if(_currentState != null) _currentState.Tick();
    }

    public void ChangeState(IBossState nextState)
    {
        if (_currentState != null) _currentState.Exit();
        _currentState = nextState;
        _currentState.Enter();
    }

    public void Move(Vector2 direction)
    {
        Vector2 velocity = direction.normalized * _bossData.BossMoveSpeed;
        _rigidbody.linearVelocity = velocity;
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_bossData.ContactDamage);
        }
    }
    
}
