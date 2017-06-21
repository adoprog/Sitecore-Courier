Sitecore-Courier
================

![alt text](http://s15.postimage.org/ccekm5nif/user4919_pic11858_1270227074.jpg?noCache=1349180768)

Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS. 

It lets you build Sitecore Update packages automatically, by analyzing serialized Sitecore items and packaging only changed items.
The module can be installed as a Sitecore package, or used in build system with simple .exe runner.

You can download it here: https://bit.ly/SitecoreCourier (now also includes Console Runner) or install it in a few seconds via Chocolatey - https://chocolatey.org/packages/sitecore-courier.

**Introduction video about the module: 

[![Introduction video about the module](https://img.youtube.com/vi/-_uA6FDojKY/0.jpg)](https://www.youtube.com/watch?v=-_uA6FDojKY)

**Usage workflow**

Simple: Just point Sitecore Courier to the folder where your serialized items are stored (standard .item or Rainbow's .yml) and it will create a package that can be installed at any Sitecore website.

Advanced: After you deploy the initial version of your website, you should:

1. Serialize all items you want to move between the servers (usually, all custom items in Core and Master DB). Get the latest Serialization Guide here.
2. Create a TAG from it in a version control system
3. Keep doing changes in TRUNK, serialize changed items, commit them, etc.
4. And packages will be generated automatically, by comparing serialization from TAG (source) to the TRUNK (target). Incremental package will contain only changed items.

## Using console runner at your build server

Sample Habitat script here: https://github.com/adoprog/Habitat/blob/master/scripts/courier.ps1

*Sitecore.Courier.Runner.exe* -s C:\Source -t C:\Target -o C:\Package.update

-s - Source folder

-t - Target folder

-o - Output package (will be created)

## Using Windows shell extension

Sitecore Courier can now be installed via Chocolatey: https://chocolatey.org/packages/sitecore-courier
After it is installed, just put all your items and files into a single folder, and right-click on it to create a package

![alt text](http://3.bp.blogspot.com/-voh_5SsBcyk/VKEV_I0OpyI/AAAAAAAACb0/K1ptEj0iNQk/s1600/courier.png)

## Excluding items for build configurations
Additional optional parameters have been added to accommodate configuration-driven exclusion of items, i.e. testing or sandbox pages or templates not intended for production use. This is dependent on having a target configuration (associated with the Visual Studio build configuration) and an xml dictionary of the serialized items and any configuration-based exclusions. 

Default code handles the .scproj file format.

*Note: by default, the exclusions are performed by executing a file delete on the sitecore items targeted for exclusion. This is designed to be used in a build server environment; for local development and testing, it is recommended to copy the source and target folders to a temporary location that will not affect versioned items and unintentionally delete files from source control.*

## Calling Sitecore Courier on a build server with an excluded configuration
*Sitecore.Courier.Runner.exe* -s C:\Source -t C:\Target -o c:\Output\package.update -b DevelopmentConfiguration -p C:\MyProject\Tds\myProj.scproj

This will initially remove any items that are irrelevant to the build configuration and then perform normal Courier diffing to find any changes to relevant items. For example, any changes to sandbox/testing items are removed from comparison and will not be deployed to a production environment, but any production-level items that have been changed since the last release are picked up and packaged as usual.
