# viDoc SDK for Unity

`com.vigram.sdk` connects Unity applications to viDoc devices for GNSS, NTRIP corrections and laser measurements.

## Installation

In Unity Package Manager, choose **Add package from git URL** and provide the repository URL with this package path:

```text
https://github.com/vigram-sw/sdk-unity-vidoc-distribution.git?path=/Packages/com.vigram.sdk#<version>
```

Replace `<version>` with the required stable SDK tag, for example `1.0.0`.

## GPS and NTRIP

Create the GPS service with the same `IBluetoothService` that connected the peripheral. This enables device-disconnect monitoring. `automaticallyReconnectNtrip` defaults to `true`; it retries transient NTRIP transport failures only after one successful NTRIP connection.

```csharp
var ntripTask = Vigram.NtripService.Task(connection, mountPoint, latestGga);

var gps = Vigram.GPSService(
    peripheralService,
    bluetoothService,
    ntripTask,
    automaticallyReconnectNtrip: true);

gps.ShouldDisconnect(signal =>
{
    Debug.Log($"GPS disconnect: {signal.DisconnectReason}");
});

gps.Start(state => Debug.Log(state.Description()));
```

`ShouldDisconnect` reports `ConfigurationFailed`, `DeviceNotFound`,
`NtripDisrupted`, and `InvalidGnssPosition`.

## NTRIP connection results and data errors

Register callbacks before starting the task.

```csharp
ntripTask.Data((bytes, error) =>
{
    if (error != null)
    {
        Debug.LogError(error.Message);
        return;
    }

    Debug.Log($"Received {bytes.Length} correction bytes");
});

ntripTask.Start(
    actionSuccess: () => Debug.Log("NTRIP handshake succeeded"),
    actionFailure: error => Debug.LogError(error.Message));
```

## HTTPS NTRIP

Enable TLS independently for correction data and mountpoint discovery.

```csharp
var connection = new NtripConnectionInformation(
    hostname,
    port,
    username,
    password,
    forceHttpsConnection: true,
    forceHttpsMountpointsConnection: true);
```

The task exposes its `Host`, `Port` and `MountPoint` for diagnostics without exposing credentials.
