# Changelog

## [1.0.0] - 2026-07-14

**What's New**

Major SDK update introducing Android support, expanded GPS and NTRIP capabilities, and a standardized public C# API.

### Added

- Added full Android support, including Bluetooth LE communication.
- Added HTTPS support for NTRIP correction streams and mountpoint requests.
- Added NTRIP connection result callbacks with explicit error reporting.
- Added error reporting for interrupted NTRIP correction streams.
- Added `Host`, `Port`, and `MountPoint` properties to NTRIP tasks.
- Added structured GPS disconnect notifications for configuration failures, missing devices, interrupted NTRIP connections, and invalid GNSS positions.
- Added automatic NTRIP reconnection after a previously successful connection.
- Added offline authorization support and additional authentication error reasons.
- Added binary distribution using managed assemblies, Android AAR, and iOS XCFramework.

### Changed

- Standardized public methods, properties, fields, models, and enum values using uppercase/PascalCase naming.
- Added the `I` prefix to all public service interfaces.
- Replaced the old NMEA interface and separate message types with the closed
  `NmeaMessage.Gga`, `NmeaMessage.Gst`, and `NmeaMessage.Txt` hierarchy.
- Updated `Vigram.GPSService(...)` to accept `IBluetoothService` and an automatic NTRIP reconnection option.

### Breaking Changes

- Removed the legacy NTRIP `Start()` method without connection result callbacks.
- Removed the legacy NTRIP data callback without an error channel.
- Removed the legacy `Vigram.GPSService(peripheral, ntripTask)` factory.

## [0.2.1] - 2026-06-16

**What's New**

- Minor maintenance update.
- Small improvements and fixes.

## [0.2.0] - 2026-01-14

**What's New**

- Added support for a 15 Hz GNSS message rate.
- Added GPS and Galileo-only satellite configuration.
- Added structured SDK diagnostic messages.
- Added GPS service integration.
- Improved Bluetooth packet delivery and timeout handling.
- Improved device, satellite, and peripheral message parsing.
- Added required iOS Bluetooth permissions.

## [0.1.9] - 2025-06-01

**What's New**

- Added file-based SDK logging through `ILogger`.
- Added device model detection.
- Added iOS post-build configuration.
- Added file management utilities.
- Improved laser measurement configuration and validation.
- Improved authentication, Bluetooth, GPS, NTRIP, and peripheral handling.

## [0.1.8] - 2025-05-15

**What's New**

- Added viDoc authentication and verification.
- Added authentication result and error models.
- Added DMM coordinate parsing.
- Added motion data serialization.
- Added NTRIP connection persistence.
- Added buffered peripheral message delivery.
- Improved peripheral configuration state and error reporting.
- Improved GNSS, authentication, and NMEA parsers.

## [0.1.6] - 2025-03-27

**What's New**

- Added the Bluetooth peripheral model.
- Improved Bluetooth device discovery and connection handling.
- Improved NTRIP mountpoint and correction stream handling.
- Improved GPS, laser, peripheral, and authentication services.
- Updated SDK documentation.

## [0.1.5] - 2024-12-06

**What's New**

- Initial viDoc SDK release for Unity.
- Added Bluetooth device discovery and connection.
- Added peripheral communication.
- Added GNSS and NTRIP correction services.
- Added laser measurement functionality.
- Added device motion support.
- Added the initial Unity demo application and SDK documentation.
