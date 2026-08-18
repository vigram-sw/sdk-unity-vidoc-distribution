using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ViGram;
using static ViGram.SatelliteMessage;

public partial class Demo
{
    private NmeaMessage.Gga? ggaMessage;

    private void LaunchButtonNtripInfo(NtripInfo ntripInfo)
    {
        GameObject buttonPrefab = Instantiate(ntripInfoPref, parentNtripInfo.transform);

        var btnNtripInfo = buttonPrefab.GetComponent<Button>();
        var txtNtripInfo = buttonPrefab.GetComponentInChildren<Text>();
        var btnremoveNtripInvo = buttonPrefab.transform.GetChild(1).GetComponent<Button>();

        var ntrip = ntripInfo.NtripConfig;
        var mount = ntripInfo.Mount;

        txtNtripInfo.text = $"{ntrip.Hostname} {ntrip.Username} / {mount}";

        btnNtripInfo.onClick.AddListener(() =>
        {
            iHost.text = ntrip.Hostname;
            iPort.text = ntrip.Port.ToString();
            iUser.text = ntrip.Username;
            iPsw.text = ntrip.Password;
            iMount.text = mount;
        });

        btnremoveNtripInvo.onClick.AddListener(() =>
        {
            ntripInfoHelper.Remove(ntripInfo);
            Destroy(buttonPrefab);
        });

    }

    private void InitializeMountsPanel()
    {
        mountObject.btnClosePanel
            .GetComponent<Button>()
            .onClick.AddListener(CloseMountsPanel);
    }

    private void CloseMountsPanel()
    {
        mountObject.panel.SetActive(false);
    }

    public void GetMounts()
    {
        mountObject.panel.SetActive(true);

        ntripConnection = new NtripConnectionInformation(
            iHost.text, int.Parse(iPort.text), iUser.text, iPsw.text
        );

        ntripService.Mountpoints(ntripConnection,
        actionSuccess =>
            {
                Debug.Log($"{TAG} NTRIP mounts received count={actionSuccess.Count}");
                actionSuccess.ForEach(ntripMountPoint =>
                {
                    LaunchMountButton(ntripMountPoint);
                });

            }, actionFailure =>
            {
                Debug.LogError($"{TAG} NTRIP mounts failed: {actionFailure}");
                TopNotification.Show(actionFailure.Message);

                mountObject.panel.SetActive(false);
            });
    }

    private void LaunchMountButton(NtripMountPoint ntripMountPoint)
    {
        GameObject buttonPrefab = Instantiate(btnPref, mountObject.parent.transform);
        buttonPrefab.GetComponentInChildren<Text>().text = ntripMountPoint.Name;
        buttonPrefab.GetComponent<Button>().onClick.AddListener(() =>
        {
            iMount.text = ntripMountPoint.Name;

            foreach (Transform child in mountObject.parent.transform)
            {
                Destroy(child.gameObject);
                mountObject.panel.SetActive(false);
            }
        });
    }

    public void NtripDisconnect()
    {
        ntripTask?.Disconnect();
        tNtripStatus.text = NtripState.DISCONNECTED.Description();
        ClearNtripDataHistory();
        _ui.SetNtripControlsConnected(false);
    }

    public void NtripReconnect()
    {
        ClearNtripDataHistory();

        if (gpsService == null)
        {
            TopNotification.Show("NTRIP is not connected");
            return;
        }

        gpsService.Reconnect();
    }

    public void NtripConnect()
    {
        ClearNtripDataHistory();
        Debug.Log($"{TAG} NTRIP connect requested");

        if (ggaMessage == null)
        {
            tNtripStatus.text = "GGA is not available";
            _ui.SetNtripControlsConnected(false);
            return;
        }

        ntripConnection = new NtripConnectionInformation(
        iHost.text, int.Parse(iPort.text), iUser.text, iPsw.text);

        ntripInfoHelper.Add(ntripConnection, iMount.text);
        Debug.Log($"{TAG} NTRIP start host={iHost.text} port={iPort.text} mount={iMount.text}");

        ntripTask = ntripService.Task(
            ntripConnection,
            iMount.text,
            (NmeaMessage.Gga)ggaMessage);

        ntripTask.Data((data, error) =>
        {
            if (error != null)
            {
                Debug.LogError($"{TAG} NTRIP data error: {error}");
                TopNotification.Show(error.Message);
                return;
            }

            if (data == null)
            {
                return;
            }

            var previewLength = Math.Min(data.Length, 16);
            var preview = BitConverter.ToString(data, 0, previewLength);
            var messageType = data.Length >= 5 && data[0] == 0xD3
                ? (data[3] << 4) | (data[4] >> 4)
                : -1;
            var text = "";

            if (messageType == 1029 &&
                data.Length >= 12 &&
                data.Length >= 12 + data[11])
            {
                text = Encoding.UTF8.GetString(data, 12, data[11])
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n");
            }

            Debug.Log($"{TAG} NTRIP data bytes={data.Length} type={messageType} text={text} hex={preview}");
            AddNtripDataHistoryEntry(data.Length);
        });

        gpsService = Vigram.GPSService(
            peripheralService,
            bluetoothService,
            ntripTask: ntripTask);

        gpsService.ShouldDisconnect(HandleGpsDisconnect);

        gpsService.Start(obj =>
        {
            Debug.Log($"{TAG} NTRIP state {obj}: {obj.Description()}");
            tNtripStatus.text = obj.Description();
            _ui.SetNtripControlsConnected(obj == NtripState.CONNECTED);
        });

        gpsService.Coordinate(coordinate => { });

        gpsService.Hdop(hdop => tHDOP.text = hdop?.ToString("F2") ?? "-");

        gpsService.HorizontalAccuracy(value => { });

        gpsService.Quality((GPSQualityIndicator? value) => { });

        gpsService.TimestampedCoordinate((dateTime, coordinate) => { });

        gpsService.VerticalAccuracy(value => { });
    }

    private void HandleGpsDisconnect(GPSDisconnectSignal signal)
    {
        var message = $"GPS disconnected: {signal.DisconnectReason}";

        tNtripStatus.text = message;
        _ui.SetNtripControlsConnected(false);
        TopNotification.Show(message);
    }

    private void ClearNtripDataHistory()
    {
        _ntripDataHistory.Clear();
        _ntripDataCounter = 0;
        UpdateNtripDataHistoryText();
    }

    private void AddNtripDataHistoryEntry(int bytesCount)
    {
        _ntripDataCounter++;
        var time = DateTime.Now.ToString("HH:mm:ss");
        _ntripDataHistory.Add($"#{_ntripDataCounter} {time} {bytesCount} byte");
        UpdateNtripDataHistoryText();
    }

    private void UpdateNtripDataHistoryText()
    {
        if (tNTRIPData == null)
        {
            return;
        }

        tNTRIPData.text = string.Join("\n", _ntripDataHistory);

        var rows = Mathf.Max(NtripDataVisibleRows, _ntripDataHistory.Count);
        tNTRIPData.rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            rows * NtripDataLineHeight);

        Canvas.ForceUpdateCanvases();

        var scrollRect = tNTRIPData.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
