using UnityEngine;

public abstract class ElementSkill : ScriptableObject
{
    [SerializeField] private int _damage;
    [SerializeField] private float _cooldown;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private ElementType _element;
    [SerializeField] private GameObject _castVfx;
    [SerializeField] private float _castVfxLifetime;
    [SerializeField] private GameObject _orbVfx;
    [SerializeField] private GameObject _chargeVfx;
    [SerializeField] private GameObject _chargedVfx;
    [SerializeField] private float _minChargeTime;
    [SerializeField] private GameObject _selectVfx;
    [SerializeField] private float _selectVfxLifetime;
    [SerializeField] private GameObject _pointVfx;
    [SerializeField] private float _pointVfxLifetime;
    [SerializeField] private Color _glowColor;
    // 켜면 플레이어 몸이 아니라 지팡이 끝에서 시작한다 (파이어볼)
    [SerializeField] private bool _castFromStaff;

    public int Damage => _damage;
    public float Cooldown => _cooldown;
    public GameObject CastVfx => _castVfx;
    public float CastVfxLifetime => _castVfxLifetime;
    public GameObject OrbVfx => _orbVfx;
    public GameObject ChargeVfx => _chargeVfx;
    public GameObject ChargedVfx => _chargedVfx;
    public float MinChargeTime => _minChargeTime;
    public GameObject SelectVfx => _selectVfx;
    public float SelectVfxLifetime => _selectVfxLifetime;
    public GameObject PointVfx => _pointVfx;
    public float PointVfxLifetime => _pointVfxLifetime;
    public Color GlowColor => _glowColor;
    public bool CastFromStaff => _castFromStaff;

    public abstract void Use(Vector2 playerPosition, Vector2 mousePosition, TileGrid grid);
    
    protected void DamageInRange(Vector2 center, float radius, Vector2 direction, float maxAngle = 180f)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, _targetLayer);
        foreach (Collider2D hit in hits)
        {
            Vector2 toTarget = (Vector2)hit.transform.position - center;
            if (Vector2.Angle(direction, toTarget) > maxAngle) continue;

            if (hit.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(_damage, _element);
                if (hit.TryGetComponent(out ElementStatus status))
                {
                    status.Apply(_element);
                }
            }
        }
    }
}
