using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private float _despawnTime;
    private ProjectilePool _pool;   
    private int _damage;
    
    private void Awake() => _rigidbody = GetComponent<Rigidbody2D>();

    public void Launch(Vector2 position, Vector2 direction, float boltSpeed, float boltLifeTime,  int damage)
    {
        transform.position = position;
        _rigidbody.linearVelocity = direction * boltSpeed;
        _despawnTime = Time.time + boltLifeTime;
        _damage = damage;
    }

    private void Update()
    {
        if (Time.time >= _despawnTime) Despawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damage);
        }
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
