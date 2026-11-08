    using UnityEngine;

    public class BossRadialBurstState : IBossState
    {
        private BossController _bossController;
        private float _timer;
        private Color _originColor;
        
        public BossRadialBurstState(BossController controller)
        {
            _bossController = controller;
        }
        
        public void Enter()
        {
            _timer = 0f;
            _bossController.Move(Vector2.zero);
            _originColor = _bossController.CoreRenderer.color;
            _bossController.CoreRenderer.color = Color.white;
        }

        public void Tick()
        {
            _timer += Time.deltaTime;
            if (_timer >= _bossController.BossData.CircularBarrageIndicator)
            {
                for (int i = 0; i < _bossController.BossData.CircularBarrageBulletCount; i++)
                {
                    float step = 360f / _bossController.BossData.CircularBarrageBulletCount;
                    float angle = i * step;
                    float rad = angle * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    
                    Projectile projectile = _bossController.ProjectilePool.Get();
                    projectile.Launch(_bossController.transform.position,
                        dir,
                        _bossController.BossData.BossBulletSpeed,
                        _bossController.BossData.BossBulletLifeTime,
                        _bossController.BossData.ContactDamage
                    );
                }
                _bossController.ChangeState(_bossController.IdleState);
            }
        }

        public void Exit()
        {
            _bossController.CoreRenderer.color = _originColor;
        }
    }
