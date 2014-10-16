Sitecore-Courier
================

[![Alt Text](http://buildsitecore.cloudapp.net/app/rest/builds/buildType:SitecoreCourier_BuildAndPackage/statusIcon)](http://buildsitecore.cloudapp.net?guest=1)

![alt text](http://s15.postimage.org/ccekm5nif/user4919_pic11858_1270227074.jpg?noCache=1349180768)

Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS. You can download it here: https://bit.ly/SitecoreCourier (now also includes Console Runner).

It lets you build Sitecore Update packages automatically, by analyzing serialized Sitecore items and packaging only changed items.

 The module can be installed as a Sitecore package, or used in build system with simple .exe runner.

**Suggested usage workflow**

After you deploy the initial version of your website, you should:

1. Serialize all items you want to move between the servers (usually, all custom items in Core and Master DB). Get the latest Serialization Guide here.
2. Create a TAG from it in a version control system
3. Keep doing changes in TRUNK, serialize changed items, commit them, etc.
4. And packages will be generated automatically, by comparing serialization from TAG (source) to the TRUNK (target). Incremental package will contain only changed items.

## Using console runner at your build server

*Sitecore.Courier.Runner.exe* -s C:\Source -t C:\Target -o C:\Package.update

-s - Source folder

-t - Target folder

-o - Output package (will be created)


## Using web version for preview

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
