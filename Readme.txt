
Welcome to the Daz-Max-Bridge

-----------------------Page 1-----------------------

What is the purpose of this project?

To enable the use of Daz assets in 3DS Max quickly and easily.

Why is it needed?

Interchange formats are limited, by their design (e.g. OBJ - no skinning data) or implementation (e.g. FBX - buggy) making the transfer of assets time consuming and frustrating.

What does it do?

Take a set of props or figures from Daz, and create counterparts in Max, complete with all world transforms, morphs, skinning data and materials which operate in Max just how they did in Daz.

How does it do it?

By providing bespoke plugins for both tools that are highly coupled for maximum efficiency and robustness, both to eachother and their host applications, enabling the quick transfer of data between the two.

What does it consist of?

* A plugin for Daz
* A MaxScript for Max
* A .NET tool library for the MaxScript for Max
* A .NET plugin for Max

What can't it do?

Allow editing of morphs or pose once in Daz
Transfer advanced hair and cloth systems
Transfer joint deformation controllers
Transfer rigging

What is it not?

An interchange or storage format - the files generated are for single use only and no guarantees are made about them. Neither will material maps be copied; Max will continue to reference the originals.

Known Limitations & Bugs

Materials of geografted geometry are missing

Roadmap

Automatically transfer clothing of a character
Transfer basic rigging
Transfer joint deformers
Export to shared memory
Re-import single components (e.g. just vertex positions for when a morph changes)


-----------------------Page 2-----------------------

Building

Before building make sure the following steps are complete:

1. Install the Daz SDK (The Daz 4.5 SDK is the latest one and works with Daz 4.6)
2. Create a SYSTEM ENVIRONMENT VARIABLE called DazInstallDirectory and set its value to the installation directory of Daz Studio (e.g. C:\Program Files\DAZ 3D\DAZStudio4)
3. Select a location for the MaxScript scripts and copy the contents of the MaxScript folder to it
4. Update the Post-build Event Command Line in the MaxBridgeLib project with the location selected in (3)

Installing

When the project builds it should copy all the required files to their correct locations and to use the bridge outside the development environment, start Daz and Max as normal and transfer exactly as you would if debugging.


-----------------------Page 3-----------------------

Shortcuts in Daz

The exporter can be executed from DazScript in a couple of lines, and this script bound to a shortcut or menu item, to allow quick execution.

1. Create a script with the following content in the Daz Scripting IDE:

var oExporter = App.getExportMgr().findExporterByClassName( "MyDazExporter" );
var oSettings = new DzFileIOSettings();
oExporter.writeFile( "E:\\Daz3D\\Scripting\\Scratch.characterkit", oSettings );

2. Save it in the [Daz Content]\Scripts folder.

3. Navigate to the script in the Content Library in Daz, right-click and select 'Create Custom Action'. A new menu item under the 'Scripts' menu will be created on the main toolbar bar.

(4.) To bind this to a shortcut, go to Window -> Workspace -> Customize..., and under 'Custom' the first entry, in the left pane, right click the action and select 'Change Keyboard Shortcut'. Press the appropriate key combination and then 'Accept'.

-----------------------Page 4-----------------------

Overview

This project consists of four components:
The Daz Plugin
This unmanaged C++ plugin appears to Daz as an exporter, based on a QObject (Daz uses Qt throughout). It is responsible for collecting the data from Daz via the Daz provided SDK and packing it into a binary stream.The Serialiser
MessagePack is a cross-language serialiser/deserialiser/message interchange format similar to JSON that can be used to exchange binary data. Implementations of it in C++ and C# are used to get the scene data between the Daz Plugin and the managed layer. It handles all the serialisation, deserialisation, and file layout.The Managed Layer
Once the plugin has packed the scene data, the managed layer uses MessagePack to unpack them into a structure in 'managed space' that can be accessed by CLR based programs (guess which ones...)The MaxScript
Finally, a MaxScript script loads the .NET library (the managed layer) and uses its methods to read the scene data into Max, using the Max API to create and configure objects.Project Status

The project has not been released (that means it isn't anywhere near an alpha stage let alone anywhere else)


The Daz Plugin

The Managed Layer
The managed layer should do as little as possible, as it is intended only as a tool to for MaxScript to interact with the data structure provided by the Daz plugin. To put it another way, if a third application, such as Blender, were to be supported, a new Blender plugin would be created that used a MessagePack implementation to read the data provided by the Daz plugin, rather than use the managed layer.
Its responsibilities are:
1. Deserialise the stream (as MaxScript does not have a MessagePack implementation)
2. Present the data using a small subset of basic types (as expanded on below, MaxScript supports transferring only simple types between it and .NET)
This allows the managed layer to be highly coupled to the MaxScript implementation to make both as simple as possible.

The MaxScript
The MaxScript script is the last stage and the one that creates and configures the objects in 3DS Max. MaxScript has the ability to load .NET DLLs, create objects, and interact with their methods and properties. This opens up a great deal of functionality beyond MaxScript, for example, creating tools to speed up tasks such as image processing. Passing data between .NET and MaxScript however is limited to a few basic types (numbers & strings) with no support for arbitrary data types. 
MaxScript then has four responsibilities:
1. Pull the data from the library in the form of basic value types and unpack it into 3DSMax native types (e.g. float[3] to Point3)
2. Convert all the 0 based indices to 1 based indices
3. Create the 3DSMax objects (meshes, materials, skins)
4. Convert Daz settings to Max settings
The interpretation of values such as opacity, and specular power, will change between renderers and applications, so the final stage is to convert these material parameters from Daz to Max. This is done in MaxScript as well.
MaxScript is by far the slowest stage of the pipeline. It could be replaced with a C++ plugin (Autodesk provide an SDK) - the functionality in the script is not complex - which would increase speed significantly, however Maxscript does have some advantages:

1. It is compatiable with all versions of 3DS Max (plugins must be recompiled)
2. It allows each user to edit how the data is used (e.g. new material types or skinning techniques) without needing the plugin source

