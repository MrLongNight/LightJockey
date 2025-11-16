# Task 14: Security Concept - Configuration Encryption

## 1. Overview

This document outlines the security concept for protecting sensitive configuration data in the LightJockey application, specifically focusing on the encryption of the `AppKey` and other sensitive information.

## 2. Threat Model

- **Threat**: Unauthorized access to sensitive configuration data stored on the user's machine.
- **Sensitive Data**:
  - Philips Hue Bridge `AppKey`
  - User preferences that might be considered private.
- **Goal**: Protect this data from being read by unauthorized users or malicious software.

## 3. Implementation

### 3.1. Encryption Method: DPAPI

The `ConfigurationService` uses the **Windows Data Protection API (DPAPI)** to encrypt and decrypt sensitive configuration data. DPAPI is a secure and recommended way to store secrets on Windows, as it ties the encryption to the user's Windows account.

- **Encryption Scope**: `DataProtectionScope.CurrentUser`
  - This ensures that only the user who encrypted the data can decrypt it.
  - The encryption key is managed by the Windows operating system and is not stored in the application.

### 3.2. `ConfigurationService`

The `IConfigurationService` interface provides a simple API for securely storing, retrieving, and managing configuration values. The implementation, `ConfigurationService`, handles all encryption and decryption operations transparently.

- **Storage Location**:
  - Encrypted data is stored in a file named `secure-config.dat` in the `%APPDATA%\LightJockey` directory.
  - The data is serialized as JSON, with the encrypted values being Base64-encoded strings.

### 3.3. Integration with `HueService`

The `HueService` is now integrated with the `ConfigurationService` to securely manage the Philips Hue Bridge `AppKey`.

- **Storing the `AppKey`**:
  - After a successful registration with a new Hue Bridge, the `HueService` calls `_configurationService.SetSecureValueAsync("HueAppKey", appKey)` to securely store the new `AppKey`.
- **Retrieving the `AppKey`**:
  - When connecting to a bridge, the `HueService` first checks if an `AppKey` has been provided.
  - If not, it calls `_configurationService.GetSecureValueAsync("HueAppKey")` to retrieve the stored key.
  - This allows the application to automatically reconnect to the bridge on subsequent startups without requiring the user to re-register.

## 4. Security Considerations

- **Platform Dependency**: DPAPI is specific to Windows. If the application were to be ported to other operating systems, a different encryption mechanism would be required.
- **No Additional Entropy**: The current implementation does not use additional entropy (a "salt") for encryption. While DPAPI is secure without it, adding entropy could provide an additional layer of security if needed in the future.
- **Data at Rest**: This solution effectively protects data at rest. Protection of data in memory is handled by the operating system's memory management.

## 5. Conclusion

The integration of the `ConfigurationService` with the `HueService` provides a robust and secure method for protecting the Philips Hue `AppKey` and other sensitive configuration data on the user's machine. It is a well-established and recommended approach for Windows applications.
