using System.Collections;
using UnityEngine;

// 기본 공격: 클릭하면 바로 3연발을 쏘고, 쿨타임이 지나야 다시 쏠 수 있다.
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private SkillData _skillData;
    [SerializeField] private ProjectilePool _projectilePool;
    [SerializeField] private int _shotCount;
    [SerializeField] private float _shotInterval;
    [SerializeField] private GameObject _muzzleVfx;
    [SerializeField] private float _muzzleVfxLifetime;

    private PlayerController _playerController;
    private PlayerSkill _playerSkill;
    private PlayerSpriteAnimator _spriteAnimator;
    private float _nextFireTime;

    public float Cooldown => _skillData.Cooldown;
    public float CooldownRemaining => Mathf.Max(0f, _nextFireTime - Time.time);

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerSkill = GetComponent<PlayerSkill>();
        _spriteAnimator = GetComponent<PlayerSpriteAnimator>();
    }

    private void Update()
    {
        if (_playerController.IsDashing || _playerSkill.BlocksAttack) return;
        if (_playerController.IsAttackTriggered && Time.time >= _nextFireTime)
        {
            StartCoroutine(FireBurst());
            _nextFireTime = Time.time + _skillData.Cooldown;
        }
    }

    private IEnumerator FireBurst()
    {
        for (int i = 0; i < _shotCount; i++)
        {
            if (i > 0) yield return new WaitForSeconds(_shotInterval);
            // 지팡이 끝에서 쏜다. 연발 사이에 방향을 바꾸면 그때의 지팡이 끝에서 나간다.
            Vector2 staffTip = (Vector2)transform.position + _spriteAnimator.StaffTipOffset;
            Destroy(Instantiate(_muzzleVfx, staffTip, Quaternion.identity, transform), _muzzleVfxLifetime);
            Projectile projectile = _projectilePool.Get();
            projectile.Launch(staffTip, _playerController.AimDirection, _skillData.BoltSpeed, _skillData.BoltLifeTime, _skillData.Damage);
        }
    }
}
