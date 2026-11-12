using UnityEngine;

[CreateAssetMenu(fileName = "WaterSkillData", menuName = "ScriptableObjects/WaterSkillData")]
public class WaterSkill : ElementSkill
{
    [SerializeField] private float _range;
    [SerializeField] private float _halfAngle;
    [SerializeField] private float _wetDuration;
    
    public override void Use(Vector2 playerPosition, Vector2 mousePosition, TileGrid grid)
    {
        Vector2 direction = mousePosition - playerPosition;
        grid.ApplyStateInCone(FloorState.Wet, playerPosition, _range, direction, _halfAngle, _wetDuration);
        DamageInRange(playerPosition, _range, direction, _halfAngle);
    }
}
