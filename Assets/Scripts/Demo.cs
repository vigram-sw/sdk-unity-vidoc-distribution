using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ViGram;
using System.Linq;

[Serializable]
public class MountObject
{
    public GameObject btnGet;
    public GameObject parent;
    public GameObject panel;
    public GameObject btnClosePanel;
}

public partial class Demo : MonoBehaviour
{
    private static readonly string TAG = "[Demo]";

    public Text tDeviceName, tDeviceNumber, tProtocol, tSerialNumber, tHasFrontLaser, tHasBottomLaser, tHasImu, tNmeaStatus,
    tHousing, tMountDevice, tHWRev, tHWBat, tHWM88, tHWL81, tHasCalibration, tCurrentDeviceType, tHardware, tSoftware,
    tBattery, tConnectionPeripheralStatus, tConnectionDeviceStatus, tStartingDeviceStatus, tLaserState, tLaserDistance,
    tLaserQuality, tLaserQualityRAW, tNtripStatus, txtDynamicState, txtConstellationInfo, txtElevation, txtRate, tNtripSize, tCorrection,
    tLon, tLat, tAlt, tVersionSDK, evTiText, txtQuality, txtSatellite, tCurrentTime, tUTCTime, tGNSSTime, tVertAcc, tHorAcc, tLatError, tLonError,
    tNorthVel, tEastVel, tDownVel, tPDOP, tVDOP, tHDOP, tTDOP, tGDOP, tNTRIPData;

    public GameObject parent, parentNtripInfo, btnPref, btnRemoveNtripInfoPref, ntripInfoPref, viewSearch, viewInfo, viewEvent,
    viewNtripAccaunts, viewSettings;
    public GameObject btnSettings, btnTopDisconnect, btnNtripConnect, btnNtripDisconnect, btnNtripReconnect;
    public GameObject[] settingsConnectedOnlyViews;
    public Text tScanButton;
    public Text topBarTitle;

    public InputField iHost, iPort, iUser, iPsw, iMount;

    public Dropdown ddPosLaser, ddModeLaser;
    public InputField durationLaser;

    private IAuthenticationService authentication;
    private IBluetoothService bluetoothService = Vigram.BluetoothService;
    private IPeripheralService peripheralService;
    private INtripService ntripService = Vigram.NtripService;
    private INtripTaskService ntripTask;
    private IGPSService gpsService;
    private ILaserService laserService;

    private NtripConnectionInformation ntripConnection;
    private NtripMountPoint ntripMount;
    private NtripInfoHelper ntripInfoHelper = new NtripInfoHelper();
    public MountObject mountObject;

    private IDeviceService deviceService = Vigram.DeviceService();
    private DemoUiController _ui;
    private DemoUiScreen _currentScreen = DemoUiScreen.Search;
    private bool _isPeripheralConnected;
    private bool _isBluetoothScanning;
    private readonly List<string> _ntripDataHistory = new();
    private int _ntripDataCounter;

    private float uiUpdateTimer = 0;
    private const float UI_UPDATE_RATE = 0.1f; // 10 FPS
    private const int NtripDataVisibleRows = 10;
    private const float NtripDataLineHeight = 62f;

