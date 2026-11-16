using UnityEngine;

[CreateAssetMenu(fileName = "ElementData", menuName = "ScriptableObjects/ElementData")]
public class ElementData : ScriptableObject
{
    [SerializeField] private float _fireDuration;
    [SerializeField] private float _waterDuration;
    [SerializeField] private float _lightningDuration;
    [SerializeField] private float _burnInterval;
    [SerializeField] private int _burnDamage;
    [SerializeField] private float _overloadRadius;
    [SerializeField] private float _knockbackSpeed;
    [SerializeField] private float _knockbackDuration;
    [SerializeField] private float _steamRadius;
    [SerializeField] private float _steamDuration;

    public float FireDuration => _fireDuration;
    public float WaterDuration => _waterDuration;
    public float LightningDuration => _lightningDuration;
    public float BurnInterval => _burnInterval;
    public int BurnDamage => _burnDamage;
    public float OverloadRadius => _overloadRadius;
    public float KnockbackSpeed => _knockbackSpeed;
    public float KnockbackDuration => _knockbackDuration;
    public float SteamRadius => _steamRadius;
    public float SteamDuration => _steamDuration;

    public float GetDuration(ElementType type)
    {
        switch (type)
        {
            case ElementType.Fire: return _fireDuration;
            case ElementType.Water: return _waterDuration;
            case ElementType.Lightning: return _lightningDuration;
        }
        return 0;
    }
}
    
    
