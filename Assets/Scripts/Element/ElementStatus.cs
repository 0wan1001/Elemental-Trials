using UnityEngine;

public class ElementStatus : MonoBehaviour
{
    [SerializeField] private ElementData _elementData;
    private float[] _timers;
    private float _nextBurnTime;
    private IDamageable _target;
    
    private void Awake()
    {
        _timers = new float[3];
        _target = GetComponent<IDamageable>();
    }

    private void Update()
    {
        for (int i = 0; i < _timers.Length; i++)
        {
            _timers[i] -= Time.deltaTime;
        }
        if (Has(ElementType.Fire) && Time.time >= _nextBurnTime)
        {
            _target.TakeDamage(_elementData.BurnDamage);
            _nextBurnTime += _elementData.BurnInterval;
        }
    }
    public void Apply(ElementType type)
    {
        if (!Has(ElementType.Fire))
        {
            _nextBurnTime = Time.time + _elementData.BurnInterval;
        }
        _timers[(int)type] = _elementData.GetDuration(type);
    }

    public bool Has(ElementType type) => _timers[(int)type] > 0;

}
