using UnityEngine;

public class PlayerChargeEffect : MonoBehaviour
{
    [SerializeField] private GameObject _readyFlashVfx;
    [SerializeField] private float _readyFlashLifetime;

    private PlayerSpriteAnimator _spriteAnimator;
    private GameObject _current;
    private GameObject _chargedPrefab;
    private Object _owner;
    private bool _isReady;

    private void Awake()
    {
        _spriteAnimator = GetComponent<PlayerSpriteAnimator>();
    }

    public void Begin(Object owner, GameObject chargingPrefab, GameObject chargedPrefab)
    {
        Clear();
        _owner = owner;
        _chargedPrefab = chargedPrefab;
        _isReady = false;
        _current = SpawnAtStaffTip(chargingPrefab);
    }
    
    public void MarkReady(Object owner)
    {
        if (owner != _owner || _current == null || _isReady) return;
        _isReady = true;
        Destroy(_current);
        _current = SpawnAtStaffTip(_chargedPrefab);
        Destroy(SpawnAtStaffTip(_readyFlashVfx), _readyFlashLifetime);
        FeedbackManager.Instance.ChargeReady();
    }

    public void End(Object owner)
    {
        if (owner != _owner) return;
        Clear();
    }

    private void LateUpdate()
    {
        if (_current == null) return;
        _current.transform.localPosition = _spriteAnimator.StaffTipOffset;
        if (_isReady) FeedbackManager.Instance.SetChargeZoom(true, transform.position);
    }

    private GameObject SpawnAtStaffTip(GameObject prefab)
    {
        GameObject spawned = Instantiate(prefab, transform);
        spawned.transform.localPosition = _spriteAnimator.StaffTipOffset;
        return spawned;
    }

    private void Clear()
    {
        if (_current != null)
        {
            Destroy(_current);
            FeedbackManager.Instance.SetChargeZoom(false, transform.position);
        }
        _current = null;
        _owner = null;
    }
}
