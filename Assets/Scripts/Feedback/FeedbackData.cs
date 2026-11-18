using UnityEngine;

[CreateAssetMenu(fileName = "FeedbackData", menuName = "ScriptableObjects/FeedbackData")]
public class FeedbackData : ScriptableObject
{
    [Header("히트스톱 (초)")]
    [SerializeField] private float _shockHitstop;
    [SerializeField] private float _overloadHitstop;
    [SerializeField] private float _playerHitHitstop;

    [Header("화면 흔들림 (강도 px, 시간 초)")]
    [SerializeField] private float _shockShake;
    [SerializeField] private float _overloadShake;
    [SerializeField] private float _fireBallShake;
    [SerializeField] private float _chargeShake;
    [SerializeField] private float _playerHitShake;
    [SerializeField] private float _teleportShake;
    [SerializeField] private float _shakeDuration;

    [Header("차징 카메라 확대")]
    [SerializeField] private float _chargeZoomAmount;
    [SerializeField] private float _chargeZoomFollow;
    [SerializeField] private float _chargeZoomSpeed;

    [Header("플래시 (초)")]
    [SerializeField] private float _bossFlashDuration;
    [SerializeField] private float _redScreenDuration;
    [SerializeField] private float _redScreenAlpha;
    [SerializeField] private float _lightningFlashDuration;
    [SerializeField] private float _lightningFlashAlpha;

    [Header("원소 스킬 풀차지")]
    [SerializeField] private float _chargeReadyFlashDuration;
    [SerializeField] private float _chargeReadyFlashAlpha;
    [SerializeField] private float _chargeReadyShake;

    [Header("데미지 숫자")]
    [SerializeField] private float _damageNumberDuration;
    [SerializeField] private float _damageNumberRise;
    [SerializeField] private float _damageNumberHeight;
    [SerializeField] private int _damageNumberPoolSize;

    [Header("반응 팝업")]
    [SerializeField] private float _popupDuration;

    [Header("원소 색")]
    [SerializeField] private Color _fireColor;
    [SerializeField] private Color _waterColor;
    [SerializeField] private Color _lightningColor;
    [SerializeField] private Color _noneColor;

    public float ShockHitstop => _shockHitstop;
    public float OverloadHitstop => _overloadHitstop;
    public float PlayerHitHitstop => _playerHitHitstop;
    public float ShockShake => _shockShake;
    public float OverloadShake => _overloadShake;
    public float FireBallShake => _fireBallShake;
    public float ChargeShake => _chargeShake;
    public float PlayerHitShake => _playerHitShake;
    public float TeleportShake => _teleportShake;
    public float ShakeDuration => _shakeDuration;
    public float ChargeZoomAmount => _chargeZoomAmount;
    public float ChargeZoomFollow => _chargeZoomFollow;
    public float ChargeZoomSpeed => _chargeZoomSpeed;
    public float BossFlashDuration => _bossFlashDuration;
    public float RedScreenDuration => _redScreenDuration;
    public float RedScreenAlpha => _redScreenAlpha;
    public float LightningFlashDuration => _lightningFlashDuration;
    public float LightningFlashAlpha => _lightningFlashAlpha;
    public float ChargeReadyFlashDuration => _chargeReadyFlashDuration;
    public float ChargeReadyFlashAlpha => _chargeReadyFlashAlpha;
    public float ChargeReadyShake => _chargeReadyShake;
    public float DamageNumberDuration => _damageNumberDuration;
    public float DamageNumberRise => _damageNumberRise;
    public float DamageNumberHeight => _damageNumberHeight;
    public int DamageNumberPoolSize => _damageNumberPoolSize;
    public float PopupDuration => _popupDuration;

    public Color GetColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return _fireColor;
            case ElementType.Water: return _waterColor;
            case ElementType.Lightning: return _lightningColor;
        }
        return _noneColor;
    }
}
