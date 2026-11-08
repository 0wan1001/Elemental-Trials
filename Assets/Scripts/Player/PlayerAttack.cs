using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private SkillData _skillData;
    [SerializeField] private ProjectilePool _projectilePool;
    
    private PlayerController _playerController;
    private float _nextFireTime;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_playerController.IsAttackPressed && !_playerController.IsDashing && Time.time >= _nextFireTime)
        {
            Projectile projectile = _projectilePool.Get();
            projectile.Launch(transform.position, _playerController.AimDirection, _skillData.BoltSpeed, _skillData.BoltLifeTime);
            _nextFireTime = Time.time + _skillData.Cooldown;
        }
    }
}
