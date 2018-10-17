[![Release](https://img.shields.io/github/release/DecaTec/Wsapm/all.svg)](https://github.com/DecaTec/Wsapm/releases)
[![Microsoft Public License](https://img.shields.io/github/license/DecaTec/Wsapm.svg)](https://github.com/DecaTec/Wsapm/blob/master/LICENSE)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=RMPESAJXPHH2U)

# Windows Server Advanced Power Management

Windows Server Advanced Power Management is an application for advanced power management on Windows (home) servers. Policies can be defined to prevent Windows to enter standby mode when the computer is still in use. Thereby, energy can be saved because the hardware is only running when it is actually required.

Windows Server Advanced Power Management is specifically designed for home servers, but it can also be used on desktop computers to prevent unintended standby mode.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=JVKUJE26S27Y2)

## Features
- Rule based supression of standby mode. The following policies can be checked:
  - Running programs
  - Online network devices (computers, smart phones, TVs, streaming clients, gaming consoles, etc.)
  - Network load (upload, download and combinbed load)
  - Access to network shares
  - CPU load
  - Load of logical disks
  - Memory load
- User defined actions can be executed after policy check:
  - Start programs
  - Send the Computer into standby/hibernation mode or shut down
- Restart of Windows services after waking from standby
- Start programs after waking from standby
- Time-controlled wake from standby
  - One-time
  - Periodic
  - Periodic with end time specified
  - Start programs after time-controlled wake from standby
- Definition of uptimes, i.e. time spans when the computer must not enter standby mode
  - Temporary (activated in the main window of the application)
  - Planned (one-time and/or periodic – defined in the application’s settings)
- Plugin interface: Windows Server Advanced Power Management can be extended with plugins defining their own policies
- Remote shut down: Computers can be shut down, restarted or sent to standby or hibernate mode remotely. Therefor the app [MagicPacket](https://decatec.de/software/magicpacket_en/) is required (available for Windows and Windows Phone)
- The software is running without user logon

Download the [user manual](https://decatec.de/?ddownload=1087) for a detailed description of the single features. It also offers many tips for the configuration of the software in different scenarios and some background information.

## Download
| | |
| - | - |
| **Download:** | [decatec.de](https://decatec.de/software/windows-server-advanced-power-management_en/) (size approx. 11 MB) <br />The [.NET Framework 4](http://www.microsoft.com/en-us/download/details.aspx?id=24872) is required and included in the installer. |
| **Changelog:** | [English](https://decatec.de/downloads/wsapm/changelog/Changelog_en.txt), [German](https://decatec.de/downloads/wsapm/changelog/Changelog_de.txt) |
| **User manual:** | [User manual (English)](https://decatec.de/?ddownload=1087)/[User manual (German)](https://decatec.de/?ddownload=1086) |
| **Supported operating systems:** | Windows XP, Windows Vista, Windows 7, Windows 8, Windows 8.1, Windows 10, Windows Server 2003 (R2), Windows Server 2008 (R2), Windows Server 2012 (R2), Windows Home Server 2011 |
| **Supported languages:** | English, German |

**Note:**
When downloading the setup, there may appear a warning that the software may be malicious (from browsers and/or Windows). This is due to the installer is not yet known to the browser, especially when a new version of the program was released.
However, Windows Server Advanced Power Management does not contain any malware/spyware/adware!

## Development
In order to clone and build the project, you'll need:
- Visual Studio 2010 or later
- [WiX Toolset](http://wixtoolset.org/) (for building the installer)

## Plugins
Windows Server Advanced Power Management offers a plugin interface to extend the program with own policies to check.

Plugins are installed in the program’s settings. Therefor, open the settings window and change to the tab *Plugins*. After clicking *Install plugin*,choose the downloaded plugin file. WSAPM is restarted in order to install the plugin. Directly after installation, a plugin is not activated.This is also done in the tab *Plugins* in the program’s settings.

Some plugins offer their own settings which can be opened with the button *Plugin settings*.

In order to uninstall a plugin, simply select it and click the button *Uninstall plugin*.

### Available plugins
| Plugin | Version | Description | Developer |
| - | - | - | - |
| [Local Printers Online](https://github.com/DecaTec/Wsapm-LocalPrintersOnline) | 1.1.0 | Plugin to suppress standby if a local printer is switched on | [DecaTec](https://decatec.de) | 
| [Removable Devices](https://github.com/DecaTec/Wsapm-RemovableDevices) | 1.1.0 | Plugin to suppress standby if any removable devices are present | [DecaTec](https://decatec.de) | 
| [LoggedOnUsers](https://github.com/Seji64/LoggedOnUsersPlugin) | 1.0.2 | Plugin to suppress standby if defined users are logged on | [Seji](https://github.com/Seji64) | 
| [PlexIsBusy](https://github.com/Seji64/PlexIsBusyPlugin) | 1.0.2 | Plugin to suppress standby if media is streamed by Plex | [Seji](https://github.com/Seji64) | 
| [OpenNetworkConnections](https://github.com/ErdnussFlipS/WSAPM-OpenNetworkConnections) | 1.0.3.1 | Plugin to suppress standby if specified TCP connections are active | [ErdnussFlipS](https://github.com/ErdnussFlipS) | 

### Template for plugin development
Everybody can develop own plugins for Windows Server Advanced Power Management. A detailed description for plugin development is also contained in the user manual.
If you have developed your own plugin and want to offer it on this website, please contact the developer of WSAPM.

To make plugin development really simple, there are some templates available as Visual Studio Project (these templates require Windows Server Advanced Power Management version 1.3.0): [Wsapm-PluginTemplates @ GitHub](https://github.com/DecaTec/Wsapm-PluginTemplates)

## Screenshots

**Main window**
![Windows Server Advanced Power Management: Main window](/Doc/en/Screenshots/Main_Window.png "Windows Server Advanced Power Management: Main window")

**Settings – General**
![Windows Server Advanced Power Management: Settings – General](/Doc/en/Screenshots/Settings_General.png "Windows Server Advanced Power Management: Settings – General")

**Settings – Monitoring (system)**
![Windows Server Advanced Power Management: Settings – Monitoring (system)](/Doc/en/Screenshots/Settings_MonitoringSystem.png "Windows Server Advanced Power Management: Settings – Monitoring (system)")

**Settings – Monitoring (advanced)**
![Windows Server Advanced Power Management: Settings – Monitoring (advanced)](/Doc/en/Screenshots/Settings_MonitoringAdvanced.png "Windows Server Advanced Power Management: Settings – Monitoring (advanced)")


**Settings – After policy check**
![Windows Server Advanced Power Management: Settings – After policy check](/Doc/en/Screenshots/Settings_AfterPolicyCheck.png "Windows Server Advanced Power Management: Settings – After policy check")

**Settings – Wake**
![Windows Server Advanced Power Management: Settings – Wake](/Doc/en/Screenshots/Settings_Wake.png "Windows Server Advanced Power Management: Settings – Wake")

**Settings – Uptime**
![Windows Server Advanced Power Management: Settings – Uptime](/Doc/en/Screenshots/Settings_Uptime.png "Windows Server Advanced Power Management: Settings – Uptime")

**Settings – Plugins**
![Windows Server Advanced Power Management: Settings – Plugins](/Doc/en/Screenshots/Settings_Plugins.png "Windows Server Advanced Power Management: Settings – Plugins")

**Settings – Remote shut down**
![Windows Server Advanced Power Management: Settings – Remote shut down](/Doc/en/Screenshots/Settings_RemoteShutdown.png "Windows Server Advanced Power Management: Settings – Remote shut down")
