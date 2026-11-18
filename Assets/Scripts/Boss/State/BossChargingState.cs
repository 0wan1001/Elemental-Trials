using UnityEngine;

public class BossChargingState : IBossState
{
    private BossController _bossController;
    private float _timer;
    private bool _isCharging;
    private Vector2 _direction;
    private Vector2 _bossPosition;
    public BossChargingState(BossController controller)
    {
        _bossController = controller;
    }
    
    public void Enter()
    {
        _timer = 0f;
        _isCharging = false;
        _bossController.Move(Vector2.zero, 0);
        _direction = ((Vector2)_bossController.Player.position - (Vector2)_bossController.transform.position).normalized;
        _bossController.ChargeLineView.Show(_direction, _bossController.BossData.ChargeDistance);
    }

    public void Tick()
    {
        _timer += _bossController.DeltaTime;
        if (!_isCharging) _bossController.ChargeLineView.SetProgress(_timer / _bossController.BossData.ChargeIndicator);
        if (_bossController.BossData.ChargeIndicator < _timer && !_isCharging)
        {
            _bossController.ChargeLineView.Hide();
            _bossPosition = _bossController.transform.position;
            _bossController.Move(_direction, _bossController.BossData.ChargeSpeed);
            _isCharging = true;
            _bossController.DashView.Begin(_direction);
        }

        if (_isCharging)
        {
            if (Vector2.Distance(_bossPosition,_bossController.transform.position) >=
                _bossController.BossData.ChargeDistance || _timer >= _bossController.BossData.ChargeIndicator + _bossController.BossData.ChargeDistance / _bossController.BossData.ChargeSpeed
                + _bossController.BossData.ChargeTimeMargin)
            {
                _bossController.Move(Vector2.zero, 0);
                _bossController.DashView.End();
                FeedbackManager.Instance.ChargeImpact();
                _bossController.SpawnDashImpact();
                _bossController.FireRadial(_bossController.BossData.ChargeBulletCount, _bossController.BossData.ChargeBulletSpeed);
                _bossController.ChangeState(_bossController.IdleState);
            }
        }
    }

    public void Exit()
    {
        _bossController.ChargeLineView.Hide();
        _bossController.DashView.End();
    }
}
