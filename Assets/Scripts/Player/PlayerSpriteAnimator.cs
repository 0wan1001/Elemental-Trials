using UnityEngine;

public class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] _idleFrames;
    [SerializeField] private Sprite[] _walkFrames;
    [SerializeField] private float _fps;
    [SerializeField] private float _walkFps;
    [SerializeField] private Vector2[] _staffTipOffsets;

    private const int DIRECTION_COUNT = 8;

    private PlayerController _playerController;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
    private float _walkPhase;
    
    public int FacingDirection => GetDirection(_playerController.AimDirection);
    
    public Vector2 StaffTipOffset => _staffTipOffsets[FacingDirection];

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float speed = _rigidbody.linearVelocity.magnitude;
        bool isWalking = speed > 0.1f;
        Sprite[] frames = isWalking ? _walkFrames : _idleFrames;
        int perDirection = frames.Length / DIRECTION_COUNT;
        int frame;
        if (isWalking)
        {
            _walkPhase += Time.deltaTime * _walkFps * Mathf.Clamp(speed / _playerController.MoveSpeed, 0.3f, 1f);
            frame = (int)_walkPhase % perDirection;
        }
        else
        {
            _walkPhase = 0f;
            frame = (int)(Time.time * _fps) % perDirection;
        }
        _spriteRenderer.sprite = frames[FacingDirection * perDirection + frame];
    }
    
    public static Vector2 DirectionVector(int direction)
    {
        float angle = (90f + direction * 45f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private static int GetDirection(Vector2 aim)
    {
        float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg - 90f;
        return ((Mathf.RoundToInt(angle / 45f) % DIRECTION_COUNT) + DIRECTION_COUNT) % DIRECTION_COUNT;
    }
}
