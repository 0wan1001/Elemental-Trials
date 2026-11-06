using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashCooldown;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _dashInvincibleDuration; 
    
    public float MoveSpeed => _moveSpeed;
    public float DashSpeed => _dashSpeed;
    public float DashCooldown => _dashCooldown;
    public float DashDuration => _dashDuration;
    public float DashInvincibleDuration => _dashInvincibleDuration;
    
}
