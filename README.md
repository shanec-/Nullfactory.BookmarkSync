#Bookmarks Organizer for Windows 7.5
Windows Phone does not provide a way to manipulate internet explorer bookmarks. This app attempts to workaround this and give the user the ability add/edit/delete and synchronize bookmarks with the cloud.

The phone stores each bookmark as a ``.url`` files within the ``\Windows\Favorites`` folder. Internet Explorer treats every file with the same extension as a new bookmark entry. It was only a matter of reading and manipulating these files.


## Build Pre-Requisites
- Visual Studio 2010 or higher
- Windows Phone SDK 7.1
- [Register](https://account.live.com/developers/applications?biciid=LcIsdk) as a new application in order to obtain a Live Sdk Client Id for SkyDrive (OneDrive) integration, .
- Update the App.xaml file with your new key:
``<clr:String x:Key="LiveConnectClientId">YOUR KEY HERE</clr:String>``

##Installation Pre-Requisites
- Interop-Unlocked Windows Phone running Mango (7.5) - [Complete Guide](http://windowsphonehacker.com/articles/the_complete_guide_to_jailbreaking_windows_phone_7_and_7.5-09-24-11)
- [WP7 Root Tools by Heathcliff74](http://www.wp7roottools.com/) installed on the device.

##Installation
1. Deploy the .XAP file using your favourite deployment tool.
2. Make *Bookmarks Organizer* a trusted app using WP7 Root Tools
3. ...
4. Profit!

##Disclaimer 
This application interacts with folders and files that were not intended to be accessed. Use at your own risk!

