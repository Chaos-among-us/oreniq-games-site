using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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

    [Serializable]
    private sealed class QaSubmissionConfig
    {
        public string uploadUrl;
        public string submissionButtonLabel;
        public string testerBuildUrl;
        public string testerSurveyUrl;

        public static QaSubmissionConfig CreateDefault()
        {
            return new QaSubmissionConfig
            {
                uploadUrl = string.Empty,
                submissionButtonLabel = "Send QA Package",
                testerBuildUrl = string.Empty,
                testerSurveyUrl = string.Empty
            };
        }
    }

    [Serializable]
    private sealed class QaPackageMetadata
    {
        public string schemaVersion;
        public string exportedAtUtc;
        public QaRunSummary run;
        public string collisionFeel;
        public string difficultyCurve;
        public string rewardFeel;
        public string replayPull;
        public string testerBuildUrl;
        public string testerSurveyUrl;
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

    private enum QaSubmissionState
    {
        Idle,
        Preparing,
        Uploading,
        Success,
        Failed
    }

    private const string QaModeEnabledKey = "Qa_ModeEnabled";
    private const string QaNoticeAcceptedKey = "Qa_NoticeAccepted";
    private const string QaPracticeRunRequestedKey = "Qa_PracticeRunRequested";
    private const string ReportDirectoryName = "qa-reports";
    private const string SubmissionDirectoryName = "qa-submissions";
    private const string RuntimeObjectName = "QaTestingSystemRuntime";
    private const string RuntimeCallbackMethod = "OnNativeQaEvent";
    private const string SubmissionConfigResourcePath = "QaSubmissionConfig";

    private static readonly string[] FairnessOptions = { "Unfair", "Fair", "Great" };
    private static readonly string[] DifficultyOptions = { "Too Easy", "Balanced", "Too Hard" };
    private static readonly string[] RewardOptions = { "Flat", "Good", "Exciting" };
    private static readonly string[] ReplayOptions = { "No Pull", "Maybe", "Yes" };

    private static QaCaptureState captureState = QaCaptureState.Idle;
    private static QaSubmissionState submissionState = QaSubmissionState.Idle;
    private static QaRunSummary currentRunSummary;
    private static QaSurveyState currentSurvey = QaSurveyState.CreateDefault();
    private static string lastCapturePath = string.Empty;
    private static string lastReportPath = string.Empty;
    private static string lastExportUri = string.Empty;
    private static string lastSubmissionZipPath = string.Empty;
    private static string lastSubmittedRunId = string.Empty;
    private static string submissionStatusMessage = string.Empty;
    private static string liveStatusMessage = "QA mode is off.";
    private static QaSubmissionConfig cachedSubmissionConfig;
    private static bool submissionConfigLoaded;
    private static QaTestingRuntime runtimeInstance;

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
            submissionState = QaSubmissionState.Idle;
            currentRunSummary = null;
            currentSurvey = QaSurveyState.CreateDefault();
            lastExportUri = string.Empty;
            lastSubmissionZipPath = string.Empty;
            lastSubmittedRunId = string.Empty;
            submissionStatusMessage = string.Empty;
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
            QaTestingRuntime runtime = existing.GetComponent<QaTestingRuntime>();

            if (runtime == null)
                runtime = existing.AddComponent<QaTestingRuntime>();

            runtimeInstance = runtime;

            return;
        }

        GameObject runtimeObject = new GameObject(RuntimeObjectName);
        runtimeInstance = runtimeObject.AddComponent<QaTestingRuntime>();
        UnityEngine.Object.DontDestroyOnLoad(runtimeObject);
    }

    public static string GetNoticeBody()
    {
        QaSubmissionConfig config = GetSubmissionConfig();
        string handoffAction = HasUploadTargetConfigured()
            ? GetConfiguredSubmissionButtonLabel()
            : "Share QA Package";
        string optionalDownloadLine = string.IsNullOrWhiteSpace(config.testerBuildUrl)
            ? string.Empty
            : "\nTester build link is configured for this QA lane.\n";

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
            optionalDownloadLine +
            "\n" +
            "Tester flow: Practice Tutorial Run once, then after each QA run tap Save QA Bundle or " +
            handoffAction +
            ", and delete the files when testing is done.";
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
        string sendLabel = HasUploadTargetConfigured() ? "Send: One tap upload" : "Send: Share sheet fallback";
        return "Mode: " + modeLabel + "\n" + artifactLabel + "\n" + sendLabel + "\n" + liveStatusMessage;
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
        lastSubmissionZipPath = string.Empty;
        submissionState = QaSubmissionState.Idle;
        submissionStatusMessage = string.Empty;
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

        if (submissionState == QaSubmissionState.Preparing || submissionState == QaSubmissionState.Uploading)
            return "Sending the QA package directly...";

        if (IsCurrentRunAlreadySubmitted())
            return "QA package sent successfully. You can still save a local copy.";

        if (submissionState == QaSubmissionState.Failed && !string.IsNullOrWhiteSpace(submissionStatusMessage))
            return submissionStatusMessage;

        if (captureState == QaCaptureState.Ready && HasLastCapture())
            return lastExportUri.Length > 0
                ? "Video and notes are saved in Downloads. You can still share or delete them."
                : "Video and notes are ready. Save to Downloads or send them now.";

        if (captureState == QaCaptureState.Denied || captureState == QaCaptureState.Failed)
            return lastExportUri.Length > 0
                ? "Notes were saved in Downloads. Video was unavailable for this run."
                : "Video is unavailable for this run. Notes-only sending is still ready.";

        return "Tap each row to tune the report, then save the bundle or send it.";
    }

    public static string GetShareButtonLabel()
    {
        if (captureState == QaCaptureState.Finalizing)
            return "Finishing Clip...";

        if (submissionState == QaSubmissionState.Preparing || submissionState == QaSubmissionState.Uploading)
            return "Sending...";

        if (IsCurrentRunAlreadySubmitted())
            return "Sent";

        if (HasUploadTargetConfigured())
            return HasLastCapture() ? GetConfiguredSubmissionButtonLabel() : "Send QA Notes";

        return HasLastCapture() ? "Share QA Package" : "Share QA Notes";
    }

    public static string GetSaveButtonLabel()
    {
        if (IsCurrentRunAlreadySubmitted())
            return "Save Local Copy";

        return lastExportUri.Length > 0 ? "Saved To Downloads" : "Save QA Bundle";
    }

    public static bool CanShareCurrentRun()
    {
        if (currentRunSummary == null || captureState == QaCaptureState.Finalizing)
            return false;

        if (submissionState == QaSubmissionState.Preparing || submissionState == QaSubmissionState.Uploading)
            return false;

        return !IsCurrentRunAlreadySubmitted();
    }

    public static bool CanSaveCurrentRun()
    {
        if (currentRunSummary == null || captureState == QaCaptureState.Finalizing)
            return false;

        if (submissionState == QaSubmissionState.Preparing || submissionState == QaSubmissionState.Uploading)
            return false;

        return lastExportUri.Length == 0;
    }

    public static bool HasLastPackage()
    {
        return HasLastCapture() || HasLastReport() || lastExportUri.Length > 0 || HasLastSubmissionZip();
    }

    public static bool ShareCurrentRun()
    {
        if (!CanShareCurrentRun())
            return false;

        if (HasUploadTargetConfigured())
            return SubmitCurrentRun();

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

        if (HasLastSubmissionZip())
        {
            File.Delete(lastSubmissionZipPath);
            deletedAny = true;
        }

        if (deletedAny)
        {
            lastCapturePath = string.Empty;
            lastReportPath = string.Empty;
            lastExportUri = string.Empty;
            lastSubmissionZipPath = string.Empty;
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
        string submissionDirectory = GetSubmissionDirectory();

        if (Directory.Exists(reportDirectory))
        {
            string[] reportFiles = Directory.GetFiles(reportDirectory, "*.txt", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < reportFiles.Length; i++)
            {
                File.Delete(reportFiles[i]);
                deletedAny = true;
            }
        }

        if (Directory.Exists(submissionDirectory))
        {
            string[] submissionFiles = Directory.GetFiles(submissionDirectory, "*.zip", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < submissionFiles.Length; i++)
            {
                File.Delete(submissionFiles[i]);
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
            lastSubmissionZipPath = string.Empty;
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

    public static bool HasUploadTargetConfigured()
    {
        return !string.IsNullOrWhiteSpace(GetSubmissionConfig().uploadUrl);
    }

    internal static void RegisterRuntime(QaTestingRuntime runtime)
    {
        runtimeInstance = runtime;
    }

    internal static IEnumerator BeginQaSubmissionCoroutine(string uploadUrl, string zipPath, string runId)
    {
        return UploadCurrentRunCoroutine(uploadUrl, zipPath, runId);
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

    private static bool SubmitCurrentRun()
    {
        if (runtimeInstance == null)
            EnsureRuntime();

        if (runtimeInstance == null)
        {
            submissionState = QaSubmissionState.Failed;
            submissionStatusMessage = "Unable to start the QA upload helper.";
            liveStatusMessage = submissionStatusMessage;
            return false;
        }

        QaSubmissionConfig config = GetSubmissionConfig();

        if (string.IsNullOrWhiteSpace(config.uploadUrl))
            return false;

        try
        {
            submissionState = QaSubmissionState.Preparing;
            submissionStatusMessage = "Preparing the QA package for upload...";
            liveStatusMessage = submissionStatusMessage;
            lastReportPath = WriteCurrentRunReport();
            lastSubmissionZipPath = WriteCurrentRunSubmissionZip();
        }
        catch (Exception exception)
        {
            submissionState = QaSubmissionState.Failed;
            submissionStatusMessage = "Could not package the QA run for upload.";
            liveStatusMessage = submissionStatusMessage;
            Debug.LogWarning("QA submission packaging failed: " + exception.Message);
            return false;
        }

        runtimeInstance.BeginQaSubmission(
            config.uploadUrl.Trim(),
            lastSubmissionZipPath,
            currentRunSummary != null ? currentRunSummary.runId : "qa-run");
        return true;
    }

    private static IEnumerator UploadCurrentRunCoroutine(string uploadUrl, string zipPath, string runId)
    {
        submissionState = QaSubmissionState.Uploading;
        submissionStatusMessage = "Uploading the QA package...";
        liveStatusMessage = submissionStatusMessage;

        byte[] zipBytes;

        try
        {
            zipBytes = File.ReadAllBytes(zipPath);
        }
        catch (Exception exception)
        {
            submissionState = QaSubmissionState.Failed;
            submissionStatusMessage = "Could not read the QA package for upload.";
            liveStatusMessage = submissionStatusMessage;
            Debug.LogWarning("QA submission read failed: " + exception.Message);
            yield break;
        }

        List<IMultipartFormSection> formSections = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("run_id", runId ?? string.Empty),
            new MultipartFormDataSection("package_id", currentRunSummary != null ? currentRunSummary.packageId : Application.identifier),
            new MultipartFormDataSection("build_version", currentRunSummary != null ? currentRunSummary.buildVersion : Application.version),
            new MultipartFormDataSection("survey_json", BuildSurveyJson()),
            new MultipartFormDataSection("report_text", BuildDetailedReportText()),
            new MultipartFormFileSection("package", zipBytes, Path.GetFileName(zipPath), "application/zip")
        };

        using (UnityWebRequest request = UnityWebRequest.Post(uploadUrl, formSections))
        {
            request.timeout = 120;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                submissionState = QaSubmissionState.Success;
                lastSubmittedRunId = runId ?? string.Empty;
                submissionStatusMessage = "QA package sent successfully.";
                liveStatusMessage = submissionStatusMessage;
                yield break;
            }

            submissionState = QaSubmissionState.Failed;
            submissionStatusMessage = "Send failed. Try again or save a local copy.";
            liveStatusMessage = submissionStatusMessage;
            Debug.LogWarning(
                "QA submission upload failed: " +
                request.result +
                " " +
                request.responseCode +
                " " +
                request.error);
        }
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

    private static string BuildMetadataJson()
    {
        QaSubmissionConfig config = GetSubmissionConfig();
        QaPackageMetadata metadata = new QaPackageMetadata
        {
            schemaVersion = "1",
            exportedAtUtc = DateTime.UtcNow.ToString("o"),
            run = currentRunSummary,
            collisionFeel = FairnessOptions[currentSurvey.fairnessIndex],
            difficultyCurve = DifficultyOptions[currentSurvey.difficultyIndex],
            rewardFeel = RewardOptions[currentSurvey.rewardIndex],
            replayPull = ReplayOptions[currentSurvey.replayIndex],
            testerBuildUrl = config.testerBuildUrl,
            testerSurveyUrl = config.testerSurveyUrl
        };
        return JsonUtility.ToJson(metadata, true);
    }

    private static string BuildSurveyJson()
    {
        QaPackageMetadata metadata = new QaPackageMetadata
        {
            schemaVersion = "1",
            exportedAtUtc = DateTime.UtcNow.ToString("o"),
            run = currentRunSummary,
            collisionFeel = FairnessOptions[currentSurvey.fairnessIndex],
            difficultyCurve = DifficultyOptions[currentSurvey.difficultyIndex],
            rewardFeel = RewardOptions[currentSurvey.rewardIndex],
            replayPull = ReplayOptions[currentSurvey.replayIndex]
        };
        return JsonUtility.ToJson(metadata);
    }

    private static string WriteCurrentRunSubmissionZip()
    {
        string submissionDirectory = GetSubmissionDirectory();
        Directory.CreateDirectory(submissionDirectory);

        string packageFileName = (currentRunSummary != null && !string.IsNullOrEmpty(currentRunSummary.runId)
                ? currentRunSummary.runId
                : "qa-run-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")) +
            ".zip";
        string packagePath = Path.Combine(submissionDirectory, packageFileName);

        if (File.Exists(packagePath))
            File.Delete(packagePath);

        using (FileStream stream = new FileStream(packagePath, FileMode.Create, FileAccess.Write))
        using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            AddFileToArchive(archive, lastCapturePath, Path.GetFileName(lastCapturePath));
            AddFileToArchive(archive, lastReportPath, Path.GetFileName(lastReportPath));
            AddTextEntryToArchive(archive, "qa-metadata.json", BuildMetadataJson());
        }

        return packagePath;
    }

    private static string GetReportDirectory()
    {
        return Path.Combine(Application.persistentDataPath, ReportDirectoryName);
    }

    private static string GetSubmissionDirectory()
    {
        return Path.Combine(Application.temporaryCachePath, SubmissionDirectoryName);
    }

    private static void AddFileToArchive(ZipArchive archive, string sourcePath, string entryName)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) ||
            string.IsNullOrWhiteSpace(entryName) ||
            !File.Exists(sourcePath))
            return;

        ZipArchiveEntry entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Optimal);

        using (Stream inputStream = File.OpenRead(sourcePath))
        using (Stream outputStream = entry.Open())
        {
            inputStream.CopyTo(outputStream);
        }
    }

    private static void AddTextEntryToArchive(ZipArchive archive, string entryName, string body)
    {
        if (archive == null || string.IsNullOrWhiteSpace(entryName))
            return;

        ZipArchiveEntry entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Optimal);

        using (StreamWriter writer = new StreamWriter(entry.Open(), Encoding.UTF8))
        {
            writer.Write(body ?? string.Empty);
        }
    }

    private static bool HasLastCapture()
    {
        return !string.IsNullOrEmpty(lastCapturePath) && File.Exists(lastCapturePath);
    }

    private static bool HasLastReport()
    {
        return !string.IsNullOrEmpty(lastReportPath) && File.Exists(lastReportPath);
    }

    private static bool HasLastSubmissionZip()
    {
        return !string.IsNullOrEmpty(lastSubmissionZipPath) && File.Exists(lastSubmissionZipPath);
    }

    private static bool IsCurrentRunAlreadySubmitted()
    {
        return submissionState == QaSubmissionState.Success &&
            currentRunSummary != null &&
            !string.IsNullOrEmpty(currentRunSummary.runId) &&
            currentRunSummary.runId == lastSubmittedRunId;
    }

    private static string GetConfiguredSubmissionButtonLabel()
    {
        string configuredLabel = GetSubmissionConfig().submissionButtonLabel;
        return string.IsNullOrWhiteSpace(configuredLabel) ? "Send QA Package" : configuredLabel.Trim();
    }

    private static QaSubmissionConfig GetSubmissionConfig()
    {
        if (submissionConfigLoaded)
            return cachedSubmissionConfig;

        submissionConfigLoaded = true;
        cachedSubmissionConfig = QaSubmissionConfig.CreateDefault();
        TextAsset configAsset = Resources.Load<TextAsset>(SubmissionConfigResourcePath);

        if (configAsset == null || string.IsNullOrWhiteSpace(configAsset.text))
            return cachedSubmissionConfig;

        try
        {
            QaSubmissionConfig loadedConfig = JsonUtility.FromJson<QaSubmissionConfig>(configAsset.text);

            if (loadedConfig != null)
                cachedSubmissionConfig = loadedConfig;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("QA submission config could not be parsed: " + exception.Message);
        }

        if (string.IsNullOrWhiteSpace(cachedSubmissionConfig.submissionButtonLabel))
            cachedSubmissionConfig.submissionButtonLabel = "Send QA Package";

        cachedSubmissionConfig.uploadUrl = cachedSubmissionConfig.uploadUrl ?? string.Empty;
        cachedSubmissionConfig.testerBuildUrl = cachedSubmissionConfig.testerBuildUrl ?? string.Empty;
        cachedSubmissionConfig.testerSurveyUrl = cachedSubmissionConfig.testerSurveyUrl ?? string.Empty;
        return cachedSubmissionConfig;
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
        QaTestingSystem.RegisterRuntime(this);
        DontDestroyOnLoad(gameObject);
    }

    public void BeginQaSubmission(string uploadUrl, string zipPath, string runId)
    {
        StartCoroutine(QaTestingSystem.BeginQaSubmissionCoroutine(uploadUrl, zipPath, runId));
    }

    public void OnNativeQaEvent(string json)
    {
        QaTestingSystem.HandleNativeEvent(json);
    }
}
