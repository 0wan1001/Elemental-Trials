using UnityEngine;

[CreateAssetMenu(fileName = "WaterSkillData", menuName = "ScriptableObjects/WaterSkillData")]
public class WaterSkill : ElementSkill
{
    [SerializeField] private float _range;
    [SerializeField] private float _halfAngle;
    [SerializeField] private float _wetDuration;
    [SerializeField] private GameObject _sprayVfx;
    [SerializeField] private float _vfxLifetime;
    [SerializeField] private float _wetSpreadTime;

    public override void Use(Vector2 playerPosition, Vector2 mousePosition, TileGrid grid)
    {
        Vector2 direction = mousePosition - playerPosition;
        Quaternion sprayRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        Destroy(Instantiate(_sprayVfx, playerPosition, sprayRotation), _vfxLifetime);
        SoundManager.Instance.Play(SoundType.WaterWave);
        grid.SpreadStateInCone(FloorState.Wet, playerPosition, _range, direction, _halfAngle, _wetDuration, _wetSpreadTime);
        DamageInRange(playerPosition, _range, direction, _halfAngle);
    }
}
