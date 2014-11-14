What is Gorgon?
===============

A modular set of libraries useful for graphics and/or video game development.  Gorgon uses Direct 3D (via [SharpDX](http://sharpdx.org)) to provide high performance graphics for your applications.

What’s the licensing? 
-------------------------------------------------

Gorgon is licensed under the [MIT license](http://opensource.org/licenses/MIT).

How far along are you?  Is it done yet?  How about now?
-------------------------------------------------------

Gorgon 2.0 is currently in active development.  At this point there is no definite release date.

What can it do?
---------------

Gorgon provides a set of libraries that are capable of handling pretty much any task thrown at it.  It includes:
  * Common utility functionality (Gorgon.Common).
  * Plug-in support.
  * A virtual file system that can mount a directory as a file system root, or using various plug-ins, can mount a packed file (e.g. a zip file) as a virtual file system root.  This module is based on the popular [PhysFS library](http://icculus.org/physfs/).
  * A flexible input library to handle joysticks (gamepads), keyboard and mouse input.  Using plug-ins the input system can utilize Xbox360 controllers, standard joysticks, raw input from the keyboard and mouse and even via raw HID data.  It even includes a plug-in that wraps up windows forms input events.  The input library can use events or polling to retrieve data from the various input sources.
  * A "low-level" graphics API similar to Direct 3D 11.  It improves on Direct 3D by handling some of the more mundane tasks for initializing objects in Direct 3D and provides more extensive error handling for invalid input.  This module supports all the bells and whistles that come with Direct 3D 11 such as device feature levels to allow running on older (Direct 3D 10 or better) hardware, pixel, vertex, geometry, hull, domain and compute shader support, etc...
  * An extensive bitmap font creation interface (within the graphics module) that supports kerning.
  * A 2D renderer that sits on top of the graphics module to make developing 2D games/applications much easier.  It provides sprites, drawing primitives (rectangles, lines, ellipses, etc...), drawing of text with the aforementioned fonts, etc...
  * An animation module that allows the creation of flexible animations for various types of objects.

What's required?
----------------

  * [.NET 4.5](http://www.microsoft.com/en-ca/download/details.aspx?id=30653)
  * Windows Vista SP2, Windows 7, Windows 8, Windows 8.1 or later.
  * Microsoft DirectX 11

To compile the library
----------------------

  * [Visual C# 2012](http://msdn.microsoft.com/en-gb/vstudio/) or later. 

To use the library
------------------

  * You may use any .NET 4.5 enabled language (e.g. Visual Basic .NET) to write an application with Gorgon.

Source code
-----------

The master branch contains the current release version of Gorgon 1.1.  

The 2.x branch contains version 2.0 of Gorgon which supports Direct 3D 11. 
**Please note:** *This version is still in development and should not be used in a production environment as breaking changes can and will happen.*
