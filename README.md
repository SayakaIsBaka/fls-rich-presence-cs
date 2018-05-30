# fls-rich-presence-cs
C# rewrite of [fls-rich-presence](https://github.com/SayakaIsBaka/fls-rich-presence), originally written in JavaScript (Node.js)

![](https://sayakaisbaka.s-ul.eu/vzEJx3bb.png)

## Requirements

- .NET Framework 4 or above
- Discord and FL Studio (do I really have to say why)

## Download

Grab the lastest release [here!](https://github.com/SayakaIsBaka/fls-rich-presence/releases/lastest)

## Setup

- Download the lastest release above and extract it somewhere
- Run FL Studio and wait for the main window to appear
- Run `fls-rich-presence-cs.exe`

When the program is running, a FL icon is showing in the system tray. Right-clicking it displays a menu with some options such as enable or disable the Rich Presence or killing the program.
The program kills itself if you quit FL Studio.

## Issues

- Timer may act weird sometimes

If you encounter any issues during installation or after, you can join this Discord server: https://discord.gg/Dq68J8r (I don't guarantee that I'll be able to help but I'll still try my best!)

## To Do

- Make something to easily access the debug log
- Updating this to support FL 20 (It should work with the current build but it'll still show FL Studio 12 as presence)

## Credits

- discord-rpc-csharp by Lachee: https://github.com/Lachee/discord-rpc-csharp
