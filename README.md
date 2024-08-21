# CET-VR
The Cybersickness Evaluation Testbed VR (or CET-VR) is a tool which helps to select and evaluate the best suitable cybersickness mitigation technique to adopt in a given VR application scenario.

* [Introduction](#introduction)
* [Videos](#videos)
* [Experimental Material](#experimental-material)
* [Builds and Building](#builds)
* [Tool Usage](#tool-usage)
* [Advanced Configuration](#advanced-configuration)
* [Forking Policies](#forking-policies)
* [Citation](#citation)
* [Contacts](#contact)
* [License](#license)
* [Acknowledgements](#acknowledgements)

## Introduction

COMING SOON

## Videos

Some videos showing tasks execution with and without a openly available mitigation technique (VR Tunnelling Pro) for each scenario are available at this [**Link**](http://tiny.cc/64rdzz)

## Experimental Material

Additional material to support the user-study can be found in the [**Experimental Material/**](Experimental%20Material/)
folder. In particular:

- [**Administrator Script.docx**](Experimental%20Material/Administrator%20Script.docx): Script supporting the test
  administrator in providing information to the participant -> COMING SOON
- [**Questionnaire.docx**](Experimental%20Material/Questionnaire.docx): The full questionnaire

## Builds

The application targets only **Windows 10/11** and can be deployed to any VR system compatible with the **OpenXR** API,
although minimum modifications may be necessary to support non-tested hand controllers

- The builds have been tested with **Meta Quest 2/Pro** (via Link/AirLink)** w/ Touch/Touch Pro controllers.

### Build *CET-VR*

Instructions to compile the project:

#### Infos

The project was revamped and tested with [**Unity 2021.3.x (LTS)**](https://unity3d.com/unity/qa/lts-releases?version=2021.3)

The list of unity package dependencies is in the [**manifest file**](UnityProject/Packages/manifest.json) and will be
automatically managed by the Unity editor. You will need also [**Blender**](https://www.blender.org/download/) to be
installed (v2.8+), to have Unity correctly load the blend files in the project.

Before building, it is also necessary to import a set of free assets available in the Unity Asset Store. To facilitate integration, the corresponding .meta files have been provided. Upon opening the project, Unity will delete the .meta files related to missing files, so when importing the asset, you need to avoid overwriting the corresponding .meta files or restore the original version found in the repository. The list of required assets can be found in the [Acknowledgements](#acknowledgements) section.

**Important**: a Unity account is required to access the Unity Asset Store.

#### Prepare to Build

1. In Unity, open an existing project and select the [**UnityProject/**](UnityProject/) folder
    1. If package errors are reported, press Continue, open the Package Manager (Window -> Package Manager) and try
       updating the involved packages, after that restart the project
1. Scene files for each scenario are placed inside the [**UnityProject/Assets/Scenes/**](UnityProject/Assets/Scenes/)
   folder. Before building, open each scene file and perform the bake of the lighting (Window -> Lighting Tab ->
   Generate Lighting).
    1. NOTE: baking is a computationally intensive task, and the time required for completing it can vary based on the
       hardware. By default, Progressive GPU Lightmapper is selected, switch back to CPU
       in case of low-performance graphics adapters. Also, to pick a specific GPU device to be used for the baking
       please refer to the [**official Unity manual page**](https://docs.unity3d.com/2018.4/Documentation/Manual/GPUProgressiveLightmapper.html)

#### Project Build Instructions

To build the project in Unity, follow these steps (N.B. ensure the necessary scenes are correctly ordered and lighting has been baked for all scenarios):

- **Build for Windows Platform**:
  - Open **Build Settings** from **File** > **Build Settings**.
  - Ensure the **Windows** platform is selected. If not, click **Windows** and then select **Switch Platform**.
  - All the the necessary scenes should be already selected and ordered correctly in the **Scenes in Build** list.

- **Build the Project**:
  - Click **Build** and choose the destination folder for the build files.

## Tool Usage

To use the tool, follow the steps outlined below:

### Main Menu Selection

![main](https://github.com/user-attachments/assets/895e3b41-c7eb-40d3-a200-c29a51145e96)

- Select one of the four available scenarios from the main menu.
- Optionally, choose one of the supported mitigation techniques.

You can interact with the interface using the mouse or via keyboard shortcuts:

- **Keyboard Shortcuts**:
  - Each button has a highlighted letter that corresponds to a selectable key.
  - Toggle left-handed mode by pressing `H`.
  - Increase or decrease the value in the user ID input field using the `+` and `-` keys.
  - Cycle between mitigation techniques and controller configurations using the arrow keys:
    - **Up/Down Arrows**: Cycle through mitigation techniques.
    - **Left/Right Arrows**: Cycle through controller configurations (e.g., for Vive controller select `Trackpad` or Quest controller select `Thumbstick`).

### Scenario Loading

Once the desired scenario is selected and loaded, the user will start in VR at the designated starting point, facing a panel with instructions for the experience.

To begin the experience, follow these steps in order:
1. The experimenter must press `CTRL+I` to arm the start.
2. The user must press one of the controller triggers.
   - Alternatively, the experimenter can force the start by pressing the `SPACE` key.

### Shortcuts Information

All available shortcuts are displayed on the experimenter's monitor interface.
  - The interface can be hidden using `CTRL+H`.
  - **Note**: Shortcuts may vary between different scenarios.


### Discomfort Scale Monitoring
  - Every minute, a highly visible indicator will prompt the experimenter to update the Discomfort Scale value (from 1 to 10) by asking the user how they feel.
  - If the score reaches 10, the simulation will be interrupted automatically (equivalent to using the `CTRL+L` combination for cybersickness withdrawal).
  - Always use `CTRL+L` to terminate the experience in case of extreme cybersickness symptoms (**Important**: Do not use `CTRL+Q`, as it does not ensure an orderly termination of the logging component).

## Advanced Configuration

COMING SOON

## Forking Policies

Please contact [Davide Calandra](mailto:davide.calandra@polito.it?subject=[GitHub]%20CET-VR) **BEFORE**
forking the Project

## Citation

Please cite this paper in your publications if it helps your research.

    @article{cetvr,
      author = {Calandra, Davide and Lamberti, Fabrizio},
      journal = {IEEE Transactions on Visualization and Computer Graphics},
      title = {A Testbed for Studying Cybersickness and its Mitigation in Immersive Virtual Reality},
      numpages= {18},
      volume = {(in press)},
      year = {2024}
    }

## Contact

Maintained by [Davide Calandra](mailto:davide.calandra@polito.it?subject=[GitHub]%20CET-VR) - feel free
to contact me!

## License

Experimental material and Unity project are licensed under MIT License

## Acknowledgements

The project requires the following repositories and assets:
- [**GingerVR**](https://github.com/angsamuel/GingerVR): A collection of cybersickness mitigation techniques in VR for Unity, modified to work with OpenXR (included, further bug fixes might be required).
- [**VR Tunnelling Pro**](https://github.com/sigtrapgames/VrTunnellingPro-Unity): An asset for reducing cybersickness via visual effects (included).
- [**Viking Village**](https://assetstore.unity.com/packages/essentials/tutorial-projects/viking-village-urp-29140) (to be imported)
- [**Race Tracks**](https://assetstore.unity.com/packages/3d/environments/roadways/race-tracks-140501) (to be imported)
- [**MS Vehicle System (free version)**](https://assetstore.unity.com/packages/tools/physics/ms-vehicle-system-free-version-90214) (to be imported)
- [**Tiny Robot Packs**](https://assetstore.unity.com/packages/3d/characters/robots/tiny-robots-pack-98930) (to be imported)
- [**Bézier Path Creator**](https://assetstore.unity.com/packages/tools/utilities/b-zier-path-creator-136082) (to be imported)
- [**3D Game Effects Pack Free**](https://assetstore.unity.com/packages/vfx/particles/3d-games-effects-pack-free-42285) (to be imported)
- [**World Material Free**](https://assetstore.unity.com/packages/2d/textures-materials/world-materials-free-150182) (to be imported)

<p align="center" width="100%">
    <img width="800" src="https://vr.polito.it/wp-content/uploads/2021/09/logo_intero_vr_polito_novel.png"> 
</p>
