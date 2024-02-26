using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Mannequins.Util;
using Vintagestory.API.Common.CommandAbbr;
using Mannequins.Items;
using Mannequins.Entities;
using HarmonyLib;
using System.Reflection;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Newtonsoft.Json.Linq;
using System;

namespace Mannequins

{   /// <summary>
    /// Represents the core functionality of the Mannequin Stand mod.
    /// </summary>
    class MannequinStandCore : ModSystem
    {
        private NameTagHandler Handler = new NameTagHandler();

        /// <summary>
        /// Starts the Mannequin Stand mod by registering entities and items.
        /// </summary>
        /// <param name="api">The core API instance.</param>
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            RegisterClass(api);
        }

        /// <summary>
        /// Register class required by the Mannequin Stand mod.
        /// </summary>
        /// <param name="api">The core API instance.</param>
        private void RegisterClass(ICoreAPI api)
        {
            
            api.RegisterEntity("EntityMannequinStand", typeof(EntityMannequin));

            api.RegisterItemClass("ItemMannequinStand", typeof(ItemMannequin));

            api.RegisterEntityBehaviorClass("nameable", typeof(EntityBehaviorNameable));

            api.RegisterItemClass("ItemNameTag", typeof(ItemNameTag));
        }

        private struct BehaviorAsJsonObj
        {
            public string code;
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);

            BehaviorAsJsonObj newBehavior = new()
            {
                code = "nameable"
            };
            JsonObject newBehaviorJson = new(JToken.FromObject(newBehavior));

            if (api.Side.IsServer())
            {
                foreach (EntityProperties entityType in api.World.EntityTypes)
                {
                    bool alreadyHas = entityType.Server.BehaviorsAsJsonObj.Any(behavior => behavior["code"].AsString() == newBehavior.code);
                    if (!alreadyHas)
                    {
                        try
                        {
                            entityType.Server.BehaviorsAsJsonObj = entityType.Server.BehaviorsAsJsonObj.Append(newBehaviorJson).ToArray();
                            //api.Logger.VerboseDebug("[FSMlib] Adding behavior '{0}' to entity '{1}:{2}'", newBehavior.code, entityType.Class, entityType.Code);
                        }
                        catch (Exception ex)
                        {
                            api.Logger.Error($"Failed to add behavior '{newBehavior.code}' to entity '{entityType.Class}:{entityType.Code}': {ex.Message}");
                            // Optionally, handle the error or throw an exception
                        }
                    }
                }
            }

            if (api.Side.IsClient())
            {
                foreach (EntityProperties entityType in api.World.EntityTypes)
                {
                    bool alreadyHas = entityType.Client.BehaviorsAsJsonObj.Any(behavior => behavior["code"].AsString() == newBehavior.code);
                    if (!alreadyHas)
                    {
                        try
                        {
                            entityType.Client.BehaviorsAsJsonObj = entityType.Client.BehaviorsAsJsonObj.Append(newBehaviorJson).ToArray();
                            //api.Logger.VerboseDebug("[FSMlib] Adding behavior '{0}' to entity '{1}:{2}'", newBehavior.code, entityType.Class, entityType.Code);
                        }
                        catch (Exception ex)
                        {
                            api.Logger.Error($"Failed to add behavior '{newBehavior.code}' to entity '{entityType.Class}:{entityType.Code}': {ex.Message}");
                            // Optionally, handle the error or throw an exception
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts the server-side logic for managing chat commands.
        /// </summary>
        /// <param name="api">The core server API.</param>
        public override void StartServerSide(ICoreServerAPI api)
        {
            IChatCommandApi chatCommands = api.ChatCommands;
            CommandArgumentParsers parsers = api.ChatCommands.Parsers;

            chatCommands
                .Create("nametag")
                .WithDescription(Lang.Get("mannequins:command-nametag-desc"))
                .RequiresPrivilege(Privilege.gamemode)
                .BeginSubCommand("set")
                    .WithDescription(Lang.Get("mannequins:subcommand-set-desc")) 
                    .WithArgs(new ICommandArgumentParser[] { parsers.All("set name") }) 
                    .HandleWith(Handler.SetNameTagCommand) 
                .EndSub() 
                .BeginSubCommand("remove")
                    .WithDescription(Lang.Get("mannequins:subcommand-remove-desc")) 
                    .HandleWith(Handler.RemoveNameTagCommand);
        }
    }
}
