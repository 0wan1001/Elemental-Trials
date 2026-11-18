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
            _bossController.Move(Vector2.zero, 0);
            _originColor = _bossController.CoreRenderer.color;
            _bossController.CoreRenderer.color = Color.white;
        }

        public void Tick()
        {
            if (_timer >= _bossController.BossData.CircularBarrageIndicator)
            {
                _bossController.FireRadial(_bossController.BossData.CircularBarrageBulletCount, _bossController.BossData.BossBulletSpeed);
                _bossController.ChangeState(_bossController.IdleState);
            }
            else
            {
                _timer += _bossController.DeltaTime;
            }
        }

        public void Exit()
        {
            _bossController.CoreRenderer.color = _originColor;
        }
    }
