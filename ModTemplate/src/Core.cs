using Vintagestory.API.Common;
using Vintagestory.API.Config;
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
            api.RegisterItemClass("ItemNameTag", typeof(ItemNameTag));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            IChatCommandApi chatCommands = api.ChatCommands;
            CommandArgumentParsers parsers = api.ChatCommands.Parsers;
            
            chatCommands
                .Create("nametag")
                .WithDescription( Lang.Get("mannequins:command-nametag-desc"))
                .WithArgs(new ICommandArgumentParser[] { parsers.All("custom name") })
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(NameTagCommand);
        }

        public TextCommandResult NameTagCommand(TextCommandCallingArgs args)
        {
            IPlayer player = args.Caller.Player;
            EntityMannequin entityMannequin = player.CurrentEntitySelection?.Entity as EntityMannequin;
            ItemStack itemStack = player.Entity.ActiveHandItemSlot?.Itemstack;

            if (entityMannequin != null && !player.Entity.World.Claims.TryAccess(player, args.Caller.Pos.AsBlockPos, EnumBlockAccessFlags.BuildOrBreak))
            {
                return TextCommandResult.Success(Lang.GetMatching("", entityMannequin.GetName()));
            }

            if (player.Role.Code.StartsWith("admin") && SetEntityNameTag(entityMannequin, args.Parsers[0].GetValue().ToString()))
            {
                return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-success-entity"));
            }
         
            if (GetActiveHandSlot(player.Entity.ActiveHandItemSlot)) 
            {
                if (IsNameTag(player.Entity.ActiveHandItemSlot.Itemstack))
                {
                    if (GetOffHandSlot(player.Entity.LeftHandItemSlot))
                    {
                        if (SetItemNameTag(player, itemStack, args.Parsers[0].GetValue().ToString()))
                        {
                            return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-success-item"));
                        }
                    }
                    else 
                    {
                        return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-missing-item"));
                    }
                }
            }

            return TextCommandResult.Success();
        }

        public bool GetActiveHandSlot(ItemSlot rightHand) 
        {
            return !rightHand.Empty;
        }

        public bool GetOffHandSlot(ItemSlot leftHand)
        {
            return leftHand.Empty == false ? leftHand.Itemstack.Collectible.Code.FirstCodePart().Equals("inkandquill") : false;
        }

        public bool IsNameTag(ItemStack itemStack) 
        {
            return itemStack.Collectible.Code.FirstCodePart().Equals("nametag");
        }

        public bool SetEntityNameTag(EntityMannequin mannequin, string name)
        {
            if (mannequin != null)
            {
                mannequin.WatchedAttributes.SetString("nametag", name);
                return true;
            }
            return false;
        }

        public bool SetItemNameTag(IPlayer player, ItemStack itemStack, string name)
        {
            itemStack.Attributes.SetString("nametag", name);
            player.Entity.ActiveHandItemSlot.Itemstack = itemStack;
            player.InventoryManager.BroadcastHotbarSlot();
            player.Entity.ActiveHandItemSlot.MarkDirty();
            return true;
        }
    }
}
