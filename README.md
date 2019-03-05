![gorgon_2 x_logo_full_site](https://cloud.githubusercontent.com/assets/9710137/23449195/86a8701c-fe12-11e6-9079-c4a8997bd311.png) 

A modular set of libraries useful for graphics and/or video game development.  Gorgon uses Direct 3D 11.4 (via [SharpDX](http://sharpdx.org)) to provide high performance graphics for your applications.

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
  
  - [x] __[Gorgon.Windows](Gorgon/Gorgon.Windows)__
  
  Core Windows specific functionality. Provides UI functionality, such as custom message box dialogs, timing functionality using QPC and/or the windows multimedia timer and various other bits of utility/diagnostic functionality. 
  
  - [x] __[Gorgon.FileSystem](Gorgon/Gorgon.FileSystem)__ 
  
  A virtual file system that can mount a directory as a file system root, or using various file system providers, can mount a packed file as a virtual file system root.  This code is based on the popular [PhysFS library](http://icculus.org/physfs/).
  
  By default, Gorgon's basic virtual file system is based on the folder/files on the Windows file system, but using filesystem providers via plug ins, applications can read any type of file storage container can be used if the appropriate plug in is available for it. Gorgon comes with two plug ins for file system providers:
   * __[Gorgon.FileSystem.GorPack](PlugIns/Gorgon.FileSystem.GorPack):__ Gorgon's proprietary packed file system format, using BZip2 compression.
   * __[Gorgon.FileSystem.Zip](PlugIns/Gorgon.FileSystem.Zip):__ Mounts standard .zip files as virtual file systems.
  
  By default, the file system provider
  
  - [x] __[Gorgon.Input](Gorgon/Gorgon.Input)__ 
  
  A flexible input library to handle joysticks/gamepads, keyboard and mouse input. The input library can use events or polling to retrieve data from the various input sources. 
 
 Keyboard and mouse input is provided using the Windows Raw Input API, and joystick/gamepad support is driven by the following plug ins:
 * __[Gorgon.Input.XInput](PlugIns/Gorgon.Input.XInput):__ Support for the XBox 360 controller (and potentially XBox One controller - not tested)
 * __[Gorgon.Input.DirectInput](PlugIns/Gorgon.Input.DirectInput):__ Support for gaming devices that are not covered by the XInput API.
      
  - [x] __[Gorgon.Graphics.Core](Gorgon/Gorgon.Graphics.Core)__ 
  
  A "low-level" graphics API that sits on top of Direct 3D 11.4. Provides a simplified system to build objects such as render targets, swap chains, buffers, etc... The rendering portion of the API provides a simple mechanism to submit batched state and draw information back to the underlying D3D API.
  
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

  In addition to the support above, applications can introduce their own codecs to read/write whatever font types they wish by extending the [GorgonFontCodec](Gorgon/Gorgon.Graphics.Fonts/Codecs/GorgonFontCodec.cs) type. (Plug in support is pending at this time)
  
  - [x] __[Gorgon.Renderers.Gorgon2D](Gorgon/Gorgon.Renderers/Gorgon2D)__ 
  
  A 2D renderer that sits on top of the graphics module to make developing 2D games/applications much easier. It supports:
   * Sprites
   * Primitives (triangles, lines, ellipses, arcs, and rectangles)
   * Text rendering
   * A shader based effects system
  
  All of these are provided using batched rendering, similar to [MonoGame](http://www.monogame.net/) for maximum performance.   
  
  - [x] __[Gorgon.IO.Gorgon2D](Gorgon/Gorgon.Renderers/IO.Gorgon2D)__ 
  
  IO functionality for serializing sprite and polysprite data to and from various formats using codecs.

  - [x] __[Gorgon.Animation](Gorgon/Gorgon.Animation)__ 
  
  An animation module that allows the creation and playback of key framed animations for various types of objects. 
  
  Animation controllers for the 2D renderer are provided by the [Gorgon.Animation.Gorgon2D](Gorgon/Gorgon.Renderers/Animation.Gorgon2D) assembly.
  
  - [x] __[Gorgon.Editor](Tools/Editor)__ _**(In Development)**_ 
  
  A flexible content editor to allow for the creation and editing of content.        
   * Supports a plug in based architecture to allow developers to extend the editor indefinitely. 
   * Supports file management of content by using a simple tree layout for folders and files.
   * Comes with an image editor plug in which allows users to add depth slices to 3D images (I have yet, for the life of me to find anything on the web that does this), mip maps and array indices, and other simple functions.
   * Comes with a sprite editor plug in which allows users to clip sprites from an image and store them as a file.    
   * Can output the files as a packed file. The type of file that be written out is provided via plug in support (currently only supports the proprietary Gorgon packed file format).
   * Can import packed files using file system plug ins (currently has support for zip and the proprietary Gorgon packed file formats - included with Gorgon as file system plug ins).
   * _TBD..._

What's required?
----------------

  * [.NET 4.7.2](https://www.microsoft.com/net/download/dotnet-framework-runtime)
  * Windows 10 (Build 15063 or later).
  * Microsoft DirectX 11.4

To compile the library
----------------------

  * [Visual C# 2017](https://www.microsoft.com/en-us/store/b/visualstudio) [v15.8](https://docs.microsoft.com/en-us/visualstudio/releasenotes/vs2017-relnotes#whats-new-in-158) or later. 

To use the library
------------------

  * You may use any .NET 4.7.2 enabled language (e.g. Visual Basic .NET) to write an application with Gorgon.

Source code
-----------

The master branch contains the current release version of Gorgon 2.0.  

The 3.0 branch contains version 3.0 of Gorgon which supports Direct 3D 11.4.  
**Please note:** *This version is still in development and should not be used in a production environment as breaking changes can and will happen.*

Acknowledgements
================

Gorgon uses icons from the following sources:

Oxygen 
https://github.com/pasnox/oxygen-icons-png  
http://www.iconarchive.com/show/oxygen-icons-by-oxygen-icons.org.html  

Icons8
http://https://icons8.com

Sprites example textures from:
http://millionthvector.blogspot.com/

This following image(s) is/are not redistributable without permission from the original author.

["HotPocket.dds"](Resources/Textures/HotPocket.DDS) by Starkiteckt 
https://www.deviantart.com/starkiteckt/art/Hot-Pocket-433337115

Third Party True Type fonts:

Tequila	by [uZiMweB](http://www.zimina.net/) \
A Charming Font	by [GemFonts](http://www.moorstation.org/typoasis/designers/gemnew/) \
Sunset by [Harold's Fonts](http://haroldsfonts.com/) \
Monsters Attack ! by [The Empire of the Claw](http://www.empire-of-the-claw.com/) \
Grunja by Apostrophic Labs \
The Bold Font by ??? 
