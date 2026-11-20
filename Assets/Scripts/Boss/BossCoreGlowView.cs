using UnityEngine;

// 원형 탄막 예고: 코어 자리에서 빛 덩어리와 십자 빛줄기가 점점 커지고 밝아지다가 발사 직전에 가장 환해진다.
// 진행도는 예고 상태가 보스 시간(DeltaTime)으로 넘겨 주므로, 느려짐 중에는 반짝임도 같이 느려진다.
public class BossCoreGlowView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _glow;
    [SerializeField] private SpriteRenderer _glintHorizontal;
    [SerializeField] private SpriteRenderer _glintVertical;

    [Header("Glow")]
    [SerializeField] private Color _startColor;
    [SerializeField] private Color _endColor;
    [SerializeField] private float _glowMinScale;
    [SerializeField] private float _glowMaxScale;

    [Header("Glint")]
    [SerializeField] private float _glintMaxLength;
    [SerializeField] private float _glintThickness;

    [Header("Pulse")]
    [SerializeField] private int _pulseCount;
    [SerializeField] private float _pulseAmount;

    private void Awake() => Hide();

    // progress: 예고 시작 0 → 발사 1
    public void Show(float progress)
    {
        float intensity = Mathf.Clamp01(progress);
        intensity *= intensity;
        float pulse = Mathf.Sin(progress * Mathf.PI * 2f * _pulseCount) * 0.5f + 0.5f;
        float wave = 1f + _pulseAmount * pulse;
        Color color = Color.Lerp(_startColor, _endColor, intensity);

        _glow.transform.localScale = Vector3.one * (Mathf.Lerp(_glowMinScale, _glowMaxScale, intensity) * wave);
        _glow.color = color;

        float length = _glintMaxLength * intensity * wave;
        _glintHorizontal.transform.localScale = new Vector3(length, _glintThickness, 1f);
        _glintVertical.transform.localScale = new Vector3(_glintThickness, length, 1f);
        _glintHorizontal.color = color;
        _glintVertical.color = color;

        SetVisible(true);
    }

    public void Hide() => SetVisible(false);

    private void SetVisible(bool isVisible)
    {
        _glow.enabled = isVisible;
        _glintHorizontal.enabled = isVisible;
        _glintVertical.enabled = isVisible;
    }
}
