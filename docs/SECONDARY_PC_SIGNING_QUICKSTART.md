# Secondary PC Signing Quickstart

Use this on the laptop when you want it to update the same phone-installed QA build as the primary PC.

## Primary PC Status
- The primary PC is currently serving the shared signing files over the local network.
- Primary PC hostname:
  - `HOLLAND_WORK_PC`
- Primary PC fallback IP on the current Wi-Fi:
  - `10.0.0.7`
- Keep the primary PC awake while the laptop syncs the signing files.

## Laptop Steps
1. Open PowerShell in the repo root.
2. Pull the latest repo state:

```powershell
git pull
```

3. Sync the shared signing files from the primary PC:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\sync-network-shared-android-signing.ps1 -ServerRoot "http://HOLLAND_WORK_PC:8765"
```

4. If the hostname does not resolve, use the fallback IP instead:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\sync-network-shared-android-signing.ps1 -ServerRoot "http://10.0.0.7:8765"
```

5. If Unity is already open, close it.
6. Run the normal workstation bootstrap:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\bootstrap-workstation.ps1
```

7. Open the project in Unity `6000.4.0f1`.
8. In Unity, build/install as usual:
   - `Tools/Android/Build And Install Debug APK`

## What Success Looks Like
- The sync script downloads:
  - `release-signing.json`
  - `shared-debug.keystore`
- The laptop sets the user environment variable:
  - `%ENDLESSDODGE_SIGNING_ROOT%`
- After that, Unity should resolve the same shared signing identity the primary PC is using for the current QA build.

## If It Fails
- Make sure both PCs are on the same Wi-Fi network.
- Make sure the primary PC is still awake.
- If needed, restart the server on the primary PC:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\start-network-signing-server.ps1
```

- When the laptop is finished syncing, the primary PC can stop the temporary server with:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\stop-network-signing-server.ps1
```
