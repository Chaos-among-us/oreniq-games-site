# Wireless ADB Quickstart

This is the cleanest way to let both PCs install to the same phone without swapping the USB cable back and forth.

Official references:
- [Android Debug Bridge (ADB)](https://developer.android.com/tools/adb)
- [Run apps on a hardware device](https://developer.android.com/studio/run/device.html)

## What This Solves
- Both PCs can pair with the same phone over Wi-Fi.
- After each PC is paired once, installs and debugging can happen wirelessly from either PC.
- This removes the USB-cable bottleneck.

## Important Reality Check
- Each PC must pair separately with the phone. Pairing is per workstation.
- Only one PC should actively build/install at a time to avoid needless ADB confusion.
- All three devices must stay on the same Wi-Fi network.

## Before You Start
1. Make sure the phone and both PCs are on the same Wi-Fi network.
2. On the phone, enable:
   - `Developer options`
   - `Wireless debugging`
3. Keep the phone unlocked during pairing.
4. On each PC, open PowerShell in the repo root.

## Step 1: Check ADB Status

```powershell
powershell -ExecutionPolicy Bypass -File scripts\status-wireless-adb.ps1
```

## Step 2: Pair This PC To The Phone
1. On the phone, go to:
   - `Settings > Developer options > Wireless debugging`
2. Tap:
   - `Pair device with pairing code`
3. The phone will show:
   - a pairing IP and port
   - a six-digit pairing code

4. On the PC, run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\pair-wireless-adb.ps1 -PairHostPort "PHONE_PAIR_IP:PAIR_PORT" -PairingCode "123456"
```

Example:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\pair-wireless-adb.ps1 -PairHostPort "192.168.1.50:37099" -PairingCode "123456"
```

## Step 3: Connect This PC To The Phone
1. Stay on the phone’s `Wireless debugging` screen.
2. Note the device connection address shown there for wireless debugging.
   - This is usually a different port from the pairing port.
3. On the PC, run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\connect-wireless-adb.ps1 -DeviceHostPort "PHONE_CONNECT_IP:CONNECT_PORT"
```

Example:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\connect-wireless-adb.ps1 -DeviceHostPort "192.168.1.50:39521"
```

4. Verify the phone appears in:
   - `adb devices -l`

## Step 4: Repeat On The Other PC
- Repeat the same pair/connect process on the second PC.
- Use a fresh pairing code from the phone for the second PC.

## Step 5: Use It Normally
Once connected, installs can use the normal Unity menu:
- `Tools/Android/Build And Install Debug APK`

Unity note:
- The repo's Android install helper now prefers the wireless device automatically when both the USB serial and the wireless `IP:port` entry are present for the same phone.
- That means you do not need to manually disconnect USB just to avoid the old `more than one device` install problem.

## If Connection Drops
1. First try reconnecting only:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\connect-wireless-adb.ps1 -DeviceHostPort "PHONE_CONNECT_IP:CONNECT_PORT"
```

2. Check status:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\status-wireless-adb.ps1
```

3. If needed, disconnect and reconnect:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\disconnect-wireless-adb.ps1
```

4. If the phone forgot the workstation, pair again from the phone’s `Wireless debugging` screen.

## Known Gotchas
- Some networks interfere with wireless debugging discovery. If that happens, manual `adb connect ip:port` is the fallback, which this quickstart already uses.
- The pairing port and the connection port are not always the same.
- If you want to remove a PC later:
  - on the phone, open `Wireless debugging`
  - tap the workstation name under `Paired devices`
  - tap `Forget`
