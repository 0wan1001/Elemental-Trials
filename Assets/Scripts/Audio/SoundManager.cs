using System.Collections.Generic;
using UnityEngine;

// 효과음 재생 담당. 씬에 하나만 둔다.
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundData _soundData;
    [SerializeField] private AudioSource _audioSource;

    public static SoundManager Instance { get; private set; }

    private readonly Dictionary<SoundType, float> _lastPlayTimes = new Dictionary<SoundType, float>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Play(SoundType type)
    {
        SoundEntry entry = _soundData.Find(type);
        if (entry == null || entry.Clip == null) return;

        // 히트스톱으로 게임 시간이 멈춰도 간격은 실제 시간으로 잰다.
        float now = Time.unscaledTime;
        if (_lastPlayTimes.TryGetValue(type, out float lastTime) && now - lastTime < entry.MinInterval) return;
        _lastPlayTimes[type] = now;

        _audioSource.PlayOneShot(entry.Clip, entry.Volume);
    }
}
