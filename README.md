Sitecore-Courier
================

Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS.

**Project Description**

![alt text](http://s15.postimage.org/ccekm5nif/user4919_pic11858_1270227074.jpg?noCache=1349180768)

Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS.

It lets you build Sitecore Update packages automatically, by analyzing serialized Sitecore items and packaging only changed items.

 The module can be installed as a Sitecore package, or used in build system with simple .exe runner.

**Suggested usage workflow**

After you deploy the initial version of your website, you should:

1. Serialize all items you want to move between the servers (usually, all custom items in Core and Master DB). Get the latest Serialization Guide here.
2. Create a TAG from it in a version control system
3. Keep doing changes in TRUNK, serialize changed items, commit them, etc.
4. And packages will be generated automatically, by comparing serialization from TAG (source) to the TRUNK (target). Incremental package will contain only changed items.

![alt text](http://2.bp.blogspot.com/-B5KLMs5DgNg/UGry9eD7mgI/AAAAAAAAATM/GpMaEvweH8M/s1600/webrunner.jpg)


Additional information on the project is available in this blog post: [Sitecore Courier - Effortless Packaging](http://sitecoresnippets.blogspot.com/2012/10/sitecore-courier-effortless-packaging.html)

