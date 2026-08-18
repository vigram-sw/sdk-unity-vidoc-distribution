using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class TopNotificationPresenter : MonoBehaviour, ITopNotification
{
    private const string CanvasName = "TopNotificationCanvas";
    private const int TopSortingOrder = short.MaxValue;

    private static TopNotificationPresenter _instance;

    private Canvas _canvas;
    private GameObject _panel;
    private Text _messageText;
    private Coroutine _hideCoroutine;

    public static TopNotificationPresenter Create()
    {
        if (_instance != null)
        {
            return _instance;
        }

        var existingObject = GameObject.Find(CanvasName);
        var existingPresenter = existingObject != null
            ? existingObject.GetComponent<TopNotificationPresenter>()
            : null;
        if (existingPresenter != null)
        {
            _instance = existingPresenter;
            return _instance;
        }

        var gameObject = existingObject ?? new GameObject(CanvasName);
        _instance = gameObject.AddComponent<TopNotificationPresenter>();
        return _instance;
    }

    public void Show(string message)
    {
        Show(message, TopNotification.DefaultDuration);
    }

    public void Show(string message, float duration)
    {
        BuildUi();

        _messageText.text = message;
        _canvas.sortingOrder = TopSortingOrder;
        transform.SetAsLastSibling();
        _panel.transform.SetAsLastSibling();
        _panel.SetActive(true);

        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }

        if (duration > 0f)
        {
            _hideCoroutine = StartCoroutine(HideAfterDelay(duration));
        }
    }

    public void Hide()
    {
        HideInternal(true);
    }

    private void HideInternal(bool stopRunningTimer)
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
        }

        if (stopRunningTimer && _hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }

        _hideCoroutine = null;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUi();
    }

    private void BuildUi()
    {
        if (_panel != null)
        {
            return;
        }

        _canvas = gameObject.GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = TopSortingOrder;

        var scaler = gameObject.GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2732f, 2048f);
        scaler.matchWidthOrHeight = 0.5f;

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        _panel = new GameObject("TopNotificationPanel");
        _panel.transform.SetParent(transform, false);

        var panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.08f, 0.5f);
        panelRect.anchorMax = new Vector2(0.92f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0f, 330f);

        var background = _panel.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.98f);
        background.raycastTarget = true;

        var shadow = _panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
        shadow.effectDistance = new Vector2(0f, -10f);

        var outline = _panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.06f, 0.18f, 0.32f, 0.35f);
        outline.effectDistance = new Vector2(3f, 3f);

        var button = _panel.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = background;
        button.onClick.AddListener(Hide);

        var textObject = new GameObject("TopNotificationText");
        textObject.transform.SetParent(_panel.transform, false);

        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(48f, 34f);
        textRect.offsetMax = new Vector2(-48f, -34f);

        _messageText = textObject.AddComponent<Text>();
        _messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _messageText.fontSize = 48;
        _messageText.resizeTextForBestFit = true;
        _messageText.resizeTextMinSize = 26;
        _messageText.resizeTextMaxSize = 54;
        _messageText.alignment = TextAnchor.MiddleCenter;
        _messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _messageText.verticalOverflow = VerticalWrapMode.Truncate;
        _messageText.color = new Color(0.03f, 0.08f, 0.14f, 1f);
        _messageText.raycastTarget = false;

        _panel.SetActive(false);
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        HideInternal(false);
    }
}
