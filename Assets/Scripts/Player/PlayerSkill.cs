using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [SerializeField] private TileGrid _tileGrid;
    [SerializeField] private ElementSkill[] _elementSkills;
    private PlayerController  _playerController;
    private float[] _nextSkillTime;

    private void Awake()
    {
        _nextSkillTime = new float[_elementSkills.Length];
        _playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        for (int i = 0; i < _elementSkills.Length; i++)
        {
            
            if (_playerController.IsSkillPressed(i) && Time.time > _nextSkillTime[i])
            {
                _nextSkillTime[i] = Time.time + _elementSkills[i].Cooldown;
                _elementSkills[i].Use(_playerController.transform.position, _playerController.MousePosition, _tileGrid);
            }
        }
    }
}
