using UnityEngine;

public class BossIdleState : IBossState
{
    private BossController _bossController;
    public BossIdleState(BossController controller)
    {
        _bossController = controller;
    }
    public void Enter()
    {
    }

    public void Tick()
    {
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
