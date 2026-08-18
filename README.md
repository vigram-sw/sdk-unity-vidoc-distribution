# viDoc SDK for Unity

viDoc SDK for Unity connects Unity applications to viDoc devices over Bluetooth and provides GNSS, NTRIP corrections and laser measurement APIs.

## Version

| Field | Value |
| --- | --- |
| SDK version | `1.0.0` |
| Unity package | `com.vigram.sdk` |
| Minimum Unity version | `2021.3` |
| Platforms | `iOS`, `Android` |
| Package path | `Packages/com.vigram.sdk` |
| Public repository | `https://github.com/vigram-sw/sdk-unity-vidoc-distribution` |

Use the immutable `1.0.0` tag for production projects.

## Installation

The package is installed through Unity Package Manager using a Git URL with the package path.

### Install Release

1. Open your Unity project.
2. Go to `Window -> Package Manager`.
3. Press `+`.
4. Select `Add package from git URL...`.
5. Paste the release URL:

```text
https://github.com/vigram-sw/sdk-unity-vidoc-distribution.git?path=/Packages/com.vigram.sdk#1.0.0
```

6. Press `Add`.

Unity will add the package to `Packages/manifest.json`.

### Install Through manifest.json

You can also add the package manually to your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.vigram.sdk": "https://github.com/vigram-sw/sdk-unity-vidoc-distribution.git?path=/Packages/com.vigram.sdk#1.0.0"
  }
}
```

Keep the rest of your existing dependencies in the same `dependencies` object.

### Install Local Package

For local SDK development:

1. Open `Window -> Package Manager`.
2. Press `+`.
3. Select `Add package from disk...`.
4. Select:

```text
Packages/com.vigram.sdk/package.json
```

This links the package from the local checkout, so changes in the package files are visible in the Unity project.

## Updating

To update from one release to another, change the release tag in `Packages/manifest.json`:

```json
"com.vigram.sdk": "https://github.com/vigram-sw/sdk-unity-vidoc-distribution.git?path=/Packages/com.vigram.sdk#1.0.0"
```

After changing the tag, Unity will resolve the package again. If Unity keeps an old cached version, remove the package from Package Manager and add it again with the new release URL.

## Package Contents

The SDK package includes:

- Bluetooth connection service
- viDoc peripheral service
- GNSS and GPS service
- NTRIP service
- Laser measurement service
- Device motion service
- iOS native plugins
- Android native plugins

## Documentation

API documentation is prepared for the `1.0.0` release. Public examples are available in the demo project.
