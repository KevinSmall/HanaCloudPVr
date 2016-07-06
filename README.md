HanaCloudVr
===========
A Virtual Reality client bulit using Unity to visualise HANA Cloud Platform (HCP) SensorPhone Data.

For more information:
* About how the VR client app is used:
* http://scn.sap.com/community/business-trends/blog/2016/07/06/visualising-iphone-sensor-data-in-virtual-reality
* About how to get started using Unity (if you want to build this):
* https://developers.google.com/vr/unity/

BUILDS
------
__Important__: In Unity 5.3.4p2 a bug was introduced where VR screen overlays are broken, see https://developers.google.com/vr/unity/release-notes#v080_initial_release.  Until this bug is fixed you need to build using an earlier version of Unity.  This project was built using Unity 5.3.4f1, which you can get from https://unity3d.com/get-unity/download/archive (choose Unity Installer).

### 1) To build for Android
* Runs on Android  4.4.2 API 19 Kit-Kat or above.
* You will need Google VR SDK, Android SDK and Unity:
https://developers.google.com/vr/unity/get-started-android

### 2) To build for iOS
* I have not tried this yet, but iOS builds are supported by Unity, you need a Mac and XCode.
* https://developers.google.com/vr/unity/get-started-ios

To help orientate yourself in the project:

SCENES
------
The main scenes are in the _Scenes folder:
* logingui - GUI screen for initial login (uses old Unity IMGUI code-based layout)
* controls - ignore this scene, it was for testing the new Unity GUI and is not used.
* vrmain - the main scene. It is runnable directly, without going via the logingui scene, and defaults to playing offline. This instant execution is handled by the game object in the scene called "_dummy for instant offline scene execution", which executes the "LoginGui" script with the "run immediately" flag set.

TERMINOLOGY
-----------
Terminology used in scripts:
* World - is the VR world space where player can move around and view data
* Player - is the main camera
* VrEntity - the data points retrieved from HCP are called "VR entities" once they are created as objects in the World

SCRIPTS
-------
All scripts are in the _Scripts folder:
* LoginGui Folder - Initial GUI screen to get system details
* ConnManager Folder - Handles asynch connection to HCP and OData calls, JSON parsing, and transformation of data to make it suitable for display in the World.  The data transformation is done in CloudDataMassList.  Ultimately being able to map inbound sensor values to VR Entity attributes (eg acceleration to color or texture) should be user controllable.
* World Folder - scripts to handle the VR world, the player and the VR entities.
* System Folder - some Extension methods, otherwise mostly not used

PREFABS
-------
Most prefabs are in the Prefabs folder.

EDITING APPEARANCE OF VR ENTITIES
---------------------------------
Notable methods when editing how VR Entities get displayed:
* CloudDataMassList.TransformToWorld() - SensorPhone dataset is enriched with eg normalised coordinates, acceleration etc
* WorldManager.CreateVrEntity() - VR Entities are assigned properties from the dataset when they are created
* VrEntityManager.Start() - At display time VR entities can have values changed, eg highlighted or not


