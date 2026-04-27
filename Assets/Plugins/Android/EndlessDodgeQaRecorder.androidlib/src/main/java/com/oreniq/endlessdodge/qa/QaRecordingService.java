package com.oreniq.endlessdodge.qa;

import android.app.Activity;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ServiceInfo;
import android.graphics.Rect;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.MediaRecorder;
import android.media.projection.MediaProjection;
import android.media.projection.MediaProjectionManager;
import android.os.Build;
import android.os.Environment;
import android.os.IBinder;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.Display;
import android.view.WindowManager;

import java.io.File;
import java.io.IOException;

public final class QaRecordingService extends Service {
    private static final String TAG = "QaAudioTrace";
    private static final String ACTION_START = "com.oreniq.endlessdodge.qa.ACTION_START";
    private static final String ACTION_STOP = "com.oreniq.endlessdodge.qa.ACTION_STOP";
    private static final String EXTRA_RUN_ID = "com.oreniq.endlessdodge.qa.EXTRA_RUN_ID";
    private static final String EXTRA_RESULT_CODE = "com.oreniq.endlessdodge.qa.EXTRA_RESULT_CODE";
    private static final String EXTRA_RESULT_DATA = "com.oreniq.endlessdodge.qa.EXTRA_RESULT_DATA";
    private static final String CHANNEL_ID = "endless_dodge_qa_capture";
    private static final int NOTIFICATION_ID = 9204;
    private static final int MIN_VIDEO_BITRATE = 2_400_000;
    private static final int MAX_VIDEO_BITRATE = 6_000_000;

    private MediaProjection mediaProjection;
    private VirtualDisplay virtualDisplay;
    private MediaRecorder mediaRecorder;
    private MediaProjection.Callback projectionCallback;
    private boolean recordingStarted;
    private boolean finishing;
    private boolean terminalEventSent;
    private String activeRunId = "";
    private String outputPath = "";

