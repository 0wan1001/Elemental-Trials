using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private Graphic _pressAnyKey;
    [SerializeField] private float _blinkInterval;
    [SerializeField] private Image _fadeScreen;
    [SerializeField] private float _fadeOutDuration;

    private IDisposable _anyButtonListener;
    private bool _isStarting;

    private void OnEnable()
    {
        _anyButtonListener = InputSystem.onAnyButtonPress.CallOnce(_ => OnStart());
    }

    private void Update()
    {
        _pressAnyKey.enabled = Mathf.Repeat(Time.time, _blinkInterval * 2f) < _blinkInterval;
    }

    private void OnDisable()
    {
        _anyButtonListener?.Dispose();
    }

    public void OnStart()
    {
        if (_isStarting) return;
        _isStarting = true;
        StartCoroutine(FadeOutRoutine());
    }

    // 화면을 검게 덮은 뒤 아레나로 넘어간다. 아레나는 검은 화면에서 밝아지며 시작한다 (ArenaIntro).
    private IEnumerator FadeOutRoutine()
    {
        _fadeScreen.gameObject.SetActive(true);
        float start = Time.unscaledTime;
        while (Time.unscaledTime - start < _fadeOutDuration)
        {
            SetFadeAlpha((Time.unscaledTime - start) / _fadeOutDuration);
            yield return null;
        }
        SetFadeAlpha(1f);
        SceneManager.LoadScene("Arena");
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = _fadeScreen.color;
        color.a = alpha;
        _fadeScreen.color = color;
    }
}
