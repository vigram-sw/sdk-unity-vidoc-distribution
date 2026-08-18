using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using ViGram;
using static ViGram.SatelliteMessage;

public partial class Demo
{
    private void ReleasePeripheralService(IPeripheralService service)
    {
        if (service == null)
        {
            return;
        }

        service.State = null;
        service.ConfigurationState = null;
        service.DeviceMessages = null;
        service.SatelliteMessages = null;
        service.Nmea = null;
        service.ProtocolVersion = null;
        service.CurrentDevice = null;

        if (ReferenceEquals(peripheralService, service))
        {
            peripheralService = null;
        }
    }

    private void Connect(BPeripheral per)
    {
        var logFile = FileWorker.CreateFile(fileExtension: "txt", folder: "LOG", subfolderName: per.Name);
        var logger = Vigram.LoggerService(logFile);

        ReleasePeripheralService(peripheralService);

        var service = Vigram.PeripheralService(
            peripheral: per,
            logger: logger);
        peripheralService = service;

        bluetoothService.Connect(service.Peripheral);
        SetBluetoothScanState(false);

        service.Start();

        ShowInfoScreen();

        service.State = state =>
        {
            Debug.Log($"{TAG} Peripheral state {state}");

            switch (state)
            {
                case StatePeripheral.Connected:
                    _isPeripheralConnected = true;
                    laserService = Vigram.LaserService(service);
                    _ui.SetSettingsConnected(true);
                    SetSettingsDeviceToolsVisible(false);

                    tVersionSDK.text = Configuration.SdkVersion;
                    tConnectionPeripheralStatus.text = "+";

                    tDeviceName.text = service.Peripheral.Name;

                    service.ProtocolVersion = version =>
                    {
                        tProtocol.text = version.ToString();
                    };

                    service.CurrentDevice = device =>
                    {
                        tDeviceNumber.text = device.RawValue;
                        tHasBottomLaser.text = device.HasBottomLaser ? "+" : "-";
                        tHasFrontLaser.text = device.HasFrontLaser ? "+" : "-";
                        tHasImu.text = device.HasIMU ? "+" : "-";
                        tHousing.text = device.GetHousing.ToString();
                        tMountDevice.text = device.GetMount.ToString();
                        tHWRev.text = device.GetHardwareRevision.Value.VigramRef;
                        tHWBat.text = device.GetHardwareRevision.Value.VigramBat;
                        tHWM88.text = device.GetHardwareRevision.Value.M88Laser;
                        tHWL81.text = device.GetHardwareRevision.Value.L81Laser;
                        tHasCalibration.text = device.HasCalibrated ? "+" : "-";
                        tCurrentDeviceType.text = device.TypeOfDevice.ToString();
                    };

                    tSerialNumber.text = service.SerialNumber;

                    PeripheralConfiguration(service);
                    break;

                case StatePeripheral.Disconnected:
                    if (!IsSearchVisible())
                    {
                        ShowSearchAfterDisconnect();
                    }

                    ReleasePeripheralService(service);
                    break;
            }
        };
    }

    public void DisconnectDevice()
    {
        Debug.Log($"{TAG} Disconnect device");

        ntripTask?.Disconnect();
        ntripTask = null;
        gpsService = null;
        laserService = null;

        ShowSearchAfterDisconnect();

        ReleasePeripheralService(peripheralService);
        bluetoothService.Disconnect();
    }

    public void StartBluetoothScan()
    {
        if (_isBluetoothScanning)
        {
            return;
        }

        Debug.Log($"{TAG} BLE start scan");
        bluetoothService.StartScan();
        SetBluetoothScanState(true);
    }

    public void StopBluetoothScan()
    {
        if (!_isBluetoothScanning)
        {
            return;
        }

        Debug.Log($"{TAG} BLE stop scan");
        bluetoothService.StopScan();
        SetBluetoothScanState(false);
    }

    public void ToggleBluetoothScan()
    {
        if (_isBluetoothScanning)
        {
            StopBluetoothScan();
        }
        else
        {
            StartBluetoothScan();
        }
    }

    private void SetBluetoothScanState(bool isScanning)
    {
        _isBluetoothScanning = isScanning;
        UpdateSearchScanButton();
    }

    private void UpdateSearchScanButton()
    {
        if (tScanButton != null)
        {
            tScanButton.text = _isBluetoothScanning ? "Stop scan" : "Scan";
        }
    }

    private void ShowSearchAfterDisconnect()
    {
        _isPeripheralConnected = false;
        ResetInformationData();
        ClearNtripDataHistory();
        SetSettingsDeviceToolsVisible(false);
        _ui.SetSettingsConnected(false);
        _ui.SetNtripControlsConnected(false);

        ClearSearchDeviceButtons();

        ShowSearchScreen();
        SetBluetoothScanState(false);
    }

    private void SetSettingsDeviceToolsVisible(bool isVisible)
    {
        if (settingsConnectedOnlyViews == null)
        {
            return;
        }

        foreach (var view in settingsConnectedOnlyViews)
        {
            view?.SetActive(isVisible);
        }
    }

    private void ClearSearchDeviceButtons()
    {
        foreach (Button button in parent.GetComponentsInChildren<Button>())
        {
            if (!button.CompareTag("Peripheral"))
            {
                continue;
            }

            Destroy(button.gameObject);
        }
    }

