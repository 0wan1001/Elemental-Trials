using UnityEngine;

// 대상 스프라이트 위에 같은 모양의 덮개를 겹친다.
// Flash: 흰색으로 잠깐 번쩍임. SetTint: 상태가 이어지는 동안 반투명 색을 계속 덮음 (번쩍임이 우선).
public class SpriteFlash : MonoBehaviour
{
    private const float TINT_PULSE_SPEED = 4f;

    [SerializeField] private SpriteRenderer _target;
    [SerializeField] private Material _flashMaterial;

    private SpriteRenderer _overlay;
    private Material _material;
    private float _endTime;
    private float _duration;
    private Color _tint = Color.clear;

    private void Awake()
    {
        _overlay = new GameObject("Flash").AddComponent<SpriteRenderer>();
        _overlay.transform.SetParent(_target.transform, false);
        _overlay.material = _flashMaterial;
        _material = _overlay.material;
        _overlay.enabled = false;
    }

    public void Flash(float duration)
    {
        _duration = duration;
        _endTime = Time.time + duration;
    }

    public void SetTint(Color tint)
    {
        _tint = tint;
    }

    private void LateUpdate()
    {
        bool isFlashing = Time.time < _endTime;
        bool isTinted = _tint.a > 0f;
        bool isVisible = (isFlashing || isTinted) && _target.enabled;
        _overlay.enabled = isVisible;
        if (!isVisible) return;

        _overlay.sprite = _target.sprite;
        _overlay.flipX = _target.flipX;
        _overlay.sortingLayerID = _target.sortingLayerID;
        _overlay.sortingOrder = _target.sortingOrder + 1;

        Color color;
        if (isFlashing)
        {
            color = new Color(1f, 1f, 1f, (_endTime - Time.time) / _duration);
        }
        else
        {
            color = _tint;
            color.a *= 0.8f + 0.2f * Mathf.Sin(Time.time * TINT_PULSE_SPEED);
        }
        _material.SetColor("_FlashColor", color);
    }
}
