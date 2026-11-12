using ElementalTrials.Input;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    
    private GameInput _gameInput;
    private Rigidbody2D _rigidbody;
    private Camera _camera;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _moveInput;
    private Vector2 _aimDirection;
    private Vector2 _mousePosition;
    private PlayerMoveType _moveType;
    private Vector2 _dashDirection;
    private float _dashEndTime;
    private float _nextDashTime;
    private float _invincibleEndTime;
    
    public Vector2 AimDirection => _aimDirection;
    public Vector2 MousePosition => _mousePosition;
    public bool IsAttackPressed => _gameInput.Player.Attack.IsPressed();
    public bool IsInvincible => Time.time < _invincibleEndTime;
    public bool IsDashing => _moveType == PlayerMoveType.Dash;
    private bool CanDash => _gameInput.Player.Dash.WasPerformedThisFrame() && _moveType == PlayerMoveType.Normal && Time.time >= _nextDashTime;

    private void Awake()
    {
        _gameInput = new GameInput();
        CacheComponents();
    }
    private void OnEnable() => _gameInput.Player.Enable();
    private void OnDisable() => _gameInput.Player.Disable();
    private void OnDestroy() => _gameInput.Dispose();

    private void Update()
    {
       _moveInput = _gameInput.Player.Move.ReadValue<Vector2>();
       
       Vector2 mouseAimPosition = _gameInput.Player.Aim.ReadValue<Vector2>();
       Vector2 mouseWorldPosition= _camera.ScreenToWorldPoint(mouseAimPosition);
       _mousePosition = mouseWorldPosition;
       _aimDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;
       
       if (CanDash)
       {
           StartDash();
       }
    }

    private void FixedUpdate()
    {
        if (_moveType == PlayerMoveType.Dash)
        {
            Vector2 velocity = _dashDirection * _playerData.DashSpeed;
            _rigidbody.linearVelocity = velocity;
            if (Time.time >= _dashEndTime)
            {
                _moveType = PlayerMoveType.Normal;
                _spriteRenderer.enabled = true;
            }
        }
        else
        {
            Vector2 velocity = _moveInput * _playerData.MoveSpeed;
            _rigidbody.linearVelocity = velocity;
        }
    }

    private void StartDash()
    {
        if (_moveInput != Vector2.zero)
        {
            _dashDirection =  _moveInput.normalized;
            _moveType = PlayerMoveType.Dash;
            _spriteRenderer.enabled = false;
            _dashEndTime = Time.time + _playerData.DashDuration;
            _nextDashTime = Time.time + _playerData.DashCooldown;
            _invincibleEndTime = Time.time + _playerData.DashInvincibleDuration;
        }
    }

    public bool IsSkillPressed(int index)
    {
        switch (index)
        {
            case 0: return _gameInput.Player.Skill1.WasPerformedThisFrame();
            case 1: return _gameInput.Player.Skill2.WasPerformedThisFrame();
            case 2: return _gameInput.Player.Skill3.WasPerformedThisFrame();
        }
        return false;
    }

    private void CacheComponents()
    {
        _camera = Camera.main;
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
