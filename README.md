<p align="center">
<img src="Images/AppLogo.png" alt="Unity Connect" width="200">
</p>
<p align="center">
<img src="Images/TextLogo.png" alt="Unity Connect" width="200">
</p>

<h1 align="center"></h1>

#### Unity Connect 社区移动端 App (基于 [UIWidgets 2.0](https://github.com/Unity-Technologies/com.unity.uiwidgets))
[English Version](README_EN.md)

### 预览（iOS & Android）

<span style="border:solid 1px 000;margin:2px;"><img src="Images/Preview_iOS.png"  width="350" ></span>
<span style="border:solid 1px 000;margin:2px;"><img src="Images/Preview_Android.png"  width="350" ></span>

### 下载

<a href="https://developer.unity.cn/connectApp/download" target="_blank"><img height="60px" src="Images/UnityOfficial_ZH.png"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="https://apps.apple.com/cn/app/unity-connect/id1441624698?mt=8" target="_blank"><img height="60px" src="Images/AppStore_ZH.png"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="https://appgallery.huawei.com/#/app/C100771325" target="_blank"><img height="60px" src="Images/AppGallery_ZH.png"></a>

<a href="https://developer.unity.cn/connectApp/download" target="_blank"><img height="250px" src="Images/QRCode_ZH.png">

## 让项目运行
#### 获取源代码
```shell
$ git clone --recurse-submodules https://github.com/Unity-Technologies/ConnectAppCN2.git
$ cd ConnectAppCN2/
```
或者
```shell
$ git clone https://github.com/Unity-Technologies/ConnectAppCN2.git
$ cd ConnectAppCN2/
$ git submodule init
$ git submodule update
```

#### 在 Unity Editor 上运行
  1. 使用 Unity Editor（推荐 Editor 版本： **Unity 2019.4.32f1c1**）打开项目目录 `ConnectAppCN2/`。
  2. 在 Unity Editor 界面的 `Project` 选项卡中找到 `Assets/ConnectApp/Main` 文件目录。
  3. 双击 `ConnectApp.unity` 文件，可以预览项目的预载显示效果。
  4. 双击 `ConnectAppPanel.cs` 文件，以唤起代码编辑软件（例如：`Rider`）。
  5. 单击 `播放按钮` 后，在 `Game` 窗口查看项目运行效果。
  
  > 如果项目运行时有 `Game` 窗口中显示 "No cameras rendering" 字样提示的话，可以在 `Game` 窗口左上角菜单中**去除**勾选 "Warn if No cameras rendering" 即可消除。

#### 在 Android 上运行 (需要安装 Android Studio)
  1. 在顶部菜单找到 `Flie` -> `Build Settings` -> Switch Platform **Android**。
  2. 然后在 `Build Settings` 弹框中**勾选** `Export Project` 选项。然后点击 `Export` 按钮导出到你指定的文件夹内（例如：AndroidProject/）。
  3. 通过 Android Studio 打开安卓项目目录 `<你的指定文件夹>/Unity Connect`。
  4. 进入到 Android Studio 后，你就可以在 Android 模拟器或者真机中运行项目了。

#### 在 iOS 上运行 (需要安装 Xcode)
  1. 在顶部菜单找到 `Flie` -> `Build Settings` -> Switch Platform **iOS**。
  2. 项目中默认的配置是运行在 iOS 真机。如果想在 iOS 模拟器中运行，需要进行如下设置。
      - 找到菜单中的 `Edit` -> `Project Settings` -> `Player` -> `Other Settings`。
      - **取消**选中 `Auto Graphics API` ，然后在 `Graphics APIs` 列表中添加 `OpenGLES2` 和 `OpenGLES3`。
      - `Target SDK` 选择 `Simulator SDK`。
  3. 在 `Build Settings` 弹窗中点击 `Export` 按钮导出到你指定的文件夹内（例如：iOSProject/）。
  4. 双击 `Unity-iPhone.xcodeproj` 文件打开项目。
  5. 如果在真机上运行可能需要修改 `info.plist` 中 `Bundle identifier`。
  6. 如果在模拟器运行需要在 `Editor` 中进行 2. 中的步骤，然后导出项目。

#### 有任何问题都可以在 Issues 中向我们提出来，我们会仔细阅读并尽快回复您。
