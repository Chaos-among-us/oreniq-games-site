package com.oreniq.endlessdodge.qa;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.media.projection.MediaProjectionManager;
import android.os.Bundle;
import android.util.Log;

public final class QaRecorderPermissionActivity extends Activity {
    public static final String EXTRA_RUN_ID = "com.oreniq.endlessdodge.qa.EXTRA_RUN_ID";
    private static final String TAG = "QaAudioTrace";

    private static final int REQUEST_CAPTURE = 4107;

    private String runId;
    private boolean requestedCapture;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        overridePendingTransition(0, 0);
        runId = getIntent() != null ? getIntent().getStringExtra(EXTRA_RUN_ID) : "";
        Log.d(TAG, "PermissionActivity onCreate runId=" + runId + " requestedCapture=" + requestedCapture);

        if (savedInstanceState != null) {
            requestedCapture = savedInstanceState.getBoolean("requested_capture", false);
        }

        if (requestedCapture) {
            return;
        }

        MediaProjectionManager projectionManager =
            (MediaProjectionManager) getSystemService(Context.MEDIA_PROJECTION_SERVICE);

        if (projectionManager == null) {
            Log.w(TAG, "PermissionActivity missing MediaProjectionManager");
            QaRecorderBridge.sendEvent("error", runId, null, "MediaProjectionManager was unavailable.");
            finish();
            overridePendingTransition(0, 0);
            return;
        }

        requestedCapture = true;
        Log.d(TAG, "PermissionActivity launching capture intent runId=" + runId);
        startActivityForResult(projectionManager.createScreenCaptureIntent(), REQUEST_CAPTURE);
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        super.onSaveInstanceState(outState);
        outState.putBoolean("requested_capture", requestedCapture);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode != REQUEST_CAPTURE) {
            return;
        }

        Log.d(TAG, "PermissionActivity onActivityResult runId=" + runId + " resultCode=" + resultCode + " hasData=" + (data != null));

        if (resultCode == RESULT_OK && data != null) {
            QaRecordingService.startFromConsent(getApplicationContext(), runId, resultCode, data);
        } else {
            QaRecorderBridge.sendEvent("denied", runId, null, "Screen capture was denied for this run.");
        }

        finish();
        overridePendingTransition(0, 0);
    }
}
