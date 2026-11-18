using ElementalTrials.Input;
using UnityEngine;

public class PlayerController : MonoBehaviour, IKnockbackable
{
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private GameObject _teleportVfx;
    [SerializeField] private float _vfxLifetime;
    [SerializeField] private Vector2 _teleportVfxOffset;
    [SerializeField] private GameObject _teleportArriveVfx;
    [SerializeField] private float _arriveVfxLifetime;

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
    private int _dashCharges;
    private float _dashRechargeTimer;
    private float _invincibleEndTime;
    private float _knockbackEndTime;

    public Vector2 AimDirection => _aimDirection;
    public Vector2 MousePosition => _mousePosition;
    public bool IsAttackPressed => _gameInput.Player.Attack.IsPressed();
    public bool IsAttackReleased => _gameInput.Player.Attack.WasReleasedThisFrame();
    public bool IsAttackTriggered => _gameInput.Player.Attack.WasPressedThisFrame();
    public bool IsInvincible => Time.time < _invincibleEndTime;
    public float MoveSpeed => _playerData.MoveSpeed;
    public bool IsDashing => _moveType == PlayerMoveType.Dash;
    public int DashCharges => _dashCharges;
    public int MaxDashCharges => _playerData.MaxDashCharges;
    public float DashCooldown => _playerData.DashCooldown;
    public float DashRechargeRemaining => _dashCharges >= _playerData.MaxDashCharges ? 0f : _playerData.DashCooldown - _dashRechargeTimer;
    private bool CanDash => _gameInput.Player.Dash.WasPerformedThisFrame() && _moveType == PlayerMoveType.Normal && _dashCharges > 0;

    private void Awake()
    {
        _gameInput = new GameInput();
        CacheComponents();
        _dashCharges = _playerData.MaxDashCharges;
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
       
       RechargeDash();
       if (CanDash)
       {
           StartDash();
       }
    }
    
    private void RechargeDash()
    {
        if (_dashCharges >= _playerData.MaxDashCharges)
        {
            _dashRechargeTimer = 0f;
            return;
        }
        _dashRechargeTimer += Time.deltaTime;
        if (_dashRechargeTimer >= _playerData.DashCooldown)
        {
            _dashRechargeTimer -= _playerData.DashCooldown;
            _dashCharges++;
        }
    }

    private void SpawnTeleportVfx()
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_dashDirection.y, _dashDirection.x) * Mathf.Rad2Deg);
        Destroy(Instantiate(_teleportVfx, (Vector2)transform.position + _teleportVfxOffset, rotation), _vfxLifetime);
    }

    private void FixedUpdate()
    {
        if (Time.time < _knockbackEndTime) return;
        if (_moveType == PlayerMoveType.Dash)
        {
            Vector2 velocity = _dashDirection * _playerData.DashSpeed;
            _rigidbody.linearVelocity = velocity;
            if (Time.time >= _dashEndTime)
            {
                _moveType = PlayerMoveType.Normal;
                _spriteRenderer.enabled = true;
                Destroy(Instantiate(_teleportArriveVfx, (Vector2)transform.position + _teleportVfxOffset, Quaternion.identity), _arriveVfxLifetime);
                _rigidbody.linearVelocity = Vector2.ClampMagnitude(velocity, _playerData.MoveSpeed);
            }
        }
        else
        {
            Vector2 targetVelocity = _moveInput * _playerData.MoveSpeed;
            float sharpness = _moveInput != Vector2.zero ? _playerData.Acceleration : _playerData.Deceleration;
            float t = 1f - Mathf.Exp(-sharpness * Time.fixedDeltaTime);
            _rigidbody.linearVelocity = Vector2.Lerp(_rigidbody.linearVelocity, targetVelocity, t);
        }
    }

    private void StartDash()
    {
        if (_moveInput != Vector2.zero)
        {
            _dashDirection =  _moveInput.normalized;
            SpawnTeleportVfx();
            FeedbackManager.Instance.Teleport();
            _moveType = PlayerMoveType.Dash;
            _spriteRenderer.enabled = false;
            _dashEndTime = Time.time + _playerData.DashDuration;
            _dashCharges--;
            _invincibleEndTime = Time.time + _playerData.DashInvincibleDuration;
        }
    }
    
    public void Knockback(Vector2 direction, float speed, float duration)
    {
        if (direction == Vector2.zero || IsDashing) return;
        _rigidbody.linearVelocity = direction.normalized * speed;
        _knockbackEndTime = Time.time + duration;
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
