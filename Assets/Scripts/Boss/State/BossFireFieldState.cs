using UnityEngine;

public class BossFireFieldState : IBossState
{
    private BossController _bossController;
    private float _timer;

    public BossFireFieldState(BossController controller)
    {
        _bossController = controller;
    }
    public void Enter()
    {
        _timer = 0f;
        _bossController.Move(Vector2.zero, 0);

        Vector2 playerPosition = _bossController.GetAimPosition();
        Vector2 box = _bossController.BossData.FireFieldSpreadBox;
        for (int i=0; i < _bossController.BossData.FireFieldCount; i++)
        {
            Vector2 position = playerPosition;
            if (i != 0)
            {
                position += new Vector2(Random.Range(-box.x, box.x), Random.Range(-box.y, box.y));
            }
            _bossController.TileGrid.ApplyWarning(position,
                _bossController.BossData.FireFieldRadius,
                _bossController.BossData.FireFieldIndicator,
                _bossController.BossData.FireFieldDuration
                );
        }
    }

    public void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer >= _bossController.BossData.FireFieldRecovery)
        {
            _bossController.ChangeState(_bossController.IdleState);
        }
    }

    public void Exit()
    {

    }

}
