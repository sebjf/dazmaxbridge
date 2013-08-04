
Read the project README for details on how to set up the development environment,
below are a couple of things to bear in mind about this particular project:


* This library requires the MAX.NET plugin from ESphere for all versions of Max 
up to 2012. From this version onwards Autodesk includes .NET wrappers natively.
(These are actually based on MAX.NET)

* This does not mean some adjustments may be requried however. If anyone would
like to develop this, please create another project that references the lower
level (MaxBridge) sources (so changes are only made once) and implement the
Plugin and Plugin Importer levels in the new one.

* Find Max.NET here: http://www.ephere.com/autodesk/max/

* The Offline Installer for all versions of Max is included in the root of this
project.

* And always remember to target .NET 3.5 Client framework!!!!