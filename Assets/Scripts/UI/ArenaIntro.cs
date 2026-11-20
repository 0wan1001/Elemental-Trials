using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// 아레나 시작 연출: 검은 화면에서 밝아지며 모두 멈춘 채 보스를 확대해 보여 주고, 보스 이름이 나타난 뒤
// 카메라가 원래대로 돌아오면서 HUD가 나타나고 전투가 시작된다. 재시작이면 짧게 밝아지기만 한다.
public class ArenaIntro : MonoBehaviour
{
    private const float MAX_FRAME_STEP = 0.05f;

    // DeathPopup의 재시작에서 켠다. 다음 아레나 시작 한 번만 짧은 연출로 바꾼다.
    public static bool IsRestart;

    [SerializeField] private Image _blackScreen;
    [SerializeField] private Graphic _textBand;
    [SerializeField] private float _textBandAlpha;
    [SerializeField] private Graphic _titleText;
    [SerializeField] private Graphic _subtitleText;
    [SerializeField] private CanvasGroup _hudGroup;
    [SerializeField] private Camera _camera;
    [SerializeField] private PixelPerfectCamera _pixelCamera;
    [SerializeField] private FeedbackManager _feedbackManager;
    [SerializeField] private Transform _boss;
    [SerializeField] private Behaviour[] _pausedBehaviours;

    [SerializeField] private float _zoom;
    [SerializeField] private float _fadeInDuration;
    [SerializeField] private float _titleDelay;
    [SerializeField] private float _subtitleDelay;
    [SerializeField] private float _textFadeDuration;
    [SerializeField] private float _textHoldUntil;
    [SerializeField] private float _textFadeOutDuration;
    [SerializeField] private float _returnDuration;
    [SerializeField] private float _restartFadeDuration;

    private Vector3 _cameraOrigin;
    private float _baseSize;

    private void Start()
    {
        _cameraOrigin = _camera.transform.position;
        SetAlpha(_textBand, 0f);
        SetAlpha(_titleText, 0f);
        SetAlpha(_subtitleText, 0f);
        SetAlpha(_blackScreen, 1f);

        if (IsRestart)
        {
            IsRestart = false;
            _hudGroup.alpha = 1f;
            StartCoroutine(RestartRoutine());
            return;
        }
        _hudGroup.alpha = 0f;
        SetPaused(true);
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        yield return Fade(_restartFadeDuration, t => SetAlpha(_blackScreen, 1f - t));
    }

    // 모든 연출을 시계 하나로 계산한다. 장면을 불러오며 프레임이 크게 끊겨도 순서가 꼬이지 않게
    // 한 프레임에 흐르는 시간은 MAX_FRAME_STEP으로 막는다.
    private IEnumerator IntroRoutine()
    {
        // PixelPerfectCamera는 카메라를 그리기 직전에 orthographicSize를 화면 해상도에 맞춘다.
        // Start에서는 씬에 저장된 값이 읽히므로, 한 번 그린 뒤의 값을 기준으로 삼아야 끝에서 크기가 튀지 않는다.
        yield return null;
        _baseSize = _camera.orthographicSize;
        Vector3 bossView = BossViewPosition();
        float zoomedSize = _baseSize / _zoom;
        _pixelCamera.enabled = false;
        _camera.orthographicSize = zoomedSize;
        _camera.transform.position = bossView;

        float textEnd = _textHoldUntil + _textFadeOutDuration;
        float end = textEnd + _returnDuration;
        float elapsed = 0f;
        while (elapsed < end)
        {
            elapsed = Mathf.Min(elapsed + Mathf.Min(Time.unscaledDeltaTime, MAX_FRAME_STEP), end);

            SetAlpha(_blackScreen, 1f - Ramp(elapsed, 0f, _fadeInDuration));
            float textOut = 1f - Ramp(elapsed, _textHoldUntil, _textFadeOutDuration);
            float titleIn = Ramp(elapsed, _titleDelay, _textFadeDuration);
            SetAlpha(_titleText, titleIn * textOut);
            SetAlpha(_textBand, titleIn * textOut * _textBandAlpha);
            SetAlpha(_subtitleText, Ramp(elapsed, _subtitleDelay, _textFadeDuration) * textOut);

            float back = Ramp(elapsed, textEnd, _returnDuration);
            float k = Mathf.SmoothStep(0f, 1f, back);
            _camera.transform.position = Vector3.Lerp(bossView, _cameraOrigin, k);
            _camera.orthographicSize = Mathf.Lerp(zoomedSize, _baseSize, k);
            _hudGroup.alpha = back;
            yield return null;
        }

        _camera.transform.position = _cameraOrigin;
        _hudGroup.alpha = 1f;
        _camera.orthographicSize = _baseSize;
        _pixelCamera.enabled = true;
        SetPaused(false);
    }

    // 보스를 가운데에 두되, 확대된 화면이 아레나 밖을 비추지 않게 맞추고 그림 픽셀 격자에 붙인다
    private Vector3 BossViewPosition()
    {
        float halfHeight = _baseSize / _zoom;
        float halfWidth = halfHeight * _camera.aspect;
        float maxX = _baseSize * _camera.aspect - halfWidth;
        float maxY = _baseSize - halfHeight;
        float pixel = 1f / _pixelCamera.assetsPPU;
        float x = Mathf.Round(Mathf.Clamp(_boss.position.x, -maxX, maxX) / pixel) * pixel;
        float y = Mathf.Round(Mathf.Clamp(_boss.position.y, -maxY, maxY) / pixel) * pixel;
        return new Vector3(x, y, _cameraOrigin.z);
    }

    private void SetPaused(bool isPaused)
    {
        Time.timeScale = isPaused ? 0f : 1f;
        _feedbackManager.enabled = !isPaused;
        foreach (Behaviour behaviour in _pausedBehaviours)
        {
            behaviour.enabled = !isPaused;
        }
    }

    private static float Ramp(float time, float start, float duration)
    {
        return duration > 0f ? Mathf.Clamp01((time - start) / duration) : (time >= start ? 1f : 0f);
    }

    private static IEnumerator Fade(float duration, System.Action<float> apply)
    {
        float start = Time.unscaledTime;
        while (Time.unscaledTime - start < duration)
        {
            apply((Time.unscaledTime - start) / duration);
            yield return null;
        }
        apply(1f);
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
