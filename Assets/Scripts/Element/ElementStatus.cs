using UnityEngine;

public class ElementStatus : MonoBehaviour
{
    [SerializeField] private ElementData _elementData;
    [SerializeField] private ReactionTable _reactionTable;
    
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
        for (int i = 0; i < _timers.Length; i++)
        {
            if ((ElementType)i == type) continue;
            if (!Has((ElementType)i)) continue;
            ReactionEntry entry = _reactionTable.Find(type, (ElementType)i);
            if (entry == null) continue;

            _timers[i] = 0;
            if (entry.Damage > 0) _target.TakeDamage(entry.Damage);
            Debug.Log($"{name} 반응 : {entry.Result}");
            return;
        }
        if (!Has(ElementType.Fire))
        {
            _nextBurnTime = Time.time + _elementData.BurnInterval;
        }
        _timers[(int)type] = _elementData.GetDuration(type);
    }

    public bool Has(ElementType type) => _timers[(int)type] > 0;

}
