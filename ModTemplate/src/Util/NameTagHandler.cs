using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace MannequinStand.Util
{
    internal class NameTagHandler
    {
        /// <summary>
        /// Handles the nametag command.
        /// </summary>
        /// <param name="args">The text command calling arguments.</param>
        /// <returns>The result of the command execution.</returns>
        public TextCommandResult NameTagCommand(TextCommandCallingArgs args)
        {
            IPlayer player = args.Caller.Player;
            EntityMannequin entityMannequin = player.CurrentEntitySelection?.Entity as EntityMannequin;
            ItemStack itemStack = player.Entity.ActiveHandItemSlot?.Itemstack;

            if (entityMannequin != null && !HasBuildOrBreakPermission(player, args.Caller.Pos.AsBlockPos))
            {
                return TextCommandResult.Success(Lang.GetMatching("", entityMannequin.GetName()));
            }

            if (HasAdminPrivilege(player) && SetEntityNameTag(entityMannequin, args.Parsers[0].GetValue().ToString()))
            {
                return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-success-entity"));
            }

            if (HasNameTagInActiveHand(itemStack) && HasInkAndQuillInOffHand(player.Entity.LeftHandItemSlot))
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

            return TextCommandResult.Success();
        }

        /// <summary>
        /// Checks if the player has build or break permission at a specific position.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <param name="pos">The position to check.</param>
        /// <returns><c>true</c> if the player has build or break permission, <c>false</c> otherwise.</returns>
        public bool HasBuildOrBreakPermission(IPlayer player, BlockPos pos)
        {
            return player.Entity.World.Claims.TryAccess(player, pos, EnumBlockAccessFlags.BuildOrBreak);
        }

        /// <summary>
        /// Checks if the player has admin privilege.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><c>true</c> if the player has admin privilege, <c>false</c> otherwise.</returns>
        public bool HasAdminPrivilege(IPlayer player)
        {
            return player.Role.Code.StartsWith("admin");
        }

        /// <summary>
        /// Checks if the player has a name tag in the active hand.
        /// </summary>
        /// <param name="itemStack">The item stack to check.</param>
        /// <returns><c>true</c> if the player has a name tag in the active hand, <c>false</c> otherwise.</returns>
        public bool HasNameTagInActiveHand(ItemStack itemStack)
        {
            return itemStack != null && itemStack.Collectible.Code.FirstCodePart().Equals("nametag");
        }

        /// <summary>
        /// Checks if the player has ink and quill in the off hand.
        /// </summary>
        /// <param name="leftHand">The left hand slot to check.</param>
        /// <returns><c>true</c> if the player has ink and quill in the off hand, <c>false</c> otherwise.</returns>
        public bool HasInkAndQuillInOffHand(ItemSlot leftHand)
        {
            return !leftHand.Empty && leftHand.Itemstack.Collectible.Code.FirstCodePart().Equals("inkandquill");
        }

        /// <summary>
        /// Sets the name tag for the mannequin entity.
        /// </summary>
        /// <param name="mannequin">The mannequin entity.</param>
        /// <param name="name">The name to set.</param>
        /// <returns><c>true</c> if the name tag was successfully set, <c>false</c> otherwise.</returns>
        public bool SetEntityNameTag(EntityMannequin mannequin, string name)
        {
            if (mannequin != null)
            {
                mannequin.WatchedAttributes.SetString("nametag", name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the name tag for the item in the player's hand.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="itemStack">The item stack.</param>
        /// <param name="name">The name to set.</param>
        /// <returns><c>true</c> if the name tag was successfully set, <c>false</c> otherwise.</returns>
        public bool SetItemNameTag(IPlayer player, ItemStack itemStack, string name)
        {
            if (itemStack != null && itemStack.StackSize > 0)
            {
                ItemStack modifiedStack = itemStack.Clone();
                modifiedStack.StackSize = 1;

                modifiedStack.Attributes.SetString("nametag", name);

                player.InventoryManager.TryGiveItemstack(modifiedStack);

                itemStack.StackSize--;

                if (itemStack.StackSize == 0)
                {
                    player.InventoryManager.ActiveHotbarSlot.Itemstack = null;
                }
                else
                {
                    
                    player.InventoryManager.BroadcastHotbarSlot();
                    player.InventoryManager.ActiveHotbarSlot.MarkDirty();
                }

                return true;
            }
            return false;
        }


    }
}
