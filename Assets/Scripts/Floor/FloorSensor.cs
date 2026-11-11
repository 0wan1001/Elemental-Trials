using UnityEngine;

public class FloorSensor : MonoBehaviour
{
    [SerializeField] private int _fireFieldDamage;
    [SerializeField] private TileGrid _tileGrid;
    
    private IDamageable _target;

    private void Awake()
    {
        _target = GetComponent<IDamageable>();
    }

    private void Update()
    {
        if (_tileGrid.HasState(FloorState.Fire, transform.position))
        {
            _target.TakeDamage(_fireFieldDamage);
        }
    }
}
