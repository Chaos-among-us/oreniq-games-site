using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class QaTestingSystem
{
#pragma warning disable 0649
    [Serializable]
    private sealed class QaNativeEvent
    {
        public string status;
        public string runId;
        public string path;
        public string message;
    }
#pragma warning restore 0649

    [Serializable]
    private sealed class QaSurveyState
    {
        public int fairnessIndex;
        public int difficultyIndex;
        public int rewardIndex;
        public int replayIndex;

        public static QaSurveyState CreateDefault()
        {
            return new QaSurveyState
            {
                fairnessIndex = 1,
                difficultyIndex = 1,
                rewardIndex = 1,
                replayIndex = 1
            };
        }
    }

    [Serializable]
    private sealed class QaRunSummary
    {
        public string runId;
        public string startedAtUtc;
        public string finishedAtUtc;
        public string buildVersion;
        public string packageId;
        public string deviceModel;
        public string operatingSystem;
        public int finalScore;
        public int levelReached;
        public int coinsEarned;
        public int nearMisses;
        public int dangerPeak;
        public bool newBestScore;
        public bool isDailyChallengeRun;
        public string recordingStatus;
        public string recordingPath;
    }

    private enum QaCaptureState
    {
        Idle,
        AwaitingConsent,
        Recording,
        Finalizing,
        Ready,
        Denied,
        Failed
    }

    private const string QaModeEnabledKey = "Qa_ModeEnabled";
    private const string QaNoticeAcceptedKey = "Qa_NoticeAccepted";
    private const string QaPracticeRunRequestedKey = "Qa_PracticeRunRequested";
    private const string ReportDirectoryName = "qa-reports";
    private const string RuntimeObjectName = "QaTestingSystemRuntime";
    private const string RuntimeCallbackMethod = "OnNativeQaEvent";

    private static readonly string[] FairnessOptions = { "Unfair", "Fair", "Great" };
    private static readonly string[] DifficultyOptions = { "Too Easy", "Balanced", "Too Hard" };
    private static readonly string[] RewardOptions = { "Flat", "Good", "Exciting" };
    private static readonly string[] ReplayOptions = { "No Pull", "Maybe", "Yes" };

    private static QaCaptureState captureState = QaCaptureState.Idle;
    private static QaRunSummary currentRunSummary;
    private static QaSurveyState currentSurvey = QaSurveyState.CreateDefault();
    private static string lastCapturePath = string.Empty;
    private static string lastReportPath = string.Empty;
    private static string lastExportUri = string.Empty;
    private static string liveStatusMessage = "QA mode is off.";

    public static bool IsQaModeEnabled()
    {
        return PlayerPrefs.GetInt(QaModeEnabledKey, 0) == 1;
    }

    public static void SetQaModeEnabled(bool enabled)
    {
        EnsureRuntime();
        PlayerPrefs.SetInt(QaModeEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!enabled)
        {
            StopCapture("qa_mode_disabled");
            captureState = QaCaptureState.Idle;
            currentRunSummary = null;
            currentSurvey = QaSurveyState.CreateDefault();
            lastExportUri = string.Empty;
            liveStatusMessage = "QA mode is off.";
            return;
        }

        liveStatusMessage = "QA mode is ready. Recorded runs will ask Android for screen-capture permission.";
    }

    public static bool HasAcceptedNotice()
    {
        return PlayerPrefs.GetInt(QaNoticeAcceptedKey, 0) == 1;
    }

    public static void MarkNoticeAccepted()
    {
        PlayerPrefs.SetInt(QaNoticeAcceptedKey, 1);
        PlayerPrefs.Save();
    }

    public static void EnsureRuntime()
    {
        GameObject existing = GameObject.Find(RuntimeObjectName);

        if (IsQaModeEnabled() && captureState == QaCaptureState.Idle)
            liveStatusMessage = "QA mode is ready. Recorded runs will ask Android for screen-capture permission.";
        else if (!IsQaModeEnabled() && captureState == QaCaptureState.Idle)
            liveStatusMessage = "QA mode is off.";

        if (existing != null)
        {
            if (existing.GetComponent<QaTestingRuntime>() == null)
                existing.AddComponent<QaTestingRuntime>();

            return;
        }

        GameObject runtimeObject = new GameObject(RuntimeObjectName);
        runtimeObject.AddComponent<QaTestingRuntime>();
        UnityEngine.Object.DontDestroyOnLoad(runtimeObject);
    }

    public static string GetNoticeBody()
    {
        return
            "QA mode records only while a test run is active and Endless Dodge stays in the foreground.\n" +
            "\n" +
            "Captured: gameplay, in-game menus, and the post-run QA survey.\n" +
            "Not captured: microphone audio.\n" +
            "Android may show its own screen-capture chip or notification.\n" +
            "\n" +
            "Before each recorded QA run, Android still asks for screen-capture consent.\n" +
            "Recording stops automatically when the run ends or the app leaves the foreground.\n" +
            "Bundles can be saved to Files > Downloads > EndlessDodgeQA for easy handoff.\n" +
            "\n" +
            "Tester flow: Practice Tutorial Run once, then after each QA run tap Save QA Bundle or Share QA Package, and delete the files when testing is done.";
    }

    public static string GetMenuButtonLabel()
    {
        if (IsQaModeEnabled())
            return "QA Mode: On\nAuto-record + survey";

        return "QA Mode: Off\nTester tools";
    }

    public static string GetMenuStatusText()
    {
        int storedArtifacts = GetStoredArtifactCount();
        string artifactLabel = storedArtifacts == 1 ? "1 stored QA file" : storedArtifacts + " stored QA files";
        string modeLabel = IsQaModeEnabled() ? "Enabled" : "Disabled";
        return "Mode: " + modeLabel + "\n" + artifactLabel + "\n" + liveStatusMessage;
    }

    public static void RequestPracticeRun()
    {
        PlayerPrefs.SetInt(QaPracticeRunRequestedKey, 1);
        PlayerPrefs.Save();
    }

    public static bool ConsumePracticeRunRequest()
    {
        bool requested = PlayerPrefs.GetInt(QaPracticeRunRequestedKey, 0) == 1;

        if (requested)
        {
            PlayerPrefs.SetInt(QaPracticeRunRequestedKey, 0);
            PlayerPrefs.Save();
        }

        return requested;
    }

    public static bool IsAwaitingConsent()
    {
        return captureState == QaCaptureState.AwaitingConsent;
    }

    public static bool IsRecordingActive()
    {
        return captureState == QaCaptureState.Recording;
    }

    public static bool IsFinalizing()
    {
        return captureState == QaCaptureState.Finalizing;
    }

    public static string GetLiveCaptureStatus()
    {
        switch (captureState)
        {
            case QaCaptureState.AwaitingConsent:
                return "Waiting for Android screen-capture permission...";
            case QaCaptureState.Recording:
                return "QA capture is live for this run.";
            case QaCaptureState.Finalizing:
                return "Finishing the QA video clip...";
            case QaCaptureState.Ready:
                return HasLastCapture()
                    ? "QA clip saved and ready to share."
                    : "QA notes are ready to share.";
            case QaCaptureState.Denied:
                return "Screen capture was denied. This run can still share notes only.";
            case QaCaptureState.Failed:
                return "QA capture failed on this device. Notes only are available.";
            default:
                return liveStatusMessage;
        }
    }

    public static void BeginGameplayCapture()
    {
        if (!IsQaModeEnabled())
            return;

        EnsureRuntime();
        currentSurvey = QaSurveyState.CreateDefault();
        currentRunSummary = new QaRunSummary
        {
            runId = "qa-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"),
            startedAtUtc = DateTime.UtcNow.ToString("o"),
            buildVersion = Application.version,
            packageId = Application.identifier,
            deviceModel = SystemInfo.deviceModel,
            operatingSystem = SystemInfo.operatingSystem,
            recordingStatus = "pending"
        };
        lastCapturePath = string.Empty;
        lastReportPath = string.Empty;
        lastExportUri = string.Empty;
        captureState = QaCaptureState.AwaitingConsent;
        liveStatusMessage = "Waiting for Android to confirm screen capture for this run.";

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass bridge = new AndroidJavaClass("com.oreniq.endlessdodge.qa.QaRecorderBridge"))
            {
                bridge.CallStatic(
                    "requestCaptureConsent",
                    currentRunSummary.runId,
                    RuntimeObjectName,
                    RuntimeCallbackMethod);
            }
        }
        catch (Exception exception)
        {
            captureState = QaCaptureState.Failed;
            liveStatusMessage = "Unable to open Android screen capture.";
            Debug.LogWarning("QA capture request failed: " + exception.Message);
        }
