package com.unity3d.unityconnect;

import android.app.ActivityManager;
import android.app.Application;
import android.content.Context;
import android.os.Process;

import com.dueeeke.videoplayer.ijk.IjkPlayerFactory;
import com.dueeeke.videoplayer.player.VideoViewConfig;
import com.dueeeke.videoplayer.player.VideoViewManager;
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;
import com.unity3d.unityconnect.plugins.CommonPlugin;
import com.unity3d.unityconnect.plugins.QRScanPlugin;
import com.unity3d.unityconnect.plugins.WechatPlugin;

import java.util.List;

public class CustomApplication extends Application {
    @Override
    public void onCreate() {
        super.onCreate();
//         IWXAPI iwxapi = WXAPIFactory.createWXAPI(this, "", true);
//         WechatPlugin.getInstance().iwxapi = iwxapi;
//         WechatPlugin.getInstance().context = this;
//         iwxapi.registerApp("");
        CommonPlugin.mContext = this;
        QRScanPlugin.mContext = this;

        VideoViewManager.setConfig(VideoViewConfig.newBuilder()
                .setPlayOnMobileNetwork(true)
                .setPlayerFactory(IjkPlayerFactory.create(this))
                .setAutoRotate(false)
                .build());
    }
}
