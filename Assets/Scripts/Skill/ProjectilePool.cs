using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private int _projectilePoolSize;

    private readonly Queue<Projectile> _inactiveProjectiles = new Queue<Projectile>();
    private bool _hasWarnedExpand;

    private void Awake()
    {
        for (int i = 0; i < _projectilePoolSize; i++)
        {
            Return(Create());
        }
    }

    public Projectile Get()
    {
        Projectile projectile;
        if (_inactiveProjectiles.Count > 0)
        {
            projectile = _inactiveProjectiles.Dequeue();
        }
        else
        {
            if (!_hasWarnedExpand)
            {
                _hasWarnedExpand = true;
            }
            projectile = Create();
        }

        projectile.gameObject.SetActive(true);
        return projectile;
    }

    public void Return(Projectile projectile)
    {
        if (!projectile.gameObject.activeSelf)
        {
            return;
        }

        projectile.gameObject.SetActive(false);
        _inactiveProjectiles.Enqueue(projectile);
    }

    private Projectile Create()
    {
        Projectile projectile = Instantiate(_projectilePrefab, transform);
        projectile.SetPool(this);
        return projectile;
    }
}