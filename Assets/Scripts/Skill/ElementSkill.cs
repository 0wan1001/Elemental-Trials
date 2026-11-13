using UnityEngine;

public abstract class ElementSkill : ScriptableObject
{
    [SerializeField] private int _damage;
    [SerializeField] private float _cooldown;
    [SerializeField] private LayerMask _targetLayer;
    
    public int Damage => _damage;
    public float Cooldown => _cooldown;

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
                target.TakeDamage(_damage);
            }
        }
    }
}
