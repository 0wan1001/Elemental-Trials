using UnityEngine;

public class TitleCloudScroll : MonoBehaviour
{
    private const float PIXELS_PER_UNIT = 32f;

    [SerializeField] private float _speed;
    [SerializeField] private float _loopWidth;

    private float _offset;

    private void Update()
    {
        _offset = Mathf.Repeat(_offset + _speed * Time.deltaTime, _loopWidth);
        float x = Mathf.Round(-_offset * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
        transform.localPosition = new Vector3(x, 0f, 0f);
    }
}
