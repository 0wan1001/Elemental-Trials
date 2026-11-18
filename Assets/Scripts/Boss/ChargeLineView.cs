using UnityEngine;

// 보스 돌진 예고선 표시: 돌진 경로를 띠로 그리고, 돌진까지 남은 시간만큼 보스 쪽부터 차오르게 한다.
public class ChargeLineView : MonoBehaviour
{
    private const float PIXELS_PER_UNIT = 32f;

    [SerializeField] private SpriteRenderer _lane;
    [SerializeField] private float _width;

    private Material _material;

    public void Show(Vector2 direction, float length)
    {
        if (_material == null) _material = _lane.material;
        transform.right = direction;
        Vector2 spriteSize = _lane.sprite.bounds.size;
        _lane.transform.localPosition = new Vector3(length * 0.5f, 0f, 0f);
        _lane.transform.localScale = new Vector3(length / spriteSize.x, _width / spriteSize.y, 1f);
        _material.SetVector("_SizePixels", new Vector4(Mathf.Round(length * PIXELS_PER_UNIT), Mathf.Round(_width * PIXELS_PER_UNIT), 0f, 0f));
        SetProgress(0f);
        gameObject.SetActive(true);
    }

    public void SetProgress(float progress)
    {
        _material.SetFloat("_Progress", Mathf.Clamp01(progress));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
