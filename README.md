# TexturePacker

Requires .NET 4.6 and C# 7

> Small C# app that packs all the png from a folder int a set of square atlas textures

## Features 

* Supports download by specifing a folder by CLI.
* It takes care of current folder hierarchy.

### Supported platforms

Windows and Linux (Mono).

## Usage

Usage: TexturePacker -sp xxx -ft xxx -o xxx [-s xxx] [-b x] [-d]
          -sp | --sourcepath : folder to recursively scan for textures to pack
          -ft | --filetype   : types of textures to pack (*.png only for now)
          -o  | --output     : name of the atlas file to generate
          -s  | --size       : size of 1 side of the atlas file in pixels. Default = 1024
          -b  | --border     : nb of pixels between textures in the atlas. Default = 0
          -d  | --debug      : output debug info in the atlas

Ex: TexturePacker -sp C:\\Temp\\Textures -ft *.png -o C:\\Temp\atlas.txt -s 512 -b 2 --debug

### Packer script

In [folder *scripts*](/scripts/) you can see `packer.cmd` a Windows Shell script **including an example.**

## Issues

Having issues? Just report in [the issue section](/issues). **Thanks for the feedback!**

## Contribute

Fork this repository, make your changes and then issue a pull request. If you find bugs or have new ideas that you do not want to implement yourself, file a bug report.

## Disclaimer

Currently using [`Rectangle` class from Unity-WinForms]() for ForUnity build configuration mode. Thanks to @Meragon!

## Donate

Become a patron, by simply clicking on this button (**very appreciated!**):

[![](https://c5.patreon.com/external/logo/become_a_patron_button.png)](https://www.patreon.com/z3nth10n)

... Or if you prefer a one-time donation:

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/z3nth10n)

## Copyright

Copyright (c) 2019 z3nth10n (United Teamwork Association).

License: MIT