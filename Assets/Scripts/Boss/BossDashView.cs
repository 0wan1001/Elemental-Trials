using UnityEngine;

// 보스 돌진 연출: 몸의 잔상이 일정 간격으로 남아 옅어지고, 발밑에서 흙먼지가 뒤쪽으로 피어오른다.
public class BossDashView : MonoBehaviour
{
    private const int AFTERIMAGE_COUNT = 6;

    [SerializeField] private SpriteRenderer _body;
    [SerializeField] private Material _afterimageMaterial;
    [SerializeField] private Color _afterimageColor;
    [SerializeField] private float _afterimageInterval;
    [SerializeField] private float _afterimageLifetime;
    [SerializeField] private GameObject _dustVfx;
    [SerializeField] private float _dustInterval;
    [SerializeField] private float _dustLifetime;
    [SerializeField] private Vector2 _feetOffset;

    private SpriteRenderer[] _afterimages;
    private Material[] _afterimageMaterials;
    private float[] _afterimageEndTimes;
    private int _nextAfterimage;
    private bool _isDashing;
    private Vector2 _direction;
    private float _nextAfterimageTime;
    private float _nextDustTime;

    private void Awake()
    {
        _afterimages = new SpriteRenderer[AFTERIMAGE_COUNT];
        _afterimageMaterials = new Material[AFTERIMAGE_COUNT];
        _afterimageEndTimes = new float[AFTERIMAGE_COUNT];
        for (int i = 0; i < AFTERIMAGE_COUNT; i++)
        {
            var afterimage = new GameObject("Afterimage").AddComponent<SpriteRenderer>();
            afterimage.material = _afterimageMaterial;
            afterimage.enabled = false;
            _afterimages[i] = afterimage;
            _afterimageMaterials[i] = afterimage.material;
        }
    }

    private void OnDestroy()
    {
        foreach (SpriteRenderer afterimage in _afterimages)
        {
            if (afterimage != null) Destroy(afterimage.gameObject);
        }
    }

    public void Begin(Vector2 direction)
    {
        _isDashing = true;
        _direction = direction;
        _nextAfterimageTime = Time.time;
        _nextDustTime = Time.time;
        // 출발 순간 양쪽으로 먼지를 한 번 차올린다
        SpawnDust(direction);
        SpawnDust(-direction);
    }

    public void End()
    {
        _isDashing = false;
    }

    private void Update()
    {
        if (_isDashing && Time.time >= _nextAfterimageTime)
        {
            SpawnAfterimage();
            _nextAfterimageTime = Time.time + _afterimageInterval;
        }
        if (_isDashing && Time.time >= _nextDustTime)
        {
            SpawnDust(_direction);
            _nextDustTime = Time.time + _dustInterval;
        }

        for (int i = 0; i < AFTERIMAGE_COUNT; i++)
        {
            if (!_afterimages[i].enabled) continue;
            float remaining = _afterimageEndTimes[i] - Time.time;
            if (remaining <= 0f)
            {
                _afterimages[i].enabled = false;
                continue;
            }
            Color color = _afterimageColor;
            color.a *= remaining / _afterimageLifetime;
            _afterimageMaterials[i].SetColor("_FlashColor", color);
        }
    }

    private void SpawnAfterimage()
    {
        int index = _nextAfterimage;
        _nextAfterimage = (_nextAfterimage + 1) % AFTERIMAGE_COUNT;
        SpriteRenderer afterimage = _afterimages[index];
        Transform source = _body.transform;
        afterimage.transform.SetPositionAndRotation(source.position, source.rotation);
        afterimage.transform.localScale = source.lossyScale;
        afterimage.sprite = _body.sprite;
        afterimage.flipX = _body.flipX;
        afterimage.sortingLayerID = _body.sortingLayerID;
        afterimage.sortingOrder = _body.sortingOrder - 1;
        afterimage.enabled = true;
        _afterimageMaterials[index].SetColor("_FlashColor", _afterimageColor);
        _afterimageEndTimes[index] = Time.time + _afterimageLifetime;
    }

    // 먼지 그림은 왼쪽으로 흘러가게 그려져 있다. 오른쪽으로 달릴 때 그대로, 왼쪽으로 달릴 때 뒤집는다.
    private void SpawnDust(Vector2 moveDirection)
    {
        GameObject dust = Instantiate(_dustVfx, (Vector2)transform.position + _feetOffset, Quaternion.identity);
        if (moveDirection.x < 0f) dust.transform.localScale = new Vector3(-1f, 1f, 1f);
        Destroy(dust, _dustLifetime);
    }
}
