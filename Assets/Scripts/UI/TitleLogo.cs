using System.Collections;
using UnityEngine;

public class TitleLogo : MonoBehaviour
{
    private const float PIXELS_PER_UNIT = 32f;

    [Header("Float")]
    [SerializeField] private float _floatAmplitude;
    [SerializeField] private float _floatPeriod;

    [Header("Shine")]
    [SerializeField] private Transform _shine;
    [SerializeField] private float _shineMinInterval;
    [SerializeField] private float _shineMaxInterval;
    [SerializeField] private float _shineDuration;
    [SerializeField] private float _shineTravel;

    private Vector3 _startPosition;

    private void Awake()
    {
        _startPosition = transform.localPosition;
        _shine.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(ShineRoutine());
    }

    private void Update()
    {
        float y = Mathf.Sin(Time.time / _floatPeriod * Mathf.PI * 2f) * _floatAmplitude;
        transform.localPosition = _startPosition + new Vector3(0f, SnapToPixel(y), 0f);
    }

    private IEnumerator ShineRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_shineMinInterval, _shineMaxInterval));

            _shine.gameObject.SetActive(true);
            for (float time = 0f; time < _shineDuration; time += Time.deltaTime)
            {
                float x = Mathf.Lerp(-_shineTravel, _shineTravel, time / _shineDuration);
                _shine.localPosition = new Vector3(SnapToPixel(x), 0f, 0f);
                yield return null;
            }
            _shine.gameObject.SetActive(false);
        }
    }

    private float SnapToPixel(float value)
    {
        return Mathf.Round(value * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
    }
}
