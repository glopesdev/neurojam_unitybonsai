# README #

### What is this repository for? ###

* This repository contains a package for Unity to directly connect de information with Bonsai.

### How do I get set up? ###

* To que this set up you will need to install Unity 2017 and Bonsai in your computer.
* After that you want to create project in Unity and go to Assets -> Import Package -> Custom Package and select the package downloaded.
* After this is installed you will find some code errors, this is because Bonsai can't be running in .Net 3.5 and Unity uses this
* by default. To change this we need to go to File -> Build Settings -> Player Settings -> Other Settings and put the API compatibility level
* at the .Net 4.6 if i'm not mistaken.

### Contribution guidelines ###

* Ricardo Imperial - ARTICACC

### Who do I talk to? ###

* You can contact ARTICACC at http://artica.cc/ or Bonsai group at https://groups.google.com/forum/#!forum/bonsai-users