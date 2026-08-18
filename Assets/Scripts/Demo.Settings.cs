public partial class Demo
{
    public void OpenSettings()
    {
        ShowSettingsScreen();
    }

    public void CloseSettings()
    {
        if (_isPeripheralConnected)
        {
            ShowInfoScreen();
            return;
        }

        ShowSearchScreen();
    }
}
