![output-onlinepngtools](https://github.com/mnd0929/piget/assets/92184643/3388d645-4df8-4258-bc74-c9e7b06bc859)

# PIGET
[![Releases](https://img.shields.io/badge/All%20compiled%20versions-red)](https://github.com/mnd0929/piget/releases)
[![Discord](https://img.shields.io/badge/Discord-blue)](https://discord.gg/x7xMShAzck)

Quick Installation Script Manager (QISL). Uses the PIGET scripts library.

[PIGET](http://tgcch.byethost7.com/piget/pl.php?filter=) | [PIGET standard library](https://raw.githubusercontent.com/mnd0929/piget-library/main/library.json) | feedback@xon-4de.ru | support@xon-4de.ru 

# Installation

API installation methods:
- [API Curl Installation](https://raw.githubusercontent.com/mnd0929/api-apps/main/piget-updatecommand.pinfo) (Requires curl)
- [API Bitsadmin Installation](https://raw.githubusercontent.com/mnd0929/api-apps/main/piget-updatecommand-bitsadmin.pinfo) (Requires bitsadmin)
- [API Powershell Installation](https://raw.githubusercontent.com/mnd0929/api-apps/main/piget-updatecommand-powershell.pinfo) (Requires powershell)

<!> Supported platforms: ```win-x86```, ```win-x64```

<!> False positives of antiviruses are possible - the application does not have a digital signature

# Libraries

The PIGET library should be a public json file with the following architecture:
```json
{
  "LibraryDirectory": null,
  "Url": null,
  "ScriptListAddresses": [
    "<link to text file with Source 1>",
    "<link to text file with Source 2>"
  ],
  "Name": "<Library Name>"
}
```
You can connect the library to the PIGET client using the command ```piget library connect <LibraryLink>```.

The PIGET standard library is [here](https://raw.githubusercontent.com/mnd0929/piget-library/main/library.json)

# Sources

The resource should be a public json file with the following content:
```json
{
  "Scripts": [
    {
      "Name": "<Script Name 1>",
      "Description": "<Script Description>",
      "Resources": "<Link to zip archive with resources>",
      "InitialScript": "<Batch code>"
    },
    {
      "Name": "<Script Name 2>",
      "Description": "<Script Description>",
      "Resources": "<Link to zip archive with resources>",
      "InitialScript": "<Batch code>"
    },
    {
      "Name": "<Script Name 3>",
      "Description": "<Script Description>",
      "Resources": "<Link to zip archive with resources>",
      "InitialScript": "<Batch code>"
    },
  ]
}
```
Sources are indicated in the library manifest. 

The standard PIGET source is [here](https://raw.githubusercontent.com/mnd0929/piget-library/main/source.json)

# Commands

```piget run <ScriptName>``` - Runs the first package found with the given name

```piget search "<Keywords>"``` - Displays a list of scripts matching the keywords

```piget library connect <ManifestLink>``` - Connects the library using its Web Manifest

```piget library disconnect <ManifestLink>``` - Disconnects the library using its Web Manifest

```piget library list <LibraryName>``` - Displays a list of scripts in the library

```piget library update <LibraryName>``` - Updates the scripts of the specified library

```piget libraries disconnect``` - Disconnects from ALL libraries

```piget libraries list``` - Displays a list of connected libraries

```piget libraries update``` - Updates ALL libraries

```piget check resources``` - Checks the availability of resources for ALL scripts and removes all unavailable ones

```piget info <ScriptName>``` - Displays information about the specified script
