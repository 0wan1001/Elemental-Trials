using UnityEngine;

[CreateAssetMenu(fileName = "WaterSkillData", menuName = "ScriptableObjects/WaterSkillData")]
public class WaterSkill : ElementSkill
{
    [SerializeField] private float _range;
    [SerializeField] private float _halfAngle;
    [SerializeField] private float _wetDuration;
    
    public override void Use(Vector2 playerPosition, Vector2 mousePosition, TileGrid grid)
    {
        grid.ApplyStateInCone(FloorState.Wet, playerPosition, _range, mousePosition-playerPosition, _halfAngle,  _wetDuration );
    }
}
