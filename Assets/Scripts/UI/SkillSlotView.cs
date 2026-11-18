using UnityEngine;
using UnityEngine.UI;

// 스킬 아이콘 하나의 쿨타임 표시: 흑백(비활성) 아이콘 위로 컬러(활성) 아이콘이 시계 방향으로 걷혀 나오고 그 경계를 흰 선이 따라 돈다.
public class SkillSlotView : MonoBehaviour
{
    [SerializeField] private Image _activeIcon;
    [SerializeField] private RectTransform _hand;
    [SerializeField] private Image _readyFlash;
    [SerializeField] private GameObject _selectedFrame;
    [SerializeField] private Text _countText;
    [SerializeField] private float _flashDuration;

    private bool _isCoolingDown;
    private float _flashTimer;

    public void SetCooldown(float remaining, float duration)
    {
        bool isCoolingDown = remaining > 0f && duration > 0f;
        if (_isCoolingDown && !isCoolingDown) _flashTimer = _flashDuration;
        _isCoolingDown = isCoolingDown;

        float remainingRatio = isCoolingDown ? Mathf.Clamp01(remaining / duration) : 0f;
        _activeIcon.fillAmount = 1f - remainingRatio;
        _hand.gameObject.SetActive(isCoolingDown);
        _hand.localEulerAngles = new Vector3(0f, 0f, -(1f - remainingRatio) * 360f);
    }

    // 충전식 스킬: 남은 횟수가 0일 때만 흑백으로 바꾸고, 흰 선은 다음 충전까지의 진행을 보여 준다.
    public void SetCharges(int charges, int maxCharges, float rechargeRemaining, float rechargeDuration)
    {
        bool isRecharging = charges < maxCharges && rechargeDuration > 0f;
        float remainingRatio = isRecharging ? Mathf.Clamp01(rechargeRemaining / rechargeDuration) : 0f;
        if (_isCoolingDown && charges > 0) _flashTimer = _flashDuration;
        _isCoolingDown = charges == 0;

        _activeIcon.fillAmount = charges == 0 ? 1f - remainingRatio : 1f;
        _hand.gameObject.SetActive(isRecharging);
        _hand.localEulerAngles = new Vector3(0f, 0f, -(1f - remainingRatio) * 360f);
        _countText.text = charges.ToString();
    }

    public void SetSelected(bool isSelected)
    {
        if (_selectedFrame.activeSelf != isSelected) _selectedFrame.SetActive(isSelected);
    }

    private void Update()
    {
        if (_flashTimer > 0f) _flashTimer -= Time.deltaTime;
        Color color = _readyFlash.color;
        color.a = Mathf.Clamp01(_flashTimer / _flashDuration) * 0.8f;
        _readyFlash.color = color;
    }
}
