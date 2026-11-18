using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private FeedbackData _data;
    [SerializeField] private PixelPerfectCamera _pixelCamera;
    [SerializeField] private SpriteFlash _bossFlash;
    [SerializeField] private Image _redScreen;
    [SerializeField] private Image _whiteFlash;
    [SerializeField] private Text _popupText;
    [SerializeField] private DamageNumber _damageNumberPrefab;
    [SerializeField] private Transform _damageNumberParent;

    public static FeedbackManager Instance { get; private set; }

    private readonly Queue<DamageNumber> _numberPool = new Queue<DamageNumber>();
    private Vector3 _cameraOrigin;
    private bool _isHitstopping;
    private float _hitstopEndTime;
    private float _shakeStrength;
    private float _shakeStartTime;
    private float _redScreenEndTime;
    private float _whiteFlashEndTime;
    private float _whiteFlashDuration;
    private float _whiteFlashAlpha;
    private float _popupEndTime;
    private Camera _camera;
    private bool _isChargeZooming;
    private Vector3 _chargeZoomFocus;
    private float _chargeZoom;
    private float _baseOrthographicSize;

    private void Awake()
    {
        Instance = this;
        _cameraOrigin = _pixelCamera.transform.position;
        _camera = _pixelCamera.GetComponent<Camera>();
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

        UpdateChargeZoom();
        UpdateShake(now);

        SetAlpha(_redScreen, Fade(now, _redScreenEndTime, _data.RedScreenDuration) * _data.RedScreenAlpha);
        SetAlpha(_whiteFlash, Fade(now, _whiteFlashEndTime, _whiteFlashDuration) * _whiteFlashAlpha);

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
        _bossFlash.Flash(_data.BossFlashDuration);
        DamageNumber number = _numberPool.Count > 0 ? _numberPool.Dequeue() : CreateNumber();
        number.gameObject.SetActive(true);
        number.Show(this, damage, _data.GetColor(element), position + Vector3.up * _data.DamageNumberHeight, _data.DamageNumberDuration, _data.DamageNumberRise);
    }

    public void PlayerHit()
    {
        Hitstop(_data.PlayerHitHitstop);
        Shake(_data.PlayerHitShake);
        _redScreenEndTime = Time.unscaledTime + _data.RedScreenDuration;
    }

    public void LightningFlash() => WhiteFlash(_data.LightningFlashDuration, _data.LightningFlashAlpha);

    // 원소 스킬 풀차지 순간: 화면을 살짝 번쩍이고 약하게 흔든다
    public void ChargeReady()
    {
        WhiteFlash(_data.ChargeReadyFlashDuration, _data.ChargeReadyFlashAlpha);
        Shake(_data.ChargeReadyShake);
    }

    private void WhiteFlash(float duration, float alpha)
    {
        // 더 센 번쩍임이 진행 중이면 덮어쓰지 않는다
        float now = Time.unscaledTime;
        if (now < _whiteFlashEndTime && Fade(now, _whiteFlashEndTime, _whiteFlashDuration) * _whiteFlashAlpha > alpha) return;
        _whiteFlashDuration = duration;
        _whiteFlashAlpha = alpha;
        _whiteFlashEndTime = now + duration;
    }

    public void FireBallExplosion() => Shake(_data.FireBallShake);

    public void ChargeImpact() => Shake(_data.ChargeShake);

    public void Teleport() => Shake(_data.TeleportShake);

    // 탄 한 발이 대상에 맞은 순간: 아주 짧게 멈추고 약하게 흔든다
    public void ProjectileHit(float hitstop, float shake)
    {
        if (hitstop > 0f) Hitstop(hitstop);
        if (shake > 0f) Shake(shake);
    }
    
    public void SetChargeZoom(bool isZooming, Vector3 focus)
    {
        _isChargeZooming = isZooming;
        if (isZooming) _chargeZoomFocus = focus;
    }

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
        _pixelCamera.transform.position = ChargeZoomCameraPosition() + offset;
    }
    
    private void UpdateChargeZoom()
    {
        if (_chargeZoom <= 0f && _isChargeZooming)
        {
            _baseOrthographicSize = _camera.orthographicSize;
        }
        _chargeZoom = Mathf.MoveTowards(_chargeZoom, _isChargeZooming ? 1f : 0f, _data.ChargeZoomSpeed * Time.unscaledDeltaTime);
        _pixelCamera.enabled = _chargeZoom <= 0f;
        if (_chargeZoom > 0f)
        {
            _camera.orthographicSize = _baseOrthographicSize * (1f - _data.ChargeZoomAmount * Smooth(_chargeZoom));
        }
    }

    private Vector3 ChargeZoomCameraPosition()
    {
        if (_chargeZoom <= 0f) return _cameraOrigin;
        Vector3 toFocus = _chargeZoomFocus - _cameraOrigin;
        toFocus.z = 0f;
        return _cameraOrigin + toFocus * (_data.ChargeZoomFollow * Smooth(_chargeZoom));
    }
    
    private static float Smooth(float t) => t * t * (3f - 2f * t);

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
