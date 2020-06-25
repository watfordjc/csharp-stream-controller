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

## Solution Projects
Project names and purposes are subject to changes, and this README is likely to lag such changes.

### [NAudioWrapperLibrary](NAudioWrapperLibrary)
The purpose of this library is to interface with the Windows audio devices via the [NAudio library](https://github.com/naudio/NAudio).

### [WebSocketLibrary](WebSocketLibrary)
The purpose of this library is to provide a generic websocket client ([GenericClient](WebSocketLibrary/GenericClient.cs)) using [System.Net.WebSockets.ClientWebSocket](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=netcore-3.1).

### [OBSWebSocketLibrary](OBSWebSocketLibrary)
The purpose of this library is to provide an [obs-websocket](https://github.com/Palakis/obs-websocket) specific implementation of [WebSocketLibrary.GenericClient](WebSocketLibrary/GenericClient.cs).

### [Stream Controller](Stream%20Controller)
This is the user interface stuff, which at the moment is mostly for debugging the libraries.
