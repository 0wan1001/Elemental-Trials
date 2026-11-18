using UnityEngine;

public class BossIdleState : IBossState
{
    private BossController _bossController;
    private float _timer;
    
    public BossIdleState(BossController controller)
    {
        _bossController = controller;
    }
    public void Enter()
    {
        _timer = 0f;
    }

    public void Tick()
    {
        if (_timer >= _bossController.BossData.PatternInterval)
        {
            _bossController.ChangeState(_bossController.GetRandomPattern());
        }
        else
        {
            _timer += _bossController.DeltaTime;
        }
        
        Vector2 toPlayer = _bossController.Player.position - _bossController.transform.position;
        float distance = toPlayer.magnitude;

        if (distance > _bossController.BossData.ApproachStartDistance)
        { 
            _bossController.Move(toPlayer, _bossController.BossData.BossMoveSpeed);   
        }
        else if (distance < _bossController.BossData.RetreatStartDistance)
        {
            _bossController.Move(-toPlayer, _bossController.BossData.BossMoveSpeed);
        }
        else
        {
            _bossController.Move(Vector2.zero, 0);
        }
    }

    public void Exit()
    {
    }
    
}
