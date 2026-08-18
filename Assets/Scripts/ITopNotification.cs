public interface ITopNotification
{
    void Show(string message);

    void Show(string message, float duration);

    void Hide();
}
