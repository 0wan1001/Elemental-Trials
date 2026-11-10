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

        Vector2 playerPosition = _bossController.Player.position;
        Vector2 box = _bossController.BossData.FireFieldSpreadBox;
        int activeCount = 0;
        for (int i=0; i < _bossController.FireZone.Length; i++)
        {
            if(!_bossController.FireZone[i].gameObject.activeSelf)
            {
                Vector2 position = playerPosition;
                if (activeCount > 0)
                {
                    position += new Vector2(Random.Range(-box.x, box.x), Random.Range(-box.y, box.y));
                }
                float radius = _bossController.FireZone[i].transform.lossyScale.x * 0.5f;
                position = _bossController.ClampToArena(position, radius);

                _bossController.FireZone[i].ActiveFireField(
                    position,
                    _bossController.BossData.FireFieldIndicator,
                    _bossController.BossData.FireFieldDuration,
                    _bossController.BossData.FireFieldDamage
                );
                activeCount++;
                if(activeCount == _bossController.BossData.FireFieldCount) break;
            }
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
