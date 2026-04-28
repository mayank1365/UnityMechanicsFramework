# AudioManager Demo Recording Setup Guide

This guide will walk you through setting up the `AudioManager_UMFOSS` from scratch, demonstrating how to make it run in a newly created Unity project, and preparing your walkthrough video for your **UnityMechanicsFramework** contribution.

> [!CAUTION]
> **Avoid Duplicate Definition Errors (CS0101)**
> Do NOT manually drag-and-drop the `Runtime` or `Core` folders into your `Assets` folder if you have already imported the framework as a package. Doing so will create duplicate scripts and prevent your project from compiling. 
> 
> Choose **Module 2A** (Recommended) OR **Module 2B** below—never both!

## 1. Creating a New Unity Project (From Scratch)

1. Open **Unity Hub**.
2. Click the **New project** button in the top right corner.
3. Select the **3D (URP)** or **3D Core** template.
4. Set the **Project Name** (e.g., `AudioManagerDemoProject`).
5. Click **Create project**. Wait for Unity to initialize.

---

## 2A. Recommended: Importing via Package Manager

Use this method if you want to keep your project clean and follow the latest Unity standards.

1. In Unity, go to **Window > Package Manager**.
2. Click the **+** (plus) icon in the top-left and select **Add package from disk...**.
3. Navigate to your cloned `UnityMechanicsFramework` folder and select the `package.json` file.
4. The framework will now appear in your project under the **Packages** folder.

## 2B. Alternative: Manual Import (Legacy)

Use this method *only* if you are not using the Package Manager.

1. Drag the following folders from the repository into your Unity **Assets** folder:
   - `Runtime/Core`
   - `Runtime/Systems/AudioManager`
   - `Samples~/AudioManagerSample` (Copy the folder and rename `Samples~` to `Samples` in Unity).

---

## 3. Setting Up the Demo Scene

1. If using the **Package Manager**:
   - In the Package Manager window, select **Unity Mechanics Framework**.
   - Under the **Samples** tab, click **Import** next to "Audio Manager Sample".
   - The sample will appear in `Assets/Samples/Unity Mechanics Framework/[Version]/Audio Manager Sample`.
2. Navigate to the `Scenes` folder in the imported sample and open `DemoScene.unity`.
3. If prompted, click **Import TMP Essentials** to fix UI text.

## 4. Configuring the AudioManager

The demo scene should have a pre-configured Manager. If you need to set one up manually:

1. Create an Empty GameObject named `AudioManager`.
2. Add the **`AudioManager_UMFOSS`** component.
3. Locate the **`MainAudioSettings.asset`** (Audio Config) inside the sample assets folder.
4. Drag it into the **Audio Config** slot on the component.

## 5. Running the Demo (Live Test)

1. Press **Play**.
2. Use the UI buttons to test:
   - **Play SFX:** Test the pooling logic and pitch variance.
   - **Change Music:** Test the crossfading tracks.
   - **Volume Sliders:** Test independent category control.

## 6. Recording the Walkthrough

Your video should be **3–8 minutes** and cover:
- **Architecture**: Briefly explain the `EventBus` and `MonoSingletonGeneric` patterns.
- **Inspector**: Show the `AudioConfig` ScriptableObject setup.
- **Live Demo**: Show the crossfading and pooling in action.
- **Code**: Briefly show how `EventBus.Subscribe<PlaySFXEvent>` decouples your logic.

---
*Follow these steps carefully to ensure a clean recording for your Pull Request!*
