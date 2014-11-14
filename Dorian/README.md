What is Gorgon?
===============

A 2D graphics library for .NET and SlimDX.  Also, it’s proof that I can’t do 3D.

What’s the licensing? What did you do to my soul?
-------------------------------------------------

Versions 1.1.x and up of Gorgon are licensed under the MIT license.
Your soul is a lie.

How far along are you?  Is it done yet?  How about now?
-------------------------------------------------------

Gorgon is complete, released and in version 1.1.x.

_Version 2.0 is currently being developed which will support Direct 3D 11._

What can it do?
---------------

Oh sweet jeebus, what can’t it do?  It can slice, dice, puree, and even solve pi to 3,899,122,129,991,211,100 decimal places.  It rules.  Really, it does.  It’s better than Half Life 2 because I say it is.

That said, it:
  * Supports pixel/vertex shaders.
  * Supports stencil buffers.
  * Supports png, jpg, bmp, etc.. image formats.
  * Runs pretty swift.  Can dump well over 10,000 sprites to the screen and still get >=30 FPS.
  * Supports render targets.
  * Supports drawing to various surfaces (i.e. lines, points, circles, ellipses, etc…).
  * Supports virtual file systems for packed files and folder mounting.
  * Has a plug-in architecture, so you can extend it indefinitely.
  * Has a nifty keen animation system.
  * Fonts!  Bitmap fonts.  Flexible bitmap fonts even.
  * Has support for raw input from input devices.  Even has rudimentary joystick support.
  * Can render into any type of windows control, e.g. a panel, form, textbox (yes, textbox.  No, I don’t know why you’d want that).
  * I’m sure there’s more, but I’m too lazy to look it up.

Yeah, it’s pretty metal.

What do I need to make all my dreams come true?
-----------------------------------------------

Your dreams mean nothing, and are ultimately futile.  Anyway, you’ll require the following:
  * [.NET 3.5 SP1](http://www.microsoft.com/downloads/en/details.aspx?FamilyID=ab99342f-5d1a-413d-8319-81da479ab0d7)
  * Windows XP or later.
  * [Microsoft DirectX 9](http://www.microsoft.com/downloads/en/details.aspx?FamilyId=2DA43D38-DB71-4C1B-BC6A-9B6652CD92A3&displaylang=en) (installed by the SlimDX installer which is installed by Gorgon)

To compile the library
----------------------

  * [Visual C# 2010](http://msdn.microsoft.com/en-gb/vstudio/) to compile the library. If you're tight on funds and don't have a kidney to sell so you can put a down payment on Visual Studio, you can always opt for the [FREE Express editions](http://www.microsoft.com/express/).  This is only to compile the library, to use the library Visual Studio 2008 will work as well.

To use the library
------------------

  * You may use any .NET 3.5 enabled language to write an application with Gorgon.

Source code
-----------

The master branch contains the current release version of Gorgon.  The 2.x branch contains version 2.0 of Gorgon which supports Direct 3D 11.  **This version is still in development and should not be used in a production environment as breaking changes can and will happen.**
