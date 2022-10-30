# Overview

The LoR Watcher is a downloadable Windows application that tracks your Legends of Runeterra games. The games can be viewed in the browser at 'http://lor.watcher'
which the LoR Watcher application hosts.

##### Table of Contents  
[Install](#install)  
&nbsp;&nbsp;[Tray Icon](#tray-icon)  
[UI](#ui)  
&nbsp;&nbsp;[Game Type](#game-type)  
&nbsp;&nbsp;[Basic Replay](#basic-replay)  

# Install
To install, go to the [Releases](https://github.com/Marcus-Smallman/LoRWatcher/releases) page and click on the latest release.
You should then be able to click on the Installer executable (*e.g. LoR.Watcher.Installer.0.1.6.exe*).
The installer should then begin downloading.

Once downloaded, run the installer. This installer will take a few minutes to finish as it has to download the LoR assets.

Upon a successful install, you should now be able to run the LoR Watcher and access the UI to view your match history at 'http://lor.watcher'.

> **NOTE**: This application does not currently retrieve any data from your Riot Games account. Therefore it will only store and retrieve games it has gathered.

## Tray Icon
There is also a useful tray icon that will appear to show that you have it running.  
If you right click on the tray Icon, the following information/options appear:
 - The version of the LoR Watcher you have installed
 - The state of the LoR game client (*e.g. **Offline** if you do not currently have the game client running,
 **Menus** If you have the game client running but are not playing a game
and **In Progress** if you are currently playing a match*)
 - A quick link to the UI in the browser (http://lor.watcher)
 - An option to modify some settings
 - An option to check for updates
 - An option to exit the LoR Watcher
 
 ![image](https://user-images.githubusercontent.com/10182314/197396620-0da292ea-043b-4bc2-a558-d8972b376fb9.png)

# UI
When you access the UI for the first time without playing a game, there will be no match history.  
<img src="https://user-images.githubusercontent.com/10182314/187091200-142139ac-aa1d-4095-81cb-6c008afaafcf.png" width="800">  
After playing a few games you will start to see your match history with some overall stats.
<img src="https://user-images.githubusercontent.com/10182314/197385717-e1aae431-944b-4759-95c6-68d68f98fe47.png" width="800">  
You can then view the match you played by clicking on the record in the table.  
<img src="https://user-images.githubusercontent.com/10182314/187091440-723f5d0b-9073-4ce7-a438-5a4e13a16766.png" width="800">  

## Game Type
For the LoR Watcher tool to know what game type a match is, it needs to sync with the Riot APIs to retrieve this information. In order for the LoR Watcher to do this, you need to provide your tag line. This can be found in the LoR Game Client:
<img src="https://user-images.githubusercontent.com/10182314/197396459-d842a218-ceb7-496a-b50a-9e872994f8f7.png" width="800">  
This can then be set in the LoR Watcher UI:
<img src="https://user-images.githubusercontent.com/10182314/197396511-5e4fbfe4-d05a-41e8-ba73-1aca6c21328e.png" width="800">  
<img src="https://user-images.githubusercontent.com/10182314/197396560-ef212358-00b9-4fe8-8228-d81b05a35776.png" width="200">  



## Basic Replay

Basic replay is feature that can be veiwed by going to one of your matches and scrolling to the bottom of the page. You will then be presented with a timeline of various snapshots of your match.
<img src="https://user-images.githubusercontent.com/10182314/191859367-befc8f53-11f0-420d-a056-af8fe36d3dc3.png" width="800">  
You can then click on any one of these snapshot to see the status of your match at that time. 
<img src="https://user-images.githubusercontent.com/10182314/191859241-c82316a8-5108-4d40-bbf5-89d00f3d9cfa.png" width="800">  
