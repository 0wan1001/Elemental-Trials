using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private FeedbackData _data;
    [SerializeField] private PixelPerfectCamera _pixelCamera;
    [SerializeField] private SpriteRenderer _bossBody;
    [SerializeField] private Image _redScreen;
    [SerializeField] private Image _whiteFlash;
    [SerializeField] private Text _popupText;
    [SerializeField] private DamageNumber _damageNumberPrefab;
    [SerializeField] private Transform _damageNumberParent;

    public static FeedbackManager Instance { get; private set; }

    private readonly Queue<DamageNumber> _numberPool = new Queue<DamageNumber>();
    private Vector3 _cameraOrigin;
    private Color _bossColor;
    private bool _isHitstopping;
    private float _hitstopEndTime;
    private float _shakeStrength;
    private float _shakeStartTime;
    private float _bossFlashEndTime;
    private float _redScreenEndTime;
    private float _whiteFlashEndTime;
    private float _popupEndTime;

    private void Awake()
    {
        Instance = this;
        _cameraOrigin = _pixelCamera.transform.position;
        _bossColor = _bossBody.color;
        for (int i = 0; i < _data.DamageNumberPoolSize; i++)
        {
            ReturnNumber(CreateNumber());
        }
        _popupText.gameObject.SetActive(false);
        SetAlpha(_redScreen, 0f);
        SetAlpha(_whiteFlash, 0f);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        float now = Time.unscaledTime;

        if (_isHitstopping && now >= _hitstopEndTime)
        {
            _isHitstopping = false;
            Time.timeScale = 1f;
        }

        UpdateShake(now);

        if (_bossBody != null) _bossBody.color = now < _bossFlashEndTime ? Color.white : _bossColor;

        SetAlpha(_redScreen, Fade(now, _redScreenEndTime, _data.RedScreenDuration) * _data.RedScreenAlpha);
        SetAlpha(_whiteFlash, Fade(now, _whiteFlashEndTime, _data.LightningFlashDuration) * _data.LightningFlashAlpha);

        if (_popupText.gameObject.activeSelf && now >= _popupEndTime) _popupText.gameObject.SetActive(false);
    }

    public void Reaction(ReactionType reaction)
    {
        switch (reaction)
        {
            case ReactionType.ElectricShock:
                Hitstop(_data.ShockHitstop);
                Shake(_data.ShockShake);
                Popup("감전!", _data.GetColor(ElementType.Lightning));
                break;
            case ReactionType.Overload:
                Hitstop(_data.OverloadHitstop);
                Shake(_data.OverloadShake);
                Popup("과부하!", _data.GetColor(ElementType.Fire));
                break;
            case ReactionType.Steam:
                Popup("증기!", _data.GetColor(ElementType.Water));
                break;
        }
    }

    public void Conduct()
    {
        Popup("전도!", _data.GetColor(ElementType.Lightning));
    }

    public void BossHit(int damage, ElementType element, Vector3 position)
    {
        _bossFlashEndTime = Time.unscaledTime + _data.BossFlashDuration;
        DamageNumber number = _numberPool.Count > 0 ? _numberPool.Dequeue() : CreateNumber();
        number.gameObject.SetActive(true);
        number.Show(this, damage, _data.GetColor(element), position, _data.DamageNumberDuration, _data.DamageNumberRise);
    }

    public void PlayerHit()
    {
        Hitstop(_data.PlayerHitHitstop);
        Shake(_data.PlayerHitShake);
        _redScreenEndTime = Time.unscaledTime + _data.RedScreenDuration;
    }

    public void LightningFlash()
    {
        _whiteFlashEndTime = Time.unscaledTime + _data.LightningFlashDuration;
    }

    public void FireBallExplosion() => Shake(_data.FireBallShake);

    public void ChargeImpact() => Shake(_data.ChargeShake);

    public void ReturnNumber(DamageNumber number)
    {
        number.gameObject.SetActive(false);
        _numberPool.Enqueue(number);
    }

    private void Hitstop(float duration)
    {
        _hitstopEndTime = Mathf.Max(_hitstopEndTime, Time.unscaledTime + duration);
        _isHitstopping = true;
        Time.timeScale = 0f;
    }

    private void Shake(float strength)
    {
        if (CurrentShakeStrength(Time.unscaledTime) > strength) return;
        _shakeStrength = strength;
        _shakeStartTime = Time.unscaledTime;
    }

    private void UpdateShake(float now)
    {
        float strength = CurrentShakeStrength(now);
        Vector3 offset = Vector3.zero;
        if (strength > 0f)
        {
            float pixel = 1f / _pixelCamera.assetsPPU;
            offset.x = Mathf.Round(Random.Range(-strength, strength)) * pixel;
            offset.y = Mathf.Round(Random.Range(-strength, strength)) * pixel;
        }
        _pixelCamera.transform.position = _cameraOrigin + offset;
    }

    private float CurrentShakeStrength(float now)
    {
        float t = (now - _shakeStartTime) / _data.ShakeDuration;
        return t >= 1f ? 0f : _shakeStrength * (1f - t);
    }

    private void Popup(string message, Color color)
    {
        _popupText.text = message;
        _popupText.color = color;
        _popupText.gameObject.SetActive(true);
        _popupEndTime = Time.unscaledTime + _data.PopupDuration;
    }

    private DamageNumber CreateNumber()
    {
        DamageNumber number = Instantiate(_damageNumberPrefab, _damageNumberParent);
        number.gameObject.SetActive(true);
        return number;
    }

    private static float Fade(float now, float endTime, float duration)
    {
        return now >= endTime ? 0f : (endTime - now) / duration;
    }

    private static void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
