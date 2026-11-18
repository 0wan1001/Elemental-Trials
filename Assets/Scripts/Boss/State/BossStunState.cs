using UnityEngine;

public class BossStunState : IBossState
{   
    private BossController _bossController;
    private float _timer;
    
    public float Remaining => Mathf.Max(0f, _bossController.BossData.StunDuration - _timer);
    
    public void Enter()
    {
        _timer = 0;
        _bossController.Move(Vector2.zero, 0);
    }

    public void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer >= _bossController.BossData.StunDuration)
        {
            _bossController.ChangeState(_bossController.IdleState);
        }
    }
    public void Exit()
    {
        _bossController.StartStunImmunity();
    }
    
    public BossStunState(BossController bossController)
    {
        _bossController = bossController;
    }
}