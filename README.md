# GroupClear
**GroupClear** is an [Oxide](https://umod.org/) plugin that provides the following facilities for removing all members from an Oxide group or list of groups:
- Chat and console commands to remove all members from an individual group.
- Chat and console commands to remove all members from a preconfigured list of groups.
- Automatic removal of all members from a preconfigured list of groups at server wipe.
- API calls to allow other plugins to remove all members from individual groups or lists of groups.
### Table of Contents  
- [Requirements](#requirements)  
- [Installation](#installation)  
- [Permissions](#permissions)  
- [Commands](#commands)  
- [Configuration](#configuration)  
- [Localization](#localization)  
- [Developer API](#developer-api)  
  * [ClearGroup(string group) Example](#cleargroupstring-group-example)
  * [ClearGroups(List\<string\> groups) Example](#cleargroupsliststring-groups-example)
- [Credits](#credits)
## Requirements
| Depends On | Works With | Conflicts With |
| --- | --- | --- |
| None | Any | None |
## Installation
Download the plugin:
```bash
git clone https://github.com/rustireland/groupclear.git
```
Copy it to the Oxide plugins directory:
```bash
cp groupclear/GroupClear.cs oxide/plugins
```
Oxide will compile and load the plugin automatically.
## Permissions
This plugin uses the Oxide permission system. To assign a permission, use `oxide.grant <user or group> <name or steam id> <permission>`. To remove a permission, use `oxide.revoke <user or group> <name or steam id> <permission>`.
- `groupclear.admin` - allows access to chat and console commands
## Commands
This plugin provides both chat and console commands using the same syntax. When using a command in chat, prefix it with a forward slash: `/`.
- `cleargroup <group>` - clear all members from a group
- `clearallgroups confirm` - clear all members from the groups listed in the configuration
## Configuration
The settings and options can be configured in the `GroupClear.json` file under the `oxide/config` directory. The use of an editor and validator is recommended to avoid formatting issues and syntax errors.

When run for the first time, the plugin will create a default configuration file with server wipe group clearance *disabled*, and three example groups.
```json
{
  "Clear all group members on server wipe": false,
  "Groups": [
    "group1",
    "group2",
    "group3"
  ]
}
```
## Localization
The default messages are in the `GroupClear.json` file under the `oxide/lang/en` directory. To add support for another language, create a new language folder (e.g. **de** for German) if not already created, copy the default language file to the new folder and then customize the messages.
```json
{
  "ChatPermissionDenied": "You do not have permission to use this command.",
  "ChatClearGroupUsage": "Usage: <color=#ffa500>/cleargroup</color> <color=#add8e6><group></color>",
  "ChatClearGroup": "Cleared <color=#ffa500>{memberCount}</color> players from group <color=#ffa500>{group}</color>.",
  "ChatClearAllGroupsUsage": "Usage: <color=#ffa500>/clearallgroups</color> <color=#add8e6>confirm</color>",
  "ChatClearAllGroups": "Initializing manual Oxide group member clearance:",
  "ChatGroupNotFound": "Group <color=#ffa500>{group}</color> does not exist.",
  "ChatNoGroups": "No groups have been configured.",
  "ConsolePermissionDenied": "You do not have permission to use this command.",
  "ConsoleClearGroupUsage": "Usage: cleargroup <group>",
  "ConsoleClearGroup": "Cleared {memberCount} players from group '{group}'.",
  "ConsoleClearGroupLog": "{player} ({steamid}) cleared {memberCount} players from group '{group}'.",
  "ConsoleClearAllGroupsUsage": "Usage: clearallgroups confirm",
  "ConsoleClearAllGroups": "Initializing manual Oxide group member clearance:",
  "ConsoleClearAllGroupsWipe": "Initializing post-wipe Oxide group member clearance:",
  "ConsoleClearAllGroupsLog": "{player} ({steamid}) initialized a manual Oxide group member clearance:",
  "ConsoleGroupNotFound": "Group '{group}' does not exist.",
  "ConsoleNoGroups": "No groups have been configured.",
  "ConsoleConfigError": "The config file is corrupt. Either fix or delete it and restart the plugin.",
  "ConsoleUnloading": "Unloading plugin."
}
```
## Developer API
```c#
/*
API Call:    ClearGroup(string group)
Purpose:     Clear all members from an Oxide group.

Parameters:  - string group
               An Oxide group.

Returns:     - Tuple<bool, int>
               A tuple containing whether the group was found (bool),
               and the number of members cleared (int).
*/
(Tuple<bool, int>) ClearGroup(string group)

/*
API Call:    ClearGroups(List<string> groups)
Purpose:     Clear all members from a list of Oxide groups.

Parameters:  - List<string> groups
               A list of Oxide groups.

Returns:     - List<Tuple<string, bool, int>>
               A list of tuples - one for each group requested
               Each tuple contains the group name (string),
               whether it was found (bool),
               and the number of members cleared (int).
*/
(List<Tuple<string, bool, int>>) ClearGroups(List<string> groups)
```
### ClearGroup(string group) Example
```c#
[PluginReference] Plugin GroupClear;

// The group to clear - e.g. "group1"
string groupName = "group1";
var result = GroupClear?.Call<Tuple<bool, int>>("ClearGroup", groupName);

bool found = result.Item1;
int memberCount = result.Item2;

if (found)
{
    Puts($"Removed '{memberCount}' players from '{groupName}'.");
}
else
{
    Puts($"Group '{groupName}' not found.");
}
```
### ClearGroups(List\<string\> groups) Example
```c#
[PluginReference] Plugin GroupClear;

// The groups to clear - e.g. "group1", "group2", and "group3"
List<string> groupNames = new List<string> {"group1", "group2", "group3"};
var results = GroupClear?.Call<List<Tuple<string, bool, int>>>("ClearGroups", groupNames);

foreach (var result in results)
{
    string groupName = result.Item1;
    bool found = result.Item2;
    int memberCount = result.Item3;

    if (found)
    {
        Puts($"Removed '{memberCount}' players from '{groupName}'.");
    }
    else
    {
        Puts($"Group '{groupName}' not found.");
    }
}
```
## Credits
- [Agamemnon](https://github.com/agamemnon23) - Code, testing.
- [Black_demon6](https://github.com/TheBlackdemon6) - Testing, French translations.
