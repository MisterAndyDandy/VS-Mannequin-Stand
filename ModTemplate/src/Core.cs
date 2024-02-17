using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using MannequinStand.Util;

namespace MannequinStand

{   /// <summary>
    /// Represents the core functionality of the Mannequin Stand mod.
    /// </summary>
    class MannequinStandCore : ModSystem
    {
        NameTagHandler Handler = new NameTagHandler();

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
                .WithArgs(new ICommandArgumentParser[] { parsers.All("custom name") }) 
                .RequiresPrivilege(Privilege.chat) 
                .HandleWith(Handler.NameTagCommand);
        }
    }
}