    private bool IsSearchVisible()
    {
        return viewSearch != null &&
            viewSearch.activeSelf &&
            (viewInfo == null || !viewInfo.activeSelf);
    }

    private void ResetInformationData()
    {
        ggaMessage = null;

        SetTextValues("-",
            tVersionSDK, tDeviceName, tDeviceNumber, tProtocol, tSerialNumber,
            tHasFrontLaser, tHasBottomLaser, tHasImu, tNmeaStatus,
            tHousing, tMountDevice, tHWRev, tHWBat, tHWM88, tHWL81,
            tHasCalibration, tCurrentDeviceType, tHardware, tSoftware,
            tBattery, tConnectionPeripheralStatus, tConnectionDeviceStatus,
            tStartingDeviceStatus, tLaserState, tLaserDistance, tLaserQuality,
            tLaserQualityRAW, tNtripStatus, txtDynamicState, txtConstellationInfo,
            txtElevation, txtRate, tNtripSize, tCorrection, tLon, tLat, tAlt,
            evTiText, txtQuality, txtSatellite, tCurrentTime, tUTCTime,
            tGNSSTime, tVertAcc, tHorAcc, tLatError, tLonError, tNorthVel,
            tEastVel, tDownVel, tPDOP, tVDOP, tHDOP, tTDOP, tGDOP);
    }

    private void SetTextValues(string value, params Text[] texts)
    {
        foreach (var text in texts)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }

    private IEnumerator HideViewInfoAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        viewInfo.SetActive(false);
    }

    private void PeripheralConfiguration(IPeripheralService service)
    {
        service.ConfigurationState = state =>
        {
            Debug.Log($"{TAG} Configuration state {state.GetType().Name}: {DescribeConfigurationState(state)}");

            switch (state)
            {
                case StatePeripheralConfiguration.Done:

                    viewEvent.SetActive(false);
                    SetSettingsDeviceToolsVisible(true);
                    break;
                case StatePeripheralConfiguration.InProgress progress:
                    evTiText.text = progress.Message;
                    viewEvent.SetActive(true);
                    break;
                case StatePeripheralConfiguration.Failed:
                case StatePeripheralConfiguration.Error:

                    bluetoothService.Disconnect();
                    SetSettingsDeviceToolsVisible(false);

                    StartCoroutine(HideViewInfoAfterDelay(30f));
                    viewSearch.SetActive(true);
                    viewEvent.SetActive(false);
                    break;
            }
        };

        service.DeviceMessages = message =>
        {
            switch (message)
            {
                case DeviceMessage.Battery battery:
                    tBattery.text = $"{battery.Percentage}%";
                    break;
                case DeviceMessage.Version version:
                    tHardware.text = version.Hardware.ToString();
                    tSoftware.text = version.Software.ToString();
                    break;
                case DeviceMessage.SerialNumber serialNumber:
                    var device = serialNumber;
                    break;
                case DeviceMessage.ImuAngle:
                    break;
                case DeviceMessage.ImuACC:
                    break;
                case DeviceMessage.ImuRotation:
                    break;
                case DeviceMessage.ImuRotationRaw:
                    break;
                case DeviceMessage.ImuMagneticRaw:
                    break;
                case DeviceMessage.ImuTemp:
                    break;
                case DeviceMessage.ImuCalibrationStatus:
                    break;
                case DeviceMessage.LaserState state:
                    tLaserState.text = state.State.ToString();
                    break;
                case DeviceMessage.Measurement:
                    break;
            }

        };

        service.SatelliteMessages = message =>
        {
            switch (message)
            {
                case SatelliteMessage.Dop dop:
                    tPDOP.text = dop.PositionDop.ToString();
                    tVDOP.text = dop.VerticalDop.ToString();
                    tHDOP.text = dop.HorizontalDop.ToString();
                    tTDOP.text = dop.TimeDop.ToString();
                    tGDOP.text = dop.GeometricDop.ToString();
                    break;
                case SatelliteMessage.Pvt pvt:
                    txtSatellite.text = pvt.SatelliteCount.ToString();

                    tNorthVel.text = pvt.NorthVelocity.ToString();
                    tEastVel.text = pvt.EastVelocity.ToString();
                    tDownVel.text = pvt.DownVelocity.ToString();
                    break;
            }
        };

        service.Nmea = nmea =>
        {
            switch (nmea)
            {
                case NmeaMessage.Gga gga:
                    MessageGga(gga);
                    break;
                case NmeaMessage.Gst gst:
                    MessageGst(gst);
                    break;
                case NmeaMessage.Txt txt:
                    MessageTxt(txt);
                    break;
            }
            ;
        };
    }

    private string DescribeConfigurationState(StatePeripheralConfiguration state)
    {
        return state switch
        {
            StatePeripheralConfiguration.InProgress progress => progress.Message,
            StatePeripheralConfiguration.Done => "Done",
            StatePeripheralConfiguration.Failed failed => failed.Value?.Message ?? "Failed",
            StatePeripheralConfiguration.Error error => error.Value?.Message ?? "Error",
            _ => state.ToString()
        };
    }
}
