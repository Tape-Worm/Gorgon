What is Gorgon?
===============

A modular set of libraries useful for graphics and/or video game development.  Gorgon uses Direct 3D 11.1 (via [SharpDX](http://sharpdx.org)) to provide high performance graphics for your applications.

What’s the licensing? 
-------------------------------------------------

Gorgon is licensed under the [MIT license](http://opensource.org/licenses/MIT).

How far along are you?  Is it done yet?  How about now?
-------------------------------------------------------

Gorgon 3.0 is currently in sporadic development as time allows. At this point, there's no release planned.

What can it do?
---------------

Gorgon provides a set of libraries that are capable of handling pretty much any task thrown at it.  It includes:
  * __[Gorgon.Core]__ Core functionality, plug in support and utility functionality. This is the base library that everything else uses.
  * __[Gorgon.FileSystem]__ A virtual file system that can mount a directory as a file system root, or using various plug-ins, can mount a packed file (e.g. a zip file) as a virtual file system root.  This module is based on the popular [PhysFS library](http://icculus.org/physfs/).
  * __[Gorgon.Input]__ A flexible input library to handle joysticks (gamepads), keyboard and mouse input via the raw input API.  Using plug-ins the input system can utilize Xbox360 controllers (via XInput) or standard joysticks (via DirectInput).  The input library can use events or polling to retrieve data from the various input sources.
  * __[Gorgon.Graphics]__ A "low-level" graphics API that sits on top of Direct 3D 11.1. Provides a simplified system to build objects such as render targets, swap chains, buffers, etc... The rendering portion of the API provides a simple mechanism to create state information and provide bundled up draw calls for modern rendering techniques.
  * __[Gorgon.Graphics.Imaging]__ Functionality to load, save and manipulate many popular image formats (bmp, jpg, png, dds, tga, etc...). This library also contains functionality to use a fluent interface to manipulate images for things like cropping, scaling, etc...
  * __[TBD]__ An extensive bitmap font creation interface (within the graphics module) that supports kerning, outlining of font glyphs, and other customizations to help generate impressive looking text.
  * __[TBD]__ A 2D renderer that sits on top of the graphics module to make developing 2D games/applications much easier.  
  * __[TBD]__ An animation module that allows the creation of flexible animations for various types of objects.
  * __[TBD]__ A flexible content editor to allow for the creation of sprites, fonts, etc...  

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
