using UnityEngine;

public class ElementStatus : MonoBehaviour
{
    [SerializeField] private ElementData _elementData;
    [SerializeField] private ReactionTable _reactionTable;
    [SerializeField] private TileGrid _tileGrid;
    [SerializeField] private GameObject _burnVfx;
    [SerializeField] private GameObject _wetVfx;
    [SerializeField] private Color _wetTint;
    [SerializeField] private GameObject _shockVfx;
    [SerializeField] private float _shockVfxLifetime;
    [SerializeField] private GameObject _overloadVfx;
    [SerializeField] private float _overloadVfxLifetime;
    [SerializeField] private GameObject _steamVfx;
    [SerializeField] private float _steamVfxLifetime;
    [SerializeField] private float _statusVfxScale;
    
    private GameObject _burnInstance;
    private GameObject _wetInstance;
    private SpriteFlash _spriteFlash;
    private float[] _timers;
    private float _steamTimer;
    private float _burnTimer;
    private float _nextBurnTime;
    private IDamageable _target;
    private IStunnable _stunnable;
    private IKnockbackable _knockbackable;
    private ISlowable _slowable;

    private void Awake()
    {
        _timers = new float[3];
        CacheComponents();
        _burnInstance = SpawnOnBody(_burnVfx);
        _burnInstance.SetActive(false);
        _wetInstance = SpawnOnBody(_wetVfx);
        _wetInstance.SetActive(false);
    }

    private void Update()
    {
        for (int i = 0; i < _timers.Length; i++)
        {
            _timers[i] -= Time.deltaTime;
        }
        _steamTimer -= Time.deltaTime;
        _burnTimer -= Time.deltaTime;
        // 원소 하나는 표시만 남는다. 지속 피해는 과부하 뒤의 화상에서만 들어온다.
        if (IsBurning && Time.time >= _nextBurnTime)
        {
            _target.TakeDamage(_elementData.BurnDamage, ElementType.Fire);
            _nextBurnTime += _elementData.BurnInterval;
        }
        _burnInstance.SetActive(Has(ElementType.Fire) || IsBurning);
        _wetInstance.SetActive(Has(ElementType.Water));
        if (_spriteFlash != null) _spriteFlash.SetTint(Has(ElementType.Water) ? _wetTint : Color.clear);
    }
    public void Apply(ElementType type)
    {
        for (int i = 0; i < _timers.Length; i++)
        {
            if ((ElementType)i == type) continue;
            if (!Has((ElementType)i)) continue;
            ReactionEntry entry = _reactionTable.Find(type, (ElementType)i);
            if (entry == null) continue;

            _timers[i] = 0;
            FeedbackManager.Instance.Reaction(entry.Result);
            if (entry.Damage > 0) _target.TakeDamage(entry.Damage, type);
            if (entry.Result == ReactionType.ElectricShock && _stunnable != null) _stunnable.Stun();
            if (entry.Result == ReactionType.ElectricShock) Destroy(SpawnOnBody(_shockVfx), _shockVfxLifetime);
            if (entry.Result == ReactionType.Steam) Destroy(Instantiate(_steamVfx, transform.position, Quaternion.identity), _steamVfxLifetime);
            if (entry.Result == ReactionType.Overload) Explode();
            if (entry.Result == ReactionType.Steam) StartSlow();
            if (entry.Result == ReactionType.Steam) _tileGrid.ApplyState(FloorState.Steam, transform.position, _elementData.SteamRadius, _elementData.SteamDuration);
            return;
        }
        _timers[(int)type] = _elementData.GetDuration(type);
    }

    private void CacheComponents()
    {
        _target = GetComponent<IDamageable>();
        _stunnable = GetComponent<IStunnable>();
        _knockbackable = GetComponent<IKnockbackable>();
        _slowable = GetComponent<ISlowable>();
        _spriteFlash = GetComponent<SpriteFlash>();
    }
    
    private void Explode()
    {
        Vector2 center = transform.position;
        Destroy(Instantiate(_overloadVfx, center, Quaternion.identity), _overloadVfxLifetime);
        if (_knockbackable != null) _knockbackable.Knockback(Vector2.zero, _elementData.KnockbackSpeed, _elementData.KnockbackDuration);
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(center, _elementData.OverloadRadius))
        {
            if (hit.TryGetComponent(out ElementStatus other) && other != this)
            {
                other.HitByExplosion(center);
            }
        }
        StartBurn();
    }

    // 증기: 피해 없이 일정 시간 느려진다. 느려질 수 없는 대상이나 지속시간 0이면 아무 일도 없다.
    private void StartSlow()
    {
        if (_slowable == null || _elementData.SteamSlowDuration <= 0f) return;
        _steamTimer = _elementData.SteamSlowDuration;
        _slowable.Slow(_elementData.SteamSlowMultiplier, _elementData.SteamSlowDuration);
    }

    // 과부하 뒤 화상: 지속시간 동안 BurnInterval마다 BurnDamage를 준다.
    private void StartBurn()
    {
        if (_elementData.OverloadBurnDuration <= 0f) return;
        if (!IsBurning) _nextBurnTime = Time.time + _elementData.BurnInterval;
        _burnTimer = _elementData.OverloadBurnDuration;
    }
    
    public void HitByExplosion(Vector2 center)
    {
        ReactionEntry entry = _reactionTable.FindByResult(ReactionType.Overload);
        if (entry != null && entry.Damage > 0) _target.TakeDamage(entry.Damage, ElementType.Fire);
        if (_knockbackable != null) _knockbackable.Knockback((Vector2)transform.position - center, _elementData.KnockbackSpeed, _elementData.KnockbackDuration);
    }

    public bool Has(ElementType type) => _timers[(int)type] > 0;
    public float GetRemaining(ElementType type) => Mathf.Max(0f, _timers[(int)type]);
    public float GetDuration(ElementType type) => _elementData.GetDuration(type);
    public bool IsBurning => _burnTimer > 0f;
    public float SteamRemaining => Mathf.Max(0f, _steamTimer);
    public float BurnRemaining => Mathf.Max(0f, _burnTimer);
    
    private GameObject SpawnOnBody(GameObject prefab)
    {
        GameObject spawned = Instantiate(prefab, transform);
        spawned.transform.localScale = Vector3.one * _statusVfxScale;
        return spawned;
    }

}
