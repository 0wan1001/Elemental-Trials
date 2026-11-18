using UnityEngine;
using UnityEngine.UI;

// 전투 HUD: 플레이어 하트·상태 효과, 보스 이름·HP 바·숫자·상태 효과, 스킬 슬롯 쿨타임.
public class HudView : MonoBehaviour
{
    private const int ELEMENT_SKILL_COUNT = 3;

    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private PlayerSkill _playerSkill;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private ElementStatus _playerStatus;
    [SerializeField] private BossHealth _bossHealth;
    [SerializeField] private BossController _bossController;
    [SerializeField] private ElementStatus _bossStatus;

    [SerializeField] private RectTransform _heartRoot;
    [SerializeField] private Image[] _hearts;
    [SerializeField] private Sprite _heartActive;
    [SerializeField] private Sprite _heartInactive;
    [SerializeField] private float _heartShakeDuration;
    [SerializeField] private float _heartShakeStrength;

    [SerializeField] private Text _bossNameText;
    [SerializeField] private Image _bossHpFill;
    [SerializeField] private Image _bossHpGhost;
    [SerializeField] private Text _bossHpText;
    [SerializeField] private float _barSmoothSharpness;
    [SerializeField] private float _ghostDelay;
    [SerializeField] private float _ghostSpeed;

    // 순서: 좌클릭, 1, 2, 3, 스페이스
    [SerializeField] private SkillSlotView _attackSlot;
    [SerializeField] private SkillSlotView[] _elementSlots;
    [SerializeField] private SkillSlotView _dashSlot;

    // 순서: 화상(Fire), 젖음(Water), 감전(Lightning)
    [SerializeField] private StatusIconView[] _playerStatusIcons;
    [SerializeField] private StatusIconView[] _bossStatusIcons;
    // 반응 결과: "전도 · 기절 3", "기절 면역 10", "증기 · 느려짐 4", "과부하 · 화상 4"
    [SerializeField] private Text _bossReactionText;

    private int _shownPlayerHP = -1;
    private int _shownBossHP = -1;
    private Vector2 _heartOrigin;
    private float _heartShakeEndTime;
    private float _bossHpTarget = 1f;
    private float _ghostHoldEndTime;

    private void Start()
    {
        _bossNameText.text = _bossHealth.BossName;
        _heartOrigin = _heartRoot.anchoredPosition;
    }

    private void Update()
    {
        UpdateHearts();
        UpdateBossHp();
        UpdateSkills();
        UpdateStatuses();
    }

    private void UpdateHearts()
    {
        int hp = _playerHealth.CurrentHP;
        if (hp != _shownPlayerHP)
        {
            if (_shownPlayerHP >= 0 && hp < _shownPlayerHP) _heartShakeEndTime = Time.unscaledTime + _heartShakeDuration;
            _shownPlayerHP = hp;
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].sprite = i < hp ? _heartActive : _heartInactive;
            }
        }

        // 피격 직후 하트 줄을 잠깐 흔든다. 줄어드는 세기로 정수 픽셀만큼 움직인다.
        float remaining = _heartShakeEndTime - Time.unscaledTime;
        Vector2 offset = Vector2.zero;
        if (remaining > 0f)
        {
            float strength = _heartShakeStrength * remaining / _heartShakeDuration;
            offset = new Vector2(Mathf.Round(Random.Range(-strength, strength)), Mathf.Round(Random.Range(-strength, strength)));
        }
        _heartRoot.anchoredPosition = _heartOrigin + offset;
    }

    private void UpdateBossHp()
    {
        int hp = _bossHealth.CurrentHP;
        int maxHP = _bossHealth.MaxHP;
        if (hp != _shownBossHP)
        {
            if (_shownBossHP >= 0 && hp < _shownBossHP) _ghostHoldEndTime = Time.unscaledTime + _ghostDelay;
            _shownBossHP = hp;
            _bossHpTarget = (float)hp / maxHP;
            _bossHpText.text = $"{hp} / {maxHP}";
        }

        // 스무스 바: 목표 값으로 빠르게 미끄러진다
        float t = 1f - Mathf.Exp(-_barSmoothSharpness * Time.unscaledDeltaTime);
        _bossHpFill.fillAmount = Mathf.Lerp(_bossHpFill.fillAmount, _bossHpTarget, t);

        // 고스트 바: 잠깐 머물렀다가 천천히 따라 내려온다. 회복이면 바로 맞춘다.
        if (_bossHpGhost.fillAmount < _bossHpFill.fillAmount) _bossHpGhost.fillAmount = _bossHpFill.fillAmount;
        else if (Time.unscaledTime >= _ghostHoldEndTime)
        {
            _bossHpGhost.fillAmount = Mathf.MoveTowards(_bossHpGhost.fillAmount, _bossHpFill.fillAmount, _ghostSpeed * Time.unscaledDeltaTime);
        }
    }

    private void UpdateSkills()
    {
        _attackSlot.SetCooldown(_playerAttack.CooldownRemaining, _playerAttack.Cooldown);
        for (int i = 0; i < ELEMENT_SKILL_COUNT; i++)
        {
            _elementSlots[i].SetCooldown(_playerSkill.GetCooldownRemaining(i), _playerSkill.GetCooldown(i));
            _elementSlots[i].SetSelected(_playerSkill.SelectedIndex == i);
        }
        _dashSlot.SetCharges(_playerController.DashCharges, _playerController.MaxDashCharges,
            _playerController.DashRechargeRemaining, _playerController.DashCooldown);
    }

    private void UpdateStatuses()
    {
        for (int i = 0; i < _playerStatusIcons.Length; i++)
        {
            var type = (ElementType)i;
            _playerStatusIcons[i].SetRemaining(_playerStatus.GetRemaining(type), _playerStatus.GetDuration(type));
            _bossStatusIcons[i].SetRemaining(_bossStatus.GetRemaining(type), _bossStatus.GetDuration(type));
        }
        _bossReactionText.text = BuildReactionText();
    }

    private string BuildReactionText()
    {
        string text = "";
        if (_bossController.StunRemaining > 0f) text = AddReaction(text, "전도 · 기절", _bossController.StunRemaining);
        else if (_bossController.StunImmuneRemaining > 0f) text = AddReaction(text, "기절 면역", _bossController.StunImmuneRemaining);
        if (_bossStatus.SteamRemaining > 0f) text = AddReaction(text, "증기 · 느려짐", _bossStatus.SteamRemaining);
        if (_bossStatus.BurnRemaining > 0f) text = AddReaction(text, "과부하 · 화상", _bossStatus.BurnRemaining);
        return text;
    }

    private static string AddReaction(string text, string label, float remaining)
    {
        string line = $"{label} {Mathf.CeilToInt(remaining)}";
        return text.Length == 0 ? line : $"{text}\n{line}";
    }
}
