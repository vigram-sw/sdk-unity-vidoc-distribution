# viDoc Unity Demo

This project is the runnable demo for the public `com.vigram.sdk` package version `1.0.0`.

Open it with Unity `6000.5.2f1` or a compatible Unity version. The demo uses only public SDK APIs.

The package is resolved from the immutable public `1.0.0` tag through `Packages/manifest.json`.

## Features

- Scan for viDoc devices over Bluetooth LE.
- Connect to a selected device and show device, GNSS and laser data.
- Configure NTRIP credentials, list mount points and stream corrections.
- Observe NTRIP state, data errors, PDOP/VDOP values, GPS disconnect signals and reconnect behavior.

## SDK Token

For public builds, replace the `TOKEN` placeholder in `Assets/Scripts/Demo.cs` with a valid SDK token.

## Documentation

For more information, view the documentation on the [web](https://vigram-sw.github.io/sdk-unity-vidoc-distribution/).
