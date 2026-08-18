using UnityEngine;

public partial class Demo
{
    private void ShowScreen(DemoUiScreen screen)
    {
        _currentScreen = screen;

        SetViewActive(viewSearch, screen == DemoUiScreen.Search);
        SetViewActive(viewInfo, screen == DemoUiScreen.Info);
        SetViewActive(viewSettings, screen == DemoUiScreen.Settings);

        if (screen != DemoUiScreen.Info)
        {
            SetViewActive(viewEvent, false);
        }

        _ui?.SetScreen(screen);
    }

    private void ShowSearchScreen()
        => ShowScreen(DemoUiScreen.Search);

    private void ShowInfoScreen()
        => ShowScreen(DemoUiScreen.Info);

    private void ShowSettingsScreen()
        => ShowScreen(DemoUiScreen.Settings);

    private static void SetViewActive(GameObject view, bool isActive)
    {
        if (view != null)
        {
            view.SetActive(isActive);
        }
    }
}
