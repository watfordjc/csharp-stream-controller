# csharp-stream-controller
My WIP stream controller for live streaming.

## Work In Progress
All of the projects in this Visual Studio solution are not yet in a usable state and the program is nowhere near the point where I use it for streaming.

### Goals
The app I currently use for streaming is buggy, and is akin to a stream deck. While it does have some options for integration with the operating system and other programs, they are rather limited.

The main purpose of the solution in this repository is to create a connectivity hub for streaming. These are the things I am currently considering interfacing with:
* OBS Studio (via [obs-websocket](https://github.com/Palakis/obs-websocket))
* Windows audio devices (via [NAudio](https://github.com/naudio/NAudio))
* Windows networking devices
* Twitter
* Twitch
* An Android app (via websockets)
* A smartwatch app for live heart rate and stress level (how: TBD)

## Solution Projects
Project names and purposes are subject to changes, and this README is likely to lag such changes.

### [MessageToImageLibrary (external repository)](https://github.com/watfordjc/csharp-message-to-image-library)
The purpose of this library is to interface with Direct2D and DirectWrite so that messages such as Tweets can be converted to images with colour emoji support (among other things).

This is an offshoot repository containing a C# library project **[MessageToImageLibrary](https://github.com/watfordjc/csharp-message-to-image-library/tree/master/MessageToImageLibrary)** and a C++ project **[Direct2DWrapper](https://github.com/watfordjc/csharp-message-to-image-library/tree/master/Direct2DWrapper)**.

If *MessageToImageLibrary.dll* is in directory ```./```, it expects to find *Direct2DWrapper.dll* at the relative location ```./lib/Direct2DWrapper.dll```

The MessageToImageLibrary repository is under a separate release cycle but is under the same library versioning scheme as this repository.

### [NAudioWrapperLibrary](NAudioWrapperLibrary)
The purpose of this library is to interface with the Windows audio devices via the [NAudio library](https://github.com/naudio/NAudio).

Its short name for milestones is **NAWLib**.

### [NetworkingWrapperLibrary](NetworkingWrapperLibrary)
The purpose of this library is to interface with the Windows networking devices.

Its short name for milestones is **NWLib**.

### [OBSWebSocketLibrary](OBSWebSocketLibrary)
The purpose of this library is to provide an [obs-websocket](https://github.com/Palakis/obs-websocket) specific implementation of [WebSocketLibrary.GenericClient](WebSocketLibrary/GenericClient.cs).

Its short name for milestones is **OBSWSLib**.

### [Setup](Setup)
This library is for generating installers/releases.

### [SharedModels](SharedModels)
This library is for classes that are used by other projects/libraries and will eventually be for classes that aren't dependent on other projects and are used across projects.

Its short name for milestones is **SMLib**.

### [Stream Controller](Stream%20Controller)
This is the user interface project. All of the windows are contained in this project and it is also the thing the Setup project creates installers from.

Its short name for milestones is **SC**.

### [WebSocketLibrary](WebSocketLibrary)
The purpose of this library is to provide a generic websocket client ([GenericClient](WebSocketLibrary/GenericClient.cs)) using [System.Net.WebSockets.ClientWebSocket](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=netcore-3.1).

Its short name for milestones is **WSLib**.

## Branches
### [master](../../tree/master)
This branch is, for the time being, the only one suitable for forking from.

### Feature Branches

Feature branches for issues follow the naming convention **feature/issue-issueNumber/projectShortname-branchNumer** where:
  * issueNumber is the Github issue number in this repository;
  * projectShortname is:
    * a11y for Accessibility
    * audio for Windows Audio
    * net for Networking
    * obs for OBS
    * twitch for Twitch
    * twitter for Twitter
    * gui for UI
  * branchNumber is the version number of that Repository Project's branch of the issue.
  
  These branches are subject to rebasing, forced pushes, cherry-picking, and abandonment.
  
  Feature branches get deleted after the master branch is fast-forwarded to the HEAD of the feature branch and the issue gets closed.
