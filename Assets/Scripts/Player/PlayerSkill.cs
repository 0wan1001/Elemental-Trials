using UnityEngine;


public class PlayerSkill : MonoBehaviour
{
    [SerializeField] private TileGrid _tileGrid;
    [SerializeField] private ElementSkill[] _elementSkills;
    [SerializeField] private float _orbDistance;
    [SerializeField] private float _orbHeight;
    [SerializeField] private float _orbScale;
    // 풀 차지: 구슬이 커지고, 뒤에 원소 색 빛무리가 깜빡이며 퍼진다
    [SerializeField] private float _chargedOrbScale;
    [SerializeField] private Sprite _glowSprite;
    [SerializeField] private float _glowScale;
    [SerializeField] private float _glowPulseSpeed;
    private PlayerController  _playerController;
    private PlayerChargeEffect _chargeEffect;
    private PlayerSpriteAnimator _spriteAnimator;
    private float[] _nextSkillTime;
    private int _selectedIndex = -1;
    private GameObject _orb;
    private SpriteRenderer _orbRenderer;
    private SpriteRenderer _glow;
    private float _chargeTime;
    private bool _isCharging;
    private bool _isWaitingAttackRelease;

    public bool HasSelectedSkill => _selectedIndex >= 0;
    // 스킬을 고른 동안과, 스킬을 쏜 클릭을 아직 떼지 않은 동안은 기본 공격을 막는다
    public bool BlocksAttack => HasSelectedSkill || _isWaitingAttackRelease;
    public int SelectedIndex => _selectedIndex;
    public float GetCooldown(int index) => _elementSkills[index].Cooldown;
    public float GetCooldownRemaining(int index) => Mathf.Max(0f, _nextSkillTime[index] - Time.time);

    private void Awake()
    {
        _nextSkillTime = new float[_elementSkills.Length];
        _playerController = GetComponent<PlayerController>();
        _chargeEffect = GetComponent<PlayerChargeEffect>();
        _spriteAnimator = GetComponent<PlayerSpriteAnimator>();
    }

    private void Update()
    {
        if (_isWaitingAttackRelease && !_playerController.IsAttackPressed) _isWaitingAttackRelease = false;
        for (int i = 0; i < _elementSkills.Length; i++)
        {
            if (_playerController.IsSkillPressed(i))
            {
                SelectSkill(i);
            }
        }
        if (!HasSelectedSkill) return;
        UpdateOrbPosition();

        // 스킬을 고르면 자동으로 차지하고, 다 차면 클릭으로 쏜다. 대시 중에도 차지는 이어진다.
        ElementSkill skill = _elementSkills[_selectedIndex];
        if (!_isCharging)
        {
            _isCharging = true;
            _chargeEffect.Begin(this, skill.ChargeVfx, skill.ChargedVfx);
        }
        _chargeTime += Time.deltaTime;
        bool isCharged = _chargeTime >= skill.MinChargeTime;
        if (isCharged) _chargeEffect.MarkReady(this);
        UpdateOrbCharge(isCharged, skill.GlowColor);
        if (isCharged && _playerController.IsAttackTriggered) Cast();
    }
    
    private void SelectSkill(int index)
    {
        if (index == _selectedIndex)
        {
            ClearSelection();
            return;
        }
        if (Time.time < _nextSkillTime[index]) return;

        ClearSelection();
        _selectedIndex = index;
        ElementSkill skill = _elementSkills[index];
        _orb = Instantiate(skill.OrbVfx, transform);
        _orbRenderer = _orb.GetComponent<SpriteRenderer>();
        _orb.transform.localScale = Vector3.one * _orbScale;
        _glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
        _glow.transform.SetParent(_orb.transform, false);
        _glow.sprite = _glowSprite;
        _glow.enabled = false;
        UpdateOrbPosition();
        Destroy(Instantiate(skill.SelectVfx, transform), skill.SelectVfxLifetime);
    }
    
    private void UpdateOrbPosition()
    {
        Vector2 facing = PlayerSpriteAnimator.DirectionVector(_spriteAnimator.FacingDirection);
        _orb.transform.localPosition = -facing * _orbDistance + Vector2.up * _orbHeight;
        _orbRenderer.sortingOrder = facing.y > 0.1f ? 1 : -1;
        _glow.sortingOrder = _orbRenderer.sortingOrder - 1;
    }

    private void UpdateOrbCharge(bool isCharged, Color glowColor)
    {
        _orb.transform.localScale = Vector3.one * (isCharged ? _chargedOrbScale : _orbScale);
        _glow.enabled = isCharged;
        if (!isCharged) return;
        float pulse = Mathf.Sin(Time.time * _glowPulseSpeed) * 0.5f + 0.5f;
        _glow.transform.localScale = Vector3.one * (_glowScale * (0.9f + 0.2f * pulse));
        Color color = glowColor;
        color.a *= 0.6f + 0.4f * pulse;
        _glow.color = color;
    }

    private void Cast()
    {
        ElementSkill skill = _elementSkills[_selectedIndex];
        Vector2 target = _playerController.MousePosition;
        _nextSkillTime[_selectedIndex] = Time.time + skill.Cooldown;
        Vector2 origin = _playerController.transform.position;
        if (skill.CastFromStaff) origin += _spriteAnimator.StaffTipOffset;
        skill.Use(origin, target, _tileGrid);
        Destroy(Instantiate(skill.CastVfx, _playerController.transform), skill.CastVfxLifetime);
        Destroy(Instantiate(skill.PointVfx, target, Quaternion.identity), skill.PointVfxLifetime);
        _isWaitingAttackRelease = true;
        ClearSelection();
    }

    private void StopCharge()
    {
        if (_isCharging) _chargeEffect.End(this);
        _isCharging = false;
        _chargeTime = 0f;
    }

    private void ClearSelection()
    {
        StopCharge();
        if (_orb != null) Destroy(_orb);
        _orb = null;
        _selectedIndex = -1;
    }
}
