using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum DemoUiScreen
{
    Search,
    Info,
    Settings
}

public sealed class DemoUiController
{
    private const float InfoColumnGap = 48f;
    private const float InfoValueMinWidth = 420f;
    private const float TopBarHeightRatio = 0.115f;
    private const float TopBarMinHeight = 160f;
    private const float TopBarMaxHeight = 240f;
    private const float TopBarTitleCenterRatio = 0.56f;
    private const float TopBarContentPadding = 8f;
    private const float SearchContentTopPadding = 56f;
    private const float TopBarSeparatorHeight = 1f;
    private const float TopBarIconButtonSize = 144f;
    private const float TopBarIconCenterInset = 120f;
    private const float TopBarTitleHorizontalInset = 260f;
    private const float TopBarTitleHeight = 150f;
    private static readonly Color TopBarBackgroundColor = new Color(0.965f, 0.972f, 0.98f, 1f);
    private static readonly Color TopBarSeparatorColor = new Color(0.18f, 0.18f, 0.18f, 0.55f);

    private readonly string _tag;
    private readonly GameObject _viewSearch;
    private readonly GameObject _viewInfo;
    private readonly GameObject _viewSettings;
    private readonly GameObject _btnSettings;
    private readonly GameObject _btnTopDisconnect;
    private readonly GameObject _btnNtripConnect;
    private readonly GameObject _btnNtripDisconnect;
    private readonly GameObject _btnNtripReconnect;
    private readonly Text _topBarTitle;
    private readonly Text[] _deviceInfoValueTexts;
    private readonly Text[] _nmeaInfoValueTexts;
    private readonly List<InfoRow> _deviceInfoRows = new();
    private readonly List<InfoRow> _nmeaInfoRows = new();

    private RectTransform _topBarBackground;
    private RectTransform _topBarSeparator;
    private Rect _lastSafeArea;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    private struct InfoRow
    {
        public Text LabelText;
        public RectTransform LabelRect;
        public Text ValueText;
        public RectTransform ValueRect;
    }

    public DemoUiController(
        string tag,
        GameObject viewSearch,
        GameObject viewInfo,
        GameObject viewSettings,
        GameObject btnSettings,
        GameObject btnTopDisconnect,
        GameObject btnNtripConnect,
        GameObject btnNtripDisconnect,
        GameObject btnNtripReconnect,
        Text topBarTitle,
        Text[] deviceInfoValueTexts,
        Text[] nmeaInfoValueTexts)
    {
        _tag = tag;
        _viewSearch = viewSearch;
        _viewInfo = viewInfo;
        _viewSettings = viewSettings;
        _btnSettings = btnSettings;
        _btnTopDisconnect = btnTopDisconnect;
        _btnNtripConnect = btnNtripConnect;
        _btnNtripDisconnect = btnNtripDisconnect;
        _btnNtripReconnect = btnNtripReconnect;
        _topBarTitle = topBarTitle;
        _deviceInfoValueTexts = deviceInfoValueTexts ?? Array.Empty<Text>();
        _nmeaInfoValueTexts = nmeaInfoValueTexts ?? Array.Empty<Text>();
    }

    public void Initialize()
    {
        ApplyTopBarLayout();
        SetScreen(DemoUiScreen.Search);
        SetNtripControlsConnected(false);
        ConfigureInfoColumnLayouts();
    }

    public void Tick()
    {
        UpdateTopBarLayoutIfNeeded();
    }

    public void ShowSystemStatusBar()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        Screen.fullScreen = false;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                var window = activity.Call<AndroidJavaObject>("getWindow");
                var decorView = window.Call<AndroidJavaObject>("getDecorView");

                const int flagFullscreen = 1024;
                const int flagForceNotFullscreen = 2048;
                const int systemUiFlagVisible = 0;

                window.Call("clearFlags", flagFullscreen);
                window.Call("addFlags", flagForceNotFullscreen);
                decorView.Call("setSystemUiVisibility", systemUiFlagVisible);

                var version = new AndroidJavaClass("android.os.Build$VERSION");
                if (version.GetStatic<int>("SDK_INT") >= 30)
                {
                    var controller = window.Call<AndroidJavaObject>("getInsetsController");
                    if (controller != null)
                    {
                        var type = new AndroidJavaClass("android.view.WindowInsets$Type");
                        controller.Call("show", type.CallStatic<int>("statusBars"));
                    }
                }
            }));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"{_tag} Cannot show Android status bar: {exception.Message}");
        }
