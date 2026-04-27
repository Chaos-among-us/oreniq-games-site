package com.oreniq.endlessdodge.qa;

import android.app.Activity;
import android.content.ClipData;
import android.content.ContentResolver;
import android.content.ContentUris;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.provider.BaseColumns;
import android.provider.MediaStore;
import android.util.Log;

import org.json.JSONObject;

import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

public final class QaRecorderBridge {
    private static final String TAG = "QaAudioTrace";
    private static final String DEFAULT_RECEIVER_OBJECT = "QaTestingSystemRuntime";
    private static final String DEFAULT_RECEIVER_METHOD = "OnNativeQaEvent";

    private static String receiverObjectName = DEFAULT_RECEIVER_OBJECT;
    private static String receiverMethodName = DEFAULT_RECEIVER_METHOD;

    private QaRecorderBridge() {
    }

    public static void requestCaptureConsent(String runId, String receiverObject, String receiverMethod) {
        receiverObjectName = isBlank(receiverObject) ? DEFAULT_RECEIVER_OBJECT : receiverObject;
        receiverMethodName = isBlank(receiverMethod) ? DEFAULT_RECEIVER_METHOD : receiverMethod;
        Log.d(TAG, "requestCaptureConsent runId=" + runId + " receiver=" + receiverObjectName + "/" + receiverMethodName);

        Activity activity = getCurrentActivity();

        if (activity == null) {
            Log.w(TAG, "requestCaptureConsent missing current activity");
            sendEvent("error", runId, null, "Unity activity was unavailable for QA capture.");
            return;
        }

        Intent intent = new Intent(activity, QaRecorderPermissionActivity.class);
        intent.putExtra(QaRecorderPermissionActivity.EXTRA_RUN_ID, runId);
        activity.startActivity(intent);
    }

    public static void stopRecording(String reason) {
        Context context = getContext();

        if (context == null) {
            sendEvent("error", null, null, "Android context was unavailable while stopping QA capture.");
            return;
        }

        QaRecordingService.stopRecording(context, reason);
    }

    public static boolean shareCapturePackage(
        String capturePath,
        String reportPath,
        String chooserTitle,
        String subject,
        String body) {
        Activity activity = getCurrentActivity();

        if (activity == null) {
            return false;
        }

        ArrayList<Uri> uris = new ArrayList<>();
        File captureFile = safeFile(capturePath);
        File reportFile = safeFile(reportPath);

        if (captureFile != null && captureFile.exists()) {
            uris.add(buildUri(activity, captureFile));
        }

        if (reportFile != null && reportFile.exists()) {
            uris.add(buildUri(activity, reportFile));
        }

        Intent shareIntent;

        if (uris.size() > 1) {
            shareIntent = new Intent(Intent.ACTION_SEND_MULTIPLE);
            shareIntent.setType("*/*");
            shareIntent.putParcelableArrayListExtra(Intent.EXTRA_STREAM, uris);
        } else {
            shareIntent = new Intent(Intent.ACTION_SEND);

            if (uris.size() == 1) {
                shareIntent.putExtra(Intent.EXTRA_STREAM, uris.get(0));
                shareIntent.setType(captureFile != null && captureFile.exists() ? "video/mp4" : "text/plain");
            } else {
                shareIntent.setType("text/plain");
            }
        }

        if (!isBlank(subject)) {
            shareIntent.putExtra(Intent.EXTRA_SUBJECT, subject);
        }

        if (!isBlank(body)) {
            shareIntent.putExtra(Intent.EXTRA_TEXT, body);
        }

        if (!uris.isEmpty()) {
            shareIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            ClipData clipData = ClipData.newUri(activity.getContentResolver(), "qa_package", uris.get(0));

            for (int i = 1; i < uris.size(); i++) {
                clipData.addItem(new ClipData.Item(uris.get(i)));
            }

            shareIntent.setClipData(clipData);
        }

        Intent chooserIntent = Intent.createChooser(
            shareIntent,
            isBlank(chooserTitle) ? "Share Cavern Veerfall QA package" : chooserTitle);
        activity.startActivity(chooserIntent);
        return true;
    }

    public static boolean deleteCapture(String capturePath) {
        File captureFile = safeFile(capturePath);
        return captureFile != null && captureFile.exists() && captureFile.delete();
    }

    public static String exportCapturePackageToDownloads(String capturePath, String reportPath, String baseName) {
        Context context = getContext();

        if (context == null) {
            return "";
        }

        File captureFile = safeFile(capturePath);
        File reportFile = safeFile(reportPath);

        if ((captureFile == null || !captureFile.exists()) && (reportFile == null || !reportFile.exists())) {
            return "";
        }

        String fileName = sanitizeFileName(isBlank(baseName) ? "cavern-veerfall-qa-" + System.currentTimeMillis() : baseName) + ".zip";

        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
                return exportPackageWithMediaStore(context, captureFile, reportFile, fileName);
            }

