using UnityEngine;

[CreateAssetMenu(fileName = "BasicAttack", menuName = "ScriptableObjects/SkillData")]
public class SkillData : ScriptableObject
{
    [SerializeField] private float _cooldown;
    [SerializeField] private int _damage;
    [SerializeField] private float _boltSpeed;
    [SerializeField] private float _boltLifeTime;
    [SerializeField] private float _minChargeTime;

    public float Cooldown => _cooldown;
    public float MinChargeTime => _minChargeTime;
    public int Damage => _damage;
    public float BoltSpeed => _boltSpeed;
    public float BoltLifeTime => _boltLifeTime;
    
}
