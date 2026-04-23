# Secondary PC Wireless ADB Session

Use this on the laptop to pair to the current phone session without retyping from chat.

## Current Phone Endpoints
- Pairing endpoint:
  - `10.0.0.62:42049`
- Wireless connect endpoint:
  - `10.0.0.62:37195`

## Laptop Commands
1. Pull the latest repo state:

```powershell
git pull
```

2. Pair the laptop to the phone.
   Replace `PUT_6_DIGIT_CODE_HERE` with the live six-digit code shown on the phone:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\pair-wireless-adb.ps1 -PairHostPort "10.0.0.62:42049" -PairingCode "PUT_6_DIGIT_CODE_HERE"
```

3. Connect the laptop to the phone over wireless ADB:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\connect-wireless-adb.ps1 -DeviceHostPort "10.0.0.62:37195"
```

4. Optional status check:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\status-wireless-adb.ps1
```

5. Once connected, use Unity normally:
   - `Tools/Android/Build And Install Debug APK`

## Notes
- The pairing code changes, so only the code placeholder should be edited.
- The repo's Android install helper now prefers the wireless device automatically when both USB and Wi-Fi entries are visible.
- If the phone forgets the laptop later, generate a fresh pairing code and rerun step 2.
