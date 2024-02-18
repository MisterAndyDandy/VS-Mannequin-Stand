using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using MannequinStand.Util;
using Vintagestory.API.Common.CommandAbbr;

namespace MannequinStand

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

         
            api.RegisterItemClass("ItemNameTag", typeof(ItemNameTag));
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
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand("set")
                    .WithDescription(Lang.Get("mannequins:subcommand-set-desc")) // Description for the 'set' subcommand
                    .WithArgs(new ICommandArgumentParser[] { parsers.All("set name") }) // Arguments for the 'set' subcommand
                    .HandleWith(Handler.SetNameTagCommand) // Handler method for the 'set' subcommand
                .EndSub() // End the 'set' subcommand
                .BeginSubCommand("remove")
                    .WithDescription(Lang.Get("mannequins:subcommand-remove-desc")) // Description for the 'remove' subcommand
                    .HandleWith(Handler.RemoveNameTagCommand); // Handler method for the 'remove' subcommand
        }
    }
}