#endif
    }

    public void SetNtripControlsConnected(bool isConnected)
    {
        _btnNtripConnect?.SetActive(!isConnected);
        _btnNtripDisconnect?.SetActive(isConnected);
        _btnNtripReconnect?.SetActive(isConnected);
    }

    public void SetSettingsConnected(bool isConnected)
    {
        if (!isConnected)
        {
            _viewSettings?.SetActive(false);
            SetScreen(DemoUiScreen.Search);
        }
        else if (_viewInfo != null && _viewInfo.activeSelf)
        {
            SetScreen(DemoUiScreen.Info);
        }
    }

    public void SetScreen(DemoUiScreen screen)
    {
        ApplyTopBarLayout();

        if (_topBarTitle != null)
        {
            _topBarTitle.gameObject.SetActive(true);
            _topBarTitle.text = screen switch
            {
                DemoUiScreen.Info => "Info",
                DemoUiScreen.Settings => "Settings",
                _ => "Search"
            };
        }

        _btnTopDisconnect?.SetActive(screen == DemoUiScreen.Info);
        _btnSettings?.SetActive(screen == DemoUiScreen.Info || screen == DemoUiScreen.Search);
        SetActive(FindAppBarRectTransform("SettingsCloseButton"), screen == DemoUiScreen.Settings);
        _topBarBackground?.gameObject.SetActive(true);
        _topBarSeparator?.gameObject.SetActive(true);
    }

    public void OpenSettings()
    {
        _viewSettings?.SetActive(true);
        SetScreen(DemoUiScreen.Settings);
    }

    public void CloseSettings()
    {
        _viewSettings?.SetActive(false);
        SetScreen(_viewInfo != null && _viewInfo.activeSelf ? DemoUiScreen.Info : DemoUiScreen.Search);
    }

    private void ConfigureInfoColumnLayouts()
    {
        _deviceInfoRows.Clear();
        _nmeaInfoRows.Clear();

        AddInfoRows(_deviceInfoRows, _deviceInfoValueTexts);
        AddInfoRows(_nmeaInfoRows, _nmeaInfoValueTexts);
        UpdateInfoColumnLayouts();
    }

    private void AddInfoRows(List<InfoRow> rows, params Text[] valueTexts)
    {
        foreach (var valueText in valueTexts)
        {
            if (valueText == null || valueText.transform.parent == null)
            {
                continue;
            }

            var labelText = valueText.transform.parent
                .GetComponentsInChildren<Text>(includeInactive: true)
                .FirstOrDefault(text => text != valueText && text.text.Contains(":"));

            if (labelText == null)
            {
                continue;
            }

            rows.Add(new InfoRow
            {
                LabelText = labelText,
                LabelRect = labelText.rectTransform,
                ValueText = valueText,
                ValueRect = valueText.rectTransform
            });
        }
    }

    private void UpdateInfoColumnLayouts()
    {
        AlignInfoRows(_deviceInfoRows);
        AlignInfoRows(_nmeaInfoRows);
    }

    private void AlignInfoRows(List<InfoRow> rows)
    {
        if (rows.Count == 0)
        {
            return;
        }

        foreach (var row in rows)
        {
            if (row.LabelRect != null && row.LabelText != null)
            {
                var labelWidth = Mathf.Ceil(row.LabelText.preferredWidth + InfoColumnGap);
                row.LabelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, labelWidth);
            }

            if (row.ValueRect != null && row.ValueText != null)
            {
                if (row.LabelText != null)
                {
                    row.ValueText.fontSize = row.LabelText.fontSize;
                }

                row.ValueText.fontStyle = FontStyle.Normal;
                row.ValueText.horizontalOverflow = HorizontalWrapMode.Overflow;
                row.ValueText.verticalOverflow = VerticalWrapMode.Truncate;
                row.ValueRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    GetStableValueWidth(row));
            }
        }
    }

    private float GetStableValueWidth(InfoRow row)
    {
        var parentRect = row.ValueRect.parent as RectTransform;
        var labelWidth = row.LabelRect != null ? row.LabelRect.rect.width : 0f;
        var parentWidth = parentRect != null ? parentRect.rect.width : 0f;

        if (parentWidth <= 0f)
        {
            return InfoValueMinWidth;
        }

        return Mathf.Max(InfoValueMinWidth, parentWidth - labelWidth);
    }

    private void UpdateTopBarLayoutIfNeeded()
    {
        if (_lastScreenWidth == Screen.width &&
            _lastScreenHeight == Screen.height &&
            _lastSafeArea == Screen.safeArea)
        {
            return;
        }

        ApplyTopBarLayout();
        UpdateInfoColumnLayouts();
    }

    private void ApplyTopBarLayout()
    {
        var safeTopInset = GetSafeTopInset();
        var canvasHeight = GetCanvasHeight();
        var topBarHeight = Mathf.Clamp(canvasHeight * TopBarHeightRatio, TopBarMinHeight, TopBarMaxHeight);
        var topBarBottom = safeTopInset + topBarHeight;
        var centerY = -(safeTopInset + topBarHeight * TopBarTitleCenterRatio);

        EnsureTopBarElements();
        if (_topBarBackground != null)
        {
            _topBarBackground.anchorMin = new Vector2(0, 1);
            _topBarBackground.anchorMax = new Vector2(1, 1);
            _topBarBackground.pivot = new Vector2(0.5f, 1);
            _topBarBackground.anchoredPosition = Vector2.zero;
            _topBarBackground.sizeDelta = new Vector2(0, topBarBottom);
        }

        if (_topBarSeparator != null)
        {
            _topBarSeparator.anchorMin = new Vector2(0, 1);
            _topBarSeparator.anchorMax = new Vector2(1, 1);
            _topBarSeparator.pivot = new Vector2(0.5f, 0.5f);
            _topBarSeparator.anchoredPosition = new Vector2(0, -topBarBottom);
            _topBarSeparator.sizeDelta = new Vector2(0, TopBarSeparatorHeight);
            _topBarSeparator.SetAsLastSibling();
        }

        ApplyTopTitleLayout(_topBarTitle?.rectTransform, centerY);
        SetTopAnchoredPosition(_btnTopDisconnect?.GetComponent<RectTransform>(), centerY);
        ApplySettingsButtonLayout(centerY);

        ApplyTopInset(_viewSearch, topBarBottom);
        ApplySearchContentTopPadding(SearchContentTopPadding);
        ApplyTopInset(_viewInfo, topBarBottom);
        ApplyScrollContentTopPadding(_viewInfo, TopBarContentPadding);
        ApplySettingsHeaderLayout(centerY, topBarBottom);
        BringTopBarControlsToFront();

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        _lastSafeArea = Screen.safeArea;
    }

    private void EnsureTopBarElements()
    {
        if (_topBarBackground != null && _topBarSeparator != null)
        {
            return;
        }

        var parent = GetTopBarParent();
        if (parent == null)
        {
            return;
        }

        if (_topBarBackground == null)
        {
            var background = new GameObject("TopBarBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.layer = parent.gameObject.layer;
            background.transform.SetParent(parent, false);

            _topBarBackground = background.GetComponent<RectTransform>();
            var image = background.GetComponent<Image>();
            image.color = TopBarBackgroundColor;
            image.raycastTarget = true;

            if (_topBarTitle != null)
            {
                _topBarBackground.SetSiblingIndex(_topBarTitle.transform.GetSiblingIndex());
            }
        }

        if (_topBarSeparator == null)
        {
            var separator = new GameObject("TopBarSeparator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            separator.layer = parent.gameObject.layer;
            separator.transform.SetParent(parent, false);

            _topBarSeparator = separator.GetComponent<RectTransform>();
            var image = separator.GetComponent<Image>();
            image.color = TopBarSeparatorColor;
            image.raycastTarget = false;
        }
    }

    private RectTransform GetTopBarParent()
    {
        return _topBarTitle != null
            ? _topBarTitle.transform.parent as RectTransform
            : _viewInfo?.transform.parent as RectTransform;
    }

    private void ApplySettingsButtonLayout(float centerY)
    {
        var rect = _btnSettings != null ? _btnSettings.GetComponent<RectTransform>() : null;
        if (rect == null)
        {
            return;
        }

        ApplyTopIconButtonLayout(rect, centerY);
    }

    private void ApplyTopIconButtonLayout(RectTransform rect, float centerY)
    {
        if (rect == null)
        {
            return;
        }

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TopBarIconButtonSize);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, TopBarIconButtonSize);
        SetTopAnchoredPosition(rect, centerY);
    }

    private float GetSafeTopInset()
    {
        var canvas = _topBarTitle != null ? _topBarTitle.canvas : null;
        var canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;

        if (canvasRect == null || Screen.height <= 0)
        {
            return 0f;
        }

        var safeTopPixels = Mathf.Max(0f, Screen.height - Screen.safeArea.yMax);
        return safeTopPixels * canvasRect.rect.height / Screen.height;
    }

    private float GetCanvasHeight()
    {
        var canvas = _topBarTitle != null ? _topBarTitle.canvas : null;
        var canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
        return canvasRect != null ? canvasRect.rect.height : 2048f;
    }

    private void SetTopAnchoredPosition(RectTransform rect, float y)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
    }

    private void SetTopAnchoredPosition(RectTransform rect, float x, float y)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchoredPosition = new Vector2(x, y);
    }

    private void ApplyTopTitleLayout(RectTransform rect, float centerY)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, centerY);
        rect.sizeDelta = new Vector2(-TopBarTitleHorizontalInset * 2f, TopBarTitleHeight);

        var text = rect.GetComponent<Text>();
        if (text != null)
        {
            text.fontSize = 72;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
        }
    }

    private void BringTopBarControlsToFront()
    {
        _topBarSeparator?.SetAsLastSibling();
        _btnTopDisconnect?.transform.SetAsLastSibling();
        _btnSettings?.transform.SetAsLastSibling();
        FindAppBarRectTransform("SettingsCloseButton")?.SetAsLastSibling();
        _topBarTitle?.transform.SetAsLastSibling();
    }

    private void SetActive(RectTransform rect, bool isActive)
    {
        if (rect != null)
        {
            rect.gameObject.SetActive(isActive);
        }
    }

    private void ApplyTopInset(GameObject view, float topInset)
    {
        var rect = view != null ? view.GetComponent<RectTransform>() : null;
        if (rect == null)
        {
            return;
        }

        rect.offsetMax = new Vector2(rect.offsetMax.x, -topInset);
    }

    private void ApplyScrollContentTopPadding(GameObject view, float topPadding)
    {
        var scrollRect = view != null ? view.GetComponent<ScrollRect>() : null;
        var layoutGroup = scrollRect?.content?.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            return;
        }

        layoutGroup.padding.top = Mathf.RoundToInt(topPadding);
    }

    private void ApplySearchContentTopPadding(float topPadding)
    {
        var scrollRect = _viewSearch != null ? _viewSearch.GetComponent<ScrollRect>() : null;
        var content = scrollRect?.content;

        if (content == null && scrollRect?.viewport != null && scrollRect.viewport.childCount > 0)
        {
            content = scrollRect.viewport.GetChild(0) as RectTransform;
        }

        if (content == null)
        {
            return;
        }

        content.offsetMax = new Vector2(content.offsetMax.x, -Mathf.Max(0f, topPadding));
    }

    private void ApplySettingsHeaderLayout(float centerY, float contentTopInset)
    {
        if (_viewSettings == null)
        {
            return;
        }

        var closeButton = FindAppBarRectTransform("SettingsCloseButton");
        ApplyTopIconButtonLayout(closeButton, centerY);
        SetTopAnchoredPosition(closeButton, TopBarIconCenterInset, centerY);
        SetActive(FindRectTransform(_viewSettings.transform, "SettingsTitle"), false);

        var scrollRect = _viewSettings.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect?.GetComponent<RectTransform>() is RectTransform rect)
        {
            rect.offsetMax = new Vector2(rect.offsetMax.x, -contentTopInset);
        }
    }

    private RectTransform FindRectTransform(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == name)
        {
            return root as RectTransform;
        }

        foreach (Transform child in root)
        {
            var result = FindRectTransform(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private RectTransform FindAppBarRectTransform(string name)
    {
        return FindRectTransform(GetTopBarParent(), name);
    }
}