            return exportPackageWithLegacyDownloads(captureFile, reportFile, fileName);
        } catch (Exception exception) {
            return "";
        }
    }

    public static boolean deleteExportedPackage(String exportUri) {
        if (isBlank(exportUri)) {
            return false;
        }

        Context context = getContext();

        if (context == null) {
            return false;
        }

        try {
            Uri uri = Uri.parse(exportUri);

            if ("content".equalsIgnoreCase(uri.getScheme())) {
                return context.getContentResolver().delete(uri, null, null) > 0;
            }
        } catch (Exception ignored) {
            // Fall back to file deletion below.
        }

        File exportFile = safeFile(exportUri);
        return exportFile != null && exportFile.exists() && exportFile.delete();
    }

    public static boolean deleteAllCaptures() {
        Context context = getContext();

        if (context == null) {
            return false;
        }

        File captureDirectory = QaRecordingService.getCaptureDirectory(context);
        File[] captureFiles = captureDirectory.listFiles();
        boolean deletedAny = false;

        if (captureFiles == null) {
            return false;
        }

        for (File captureFile : captureFiles) {
            if (captureFile != null && captureFile.isFile() && captureFile.getName().endsWith(".mp4")) {
                deletedAny = captureFile.delete() || deletedAny;
            }
        }

        return deletedAny;
    }

    public static boolean deleteAllExportedPackages() {
        Context context = getContext();

        if (context == null) {
            return false;
        }

        boolean deletedAny = false;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            ContentResolver resolver = context.getContentResolver();
            Uri collection = MediaStore.Downloads.EXTERNAL_CONTENT_URI;
            String[] projection = { BaseColumns._ID };
            String selection = MediaStore.MediaColumns.RELATIVE_PATH + "=?";
            String[] selectionArgs = { getExportRelativePath() };

            try (Cursor cursor = resolver.query(collection, projection, selection, selectionArgs, null)) {
                if (cursor != null) {
                    int idColumn = cursor.getColumnIndexOrThrow(BaseColumns._ID);

                    while (cursor.moveToNext()) {
                        long id = cursor.getLong(idColumn);
                        Uri uri = ContentUris.withAppendedId(collection, id);
                        deletedAny = resolver.delete(uri, null, null) > 0 || deletedAny;
                    }
                }
            }
        } else {
            File exportDirectory = getLegacyExportDirectory();
            File[] exportFiles = exportDirectory.listFiles();

            if (exportFiles != null) {
                for (File exportFile : exportFiles) {
                    if (exportFile != null && exportFile.isFile() && exportFile.getName().endsWith(".zip")) {
                        deletedAny = exportFile.delete() || deletedAny;
                    }
                }
            }
        }

        return deletedAny;
    }

    public static int getStoredCaptureCount() {
        Context context = getContext();

        if (context == null) {
            return 0;
        }

        File captureDirectory = QaRecordingService.getCaptureDirectory(context);
        File[] captureFiles = captureDirectory.listFiles();

        if (captureFiles == null) {
            return 0;
        }

        int count = 0;

        for (File captureFile : captureFiles) {
            if (captureFile != null && captureFile.isFile() && captureFile.getName().endsWith(".mp4")) {
                count++;
            }
        }

        return count;
    }

    public static int getStoredExportCount() {
        Context context = getContext();

        if (context == null) {
            return 0;
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            ContentResolver resolver = context.getContentResolver();
            Uri collection = MediaStore.Downloads.EXTERNAL_CONTENT_URI;
            String[] projection = { BaseColumns._ID };
            String selection = MediaStore.MediaColumns.RELATIVE_PATH + "=?";
            String[] selectionArgs = { getExportRelativePath() };

            try (Cursor cursor = resolver.query(collection, projection, selection, selectionArgs, null)) {
                return cursor == null ? 0 : cursor.getCount();
            }
        }

        File exportDirectory = getLegacyExportDirectory();
        File[] exportFiles = exportDirectory.listFiles();

        if (exportFiles == null) {
            return 0;
        }

        int count = 0;

        for (File exportFile : exportFiles) {
            if (exportFile != null && exportFile.isFile() && exportFile.getName().endsWith(".zip")) {
                count++;
            }
        }

        return count;
    }

    static void sendEvent(String status, String runId, String path, String message) {
        try {
            Log.d(TAG, "sendEvent status=" + status + " runId=" + runId + " path=" + path + " message=" + message);
            JSONObject payload = new JSONObject();
            payload.put("status", status == null ? "" : status);
            payload.put("runId", runId == null ? "" : runId);
            payload.put("path", path == null ? "" : path);
            payload.put("message", message == null ? "" : message);
            Class<?> unityPlayerClass = Class.forName("com.unity3d.player.UnityPlayer");
            Method unitySendMessageMethod = unityPlayerClass.getMethod(
                "UnitySendMessage",
                String.class,
                String.class,
                String.class);
            unitySendMessageMethod.invoke(null, receiverObjectName, receiverMethodName, payload.toString());
        } catch (Exception exception) {
            try {
                Class<?> unityPlayerClass = Class.forName("com.unity3d.player.UnityPlayer");
                Method unitySendMessageMethod = unityPlayerClass.getMethod(
                    "UnitySendMessage",
                    String.class,
                    String.class,
                    String.class);
                unitySendMessageMethod.invoke(
                    null,
                    receiverObjectName,
                    receiverMethodName,
                    "{\"status\":\"error\",\"message\":\"Failed to serialize QA recorder state.\"}");
            } catch (Exception ignored) {
                // If Unity isn't reachable here, there isn't a safer reporting fallback.
            }
        }
    }

    private static Context getContext() {
        Activity activity = getCurrentActivity();
        return activity != null ? activity.getApplicationContext() : null;
    }

    private static Uri buildUri(Context context, File file) {
        try {
            Class<?> fileProviderClass = Class.forName("androidx.core.content.FileProvider");
            Method getUriForFileMethod = fileProviderClass.getMethod(
                "getUriForFile",
                Context.class,
                String.class,
                File.class);
            return (Uri) getUriForFileMethod.invoke(
                null,
                context,
                context.getPackageName() + ".qa.fileprovider",
                file);
        } catch (Exception exception) {
            throw new IllegalStateException("Unable to access FileProvider for QA sharing.", exception);
        }
    }

    private static File safeFile(String absolutePath) {
        if (isBlank(absolutePath)) {
            return null;
        }

        return new File(absolutePath);
    }

    private static String exportPackageWithMediaStore(
        Context context,
        File captureFile,
        File reportFile,
        String fileName) throws IOException {
        ContentResolver resolver = context.getContentResolver();
        ContentValues values = new ContentValues();
        values.put(MediaStore.MediaColumns.DISPLAY_NAME, fileName);
        values.put(MediaStore.MediaColumns.MIME_TYPE, "application/zip");
        values.put(MediaStore.MediaColumns.RELATIVE_PATH, getExportRelativePath());
        values.put(MediaStore.MediaColumns.IS_PENDING, 1);

        Uri uri = resolver.insert(MediaStore.Downloads.EXTERNAL_CONTENT_URI, values);

        if (uri == null) {
            return "";
        }

        boolean success = false;

        try (OutputStream outputStream = resolver.openOutputStream(uri, "w")) {
            if (outputStream == null) {
                throw new IOException("Unable to open Downloads output stream.");
            }

            writePackageZip(outputStream, captureFile, reportFile);
            success = true;
            return uri.toString();
        } finally {
            ContentValues completedValues = new ContentValues();
            completedValues.put(MediaStore.MediaColumns.IS_PENDING, 0);
            resolver.update(uri, completedValues, null, null);

            if (!success) {
                resolver.delete(uri, null, null);
            }
        }
    }

    private static String exportPackageWithLegacyDownloads(
        File captureFile,
        File reportFile,
        String fileName) throws IOException {
        File exportDirectory = getLegacyExportDirectory();

        if (!exportDirectory.exists()) {
            exportDirectory.mkdirs();
        }

        File outputFile = new File(exportDirectory, fileName);

        try (OutputStream outputStream = new BufferedOutputStream(new java.io.FileOutputStream(outputFile))) {
            writePackageZip(outputStream, captureFile, reportFile);
        }

        return outputFile.getAbsolutePath();
    }

    private static void writePackageZip(OutputStream outputStream, File captureFile, File reportFile) throws IOException {
        try (ZipOutputStream zipOutputStream = new ZipOutputStream(new BufferedOutputStream(outputStream))) {
            addFileToZip(zipOutputStream, captureFile, captureFile != null ? captureFile.getName() : null);
            addFileToZip(zipOutputStream, reportFile, reportFile != null ? reportFile.getName() : null);
            zipOutputStream.finish();
        }
    }

    private static void addFileToZip(ZipOutputStream zipOutputStream, File sourceFile, String entryName) throws IOException {
        if (sourceFile == null || !sourceFile.exists() || isBlank(entryName)) {
            return;
        }

        ZipEntry entry = new ZipEntry(entryName);
        zipOutputStream.putNextEntry(entry);

        try (FileInputStream inputStream = new FileInputStream(sourceFile)) {
            byte[] buffer = new byte[8192];
            int read;

            while ((read = inputStream.read(buffer)) != -1) {
                zipOutputStream.write(buffer, 0, read);
            }
        }

        zipOutputStream.closeEntry();
    }

    private static File getLegacyExportDirectory() {
        File downloadsDirectory = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS);
        return new File(downloadsDirectory, "CavernVeerfallQA");
    }

    private static String getExportRelativePath() {
        return Environment.DIRECTORY_DOWNLOADS + "/CavernVeerfallQA/";
    }

    private static String sanitizeFileName(String value) {
        return value.replaceAll("[^a-zA-Z0-9._-]", "_");
    }

    private static boolean isBlank(String value) {
        return value == null || value.trim().isEmpty();
    }

    private static Activity getCurrentActivity() {
        try {
            Class<?> unityPlayerClass = Class.forName("com.unity3d.player.UnityPlayer");
            Field currentActivityField = unityPlayerClass.getField("currentActivity");
            return (Activity) currentActivityField.get(null);
        } catch (Exception exception) {
            return null;
        }
    }
}
