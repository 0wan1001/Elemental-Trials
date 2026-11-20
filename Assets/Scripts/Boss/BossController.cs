using UnityEngine;

public class BossController : MonoBehaviour, IStunnable, IKnockbackable, ISlowable
{
    [SerializeField] private Transform _player;
    [SerializeField] private BossData _bossData;
    [SerializeField] private ProjectilePool _projectilePool;
    [SerializeField] private SpriteRenderer _coreRenderer;
    [SerializeField] private ChargeLineView _chargeLineView;
    [SerializeField] private TileGrid _tileGrid;
    [SerializeField] private BossDashView _dashView;
    [SerializeField] private GameObject _dashImpactVfx;
    [SerializeField] private float _dashImpactLifetime;
    [SerializeField] private Vector2 _feetOffset;

    private IBossState[] _pattern;
    private IBossState _currentState;
    private BossIdleState _idleState;
    private BossRadialBurstState _radialBurstState;
    private BossChargingState  _chargingState;
    private BossFireFieldState _fireFieldState;
    private BossFanShotState _fanShotState;
    private BossStunState _stunState;
    private Rigidbody2D _rigidbody;
    private int _lastPatternIndex = -1;
    private int _random;
    private float _stunImmuneUntil;
    private float _knockbackEndTime;
    private float _slowEndTime;
    private float _slowMultiplier = 1f;


    public Transform Player => _player;
    public ChargeLineView ChargeLineView => _chargeLineView;
    public TileGrid TileGrid => _tileGrid;
    public BossDashView DashView => _dashView;
    public BossData BossData => _bossData;
    public BossIdleState IdleState => _idleState;
    public BossRadialBurstState RadialBurstState => _radialBurstState;
    public BossChargingState ChargingState => _chargingState;
    public BossFireFieldState FireFieldState => _fireFieldState;
    public BossFanShotState FanShotState => _fanShotState;
    public BossStunState StunState => _stunState;
    public float StunRemaining => _currentState == _stunState ? _stunState.Remaining : 0f;
    public float StunImmuneRemaining => _currentState == _stunState ? 0f : Mathf.Max(0f, _stunImmuneUntil - Time.time);
    // 느려짐 중에는 이동과 패턴 시간이 이 배율로 흐른다. 기절 시간은 그대로 흐른다.
    public float SpeedScale => Time.time < _slowEndTime ? _slowMultiplier : 1f;
    public float DeltaTime => Time.deltaTime * SpeedScale;
    public ProjectilePool ProjectilePool => _projectilePool;
    public SpriteRenderer CoreRenderer => _coreRenderer;
    
    public void SpawnDashImpact() => Destroy(Instantiate(_dashImpactVfx, (Vector2)transform.position + _feetOffset, Quaternion.identity), _dashImpactLifetime);

    private void Awake()
    {
        _idleState = new BossIdleState(this);
        _radialBurstState = new BossRadialBurstState(this);
        _chargingState = new BossChargingState(this);
        _fireFieldState = new BossFireFieldState(this);
        _fanShotState = new BossFanShotState(this);
        _stunState = new BossStunState(this);
        _rigidbody = GetComponent<Rigidbody2D>();
        ChangeState(_idleState);
        _pattern = new IBossState[] { _radialBurstState, _fanShotState, _chargingState, _fireFieldState };
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

    public void Move(Vector2 direction, float speed)
    {
        if (Time.time < _knockbackEndTime) return;
        Vector2 velocity = direction.normalized * speed * SpeedScale;
        _rigidbody.linearVelocity = velocity;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_bossData.ContactDamage);
        }
    }

    public IBossState GetRandomPattern()
    {
        _random = Random.Range(0, _pattern.Length);
        while (_pattern.Length >= 2  && _random == _lastPatternIndex)
        {
            _random = Random.Range(0, _pattern.Length);

        }
        _lastPatternIndex = _random;
        return _pattern[_random];
    }
    
    public void FireRadial(int count, float speed)
    {
        SoundManager.Instance.Play(SoundType.BossBullet);
        float step = 360f / count;
        for (int i = 0; i < count; i++)
        {
            FireOne(i * step, speed);
        }
    }
    
    public void FireFan(Vector2 direction, int count, float spreadAngle, float speed)
    {
        SoundManager.Instance.Play(SoundType.BossBullet);
        float centerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float step = count > 1 ? spreadAngle / (count - 1) : 0f;
        float startAngle = centerAngle - spreadAngle / 2f;
        for (int i = 0; i < count; i++)
        {
            FireOne(startAngle + i * step, speed);
        }
    }
    
    private void FireOne(float angle, float speed)
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        Projectile projectile = _projectilePool.Get();
        projectile.Launch(transform.position,
            dir,
            speed * SpeedScale,
            _bossData.BossBulletLifeTime,
            _bossData.ContactDamage
        );
    }

    public void Stun()
    {
        if (_currentState == _stunState) return;
        if (Time.time < _stunImmuneUntil) return;
        ChangeState(_stunState);
    }

    public void StartStunImmunity()
    {
        _stunImmuneUntil = Time.time + _bossData.StunImmunity;
    }
    
    public void Knockback(Vector2 direction, float speed, float duration)
    {
        if (direction == Vector2.zero) direction = (Vector2)(transform.position - _player.position);
        _rigidbody.linearVelocity = direction.normalized * speed;
        _knockbackEndTime = Time.time + duration;
    }
    
    public void Slow(float multiplier, float duration)
    {
        _slowMultiplier = multiplier;
        _slowEndTime = Time.time + duration;
    }
}
