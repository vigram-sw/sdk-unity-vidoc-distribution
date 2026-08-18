using System;
using UnityEngine;
using ViGram;
using static ViGram.SatelliteMessage;

public partial class Demo
{
    public void LaserOn()
    {
        var position = ddPosLaser.value == 0 ? LaserConfiguration.Position.Front : LaserConfiguration.Position.Bottom;
        laserService.TurnLaserOn(position);
    }

    public void LaserOff()
    {
        laserService.TurnLaserOff(position: LaserConfiguration.Position.Bottom);
    }

    public void LaserState()
    {
        laserService.GetLasersStatus(action: (result) =>
        {
            tLaserState.text = result.State.ToString();
        });
    }

    public void LaserStart()
    {
        var config = new LaserConfiguration(
            shotMode: ddModeLaser.value == 0 ? LaserConfiguration.ShotMode.Fast :
            ddModeLaser.value == 1 ? LaserConfiguration.ShotMode.Slow : LaserConfiguration.ShotMode.Auto,
            int.Parse(durationLaser.text),
            position: ddPosLaser.value == 0 ? LaserConfiguration.Position.Front : LaserConfiguration.Position.Bottom
            );

        laserService.Record(config, actionSuccess =>
        {
            tLaserDistance.text = actionSuccess.Distance.ToString();
            tLaserQuality.text = actionSuccess.Quality.ToString();
            tLaserQualityRAW.text = actionSuccess.QualityRaw.ToString();
        }, actionFailure =>
        {
            var message = string.IsNullOrWhiteSpace(actionFailure.Message)
                ? "Laser recording failed."
                : actionFailure.Message;

            Debug.LogError($"{TAG} Laser record failed: {actionFailure}");
            TopNotification.Show(message);
        });
    }

    public void RequestBattery()
    {
        peripheralService.RequestBattery(action: battery =>
        {
            tBattery.text = $"{battery.Percentage}%";
        });
    }

    public void RequestVersion()
    {
        peripheralService.RequestVersion(action: version =>
        {
            var s = version.Software;
            var h = version.Hardware;

            tSoftware.text = s.Major + "." + s.Minor + "." + s.Patch + "." + s.Build;
            tHardware.text = h.Major + "." + h.Major;
        });
    }

    private void MessageGga(NmeaMessage.Gga gga)
    {
        ggaMessage = gga;

        tLon.text = (gga.Coordinate?.Longitude)?.ToString("F10") ?? "0.0";
        tLat.text = (gga.Coordinate?.Latitude)?.ToString("F10") ?? "0.0";
        tAlt.text = (gga.Coordinate?.Altitude)?.ToString("F6") ?? "0.0";
        tCorrection.text = gga.CorrectionAge.ToString() ?? "-";
        txtQuality.text = gga.Quality.Value.ToString() ?? "-";

        tNmeaStatus.text =
            (gga.Location?.Latitude != null && gga.Location?.Longitude != null && gga.Location?.Latitude != 0 && gga.Location?.Longitude != 0)
                ? "+" : "-";

        switch (gga.Quality)
        {
            case GPSQualityIndicator.InvalidFix:
                txtQuality.text = "Fix not valid"; break;

            case GPSQualityIndicator.SinglePoint:
                txtQuality.text = "GPS fix"; break;

            case GPSQualityIndicator.PseudoRangeDifferential:
                txtQuality.text = "Differential GPS fix (DGNSS)"; break;

            case GPSQualityIndicator.NotApplicable:
                txtQuality.text = "Not applicable"; break;

            case GPSQualityIndicator.RtkFixedAmbiguitySolution:
                txtQuality.text = "RTK Fixed"; break;

            case GPSQualityIndicator.RtkFloatingAmbiguitySolution:
                txtQuality.text = "RTK Float"; break;

            case GPSQualityIndicator.IsDeadReckoning:
                txtQuality.text = "ISN Dead reckoning"; break;

            case GPSQualityIndicator.ManualInput:
                txtQuality.text = "Manual input"; break;
        }

        tGNSSTime.text = string.Format("{0:HH:mm:ss.fff}", gga.Time);
    }

    private void MessageGst(NmeaMessage.Gst gst)
    {
        tVertAcc.text = gst.Accuracy.Vertical.ToString();
        tHorAcc.text = gst.Accuracy.Horizontal.ToString();

        tLatError.text = gst.LatitudeError.ToString();
        tLonError.text = gst.LongitudeError.ToString();
    }

    private void MessageTxt(NmeaMessage.Txt txt) { }

    private void OnError(string message)
    {
        tConnectionDeviceStatus.text = "-";
        ClearNtripDataHistory();
    }

    public void GetDynamicState()
    {
        peripheralService.GetCurrentDynamicState(action: (DynamicState type) =>
        {
            txtDynamicState.text = "Dynamic state: " + type.Value;
        });
    }

    public void SetDynamicState(int id)
    {
        peripheralService.SetDynamicState(id == 0 ? DynamicStateType.Pedestrian : DynamicStateType.Stationary);
    }

    public void ChangeStatusNavdop(bool activate)
    {
        peripheralService.ChangeStatusNAVDOP(activate);
    }

    public void ChangeStatusNavpvt(bool activate)
    {
        peripheralService.ChangeStatusNAVPVT(activate);
    }

    public void ActivateAllConstellationGnss()
    {
        peripheralService.ActivateAllConstellationGNSS();
    }

    public void GetStatusInfoGnss(string name)
    {
        peripheralService.GetCurrentStatusGNSS(NavigationSystemValue.GetFromString(name), sattelite =>
        {
            txtConstellationInfo.text = "Satellite: " + sattelite.Value + " " + "Status: " + sattelite.IsEnabled;
        });
    }

    public void changeStatusGNSS_Enabled(string name)
    {
        peripheralService.ChangeStatusGNSS(NavigationSystemValue.GetFromString(name), true);
    }

    public void changeStatusGNSS_Disabled(string name)
    {
        peripheralService.ChangeStatusGNSS(NavigationSystemValue.GetFromString(name), false);
    }

    public void GetCurrentElevation()
    {
        peripheralService.GetCurrentMinimumElevation((elevation) =>
        {
            txtElevation.text = "Current minimum elevation: " + elevation.Value + "°";
        });
    }

    public void SetElevation(int id)
    {
        peripheralService.SetMinimumElevation(angle: ElevationValue.GetFromInt(id));
    }

    public void GetChangingRateOfMessages()
    {
        peripheralService.GetChangingRateOfMessages(action: (ChangingRate changingRate) =>
        {
            txtRate.text = "Current rate: " + changingRate.Value;
        });
    }

    public void SetChangingRateOfMessages(int id)
    {

        if (RateValue.GetFromInt(id) is RateValue.Rate rate)
        {
            peripheralService.SetChangingRateOfMessages(rate);
        }
    }
}
