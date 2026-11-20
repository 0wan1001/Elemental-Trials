using UnityEngine;

public enum SoundType
{
    BasicShot,
    FireballCast,
    FireballExplosion,
    WaterWave,
    LightningStrike,
    ChargeReady,
    Teleport,
    ReactionShock,
    ReactionOverload,
    ReactionSteam,
    BossHit,
    PlayerHurt,
    BossBullet,
    DashSlam,
}

[System.Serializable]
public class SoundEntry
{
    public SoundType Type;
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = 1f;
    // 같은 소리를 이 시간(초) 안에 다시 부르면 재생하지 않는다. 연타로 소리가 겹쳐 터지는 것을 막는다.
    public float MinInterval;
}

[CreateAssetMenu(fileName = "SoundData", menuName = "ScriptableObjects/SoundData")]
public class SoundData : ScriptableObject
{
    [SerializeField] private SoundEntry[] _entries;

    public SoundEntry Find(SoundType type)
    {
        foreach (SoundEntry entry in _entries)
        {
            if (entry.Type == type) return entry;
        }
        return null;
    }
}
