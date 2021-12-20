package com.unity3d.unityconnect.plugins;

import android.content.Context;
import android.view.View;
import android.view.ViewGroup;
import android.widget.RelativeLayout;

import com.dueeeke.videoplayer.player.VideoView;
import com.unity3d.player.UnityPlayer;
import com.unity3d.unityconnect.CustomVideoController;

import java.util.HashMap;
import java.util.Map;

public class AVPlayerPlugin {
    private static AVPlayerPlugin instance;

    public Context context;

    public VideoView videoView;

    public CustomVideoController controller;

    public int limitSeconds = 0;

    public String verifyType = "empty";

    public static AVPlayerPlugin getInstance() {
        synchronized (AVPlayerPlugin.class) {
            if (instance == null) {
                instance = new AVPlayerPlugin();
            }
            return instance;
        }
    }

    public void InitPlayer(String url, String cookie, float left, float top, float width, float height, boolean isPop, String verifyType, int limitSeconds) {
        this.limitSeconds = limitSeconds;
        this.verifyType = verifyType;
        Map<String, String> header = new HashMap<>();
        header.put("Cookie", cookie);
        UnityPlayer.currentActivity.runOnUiThread(() -> {
            controller.showBack = isPop;
            controller.setPlayState(VideoView.STATE_PAUSED);
            controller.setPlayerState(VideoView.PLAYER_NORMAL);
            RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
            if (isPop) {
                lp.height = (int) height;
                // 设置 margin ,此处单位为 px
                lp.setMargins((int) left, 0, 0, 0); 
            } else {
                lp.addRule(RelativeLayout.CENTER_IN_PARENT);
            }
            // 动态改变布局
            videoView.setLayoutParams(lp); 
            if (!url.isEmpty()) {
                videoView.setVisibility(View.VISIBLE);
                videoView.setUrl(url, header);
            }
            if (!isPop) videoView.start();

        });
    }

    public void ConfigPlayer(String url, String cookie) {
        Map<String, String> header = new HashMap<>();
        header.put("Cookie", cookie);
        UnityPlayer.currentActivity.runOnUiThread(() -> {
            controller.show();
            videoView.setVisibility(View.VISIBLE);
            videoView.setUrl(url, header);
            videoView.pause();
        });
    }

    public void VideoRelease() {
        UnityPlayer.currentActivity.runOnUiThread(() -> {
            controller.hiddenUpdateView();
            controller.hiddenNoAccessView();
            videoView.release();
            videoView.setVisibility(View.GONE);
        });

    }

    public void VideoPlay() {
        UnityPlayer.currentActivity.runOnUiThread(() -> videoView.resume());

    }

    public void VideoPause() {
        UnityPlayer.currentActivity.runOnUiThread(() -> videoView.pause());

    }

    public void VideoShow() {
        UnityPlayer.currentActivity.runOnUiThread(() -> videoView.setVisibility(View.VISIBLE));

    }

    public void VideoHidden() {
        UnityPlayer.currentActivity.runOnUiThread(() -> {
            videoView.pause();
            videoView.setVisibility(View.GONE);
        });
    }
}
