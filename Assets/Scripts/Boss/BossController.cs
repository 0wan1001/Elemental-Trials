using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private BossData _bossData;
    
    private IBossState _currentState;
    private BossIdleState _idleState;
    private Rigidbody2D _rigidbody;
    public Transform Player => _player;
    public BossData BossData => _bossData;
    private void Awake()
    {
        _idleState = new BossIdleState(this);
        _rigidbody = GetComponent<Rigidbody2D>();
        ChangeState(_idleState);
    }
    
    private void Update()
    { 
        if(_currentState != null) _currentState.Tick();
    }

    public void ChangeState(IBossState nextState)
    {
        if (_currentState != null) _currentState.Exit();
        _currentState = nextState;
        _currentState.Enter();
    }

    public void Move(Vector2 direction)
    {
        Vector2 velocity = direction.normalized * _bossData.BossMoveSpeed;
        _rigidbody.linearVelocity = velocity;
    }
}
