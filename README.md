Sitecore-Courier
================

![alt text](http://s15.postimage.org/ccekm5nif/user4919_pic11858_1270227074.jpg?noCache=1349180768)

https://youtu.be/-_uA6FDojKY

Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS. You can download it here: https://bit.ly/SitecoreCourier (now also includes Console Runner) or install it in a few seconds via Chocolatey - https://chocolatey.org/packages/sitecore-courier.

It lets you build Sitecore Update packages automatically, by analyzing serialized Sitecore items and packaging only changed items.

 The module can be installed as a Sitecore package, or used in build system with simple .exe runner.

**Suggested usage workflow**

After you deploy the initial version of your website, you should:

1. Serialize all items you want to move between the servers (usually, all custom items in Core and Master DB). Get the latest Serialization Guide here.
2. Create a TAG from it in a version control system
3. Keep doing changes in TRUNK, serialize changed items, commit them, etc.
4. And packages will be generated automatically, by comparing serialization from TAG (source) to the TRUNK (target). Incremental package will contain only changed items.

## Using Windows shell extension

Sitecore Courier can now be installed via Chocolatey: https://chocolatey.org/packages/sitecore-courier
After it is installed, just put all your items and files into a single folder, and right-click on it to create a package

![alt text](http://3.bp.blogspot.com/-voh_5SsBcyk/VKEV_I0OpyI/AAAAAAAACb0/K1ptEj0iNQk/s1600/courier.png)

## Using console runner at your build server

*Sitecore.Courier.Runner.exe* -s C:\Source -t C:\Target -o C:\Package.update

-s - Source folder

-t - Target folder

-o - Output package (will be created)


## (Deprecated) Using web version for preview

![alt text](http://2.bp.blogspot.com/-B5KLMs5DgNg/UGry9eD7mgI/AAAAAAAAATM/GpMaEvweH8M/s1600/webrunner.jpg)


Additional information on the project is available in this blog post: [Sitecore Courier - Effortless Packaging](http://sitecoresnippets.blogspot.com/2012/10/sitecore-courier-effortless-packaging.html)

**Important**

After you install the package add the following to the configSections section of web.config:

```xml
    <section name="sitecorediff" type="Sitecore.Update.Configuration.ConfigReader, Sitecore.Update"/>
```

and the following to the &lt;configuration&gt; section (right above &lt;sitecore database="SqlServer"&gt;)

```xml
  <sitecorediff>
    <commandfilters>
      <filter id="changedFieldsFilter" mode="on" type="Sitecore.Update.Commands.Filters.ChangedFieldsFilter, Sitecore.Update">
        <fields hint="list">
          <field>__Created</field>
          <field>{5DD74568-4D4B-44C1-B513-0AF5F4CDA34F}</field>
          <field>__Revision</field>
          <field>__Updated</field>
          <field>__Updated by</field>
        </fields>
      </filter>
    </commandfilters>
    <dataproviders>
      <dataprovider id="filesystemmain" type="Sitecore.Update.Data.Providers.FileSystemProvider, Sitecore.Update">
        <param>$(id)</param>
      </dataprovider>
      <dataprovider id="snapshotprovider" type="Sitecore.Update.Data.Providers.SnapShotProvider, Sitecore.Update">
        <param>$(id)</param>
      </dataprovider>
    </dataproviders>

    <source type="Sitecore.Update.Data.DataManager, Sitecore.Update">
      <param>source</param>
    </source>

    <target type="Sitecore.Update.Data.DataManager, Sitecore.Update">
      <param>target</param>
    </target>
  </sitecorediff>
```

#Excluding items for build configurations#
Additional optional parameters have been added to accommodate configuration-driven exclusion of items, i.e. testing or sandbox pages or templates not intended for production use. This is dependent on having a target configuration (associated with the Visual Studio build configuration) and an xml dictionary of the serialized items and any configuration-based exclusions. 

Default code handles the .scproj file format produced by integration with [Team Development for Sitecore.](http://www.hhogdev.com/products/team-development-for-sitecore/overview.aspx)

*Note: by default, the exclusions are performed by executing a file delete on the sitecore items targeted for exclusion. This is designed to be used in a build server environment; for local development and testing, it is recommended to copy the source and target folders to a temporary location that will not affect versioned items and unintentionally delete files from source control.*

##Calling Sitecore Courier on a build server with an excluded configuration##
*Sitecore.Courier.Runner.exe* -s C:\Source -t C:\Target -o c:\Output\package.update -b DevelopmentConfiguration -p C:\MyProject\Tds\myProj.scproj

This will initially remove any items that are irrelevant to the build configuration and then perform normal Courier diffing to find any changes to relevant items. For example, any changes to sandbox/testing items are removed from comparison and will not be deployed to a production environment, but any production-level items that have been changed since the last release are picked up and packaged as usual.
