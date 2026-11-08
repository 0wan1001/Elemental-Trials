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
        _timer += Time.deltaTime;
        if (_timer >= _bossController.BossData.PatternInterval)
        {
            _bossController.ChangeState(_bossController.RadialBurstState);
            return;
        }
        
        Vector2 toPlayer = _bossController.Player.position - _bossController.transform.position;
        float distance = toPlayer.magnitude;

        if (distance > _bossController.BossData.ApproachStartDistance)
        {
         _bossController.Move(toPlayer);   
        }
        else if (distance < _bossController.BossData.RetreatStartDistance)
        {
            _bossController.Move(-toPlayer);
        }
        else
        {
            _bossController.Move(Vector2.zero);
        }
    }

    public void Exit()
    {
    }
}
