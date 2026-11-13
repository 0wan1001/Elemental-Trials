using UnityEngine;

public class FireBall : MonoBehaviour
{
    private FireSkill _skill;
    private TileGrid _grid;
    private Rigidbody2D _rigidbody2D;
    private float _arriveTime;
    private bool _isExploded;

    private void Awake() => _rigidbody2D = GetComponent<Rigidbody2D>();

    private void Update()
    {
        if (Time.time >= _arriveTime)
        {
            Explode();
        }
    }
    
    public void Init(FireSkill skill, Vector2 start, Vector2 target, float speed, TileGrid grid)
    {
        _skill = skill;
        _grid = grid;
        _rigidbody2D.linearVelocity = (target - start).normalized * speed;
        _arriveTime = Time.time + Vector2.Distance(start, target) / speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Explode();
    }

    private void Explode()
    {
        if (_isExploded) return;
        _isExploded = true;
        _skill.Explode(transform.position, _grid);
        Destroy(gameObject);
    }
}