    void Start()
    {

        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        _ui = CreateUiController();
        _ui.ShowSystemStatusBar();

        InitializeStartupUi();

        Configuration.Debug = true;
        Debug.Log($"{TAG} Start SDK {Configuration.SdkVersion}");
        Debug.Log($"{TAG} Auth init start package={Application.identifier}");

        try
        {
        authentication = Vigram.Initial(token: "TOKEN");
        }
        catch (Exception exception)
        {
            ShowStartupError("Invalid SDK token.", exception);
            return;
        }

        Debug.Log($"{TAG} Auth init success");

        Debug.Log($"{TAG} Auth check start");
        try
        {
            authentication.Check(result =>
            {
                switch (result)
                {
                    case AuthenticationResult.Success success:
                        {
                            Debug.Log($"{TAG} Auth success");

                            bluetoothService.ObserveAvailableDevices((peripherals) =>
                            {
                                Debug.Log($"{TAG} Devices observed {peripherals.Count()}");
                                GameObject[] players = GameObject.FindGameObjectsWithTag("Peripheral");

                                foreach (GameObject gameObject in players)
                                {
                                    Destroy(gameObject);
                                }

                                foreach (var per in peripherals)
                                {
                                    Debug.Log($"{TAG} Device {per.Name}");
                                    GameObject button = Instantiate(btnPref, parent.transform);
                                    button.GetComponentInChildren<Text>().text = per.Name;
                                    button.GetComponent<Button>().onClick.AddListener(() =>
                                {
                                    Connect(per);
                                });
                                }
                            });

                            bluetoothService.State(state =>
                            {
                                Debug.Log($"{TAG} Bluetooth state {state}");
                                if (_isPeripheralConnected)
                                {
                                    tConnectionDeviceStatus.text = state.ToString();
                                }

                                if (state != CBManagerState.CBManagerStatePoweredOn)
                                {
                                    SetBluetoothScanState(false);
                                }
                            });

                            break;
                        }
                        ;
                    case AuthenticationResult.Error error:
                        {
                            ShowAuthenticationError(error);
                            break;
                        }
                        ;
                }
            });
        }
        catch (Exception exception)
        {
            ShowStartupError("SDK authentication failed.", exception);
            return;
        }

        ntripInfoHelper.GetAll((listNtripInfo) =>
        {
            if (listNtripInfo.Count > 0)
            {
                viewNtripAccaunts.SetActive(true);
            }
            else
            {
                viewNtripAccaunts.SetActive(false);
            }

            foreach (Button button in parentNtripInfo.GetComponentsInChildren<Button>())
            {
                Destroy(button.gameObject);
            }

            listNtripInfo.ForEach((ntripInfo) =>
        {
            LaunchButtonNtripInfo(ntripInfo);
        });
        });
    }

    private void InitializeStartupUi()
    {
        durationLaser.text = "5";

        _ui.Initialize();
        InitializeMountsPanel();
        ShowSearchScreen();
        SetSettingsDeviceToolsVisible(false);
        SetBluetoothScanState(false);

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        tVersionSDK.text = Configuration.SdkVersion;
    }

    private void ShowAuthenticationError(AuthenticationResult.Error error)
    {
        Debug.LogError($"{TAG} SDK authentication failed: {error.Enum}");
        TopNotification.Show("SDK authentication failed. Check the SDK token.", 12f);
    }

    private void ShowStartupError(string userMessage, Exception exception)
    {
        Debug.LogError($"{TAG} {userMessage}");
        TopNotification.Show(userMessage, 12f);
    }

    private DemoUiController CreateUiController()
    {
        return new DemoUiController(
            TAG,
            viewSearch,
            viewInfo,
            viewSettings,
            btnSettings,
            btnTopDisconnect,
            btnNtripConnect,
            btnNtripDisconnect,
            btnNtripReconnect,
            topBarTitle,
            new[]
            {
                tVersionSDK, tDeviceName, tProtocol, tSerialNumber, tDeviceNumber,
                tHasFrontLaser, tHasBottomLaser, tHasImu, tHasCalibration,
                tHousing, tMountDevice, tCurrentDeviceType, tHardware, tSoftware,
                tBattery, tConnectionPeripheralStatus, tConnectionDeviceStatus,
                tStartingDeviceStatus, tHWRev, tHWBat, tHWM88, tHWL81
            },
            new[]
            {
                tNmeaStatus, tCurrentTime, tUTCTime, tGNSSTime, tLat, tLon, tAlt,
                txtQuality, txtSatellite, tVertAcc, tHorAcc, tLatError, tLonError,
                tNorthVel, tEastVel, tDownVel, tPDOP, tVDOP, tHDOP, tTDOP, tGDOP,
                tCorrection
            });
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            _ui?.ShowSystemStatusBar();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            _ui?.ShowSystemStatusBar();
        }
    }

    void Update()
    {
        uiUpdateTimer += Time.deltaTime;
        if (uiUpdateTimer < UI_UPDATE_RATE) return;
        uiUpdateTimer = 0f;

        if (_isPeripheralConnected)
        {
            tCurrentTime.text = string.Format("{0:HH:mm:ss.fff}", DateTime.Now);
            tUTCTime.text = string.Format("{0:HH:mm:ss.fff}", DateTime.UtcNow);
        }

        _ui?.Tick();
    }

}
