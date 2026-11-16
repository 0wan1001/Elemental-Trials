using UnityEngine;

[CreateAssetMenu(fileName = "FireSkillData", menuName = "ScriptableObjects/FireSkillData")]
public class FireSkill : ElementSkill
{
    [SerializeField] private FireBall _fireBallPrefab;
    [SerializeField] private float _speed;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private float _fireRadius;
    [SerializeField] private float _fireDuration;
    
    public override void Use(Vector2 playerPosition, Vector2 mousePosition, TileGrid grid)
    {
        FireBall ball = Instantiate(_fireBallPrefab, playerPosition, Quaternion.identity);
        ball.Init(this, playerPosition, mousePosition, _speed, grid);
    }

    public void Explode(Vector2 position, TileGrid grid)
    {
        FeedbackManager.Instance.FireBallExplosion();
        grid.ApplyState(FloorState.Fire, position, _fireRadius, _fireDuration);
        DamageInRange(position, _explosionRadius, Vector2.right);
    }
}
