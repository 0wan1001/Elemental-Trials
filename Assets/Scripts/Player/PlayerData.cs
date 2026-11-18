using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashCooldown;
    [SerializeField] private int _maxDashCharges;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _dashInvincibleDuration;
    [SerializeField] private int _playerMaxHP;
    [SerializeField] private float _hitInvincibleDuration;
    
    public float MoveSpeed => _moveSpeed;
    public float Acceleration => _acceleration;
    public float Deceleration => _deceleration;
    public float DashSpeed => _dashSpeed;
    public float DashCooldown => _dashCooldown;
    public int MaxDashCharges => _maxDashCharges;
    public float DashDuration => _dashDuration;
    public float DashInvincibleDuration => _dashInvincibleDuration;
    public int PlayerMaxHP => _playerMaxHP;
    public float HitInvincibleDuration => _hitInvincibleDuration;
}
