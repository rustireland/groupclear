using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("GroupClear", "Agamemnon", "1.0.0")]
    [Description("Removes all members from an Oxide group (or list of groups), either by command or at server wipe.")]
    class GroupClear : RustPlugin
    {
        private const string _permAdmin = "groupclear.admin";

        private ConfigData _configData;
        private bool _wiped = false;

        #region Oxide Hooks
        private void OnServerInitialized()
        {
            permission.RegisterPermission(_permAdmin, this);

            if (!LoadConfigVariables())
            {
                PrintError(Lang("ConsoleConfigError"));
                PrintError(Lang("ConsoleUnloading"));
                Interface.Oxide.UnloadPlugin(this.Title);
                return;
            }

            if (_configData.ClearOnWipe)
            {
                if (_wiped)
                {
                    Puts(Lang("ConsoleClearAllGroupsWipe"));

                    int groupCount = 0;
                    var groups = ClearGroups(_configData.Groups);

                    foreach (var group in groups)
                    {
                        string groupName = group.Item1;
                        bool found = group.Item2;
                        int memberCount = group.Item3;

                        if (found)
                        {
                            Puts(Lang("ConsoleClearGroup",
                                new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                                new KeyValuePair<string, string>("group", groupName)));
                        }
                        else
                        {
                            Puts(Lang("ConsoleGroupNotFound",
                                new KeyValuePair<string, string>("group", groupName)));
                        }

                        groupCount = groupCount + 1;
                    }

                    if (groupCount == 0)
                        Puts(Lang("ConsoleNoGroups"));
                }
            }
            else
            {
                Unsubscribe(nameof(OnNewSave));
            }
        }

        private void OnNewSave()
        {
            _wiped = true;
        }
        #endregion

        #region Chat Commands
        [ChatCommand("cleargroup")]
        private void chatCmdClearGroup(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _permAdmin))
            {
                SendReply(player, Lang("ChatPermissionDenied"));
                return;
            }

            List<string> argsList = new List<string>(args);
            if (argsList.Count == 0)
            {
                SendReply(player, Lang("ChatClearGroupUsage"));
                return;
            }

            string groupName = argsList[0];
            var group = ClearGroup(groupName);
            bool found = group.Item1;
            int memberCount = group.Item2;

            if (found)
            {
                SendReply(player, Lang("ChatClearGroup",
                    new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                    new KeyValuePair<string, string>("group", groupName)));

                Puts(Lang("ConsoleClearGroupLog",
                    new KeyValuePair<string, string>("player", player.displayName),
                    new KeyValuePair<string, string>("steamid", player.UserIDString),
                    new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                    new KeyValuePair<string, string>("group", groupName)));
            }
            else
            {
                SendReply(player, Lang("ChatGroupNotFound",
                    new KeyValuePair<string, string>("group", groupName)));
            }
        }

        [ChatCommand("clearallgroups")]
        private void chatCmdClearAllGroups(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, _permAdmin))
            {
                SendReply(player, Lang("ChatPermissionDenied"));
                return;
            }

            List<string> argsList = new List<string>(args);
            if (argsList.Count == 0)
            {
                SendReply(player, Lang("ChatClearAllGroupsUsage"));
                return;
            }

            string confirm = argsList[0];

            if (confirm != "confirm")
            {
                SendReply(player, Lang("ChatClearAllGroupsUsage"));
                return;
            }
   
            SendReply(player, Lang("ChatClearAllGroups"));

            Puts(Lang("ConsoleClearAllGroupsLog",
                new KeyValuePair<string, string>("player", player.displayName),
                new KeyValuePair<string, string>("steamid", player.UserIDString)));

            int groupCount = 0;
            var groups = ClearGroups(_configData.Groups);

            foreach (var group in groups)
            {
                string groupName = group.Item1;
                bool found = group.Item2;
                int memberCount = group.Item3;

                if (found)
                {
                    SendReply(player, Lang("ChatClearGroup",
                        new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                        new KeyValuePair<string, string>("group", groupName)));

                    Puts(Lang("ConsoleClearGroupLog",
                        new KeyValuePair<string, string>("player", player.displayName),
                        new KeyValuePair<string, string>("steamid", player.UserIDString),
                        new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                        new KeyValuePair<string, string>("group", groupName)));
                }
                else
                {
                    SendReply(player, Lang("ChatGroupNotFound",
                        new KeyValuePair<string, string>("group", groupName)));

                    Puts(Lang("ConsoleGroupNotFound",
                        new KeyValuePair<string, string>("group", groupName)));
                }

                groupCount = groupCount + 1;
            }

            if (groupCount == 0)
            {
                SendReply(player, Lang("ChatNoGroups"));
                Puts(Lang("ConsoleNoGroups"));
            }
        }
        #endregion

        #region Console Commands
        [ConsoleCommand("cleargroup")]
        private void consoleCmdClearGroup(ConsoleSystem.Arg arg)
        {
            string userName = "Unknown";
            string userId = "0";

            if (arg.Connection != null)
            {
                BasePlayer player = arg.Connection.player as BasePlayer;
                userName = player.displayName;
                userId = player.UserIDString;

                if (!permission.UserHasPermission(player.UserIDString, _permAdmin))
                {
                    SendReply(arg, Lang("ConsolePermissionDenied"));
                    return;
                }
            }

            if (arg.Args == null)
            {
                SendReply(arg, Lang("ConsoleClearGroupUsage"));
                return;
            }

            List<string> argsList = new List<string>(arg.Args);
            string groupName = argsList[0];
            var group = ClearGroup(groupName);
            bool found = group.Item1;
            int memberCount = group.Item2;

            if (found)
            {
                SendReply(arg, Lang("ConsoleClearGroup",
                    new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                    new KeyValuePair<string, string>("group", groupName)));

                if (arg.Connection != null)
                {
                    Puts(Lang("ConsoleClearGroupLog",
                        new KeyValuePair<string, string>("player", userName),
                        new KeyValuePair<string, string>("steamid", userId),
                        new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                        new KeyValuePair<string, string>("group", groupName)));
                }
            }
            else
            {
                SendReply(arg, Lang("ConsoleGroupNotFound",
                    new KeyValuePair<string, string>("group", groupName)));
            }
        }

        [ConsoleCommand("clearallgroups")]
        private void consoleCmdClearAllGroups(ConsoleSystem.Arg arg)
        {
            string userName = "Unknown";
            string userId = "0";

            if (arg.Connection != null)
            {
                BasePlayer player = arg.Connection.player as BasePlayer;
                userName = player.displayName;
                userId = player.UserIDString;

                if (!permission.UserHasPermission(player.UserIDString, _permAdmin))
                {
                    SendReply(arg, Lang("ConsolePermissionDenied"));
                    return;
                }
            }

            if (arg.Args == null)
            {
                SendReply(arg, Lang("ConsoleClearAllGroupsUsage"));
                return;
            }

            List<string> argsList = new List<string>(arg.Args);
            string confirm = argsList[0];

            if (confirm != "confirm")
            {
                SendReply(arg, Lang("ConsoleClearAllGroupsUsage"));
                return;
            }
   
            SendReply(arg, Lang("ConsoleClearAllGroups"));

            if (arg.Connection != null)
            {
                Puts(Lang("ConsoleClearAllGroupsLog",
                    new KeyValuePair<string, string>("player", userName),
                    new KeyValuePair<string, string>("steamid", userId)));
            }

            int groupCount = 0;
            var groups = ClearGroups(_configData.Groups);

            foreach (var group in groups)
            {
                string groupName = group.Item1;
                bool found = group.Item2;
                int memberCount = group.Item3;

                if (found)
                {
                    SendReply(arg, Lang("ConsoleClearGroup",
                        new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                        new KeyValuePair<string, string>("group", groupName)));

                    if (arg.Connection != null)
                    {
                        Puts(Lang("ConsoleClearGroupLog",
                            new KeyValuePair<string, string>("player", userName),
                            new KeyValuePair<string, string>("steamid", userId),
                            new KeyValuePair<string, string>("memberCount", memberCount.ToString()),
                            new KeyValuePair<string, string>("group", groupName)));
                    }
                }
                else
                {
                    SendReply(arg, Lang("ConsoleGroupNotFound",
                        new KeyValuePair<string, string>("group", groupName)));

                    if (arg.Connection != null)
                    {
                        Puts(Lang("ConsoleGroupNotFound",
                            new KeyValuePair<string, string>("group", groupName)));
                    }
                }

                groupCount = groupCount + 1;
            }

            if (groupCount == 0)
            {
                SendReply(arg, Lang("ConsoleNoGroups"));

                if (arg.Connection != null)
                {
                    Puts(Lang("ConsoleNoGroups"));
                }
            }
        }
        #endregion

        #region API
        private Tuple<bool, int> ClearGroup(string group)
        {
            bool found = false;
            int memberCount = 0;

            if (permission.GroupExists(group))
            {
                found = true;
                foreach (string member in permission.GetUsersInGroup(group).ToList())
                {
                    string memberId = member.Substring(0, 17);
                    permission.RemoveUserGroup(memberId, group);
                    memberCount = memberCount + 1;
                }
            }

            return new Tuple<bool, int>(found, memberCount);
        }

        private List<Tuple<string, bool, int>> ClearGroups(List<string> groups)
        {
            List<Tuple<string, bool, int>> results = new List<Tuple<string, bool, int>>();

            foreach (var groupName in groups)
            {
                var result = ClearGroup(groupName);
                bool found = result.Item1;
                int memberCount = result.Item2;

                results.Add(new Tuple<string, bool, int>(groupName, found, memberCount));
            }

            return results;
        }
        #endregion

        #region Configuration
        private class ConfigData
        {
            [JsonProperty(PropertyName = "Clear all group members on server wipe")]
            public bool ClearOnWipe = false;

            [JsonProperty(PropertyName = "Groups", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<string> Groups = new List<string> { "group1", "group2", "group3" };
        }

        private bool LoadConfigVariables()
        {
            try
            {
                _configData = Config.ReadObject<ConfigData>();
            }
            catch
            {
                return false;
            }

            SaveConfig(_configData);
            return true;
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating new config file.");
            _configData = new ConfigData();
            SaveConfig(_configData);
        }

        private void SaveConfig(ConfigData config)
        {
            Config.WriteObject(config, true);
        }
        #endregion

        #region Language
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ChatPermissionDenied"] = "You do not have permission to use this command.",
                ["ChatClearGroupUsage"] = "Usage: <color=#ffa500>/cleargroup</color> <color=#add8e6><group></color>",
                ["ChatClearGroup"] = "Cleared <color=#ffa500>{memberCount}</color> players from group <color=#ffa500>{group}</color>.",
                ["ChatClearAllGroupsUsage"] = "Usage: <color=#ffa500>/clearallgroups</color> <color=#add8e6>confirm</color>",
                ["ChatClearAllGroups"] = "Initializing manual Oxide group member clearance:",
                ["ChatGroupNotFound"] = "Group <color=#ffa500>{group}</color> does not exist.",
                ["ChatNoGroups"] = "No groups have been configured.",
                ["ConsolePermissionDenied"] = "You do not have permission to use this command.",
                ["ConsoleClearGroupUsage"] = "Usage: cleargroup <group>",
                ["ConsoleClearGroup"] = "Cleared {memberCount} players from group '{group}'.",
                ["ConsoleClearGroupLog"] = "{player} ({steamid}) cleared {memberCount} players from group '{group}'.",
                ["ConsoleClearAllGroupsUsage"] = "Usage: clearallgroups confirm",
                ["ConsoleClearAllGroups"] = "Initializing manual Oxide group member clearance:",
                ["ConsoleClearAllGroupsWipe"] = "Initializing post-wipe Oxide group member clearance:",
                ["ConsoleClearAllGroupsLog"] = "{player} ({steamid}) initialized a manual Oxide group member clearance:",
                ["ConsoleGroupNotFound"] = "Group '{group}' does not exist.",
                ["ConsoleNoGroups"] = "No groups have been configured.",
                ["ConsoleConfigError"] = "The config file is corrupt. Either fix or delete it and restart the plugin.",
                ["ConsoleUnloading"] = "Unloading plugin."
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ChatPermissionDenied"] = "You do not have permission to use this command.",
                ["ChatClearGroupUsage"] = "Usage: <color=#ffa500>/cleargroup</color> <color=#add8e6><group></color>",
                ["ChatClearGroup"] = "Cleared <color=#ffa500>{memberCount}</color> players from group <color=#ffa500>{group}</color>.",
                ["ChatClearAllGroupsUsage"] = "Usage: <color=#ffa500>/clearallgroups</color> <color=#add8e6>confirm</color>",
                ["ChatClearAllGroups"] = "Initializing manual Oxide group member clearance:",
                ["ChatGroupNotFound"] = "Group <color=#ffa500>{group}</color> does not exist.",
                ["ChatNoGroups"] = "No groups have been configured.",
                ["ConsolePermissionDenied"] = "You do not have permission to use this command.",
                ["ConsoleClearGroupUsage"] = "Usage: cleargroup <group>",
                ["ConsoleClearGroup"] = "Cleared {memberCount} players from group '{group}'.",
                ["ConsoleClearGroupLog"] = "{player} ({steamid}) cleared {memberCount} players from group '{group}'.",
                ["ConsoleClearAllGroupsUsage"] = "Usage: clearallgroups confirm",
                ["ConsoleClearAllGroups"] = "Initializing manual Oxide group member clearance:",
                ["ConsoleClearAllGroupsWipe"] = "Initializing post-wipe Oxide group member clearance:",
                ["ConsoleClearAllGroupsLog"] = "{player} ({steamid}) initialized a manual Oxide group member clearance:",
                ["ConsoleGroupNotFound"] = "Group '{group}' does not exist.",
                ["ConsoleNoGroups"] = "No groups have been configured.",
                ["ConsoleConfigError"] = "The config file is corrupt. Either fix or delete it and restart the plugin.",
                ["ConsoleUnloading"] = "Unloading plugin."
            }, this, "fr");
        }

        private string Lang(string key) => string.Format(lang.GetMessage(key, this));
        private string Lang(string key, params KeyValuePair<string, string>[] replacements)
        {
            var message = lang.GetMessage(key, this);

            foreach (var replacement in replacements)
                message = message.Replace($"{{{replacement.Key}}}", replacement.Value);

            return message;
        }
        #endregion
    }
}