#else
        captureState = QaCaptureState.Failed;
        liveStatusMessage = "Automatic QA video capture is only available in Android device builds.";
#endif
    }

    public static void StopCapture(string reason)
    {
        if (captureState != QaCaptureState.Recording && captureState != QaCaptureState.Finalizing)
            return;

        captureState = QaCaptureState.Finalizing;
        liveStatusMessage = "Finalizing the QA video clip...";

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass bridge = new AndroidJavaClass("com.oreniq.endlessdodge.qa.QaRecorderBridge"))
            {
                bridge.CallStatic("stopRecording", reason ?? "qa_stop");
            }
        }
        catch (Exception exception)
        {
            captureState = QaCaptureState.Failed;
            liveStatusMessage = "Unable to stop the QA recorder cleanly.";
            Debug.LogWarning("QA capture stop failed: " + exception.Message);
        }
#endif
    }

    public static void RegisterRunFinished(
        int finalScore,
        int levelReached,
        int coinsEarned,
        int nearMisses,
        int dangerPeak,
        bool newBestScore,
        bool isDailyChallengeRun)
    {
        if (!IsQaModeEnabled())
            return;

        if (currentRunSummary == null)
        {
            currentRunSummary = new QaRunSummary
            {
                runId = "qa-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"),
                startedAtUtc = DateTime.UtcNow.ToString("o"),
                buildVersion = Application.version,
                packageId = Application.identifier,
                deviceModel = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem
            };
        }

        currentRunSummary.finishedAtUtc = DateTime.UtcNow.ToString("o");
        currentRunSummary.finalScore = finalScore;
        currentRunSummary.levelReached = levelReached;
        currentRunSummary.coinsEarned = coinsEarned;
        currentRunSummary.nearMisses = nearMisses;
        currentRunSummary.dangerPeak = dangerPeak;
        currentRunSummary.newBestScore = newBestScore;
        currentRunSummary.isDailyChallengeRun = isDailyChallengeRun;

        if (captureState == QaCaptureState.Recording)
        {
            currentRunSummary.recordingStatus = "finalizing";
            StopCapture("run_finished");
        }
        else if (captureState == QaCaptureState.AwaitingConsent)
        {
            currentRunSummary.recordingStatus = "pending";
        }
        else if (captureState == QaCaptureState.Denied)
        {
            currentRunSummary.recordingStatus = "denied";
        }
        else if (captureState == QaCaptureState.Failed)
        {
            currentRunSummary.recordingStatus = "failed";
        }
    }

    public static string GetFairnessLabel()
    {
        return "Collision Feel\n" + FairnessOptions[currentSurvey.fairnessIndex];
    }

    public static string GetDifficultyLabel()
    {
        return "Difficulty Curve\n" + DifficultyOptions[currentSurvey.difficultyIndex];
    }

    public static string GetRewardLabel()
    {
        return "Reward Feel\n" + RewardOptions[currentSurvey.rewardIndex];
    }

    public static string GetReplayLabel()
    {
        return "One-More-Run Pull\n" + ReplayOptions[currentSurvey.replayIndex];
    }

    public static void CycleFairness()
    {
        currentSurvey.fairnessIndex = (currentSurvey.fairnessIndex + 1) % FairnessOptions.Length;
    }

    public static void CycleDifficulty()
    {
        currentSurvey.difficultyIndex = (currentSurvey.difficultyIndex + 1) % DifficultyOptions.Length;
    }

    public static void CycleRewardFeel()
    {
        currentSurvey.rewardIndex = (currentSurvey.rewardIndex + 1) % RewardOptions.Length;
    }

    public static void CycleReplayPull()
    {
        currentSurvey.replayIndex = (currentSurvey.replayIndex + 1) % ReplayOptions.Length;
    }

    public static string GetSurveyStatusText()
    {
        if (!IsQaModeEnabled())
            return "QA mode is off for this build.";

        if (captureState == QaCaptureState.Finalizing)
            return "Finishing the QA clip before sharing.";

        if (captureState == QaCaptureState.Ready && HasLastCapture())
            return lastExportUri.Length > 0
                ? "Video and notes are saved in Downloads. You can still share or delete them."
                : "Video and notes are ready. Save to Downloads or share them now.";

        if (captureState == QaCaptureState.Denied || captureState == QaCaptureState.Failed)
            return lastExportUri.Length > 0
                ? "Notes were saved in Downloads. Video was unavailable for this run."
                : "Video is unavailable for this run. Notes-only sharing is still ready.";

        return "Tap each row to tune the report, then save the bundle or share it.";
    }

    public static string GetShareButtonLabel()
    {
        if (captureState == QaCaptureState.Finalizing)
            return "Finishing Clip...";

        return HasLastCapture() ? "Share QA Package" : "Share QA Notes";
    }

    public static string GetSaveButtonLabel()
    {
        return lastExportUri.Length > 0 ? "Saved To Downloads" : "Save QA Bundle";
    }

    public static bool CanShareCurrentRun()
    {
        return currentRunSummary != null && captureState != QaCaptureState.Finalizing;
    }

    public static bool CanSaveCurrentRun()
    {
        return CanShareCurrentRun() && lastExportUri.Length == 0;
    }

    public static bool HasLastPackage()
    {
        return HasLastCapture() || HasLastReport() || lastExportUri.Length > 0;
    }

    public static bool ShareCurrentRun()
    {
        if (!CanShareCurrentRun())
            return false;

        lastReportPath = WriteCurrentRunReport();
        string subject = "Endless Dodge QA run";
        string body = BuildShareBody();

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass bridge = new AndroidJavaClass("com.oreniq.endlessdodge.qa.QaRecorderBridge"))
            {
                bool shared = bridge.CallStatic<bool>(
                    "shareCapturePackage",
                    lastCapturePath ?? string.Empty,
                    lastReportPath ?? string.Empty,
                    "Share QA package",
                    subject,
                    body);

                liveStatusMessage = shared
                    ? "Opened the QA package in the Android share sheet."
                    : "Android could not open the share sheet for this QA package.";

                if (!shared)
                    MobileGrowthActions.ShareText("Share QA notes", subject, body);

                return shared;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("QA package share failed: " + exception.Message);
        }
