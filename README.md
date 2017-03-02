![gorgon_2 x_logo_full_site](https://cloud.githubusercontent.com/assets/9710137/23449195/86a8701c-fe12-11e6-9079-c4a8997bd311.png)
---

A modular set of libraries useful for graphics and/or video game development.  Gorgon uses Direct 3D 11.1 (via [SharpDX](http://sharpdx.org)) to provide high performance graphics for your applications.

Status
---
[![BuildStatus](https://ci.appveyor.com/api/projects/status/7bwgi85435urivwu/branch/master?svg=true)](https://ci.appveyor.com/project/Tape-Worm/gorgon/)


What’s the licensing? 
-------------------------------------------------

Gorgon is licensed under the [MIT license](http://opensource.org/licenses/MIT).

How far along are you?  Is it done yet?  How about now?
-------------------------------------------------------

Gorgon 3.0 is currently in sporadic development as time allows. At this point, there's no release planned.

What can it do?
---------------

Gorgon provides a set of libraries that are capable of handling pretty much any task thrown at it.  It includes:
  - [x] __[Gorgon.Core](Gorgon/Gorgon.Core)__
  
  Core functionality, plug in support and utility functionality. This is the base library that everything else uses.
  
  - [x] __[Gorgon.FileSystem](Gorgon/Gorgon.FileSystem)__ 
  
  A virtual file system that can mount a directory as a file system root, or using various file system providers, can mount a packed file as a virtual file system root.  This code is based on the popular [PhysFS library](http://icculus.org/physfs/).
  
  By default, Gorgon's basic virtual file system is based on the folder/files on the Windows file system, but using filesystem providers via plug ins, applications can read any type of file storage container can be used if the appropriate plug in is available for it. Gorgon comes with two plug ins for file system providers:
   * __[Gorgon.FileSystem.GorPack](Gorgon/PlugIns/Gorgon.FileSystem.GorPack):__ Gorgon's proprietary packed file system format, using BZip2 compression.
   * __[Gorgon.FileSystem.Zip](Gorgon/PlugIns/Gorgon.FileSystem.Zip):__ Mounts standard .zip files as virtual file systems.
  
  By default, the file system provider
  
  - [x] __[Gorgon.Input](Gorgon/Gorgon.Input)__ 
  
  A flexible input library to handle joysticks/gamepads, keyboard and mouse input. The input library can use events or polling to retrieve data from the various input sources. 
 
 Keyboard and mouse input is provided using the Windows Raw Input API, and joystick/gamepad support is driven by the following plug ins:
 * __[Gorgon.Input.XInput](Gorgon/PlugIns/Gorgon.Input.XInput):__ Support for the XBox 360 controller (and potentially XBox One controller - not tested)
 * __[Gorgon.Input.DirectInput](Gorgon/PlugIns/Gorgon.Input.DirectInput):__ Support for gaming devices that are not covered by the XInput API.
      
  - [x] __[Gorgon.Graphics.Core](Gorgon/Gorgon.Graphics.Core)__ 
  
  A "low-level" graphics API that sits on top of Direct 3D 11.1. Provides a simplified system to build objects such as render targets, swap chains, buffers, etc... The rendering portion of the API provides a simple mechanism to submit batched state and draw information back to the underlying D3D API.
  
  - [x] __[Gorgon.Graphics.Imaging](Gorgon/Gorgon.Graphics.Imaging)__ 
  
  Functionality to read and write image formats. This also contains functionality to use a fluent interface to manipulate images for things like cropping, scaling, etc... 
  
  Gorgon uses [codecs](https://github.com/Tape-Worm/Gorgon/blob/3.0/Gorgon/Gorgon.Graphics.Imaging/Codecs) to read/write images and includes codecs for the following formats:
   * __DDS__ - Direct Draw Surface
   * __TGA__ - Truevision Targa
   * __PNG__ - Portable Network Graphics
   * __JPG__ - Joint Photographic Experts Group
   * __BMP__ - Windows Bitmap
   * __GIF__ - Graphic Interchange Format (supports animated gifs as well)

   In additon to the support above, applications can extend the support for file formats by adding their own custom codec plug in to read/write in their desired format(s).
  
  - [x] __[Gorgon.Graphics.Fonts](Gorgon/Gorgon.Graphics.Fonts)__ 
  
  An extensive bitmap font creation interface (within the graphics module) that supports kerning, outlining of font glyphs, and other customizations to help generate impressive looking text.
  
  Currently Gorgon supports reading and writing of font files through codecs. Support is included for:
   * __GorFont:__ A proprietary binary format for Gorgon.
   * __[BmFont](http://www.angelcode.com/products/bmfont/):__ A popular font file type created by Andreas Jönsson (Note: this support is limited to the text based file format at this time). 

  In addition to the support above, applications can introduce their own codecs to read/write whatever font types they wish by extending the [GorgonFontCodec](Gorgon/Gorgon/Gorgon.Graphics.Fonts/Codecs) type. (Plug in support is pending at this time)
  
  - [ ] __[TBD]__ 
  
  A 2D renderer that sits on top of the graphics module to make developing 2D games/applications much easier.  
  
  - [ ] __[TBD]__ 
  
  An animation module that allows the creation of flexible animations for various types of objects.
  
  - [ ] __[TBD]__ 
  
  A flexible content editor to allow for the creation of sprites, fonts, etc...  

What's required?
----------------

  * [.NET 4.6.1](https://www.microsoft.com/en-ca/download/details.aspx?id=49981)
  * Windows Windows 7, Windows 8, Windows 8.1, Windows 10 or later.
  * Microsoft DirectX 11.1

To compile the library
----------------------

  * [Visual C# 2015](http://msdn.microsoft.com/en-gb/vstudio/) or later. 

To use the library
------------------

  * You may use any .NET 4.6.1 enabled language (e.g. Visual Basic .NET) to write an application with Gorgon.

Source code
-----------

The master branch contains the current release version of Gorgon 2.0.  

The 3.0 branch contains version 3.0 of Gorgon which supports Direct 3D 11.1.  
**Please note:** *This version is still in development and should not be used in a production environment as breaking changes can and will happen.*

Acknowledgements
================

Gorgon uses icons from the Oxygen and Onebit 4 icon sets.

Oxygen icons may be obtained from:  
https://github.com/pasnox/oxygen-icons-png  
http://www.iconarchive.com/show/oxygen-icons-by-oxygen-icons.org.html  

Onebit 4 icons may be obtained from:  
http://www.icojam.com/blog/?p=231  
http://www.iconarchive.com/show/onebit-4-icons-by-icojam.html
