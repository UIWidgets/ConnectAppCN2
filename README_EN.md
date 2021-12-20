<p align="center">
<img src="Images/AppLogo.png" alt="Unity Connect" width="200">
</p>
<p align="center">
<img src="Images/TextLogo.png" alt="Unity Connect" width="200">
</p>

<h1 align="center"></h1>

#### A Unity Connect community app made with Unity (Based on the [UIWidgets 2.0](https://github.com/UnityTech/UIWidgets))

### Preview（iOS & Android）

<span style="border:solid 1px 000;margin:2px;"><img src="Images/Preview_iOS.png"  width="350" ></span>
<span style="border:solid 1px 000;margin:2px;"><img src="Images/Preview_Android.png"  width="350" ></span>

### Download

<a href="https://developer.unity.cn/connectApp/download" target="_blank"><img height="60px" src="Images/UnityOfficial_ZH.png"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="https://apps.apple.com/cn/app/unity-connect/id1441624698?mt=8" target="_blank"><img height="60px" src="Images/AppStore_ZH.png"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="https://appgallery.huawei.com/#/app/C100771325" target="_blank"><img height="60px" src="Images/AppGallery_ZH.png"></a>

<a href="https://developer.unity.cn/connectApp/download" target="_blank"><img height="250px" src="Images/QRCode_ZH.png">

## Get started
#### Get repo
```shell
$ git clone --recurse-submodules https://github.com/Unity-Technologies/ConnectAppCN2.git
$ cd ConnectAppCN2/
```
or
```shell
$ git clone https://github.com/Unity-Technologies/ConnectAppCN2.git
$ cd ConnectAppCN2/
$ git submodule init
$ git submodule update
```
#### Running in Unity Editor
  1. Use Unity Editor (**Unity 2019.4.32f1c1**) open the `ConnectAppCN2/`.
  2. Find `Assets/ConnectApp/Main` folder in `Project` Tab.
  3. Double click the `ConnectApp.unity` for preview the project.
  4. Double click the `ConnectAppPanel.cs` evoke `Rider` for Edit Code.
  5. Click the `Play` button and you will see the project running in the `Game` window.
  PS: if "Game" window show "No cameras rendering" Toast, You can click the menu in the upper right corner of the "Game" window and uncheck the "Warn if No cameras rendering" option to make it disappear.
  
#### Running in Android (need to install Android Studio)
  1. Unity Editor Menu `Flie` -> `Build Settings` -> Switch Platform **Android**.
  2. `Build Settings` -> Check `Export Project`. Then click the `Export` button at the bottom, export to the path you want (such as AndroidProject/).
  3. Open `<your path>/Unity Connect` the project file from Android Studio.
  4. You can run it on the Android Simulator or on the Android Device.
  
#### Running in iOS (need to install Xcode)
  1. Unity Editor Menu `Flie` -> `Build Settings` -> Switch Platform **iOS**.
  2. The default configuration in the project is to run in the iPhone Device, if you want to run in the iOS Simulator requires additional settings.
      1. Find Unity Editor Menu `Edit` -> `Project Settings` -> `Player` -> `Other Settings`. 
      2. Uncheck the `Auto Graphics API`, add `OpenGLES2` and `OpenGLES3` to `Graphics APIs`.
      3. Target SDK select Simulator SDK.
  3. In `Build Settings` click the `Export` button at the bottom, export to the path you want (such as iOSProject/).
  4. Double click the `Unity-iPhone.xcodeproj` open project.
  5. If you are running in a iOS device, you may need to modify the `Bundle identifier` in `info.plist`.
  6. You can run it on the iOS Simulator(View 2.) or on the iOS Device.
  
Any questions can be raised in the issue, we will reply as soon as possible.
