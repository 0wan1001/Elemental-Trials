using UnityEngine;

public class FloorSensor : MonoBehaviour
{
    [SerializeField] private int _fireFieldDamage;
    [SerializeField] private TileGrid _tileGrid;
    [SerializeField] private float _fireTickInterval;

    private float _nextFireTickTime;
    private ElementStatus _status;
    private IDamageable _target;
    private bool _wasOnWet;
    private bool _wasOnFire;

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        bool onWet = _tileGrid.HasState(FloorState.Wet, transform.position);
        bool onFire = _tileGrid.HasState(FloorState.Fire, transform.position);
        if (onWet && (!_wasOnWet || _status.Has(ElementType.Water)))
        {
            _status.Apply(ElementType.Water);
        }
        if (onFire && (!_wasOnFire || _status.Has(ElementType.Fire)))
        {
            _status.Apply(ElementType.Fire);
        }
        _wasOnWet = onWet;
        _wasOnFire = onFire;
        if (onFire)
        {
            if (Time.time >= _nextFireTickTime)
            {
                _target.TakeDamage(_fireFieldDamage, ElementType.Fire);
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
