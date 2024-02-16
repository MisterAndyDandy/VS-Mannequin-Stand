using ProperVersion;
using System;
using System.Xml.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace MannequinStand
{
    class MannequinStandCore : ModSystem
    {

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterEntity("EntityMannequinStand", typeof(EntityMannequin));
            api.RegisterItemClass("ItemMannequinStand", typeof(ItemMannequin));
        }

        
        public override void StartServerSide(ICoreServerAPI api)
        {
            IChatCommandApi chatCommands = api.ChatCommands;

            CommandArgumentParsers parsers = api.ChatCommands.Parsers;

            chatCommands
                .Create("nametag")
                .WithDescription("")
                .WithArgs(new ICommandArgumentParser[] {parsers.All("nametag")})
                .RequiresPrivilege(Privilege.chat).HandleWith(NameTagCommand);
        }

        private TextCommandResult NameTagCommand(TextCommandCallingArgs args)
        {
            IPlayer _player = args.Caller.Player;

            if (_player is IServerPlayer player)
            {

                bool requirementItem = false;

                EntityMannequin entityMannequin = player.CurrentEntitySelection?.Entity as EntityMannequin;

                if (!args.Caller.Entity.World.Claims.TryAccess(_player, args.Caller.Pos.AsBlockPos, EnumBlockAccessFlags.BuildOrBreak))
                {
                    player.SendMessage(0, $"You don't have permission to set {_player?.CurrentEntitySelection?.Entity.GetName()}", EnumChatType.OwnMessage);
                }

                if (!requirementItem)
                {
                    ItemStack nameTagStack = null;

                    if (_player.Entity.ActiveHandItemSlot.Empty) { return TextCommandResult.Success(); }

                    if (_player.Entity.ActiveHandItemSlot.Itemstack?.Item is Item itemNameTag)
                    {
                        nameTagStack = new ItemStack(itemNameTag);
                        nameTagStack.Attributes.SetString("nametag", args.Parsers[0].GetValue().ToString());
                        _player.Entity.ActiveHandItemSlot.Itemstack = nameTagStack;
                        _player.InventoryManager.BroadcastHotbarSlot();
                        _player.Entity.ActiveHandItemSlot.MarkDirty();
                        return TextCommandResult.Success();
                    }
                }

                if (requirementItem && entityMannequin == null)
                {
                    player.SendMessage(0, $"You can't name {_player?.CurrentEntitySelection?.Entity.GetName()}", EnumChatType.OwnMessage);
                    return TextCommandResult.Success();
                }

                entityMannequin.WatchedAttributes.SetString("nametag", args.Parsers[0].GetValue().ToString());
            }

            return TextCommandResult.Success();
        }

    }
}
