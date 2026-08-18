public static class TopNotification
{
    internal const float DefaultDuration = 6f;

    private static ITopNotification _presenter;

    public static ITopNotification Presenter
    {
        get
        {
            if (_presenter == null)
            {
                _presenter = TopNotificationPresenter.Create();
            }

            return _presenter;
        }
        set => _presenter = value;
    }

    public static void Show(string message)
    {
        Presenter.Show(message);
    }

    public static void Show(string message, float duration)
    {
        Presenter.Show(message, duration);
    }

    public static void Hide()
    {
        _presenter?.Hide();
    }
}