    public static void startFromConsent(Context context, String runId, int resultCode, Intent data) {
        Log.d(TAG, "startFromConsent runId=" + runId + " resultCode=" + resultCode + " hasData=" + (data != null));
        Intent serviceIntent = new Intent(context, QaRecordingService.class);
        serviceIntent.setAction(ACTION_START);
        serviceIntent.putExtra(EXTRA_RUN_ID, runId);
        serviceIntent.putExtra(EXTRA_RESULT_CODE, resultCode);
        serviceIntent.putExtra(EXTRA_RESULT_DATA, data);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            context.startForegroundService(serviceIntent);
        } else {
            context.startService(serviceIntent);
        }
    }

    public static void stopRecording(Context context, String reason) {
        Intent stopIntent = new Intent(context, QaRecordingService.class);
        stopIntent.setAction(ACTION_STOP);
        stopIntent.putExtra("stop_reason", reason);

        try {
            context.startService(stopIntent);
        } catch (Exception ignored) {
            context.stopService(new Intent(context, QaRecordingService.class));
        }
    }

    public static File getCaptureDirectory(Context context) {
        File baseDirectory = context.getExternalFilesDir(Environment.DIRECTORY_MOVIES);

        if (baseDirectory == null) {
            baseDirectory = new File(context.getFilesDir(), "qa-captures");
        }

        File captureDirectory = new File(baseDirectory, "qa");

        if (!captureDirectory.exists()) {
            captureDirectory.mkdirs();
        }

        return captureDirectory;
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        if (intent == null) {
            Log.w(TAG, "onStartCommand received null intent");
            stopSelf();
            return START_NOT_STICKY;
        }

        String action = intent.getAction();
        Log.d(TAG, "onStartCommand action=" + action + " startId=" + startId);

        if (ACTION_STOP.equals(action)) {
            stopRecordingInternal();
            return START_NOT_STICKY;
        }

        if (!ACTION_START.equals(action)) {
            stopSelf();
            return START_NOT_STICKY;
        }

        createNotificationChannel();
        startForegroundCompat(buildNotification("Recording QA gameplay"));
        beginRecording(
            intent.getStringExtra(EXTRA_RUN_ID),
            intent.getIntExtra(EXTRA_RESULT_CODE, Activity.RESULT_CANCELED),
            intent.getParcelableExtra(EXTRA_RESULT_DATA));
        return START_NOT_STICKY;
    }

    @Override
    public void onDestroy() {
        stopRecordingInternal();
        super.onDestroy();
    }

    private void beginRecording(String runId, int resultCode, Intent projectionData) {
        activeRunId = runId == null ? "" : runId;
        outputPath = buildOutputFile(activeRunId).getAbsolutePath();
        terminalEventSent = false;
        recordingStarted = false;
        finishing = false;
        Log.d(TAG, "beginRecording runId=" + activeRunId + " resultCode=" + resultCode + " outputPath=" + outputPath);

        try {
            DisplayConfig displayConfig = resolveDisplayConfig();
            Log.d(TAG, "beginRecording display width=" + displayConfig.width + " height=" + displayConfig.height + " density=" + displayConfig.densityDpi);
            prepareRecorder(displayConfig, outputPath);

            MediaProjectionManager projectionManager =
                (MediaProjectionManager) getSystemService(Context.MEDIA_PROJECTION_SERVICE);

            if (projectionManager == null || projectionData == null) {
                Log.w(TAG, "beginRecording missing projection manager or data");
                sendError("MediaProjection was unavailable.");
                stopRecordingInternal();
                return;
            }

            mediaProjection = projectionManager.getMediaProjection(resultCode, projectionData);

            if (mediaProjection == null) {
                Log.w(TAG, "beginRecording getMediaProjection returned null");
                sendError("Android did not grant screen capture.");
                stopRecordingInternal();
                return;
            }

            projectionCallback = new MediaProjection.Callback() {
                @Override
                public void onStop() {
                    stopRecordingInternal();
                }
            };
            mediaProjection.registerCallback(projectionCallback, null);

            virtualDisplay = mediaProjection.createVirtualDisplay(
                "CavernVeerfallQaCapture",
                displayConfig.width,
                displayConfig.height,
                displayConfig.densityDpi,
                DisplayManager.VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR,
                mediaRecorder.getSurface(),
                null,
                null);
            Log.d(TAG, "beginRecording created virtual display, starting recorder");
            mediaRecorder.start();
            recordingStarted = true;
            Log.d(TAG, "beginRecording recorder started runId=" + activeRunId);
            QaRecorderBridge.sendEvent("recording_started", activeRunId, outputPath, "QA capture is live.");
        } catch (Exception exception) {
            Log.e(TAG, "beginRecording failed runId=" + activeRunId, exception);
            sendError("Failed to start QA capture: " + exception.getMessage());
            stopRecordingInternal();
        }
    }

    private void stopRecordingInternal() {
        if (finishing) {
            return;
        }

        finishing = true;
        Log.d(TAG, "stopRecordingInternal runId=" + activeRunId + " recordingStarted=" + recordingStarted + " outputPath=" + outputPath);

        if (virtualDisplay != null) {
            virtualDisplay.release();
            virtualDisplay = null;
        }

        if (mediaProjection != null && projectionCallback != null) {
            mediaProjection.unregisterCallback(projectionCallback);
            projectionCallback = null;
        }

        if (mediaRecorder != null) {
            try {
                if (recordingStarted) {
                    mediaRecorder.stop();
                }
            } catch (RuntimeException exception) {
                Log.w(TAG, "stopRecordingInternal mediaRecorder.stop failed", exception);
                File outputFile = new File(outputPath);

                if (outputFile.exists()) {
                    outputFile.delete();
                }
            }

            mediaRecorder.reset();
            mediaRecorder.release();
            mediaRecorder = null;
        }

        if (mediaProjection != null) {
            mediaProjection.stop();
            mediaProjection = null;
        }

        boolean outputExists = outputPath != null && !outputPath.isEmpty() && new File(outputPath).exists();
        Log.d(TAG, "stopRecordingInternal outputExists=" + outputExists + " runId=" + activeRunId);

        if (!terminalEventSent) {
            QaRecorderBridge.sendEvent(
                "recording_stopped",
                activeRunId,
                outputExists ? outputPath : "",
                outputExists ? "Saved QA clip." : "QA capture stopped without a saved clip.");
            terminalEventSent = true;
        }

        stopForeground(true);
        stopSelf();
    }

    private void prepareRecorder(DisplayConfig displayConfig, String absolutePath) throws IOException {
        Log.d(TAG, "prepareRecorder path=" + absolutePath);
        mediaRecorder = new MediaRecorder();
        mediaRecorder.setVideoSource(MediaRecorder.VideoSource.SURFACE);
        mediaRecorder.setOutputFormat(MediaRecorder.OutputFormat.MPEG_4);
        mediaRecorder.setVideoEncoder(MediaRecorder.VideoEncoder.H264);
        int targetBitrate = Math.max(MIN_VIDEO_BITRATE, displayConfig.width * displayConfig.height * 2);
        mediaRecorder.setVideoEncodingBitRate(Math.min(MAX_VIDEO_BITRATE, targetBitrate));
        mediaRecorder.setVideoFrameRate(30);
        mediaRecorder.setVideoSize(displayConfig.width, displayConfig.height);
        mediaRecorder.setOutputFile(absolutePath);
        mediaRecorder.prepare();
    }

    private DisplayConfig resolveDisplayConfig() {
        WindowManager windowManager = (WindowManager) getSystemService(Context.WINDOW_SERVICE);
        DisplayConfig displayConfig = new DisplayConfig();

        if (windowManager == null) {
            displayConfig.width = 1080;
            displayConfig.height = 1920;
            displayConfig.densityDpi = getResources().getDisplayMetrics().densityDpi;
            return displayConfig;
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            Rect bounds = windowManager.getMaximumWindowMetrics().getBounds();
            displayConfig.width = ensureEven(bounds.width());
            displayConfig.height = ensureEven(bounds.height());
            displayConfig.densityDpi = getResources().getConfiguration().densityDpi;
            return displayConfig;
        }

        Display defaultDisplay = windowManager.getDefaultDisplay();
        DisplayMetrics metrics = new DisplayMetrics();
        defaultDisplay.getRealMetrics(metrics);
        displayConfig.width = ensureEven(metrics.widthPixels);
        displayConfig.height = ensureEven(metrics.heightPixels);
        displayConfig.densityDpi = metrics.densityDpi;
        return displayConfig;
    }

    private File buildOutputFile(String runId) {
        String safeRunId = (runId == null || runId.trim().isEmpty())
            ? "qa-" + System.currentTimeMillis()
            : runId.trim();
        return new File(getCaptureDirectory(this), safeRunId + ".mp4");
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
            return;
        }

        NotificationManager notificationManager =
            (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

        if (notificationManager == null) {
            return;
        }

        NotificationChannel channel = new NotificationChannel(
            CHANNEL_ID,
            "Cavern Veerfall QA Capture",
            NotificationManager.IMPORTANCE_LOW);
        channel.setDescription("Active QA gameplay recordings");
        notificationManager.createNotificationChannel(channel);
    }

    private Notification buildNotification(String contentText) {
        Notification.Builder builder = Build.VERSION.SDK_INT >= Build.VERSION_CODES.O
            ? new Notification.Builder(this, CHANNEL_ID)
            : new Notification.Builder(this);

        int iconResId = getApplicationInfo() != null && getApplicationInfo().icon != 0
            ? getApplicationInfo().icon
            : android.R.drawable.presence_video_online;

        builder.setContentTitle("Cavern Veerfall QA Capture");
        builder.setContentText(contentText);
        builder.setOngoing(true);
        builder.setOnlyAlertOnce(true);
        builder.setSmallIcon(iconResId);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            builder.setForegroundServiceBehavior(Notification.FOREGROUND_SERVICE_IMMEDIATE);
        }

        return builder.build();
    }

    private void startForegroundCompat(Notification notification) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            startForeground(
                NOTIFICATION_ID,
                notification,
                ServiceInfo.FOREGROUND_SERVICE_TYPE_MEDIA_PROJECTION);
            return;
        }

        startForeground(NOTIFICATION_ID, notification);
    }

    private int ensureEven(int value) {
        return value % 2 == 0 ? value : value - 1;
    }

    private void sendError(String message) {
        terminalEventSent = true;
        QaRecorderBridge.sendEvent("error", activeRunId, outputPath, message);
    }

    private static final class DisplayConfig {
        int width;
        int height;
        int densityDpi;
    }
}
