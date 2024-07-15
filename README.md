# Visualization Mail (V-Mail)
V-Mail is  a framework of cross-platform applications, interactive techniques, and communication protocols for improved multi-person correspondence about spatial 3D datasets. Please check out our paper: 

Jung Who Nam, Tobias Isenberg, and Daniel F. Keefe. V-Mail: 3D-Enabled Correspondence about Spatial Data on (Almost) All Your Devices. IEEE Transactions on Visualization and Computer Graphics, 30(4):1853–1867, April 2024. doi: [10.1109/TVCG.2022.3229017](https://doi.org/10.1109/TVCG.2022.3229017). An open access paper version is [available on HAL](https://hal.science/hal-03924707).

If you use the code in this repository we would appreciate a citation of our paper.

## Bibtex
```
@article{Nam:2024:V3C,
  author      = {Nam, Jung Who and Tobias Isenberg and Daniel F. Keefe},
  title       = {{V}-{M}ail: {3D}-Enabled Correspondence about Spatial Data on (Almost) All Your Devices},
  journal     = {IEEE Transactions on Visualization and Computer Graphics},
  year        = {2024},
  volume      = {30},
  number      = {4},
  month       = apr,
  pages       = {1853--1867},
  doi         = {10.1109/TVCG.2022.3229017},
  shortdoi    = {10/kt43},
  doi_url     = {https://doi.org/10.1109/TVCG.2022.3229017},
  oa_hal_url  = {https://hal.science/hal-03924707},
  osf_url     = {https://osf.io/qehvs/},
  url         = {https://www.sculpting-vis.org/VMail.html},
  github_url  = {https://github.com/JungWhoNam/VisualizationMail},
  github_url2 = {https://github.com/JungWhoNam/VisualizationMailServer},
  github_url3 = {https://github.com/JungWhoNam/BrainTensorVis/tree/vmail},
  video       = {https://youtu.be/SCTlARovRBY},
}
```

## Related GitHub Repos
* PC/Mac, Android V-Mail Clients (this repo)
    * https://github.com/JungWhoNam/VisualizationMail
* V-Mail Server
    * https://github.com/JungWhoNam/VisualizationMailServer
<!-- * Integration to a data visualization application
    * https://github.com/JungWhoNam/BrainTensorVis/tree/vmail -->

# Running PC/Mac, Android V-Mail Clients
<div id="image-table">
    <table>
	    <tr>
    	    <td style="padding:4px">
        	    <img src="images/Client0.png" width="600"/>
      	    </td>
            <td style="padding:4px">
            	<img src="images/MobileApp1.png" width="600"/>
            </td>
        </tr>
    </table>
</div>

This Unity project provides implemention of two V-Mail clients.  The PC/Mac client is tested with Windows 11 and macOS Ventura. The Android client is tested with Samsung Galaxy S10 (running on Android version 12).

Steps to run these clients:
* First, start the server by following the steps written in [the server repo](https://github.com/JungWhoNam/VisualizationMailServer).
* Open this project in Unity (this is built-in Unity 2022.3.3f1).
* Start the demo PC/Mac client (left)
    - Play `VMail/Demos/Dummy/Dummy Demo - VMail.unity`
    - Use left-mouse to rotate, mouse-wheel to zoom, mouse-wheel click to pan.
    - Press 'r' to change the object's color and scale.
    - Press down or up arrow key to change the geometry, e.g., cube, sphere, cylinder.
* Or start the Android client (right)
    - Play `VMail/_Scenes/VMail Mobile.unity`

# Integrating V-Mail into another Unity project
## Import V-Mail
> Check out the latest relase: https://github.com/JungWhoNam/VisualizationMail/releases

Import `VisualizationMail_v*_*_*.unitypackage` into your project. These directories should be in your project: `VMail`, `StreamingAssets/VMail, StreamingAssets/ffmpeg`, and `Plugins/Android`.

## Configure Unity project
### Add `TextMeshPro`
* When you load V-Mail, a panel will appear with a button to install TextMeshPro. 
* Do `Windows/TextMeshPro/Import TMP Essential Resources` if not.

### Set Resolutions
* The mobile application is designed for S10 (2280x1080).
* The desktop application is designed for (1920x1080).

### Add `Annotation` layer
* Set User Layer 27 to `Annotation`. See [this link](https://docs.unity3d.com/Manual/class-TagManager.html) to see how to add a layer.
* Also make sure `DrawingLayer` property in `DrawingSettings` is set to the Annotation layer (search `ViewerSpatialAnnotation` GameObject in the scene).

## Implement interfaces and abstract classes
> See our example scripts under `VMail/Demos/Dummy/` and use `VMail/Demos/Dummy/VMail-Dummy-Desktop` prefab as a start.

<div id="image-table">
    <table>
	    <tr>
    	    <td style="padding:4px">
        	    <img src="images/ViewerExploratoryVis.png" width="600"/>
      	    </td>
            <td style="padding:4px">
            	<img src="images/dataVisToOnOff.png" width="600"/>
            </td>
        </tr>
    </table>
</div>

* Implement `VisIntegrator` and `VisStateComparator` and assigns these into `ViewerExploratoryVis` (left). 
* Assign GameObject(s) that contain your data visualization to `dataVisToOnOff` in your implementation of `VisIntegrator` (right). These objects will be turned off in the story mode. Also, these will be off when opening a menu panel for uploading or downloading a V-Mail.

## Configure Main Camera
* Set Camera's `Viewport Rect 'Y'` to `0.2593` and `Viewport Rect 'H'` to `0.7407`.
* Set HDR property of Camera to `Use Graphics Settings` (instead of `Off`);

## Integrate the camera navigation
<div id="image-table">
    <table>
	    <tr>
    	    <td style="padding:4px">
        	    <img src="images/CamMoveAroundOrigin.png" width="600"/>
      	    </td>
            <td style="padding:4px">
            	<img src="images/ManagerDesktop.png" width="600"/>
            </td>
        </tr>
    </table>
</div>

* Attach `VMail/_Scripts/Utils/CamMoveAroundOrigin.cs` to a GameObject and configure the parameters. Make sure to add an event to `OnInteracted()` and set `ViewerModeTracker.SetExploreView` (left). 
* Link the GameObject with `CamMoveAroundOrigin` as `Nav` in `ManagerDesktop` (right).

# Changing the V-Mail Server
Change values of these variables to link your own server.
* Change `CodeDirURL` in `VMail/_Scripts/Servers_php/WebIntegration.cs`; the default is "http://localhost/".
* Change `ServerDir` in `VMail/_Scripts/VMailWebManager.cs`; the default is "http://localhost/data/".
* Change `rootDirNameServer` in `VMail/_Scripts/VMailWebManager.cs`; the default is "data".

# Deep Linking
* See https://docs.unity3d.com/Manual/deep-linking.html for setting deep linking features for different platforms.
* See `VMail/_Scripts/Utils/DeepLinkIntegration.cs` for processing a deep URL.

# Known Issues
`ffmpeg` can't be opened because it's from an unidentified developer.
* This error might appear when playing the demo scene in the Editor and uploading changes. This can be solved by running `Assets/StreamingAssets/ffmpeg/Mac/ffmpeg` in Terminal once. This should be not be a problem in build. 

Deep linking does not work in a Windows build.
* Currently, Unity does not support deep linking in Windows builds but supports UWP. However, Pipe does not work in UWP, which is needed for calling `ffmpeg` executable.
