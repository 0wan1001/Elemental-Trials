using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "ScriptableObjects/BossData")]
public class BossData : ScriptableObject
{
    [SerializeField] private int _bossHealth;
    [SerializeField] private float _bossMoveSpeed;
    [SerializeField] private float _approachStartDistance;
    [SerializeField] private float _retreatStartDistance;
    [SerializeField] private float _patternInterval;
    [SerializeField] private int _contactDamage;
    [SerializeField] private float _circularBarrageIndicator;
    [SerializeField] private int _circularBarrageBulletCount;
    [SerializeField] private float _bossBulletSpeed;
    [SerializeField] private float _bossBulletLifeTime;
    
    public int BossHealth => _bossHealth;
    public float BossMoveSpeed => _bossMoveSpeed;
    public float ApproachStartDistance => _approachStartDistance;
    public float RetreatStartDistance => _retreatStartDistance;
    public float PatternInterval => _patternInterval;
    public int ContactDamage => _contactDamage;
    public float CircularBarrageIndicator => _circularBarrageIndicator;
    public int CircularBarrageBulletCount => _circularBarrageBulletCount;
    public float BossBulletSpeed => _bossBulletSpeed;
    public float BossBulletLifeTime => _bossBulletLifeTime;
}
