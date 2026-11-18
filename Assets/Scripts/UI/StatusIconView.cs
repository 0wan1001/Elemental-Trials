using UnityEngine;
using UnityEngine.UI;

// 상태 효과 하나의 남은 시간 표시: 지난 시간만큼 시계 방향으로 어두워지고, 남은 초를 숫자로 보여 준다.
public class StatusIconView : MonoBehaviour
{
    [SerializeField] private Image _elapsedMask;
    [SerializeField] private RectTransform _hand;
    [SerializeField] private Text _secondsText;

    public void SetRemaining(float remaining, float duration)
    {
        bool isActive = remaining > 0f && duration > 0f;
        if (gameObject.activeSelf != isActive) gameObject.SetActive(isActive);
        if (!isActive) return;

        float elapsedRatio = 1f - Mathf.Clamp01(remaining / duration);
        _elapsedMask.fillAmount = elapsedRatio;
        _hand.localEulerAngles = new Vector3(0f, 0f, -elapsedRatio * 360f);
        _secondsText.text = Mathf.CeilToInt(remaining).ToString();
    }
}
