using ProperVersion;
using System;
using System.Xml.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace MannequinStand
{
    class MannequinStandCore : ModSystem
    {

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterEntity("EntityMannequinStand", typeof(EntityMannequin));
            api.RegisterItemClass("ItemMannequinStand", typeof(ItemMannequin));
            api.RegisterItemClass("ItemNameTag", typeof(ItemNameTag));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            IChatCommandApi chatCommands = api.ChatCommands;
            CommandArgumentParsers parsers = api.ChatCommands.Parsers;

            chatCommands
                .Create("nametag")
                .WithDescription("")
                .WithArgs(new ICommandArgumentParser[] { parsers.All("nametag") })
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(NameTagCommand);
        }

        private TextCommandResult NameTagCommand(TextCommandCallingArgs args)
        {
            IPlayer player = args.Caller.Player;

            if (!(player is IServerPlayer serverPlayer))
                return TextCommandResult.Success();

            if (!player.Entity.World.Claims.TryAccess(player, args.Caller.Pos.AsBlockPos, EnumBlockAccessFlags.BuildOrBreak))
            {
                serverPlayer.SendMessage(0, $"You don't have permission to set {player?.CurrentEntitySelection?.Entity.GetName()}", EnumChatType.OwnMessage);
                return TextCommandResult.Success();
            }

            if (player.Entity.ActiveHandItemSlot.Empty)
                return TextCommandResult.Success();

            ItemStack nameTagStack = player.Entity.ActiveHandItemSlot.Itemstack;

            SetItemNameTag(player, nameTagStack, args.Parsers[0].GetValue().ToString());

            return TextCommandResult.Success();
        }

        public void SetItemNameTag(IPlayer player, ItemStack nameTagStack, string name)
        {
            if (!(nameTagStack?.Item is ItemNameTag itemNameTag))
                return;

            nameTagStack = new ItemStack(itemNameTag);
            nameTagStack.Attributes.SetString("nametag", name);
            player.Entity.ActiveHandItemSlot.Itemstack = nameTagStack;
            player.InventoryManager.BroadcastHotbarSlot();
            player.Entity.ActiveHandItemSlot.MarkDirty();
        }
    }
}
