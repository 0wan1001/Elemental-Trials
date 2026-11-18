using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private float _despawnTime;
    private ProjectilePool _pool;   
    private int _damage;
    [SerializeField] private GameObject _hitVfx;
    [SerializeField] private float _hitVfxLifetime;
    // 대상에 맞았을 때의 타격감. 보스 탄은 0으로 둔다.
    [SerializeField] private float _hitstop;
    [SerializeField] private float _hitShake;

    private void Awake() => _rigidbody = GetComponent<Rigidbody2D>();

    public void Launch(Vector2 position, Vector2 direction, float boltSpeed, float boltLifeTime,  int damage)
    {
        transform.position = position;
        _rigidbody.linearVelocity = direction * boltSpeed;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        _despawnTime = Time.time + boltLifeTime;
        _damage = damage;
    }

    private void Update()
    {
        if (Time.time >= _despawnTime) Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damage);
            FeedbackManager.Instance.ProjectileHit(_hitstop, _hitShake);
        }
        if (_hitVfx != null) Destroy(Instantiate(_hitVfx, transform.position, Quaternion.identity), _hitVfxLifetime);
        Despawn();
    }

    public void SetPool(ProjectilePool pool)
    {
        _pool = pool;
    } 

    private void Despawn()
    {
        _pool.Return(this);
    }
}
