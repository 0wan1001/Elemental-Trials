using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    private LightningSkill _skill;
    private Vector2 _position;
    private TileGrid _grid;
    private float _delay;
    private float _timer;
    private void Update()
    {
        _timer += Time.deltaTime;
        transform.localScale = Vector3.one * (1 - _timer / _delay);
        if (_timer >= _delay)
        {
            _skill.Strike(_position, _grid);
            Destroy(this.gameObject);
        }
    }

    public void Init(LightningSkill skill, Vector2 position, TileGrid grid, float delay)
    {
        _skill = skill;
        _position = position;
        _grid = grid;
        _delay = delay;
        _timer = 0;
    }
}