#endif

        liveStatusMessage = "Shared the QA notes text.";
        return MobileGrowthActions.ShareText("Share QA notes", subject, body);
    }

    public static bool SaveCurrentRunToDownloads()
    {
        if (!CanShareCurrentRun())
            return false;

        if (lastExportUri.Length > 0)
        {
            liveStatusMessage = "The QA bundle is already saved in Files > Downloads > EndlessDodgeQA.";
            return true;
        }

        lastReportPath = WriteCurrentRunReport();

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass bridge = new AndroidJavaClass("com.oreniq.endlessdodge.qa.QaRecorderBridge"))
            {
                string exportUri = bridge.CallStatic<string>(
                    "exportCapturePackageToDownloads",
                    lastCapturePath ?? string.Empty,
                    lastReportPath ?? string.Empty,
                    currentRunSummary != null ? currentRunSummary.runId : "endless-dodge-qa");

                if (!string.IsNullOrEmpty(exportUri))
                {
                    lastExportUri = exportUri;
                    liveStatusMessage = "Saved the QA bundle to Files > Downloads > EndlessDodgeQA.";
                    return true;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("QA package export failed: " + exception.Message);
        }
#endif

        liveStatusMessage = "Android could not save the QA bundle to Downloads.";
        return false;
    }

    public static bool DeleteLastPackage()
    {
        bool deletedAny = false;

        if (HasLastReport())
        {
            File.Delete(lastReportPath);
            deletedAny = true;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (HasLastCapture() || !string.IsNullOrEmpty(lastExportUri))
        {
            try
            {
                using (AndroidJavaClass bridge = new AndroidJavaClass("com.oreniq.endlessdodge.qa.QaRecorderBridge"))
                {
                    if (HasLastCapture())
                        deletedAny = bridge.CallStatic<bool>("deleteCapture", lastCapturePath) || deletedAny;

                    if (!string.IsNullOrEmpty(lastExportUri))
                        deletedAny = bridge.CallStatic<bool>("deleteExportedPackage", lastExportUri) || deletedAny;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("QA capture delete failed: " + exception.Message);
            }
        }
#else
        if (HasLastCapture())
        {
            File.Delete(lastCapturePath);
            deletedAny = true;
        }
#endif

        if (deletedAny)
        {
            lastCapturePath = string.Empty;
            lastReportPath = string.Empty;
            lastExportUri = string.Empty;
            liveStatusMessage = "Removed the last QA package from local storage and Downloads.";
        }
        else
        {
            liveStatusMessage = "There was no saved QA package to delete.";
        }

        return deletedAny;
    }

    public static bool DeleteAllArtifacts()
    {
        bool deletedAny = false;
        string reportDirectory = GetReportDirectory();

        if (Directory.Exists(reportDirectory))
        {
            string[] reportFiles = Directory.GetFiles(reportDirectory, "*.txt", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < reportFiles.Length; i++)
            {
                File.Delete(reportFiles[i]);
                deletedAny = true;
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass bridge = new AndroidJavaClass("com.oreniq.endlessdodge.qa.QaRecorderBridge"))
            {
                deletedAny = bridge.CallStatic<bool>("deleteAllCaptures") || deletedAny;
                deletedAny = bridge.CallStatic<bool>("deleteAllExportedPackages") || deletedAny;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("QA bulk delete failed: " + exception.Message);
        }
#endif

        if (deletedAny)
        {
            lastCapturePath = string.Empty;
            lastReportPath = string.Empty;
            lastExportUri = string.Empty;
            liveStatusMessage = "Deleted saved QA clips, reports, and exported bundles.";
        }
        else
        {
            liveStatusMessage = "No saved QA clips or reports were found.";
        }

        return deletedAny;
    }

    public static int GetStoredArtifactCount()
    {
        int reportCount = 0;
        string reportDirectory = GetReportDirectory();

        if (Directory.Exists(reportDirectory))
            reportCount = Directory.GetFiles(reportDirectory, "*.txt", SearchOption.TopDirectoryOnly).Length;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass bridge = new AndroidJavaClass("com.oreniq.endlessdodge.qa.QaRecorderBridge"))
            {
                int captureCount = bridge.CallStatic<int>("getStoredCaptureCount");
                int exportCount = bridge.CallStatic<int>("getStoredExportCount");
                return Mathf.Max(reportCount, Mathf.Max(captureCount, exportCount));
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("QA capture count lookup failed: " + exception.Message);
        }
#endif

        return reportCount;
    }

    public static void HandleApplicationFocusChanged(bool hasFocus)
    {
        if (hasFocus || !IsQaModeEnabled())
            return;

        if (captureState == QaCaptureState.Recording)
        {
            currentRunSummary.recordingStatus = "stopped_on_background";
            StopCapture("app_backgrounded");
        }
    }

    internal static void HandleNativeEvent(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        if (!IsQaModeEnabled())
            return;

        QaNativeEvent nativeEvent = JsonUtility.FromJson<QaNativeEvent>(json);

        if (nativeEvent == null)
            return;

        if (currentRunSummary == null)
        {
            currentRunSummary = new QaRunSummary
            {
                runId = nativeEvent.runId,
                startedAtUtc = DateTime.UtcNow.ToString("o"),
                buildVersion = Application.version,
                packageId = Application.identifier,
                deviceModel = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem
            };
        }

        if (!string.IsNullOrEmpty(nativeEvent.runId))
            currentRunSummary.runId = nativeEvent.runId;

        if (!string.IsNullOrEmpty(nativeEvent.path))
        {
            currentRunSummary.recordingPath = nativeEvent.path;
            lastCapturePath = nativeEvent.path;
        }

        switch (nativeEvent.status)
        {
            case "recording_started":
                captureState = QaCaptureState.Recording;
                currentRunSummary.recordingStatus = "recording";
                liveStatusMessage = "QA capture is live for this run.";
                break;
            case "recording_stopped":
                captureState = QaCaptureState.Ready;
                currentRunSummary.recordingStatus = HasLastCapture() ? "saved" : "stopped";
                liveStatusMessage = HasLastCapture()
                    ? "Saved the QA clip for this run."
                    : "QA recording stopped without a saved clip.";
                break;
            case "denied":
                captureState = QaCaptureState.Denied;
                currentRunSummary.recordingStatus = "denied";
                liveStatusMessage = string.IsNullOrWhiteSpace(nativeEvent.message)
                    ? "Screen capture was denied for this run."
                    : nativeEvent.message;
                break;
            case "error":
                captureState = QaCaptureState.Failed;
                currentRunSummary.recordingStatus = "failed";
                liveStatusMessage = string.IsNullOrWhiteSpace(nativeEvent.message)
                    ? "QA capture failed for this run."
                    : nativeEvent.message;
                break;
        }
    }

    private static string BuildShareBody()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Endless Dodge QA package attached.");
        builder.AppendLine("Score " + currentRunSummary.finalScore + " | Level " + currentRunSummary.levelReached + " | Coins +" + currentRunSummary.coinsEarned);
        builder.AppendLine("Collision: " + FairnessOptions[currentSurvey.fairnessIndex]);
        builder.AppendLine("Difficulty: " + DifficultyOptions[currentSurvey.difficultyIndex]);
        builder.AppendLine("Rewards: " + RewardOptions[currentSurvey.rewardIndex]);
        builder.AppendLine("One more run: " + ReplayOptions[currentSurvey.replayIndex]);
        return builder.ToString().Trim();
    }

    private static string WriteCurrentRunReport()
    {
        string reportDirectory = GetReportDirectory();
        Directory.CreateDirectory(reportDirectory);

        string reportFileName = (currentRunSummary != null && !string.IsNullOrEmpty(currentRunSummary.runId)
                ? currentRunSummary.runId
                : "qa-run-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")) +
            ".txt";
        string reportPath = Path.Combine(reportDirectory, reportFileName);
        File.WriteAllText(reportPath, BuildDetailedReportText());
        return reportPath;
    }

    private static string BuildDetailedReportText()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Endless Dodge QA Report");
        builder.AppendLine("======================");
        builder.AppendLine("Run ID: " + SafeValue(currentRunSummary != null ? currentRunSummary.runId : string.Empty));
        builder.AppendLine("Started UTC: " + SafeValue(currentRunSummary != null ? currentRunSummary.startedAtUtc : string.Empty));
        builder.AppendLine("Finished UTC: " + SafeValue(currentRunSummary != null ? currentRunSummary.finishedAtUtc : string.Empty));
        builder.AppendLine("Build Version: " + SafeValue(currentRunSummary != null ? currentRunSummary.buildVersion : Application.version));
        builder.AppendLine("Package ID: " + SafeValue(currentRunSummary != null ? currentRunSummary.packageId : Application.identifier));
        builder.AppendLine("Device: " + SafeValue(currentRunSummary != null ? currentRunSummary.deviceModel : SystemInfo.deviceModel));
        builder.AppendLine("OS: " + SafeValue(currentRunSummary != null ? currentRunSummary.operatingSystem : SystemInfo.operatingSystem));
        builder.AppendLine();
        builder.AppendLine("Run Results");
        builder.AppendLine("-----------");
        builder.AppendLine("Score: " + (currentRunSummary != null ? currentRunSummary.finalScore : 0));
        builder.AppendLine("Level Reached: " + (currentRunSummary != null ? currentRunSummary.levelReached : 0));
        builder.AppendLine("Coins Earned: " + (currentRunSummary != null ? currentRunSummary.coinsEarned : 0));
        builder.AppendLine("Near Misses: " + (currentRunSummary != null ? currentRunSummary.nearMisses : 0));
        builder.AppendLine("Danger Peak: " + (currentRunSummary != null ? currentRunSummary.dangerPeak : 0));
        builder.AppendLine("New Best: " + (currentRunSummary != null && currentRunSummary.newBestScore ? "Yes" : "No"));
        builder.AppendLine("Daily Challenge Run: " + (currentRunSummary != null && currentRunSummary.isDailyChallengeRun ? "Yes" : "No"));
        builder.AppendLine();
        builder.AppendLine("Capture");
        builder.AppendLine("-------");
        builder.AppendLine("Status: " + SafeValue(currentRunSummary != null ? currentRunSummary.recordingStatus : captureState.ToString()));
        builder.AppendLine("Video Path: " + SafeValue(currentRunSummary != null ? currentRunSummary.recordingPath : lastCapturePath));
        builder.AppendLine();
        builder.AppendLine("Survey");
        builder.AppendLine("------");
        builder.AppendLine("Collision Feel: " + FairnessOptions[currentSurvey.fairnessIndex]);
        builder.AppendLine("Difficulty Curve: " + DifficultyOptions[currentSurvey.difficultyIndex]);
        builder.AppendLine("Reward Feel: " + RewardOptions[currentSurvey.rewardIndex]);
        builder.AppendLine("One-More-Run Pull: " + ReplayOptions[currentSurvey.replayIndex]);
        return builder.ToString().TrimEnd();
    }

    private static string GetReportDirectory()
    {
        return Path.Combine(Application.persistentDataPath, ReportDirectoryName);
    }

    private static bool HasLastCapture()
    {
        return !string.IsNullOrEmpty(lastCapturePath) && File.Exists(lastCapturePath);
    }

    private static bool HasLastReport()
    {
        return !string.IsNullOrEmpty(lastReportPath) && File.Exists(lastReportPath);
    }

    private static string SafeValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "(none)" : value;
    }
}

public sealed class QaTestingRuntime : MonoBehaviour
{
    void Awake()
    {
        gameObject.name = "QaTestingSystemRuntime";
        DontDestroyOnLoad(gameObject);
    }

    public void OnNativeQaEvent(string json)
    {
        QaTestingSystem.HandleNativeEvent(json);
    }
}
