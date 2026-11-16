using UnityEngine;

public class ElementStatus : MonoBehaviour
{
    [SerializeField] private ElementData _elementData;
    [SerializeField] private ReactionTable _reactionTable;
    [SerializeField] private TileGrid _tileGrid;
    
    private float[] _timers;
    private float _nextBurnTime;
    private IDamageable _target;
    private IStunnable _stunnable;
    private IKnockbackable _knockbackable;
    private IAimDisruptable _aimDisruptable;

    private void Awake()
    {
        _timers = new float[3];
        CacheComponents();
    }

    private void Update()
    {
        for (int i = 0; i < _timers.Length; i++)
        {
            _timers[i] -= Time.deltaTime;
        }
        if (Has(ElementType.Fire) && Time.time >= _nextBurnTime)
        {
            _target.TakeDamage(_elementData.BurnDamage, ElementType.Fire);
            _nextBurnTime += _elementData.BurnInterval;
        }
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
            if (entry.Result == ReactionType.Overload) Explode();
            if (entry.Result == ReactionType.Steam && _aimDisruptable != null) _aimDisruptable.DisruptAim();
            if (entry.Result == ReactionType.Steam) _tileGrid.ApplyState(FloorState.Steam, transform.position, _elementData.SteamRadius, _elementData.SteamDuration);
            return;
        }
        if (!Has(ElementType.Fire))
        {
            _nextBurnTime = Time.time + _elementData.BurnInterval;
        }
        _timers[(int)type] = _elementData.GetDuration(type);
    }

    private void CacheComponents()
    {
        _target = GetComponent<IDamageable>();
        _stunnable = GetComponent<IStunnable>();
        _knockbackable = GetComponent<IKnockbackable>();
        _aimDisruptable = GetComponent<IAimDisruptable>();
    }
    
    private void Explode()
    {
        Vector2 center = transform.position;
        if (_knockbackable != null) _knockbackable.Knockback(Vector2.zero, _elementData.KnockbackSpeed, _elementData.KnockbackDuration);
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(center, _elementData.OverloadRadius))
        {
            if (hit.TryGetComponent(out ElementStatus other) && other != this)
            {
                other.HitByExplosion(center);
            }
        }
    }
    
    public void HitByExplosion(Vector2 center)
    {
        ReactionEntry entry = _reactionTable.FindByResult(ReactionType.Overload);
        if (entry != null && entry.Damage > 0) _target.TakeDamage(entry.Damage, ElementType.Fire);
        if (_knockbackable != null) _knockbackable.Knockback((Vector2)transform.position - center, _elementData.KnockbackSpeed, _elementData.KnockbackDuration);
    }

    public bool Has(ElementType type) => _timers[(int)type] > 0;

}
