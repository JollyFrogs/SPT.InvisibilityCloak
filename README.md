# SPT.InvisibilityCloak 

## Compile instructions

#### Download SPT.InvisibilityCloak sourcecode:
<a href="https://github.com/JollyFrogs/SPT.InvisibilityCloak/archive/refs/heads/main.zip">SPT.InvisibilityCloak-main.zip</a>

#### Download Visual Studio 2022 Community Edition and .Net Framework 4.8.1:

```
winget install --id Microsoft.VisualStudio.2022.Community --exact --silent --custom "--add Microsoft.VisualStudio.Workload.NativeDesktop --add Microsoft.VisualStudio.Workload.ManagedDesktop" --accept-source-agreements --accept-package-agreements --disable-interactivity
winget install -e --id Microsoft.DotNet.Framework.DeveloperPack_4
```
>[!NOTE]
>The installation of Visual Studio 2022 Community can take a while if you have low bandwidth, please be patient.

---
>[!IMPORTANT]
>After installing both .Net and Visual Studio, you must reboot your computer

---

Unzip the file 'SPT.InvisibilityCloak-main.zip' to a folder on your desktop

Right-click 'InvisibilityCloak.sln' -> Open with -> 'Visual Studio 2022' -> Click 'Always'
Either click 'Skip this for now' or sign in if you want

Customize your experience:
  Development settings    : Visual C#
  Choose your color theme : Dark

Click 'Start Visual Studio'
Close the 'What's New' tab

In the top bar, click the 'Debug' dropdown and set to 'Release'
In the top menu, click 'Build' -> 'Rebuild Solution'

After a few seconds, Visual Studio 2022 should say "Rebuild All succeeded" in the very bottom left

Close Visual Studio 2022

## Usage

>[!NOTE]
>You can download a precompiled 'InvisibilityCloak.dll' from <a href="https://github.com/JollyFrogs/SPT.InvisibilityCloak/releases">here</a>

Copy your compiled file './bin/Release/InvisibilityCloak.dll' to '/SPT/Escape From Tarkov/BepInEx/plugins/InvisibilityCloak.dll'

Start 'SPT/Escape From Tarkov/SPT.Server.exe'
Start 'SPT/Escape From Tarkov/SPT.Launcher.exe'

After the game loads and you have created your character, press F12 to open the BepInEx menu
You should see 'InvisibilityCloak 3.10.3.0' listed

Click on the entry 'InvisibilityCloak 3.10.3.0' and make sure it is 'enabled'

Start the game - enemies should ignore you completely.
