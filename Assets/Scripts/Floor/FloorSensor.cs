using UnityEngine;

public class FloorSensor : MonoBehaviour
{
    [SerializeField] private int _fireFieldDamage;
    [SerializeField] private TileGrid _tileGrid;
    [SerializeField] private float _fireTickInterval;

    private float _nextFireTickTime;
    private ElementStatus _status;
    private IDamageable _target;

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        if (_tileGrid.HasState(FloorState.Wet, transform.position))
        {
            _status.Apply(ElementType.Water);
        }
        if (_tileGrid.HasState(FloorState.Fire, transform.position))
        {
            _status.Apply(ElementType.Fire);
            if (Time.time >= _nextFireTickTime)
            {
                _target.TakeDamage(_fireFieldDamage);
                _nextFireTickTime = Time.time + _fireTickInterval;
            }
        }
    }

    private void CacheComponents()
    {
        _target = GetComponent<IDamageable>();
        _status = GetComponent<ElementStatus>();
    }
}
