
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

How do I use it?

1. Start Daz, create or load your asset, and export it as you would to any interchange format to a file.
2. Load Max, and open the import script. Set the file & run the script, then see your asset appear.

What does it consist of?

* A plugin for Daz
* A MaxScript script for Max
* A .NET tool library for the MaxScript for Max
* A .NET importer plugin for Max

What can't it do?
(What it cant do yet)

Allow editing of morphs or pose once in Daz
Transfer advanced hair and cloth systems
Transfer joint deformation controllers
Transfer rigging
Transfer smoothing parameters
Create subsurface or other advanced materials

What is it not?
(What it will never do)

An interchange or storage format - the files generated are for single use only and no guarantees are made about their layout. Neither will material maps be copied; Max will continue to reference the originals.*

(* though if you save your scene Max may copy the maps locally with it)

Known Bugs

Materials of geografted geometry are missing

Roadmap

Create HSS materials for skin
Transfer basic rigging
Re-import single components (e.g. just vertex positions for when a morph changes)
Export to shared memory
Transfer joint deformers


-----------------------Page 2-----------------------

Building

Once you have downloaded the project, the following 5 steps must be completed to set-up the development environment.

Before building make sure the following steps are complete:

1. Install the Daz SDK (The Daz 4.5 SDK is the latest one and works with Daz 4.6)

2. Create the following SYSTEM ENVIRONMENT VARIABLES:

_3DSMaxInstallDirectory		set to the directory containing 3dsmax.exe		(e.g. C:\Program Files\Autodesk\3ds Max 2011\) (*)
DazInstallDirectory			set to the directory containing dazstudio.exe	(e.g. C:\Program Files\DAZ 3D\DAZStudio4\)

(* the underscore is very intentional don't miss it out, don't miss the trailing backslashes either)

3. Under DazExporterPlugin Properties > Configuration Proeprties > Debugging, set Command to "$(DazInstallDirectory)\DAZStudio.exe" (as this is stored in a .user file that is not under source control)

4. Under MaxBridgeExporterPlugin Properties -> Debug, set Start Action to Start External Program with the parameter set to the path of 3dsmax.exe (e.g. C:\Program Files\Autodesk\3ds Max 2011\3dsmax.exe)

5. Select a location for the MaxScript scripts and copy the contents of the MaxScript folder to it (e.g. C:\Program Files\Autodesk\3ds Max 2011\Scripts\MyScripts\)

Installing

When the project builds it should copy all the required files to their correct locations. Starting a debug instance of either project should load Daz or Max with the debugger attached ready to step into the library code once the export or import has been initiated.

To use the bridge outside the development environment, start Daz and Max as normal and transfer exactly as you would if debugging.


-----------------------Page 3-----------------------

Shortcuts in Daz

The exporter can be executed from DazScript in a couple of lines, and this script bound to a shortcut or menu item, to allow quick execution.

1. Create a script with the following content in the Daz Scripting IDE:

var oExporter = App.getExportMgr().findExporterByClassName( "MyDazExporter" );
var oSettings = new DzFileIOSettings();
oExporter.writeFile( "E:\\Daz3D\\Scripting\\Scratch.dazmaxbridge", oSettings );

2. Save it in the [Daz Content]\Scripts folder.

3. Navigate to the script in the Content Library in Daz, right-click and select 'Create Custom Action'. A new menu item under the 'Scripts' menu will be created on the main toolbar bar.

(4.) To bind this to a shortcut, go to Window -> Workspace -> Customize..., and under 'Custom' the first entry, in the left pane, right click the action and select 'Change Keyboard Shortcut'. Press the appropriate key combination and then 'Accept'.

-----------------------Page 4-----------------------