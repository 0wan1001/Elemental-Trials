using UnityEngine;

// 보스 몸 그림: 움직이는 동안 걷기 프레임을 돌리고, 멈추면 대기 그림을 보여 준다.
public class BossSpriteAnimator : MonoBehaviour
{
    private const float MOVING_SPEED = 0.1f;

    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Sprite[] _walkFrames;
    [SerializeField] private float _walkFps;
    [SerializeField] private Sprite _idleSprite;

    private SpriteRenderer _spriteRenderer;
    private float _walkPhase;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        bool isMoving = _rigidbody.linearVelocity.magnitude > MOVING_SPEED;
        if (!isMoving)
        {
            _walkPhase = 0f;
            _spriteRenderer.sprite = _idleSprite;
            return;
        }
        _walkPhase += Time.deltaTime * _walkFps;
        _spriteRenderer.sprite = _walkFrames[(int)_walkPhase % _walkFrames.Length];
    }
}
