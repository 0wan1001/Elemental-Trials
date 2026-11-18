using System.Collections;
using UnityEngine;

public class TitleMoonGlow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _glow;
    [SerializeField] private float _minInterval;
    [SerializeField] private float _maxInterval;
    [SerializeField] private float _duration;
    [SerializeField] private float _maxAlpha;

    private void Start()
    {
        SetAlpha(0f);
        StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minInterval, _maxInterval));

            for (float time = 0f; time < _duration; time += Time.deltaTime)
            {
                SetAlpha(Mathf.Sin(time / _duration * Mathf.PI) * _maxAlpha);
                yield return null;
            }
            SetAlpha(0f);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = _glow.color;
        color.a = alpha;
        _glow.color = color;
    }
}
