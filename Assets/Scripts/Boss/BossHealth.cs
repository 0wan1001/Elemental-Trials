using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private BossData _bossData;
    [SerializeField] private DeathPopup _clearPopup;

    private int _bossHp;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        Init();
        CacheComponents();
    }

    private void Init()
    {
        _bossHp = _bossData.BossHealth;
    }

    private void CacheComponents()
    {
        
    }
    public void TakeDamage(int damage, ElementType element = ElementType.None)
    {
        if (IsDead) return;

        _bossHp -= damage;
        FeedbackManager.Instance.BossHit(damage, element, transform.position);
        if (_bossHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        _clearPopup.Show();
        gameObject.SetActive(false);
    }
}
