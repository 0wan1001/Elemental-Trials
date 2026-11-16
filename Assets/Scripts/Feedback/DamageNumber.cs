using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    private Text _text;
    private FeedbackManager _owner;
    private Vector3 _worldPosition;
    private float _startTime;
    private float _duration;
    private float _rise;
    private Color _color;

    private void Awake() => _text = GetComponent<Text>();

    public void Show(FeedbackManager owner, int damage, Color color, Vector3 worldPosition, float duration, float rise)
    {
        _owner = owner;
        _worldPosition = worldPosition;
        _startTime = Time.unscaledTime;
        _duration = duration;
        _rise = rise;
        _color = color;
        _text.text = damage.ToString();
        UpdateView(0f);
    }

    private void Update()
    {
        float t = (Time.unscaledTime - _startTime) / _duration;
        if (t >= 1f)
        {
            _owner.ReturnNumber(this);
            return;
        }
        UpdateView(t);
    }

    private void UpdateView(float t)
    {
        transform.position = Camera.main.WorldToScreenPoint(_worldPosition + Vector3.up * (_rise * t));
        Color color = _color;
        color.a = t < 0.5f ? 1f : (1f - t) * 2f;
        _text.color = color;
    }
}
