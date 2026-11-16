using UnityEngine;

public class BossFanShotState : IBossState
{
    private BossController _bossController;
    private float _timer;
    private int _volleyCount;

    public BossFanShotState(BossController controller)
    {
        _bossController = controller;
    }

    public void Enter()
    {
        _timer = 0f;
        _volleyCount = 0;
        _bossController.Move(Vector2.zero, 0);
    }

    public void Tick()
    {
        _timer += Time.deltaTime;
        
        if (_timer >= _bossController.BossData.FanShotIndicator + _volleyCount * _bossController.BossData.FanVolleyInterval)
        {
            Vector2 toPlayer = _bossController.GetAimPosition() - (Vector2)_bossController.transform.position;
            _bossController.FireFan(toPlayer, _bossController.BossData.FanBulletCount, _bossController.BossData.FanSpreadAngle, _bossController.BossData.FanBulletSpeed);
            _volleyCount++;

            if (_volleyCount >= _bossController.BossData.FanVolleyCount)
            {
                _bossController.ChangeState(_bossController.IdleState);
            }
        }
    }

    public void Exit()
    {
    }
}
