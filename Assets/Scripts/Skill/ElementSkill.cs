using UnityEngine;

public abstract class ElementSkill : ScriptableObject
{
    [SerializeField] private int _damage;
    [SerializeField] private float _cooldown;
    
    public int Damage => _damage;
    public float Cooldown => _cooldown;

    public abstract void Use(Vector2 playerPosition, Vector2 mousePosition, TileGrid grid);
}
