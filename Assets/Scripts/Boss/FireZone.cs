using UnityEngine;

public class FireZone : MonoBehaviour
{
    [SerializeField] private GameObject _warningVisual;
    [SerializeField] private GameObject _fireVisual;

    private int _damage;
    private float _duration;
    private float _indicator;
    private float _timer;
    private bool _isBurning;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (!_isBurning && _timer >= _indicator)
        {
            _isBurning = true;
            _warningVisual.SetActive(false);
            _fireVisual.SetActive(true);
        }

        if (_timer >= _indicator + _duration)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_isBurning) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damage);
        }
    }

    public void ActiveFireField(Vector2 position, float indicator, float duration, int damage)
    {
        transform.position = position;
        _indicator = indicator;
        _duration = duration;
        _damage = damage;
        _timer = 0f;
        _isBurning = false;

        _warningVisual.SetActive(true);
        _fireVisual.SetActive(false);
        gameObject.SetActive(true);
    }
}
