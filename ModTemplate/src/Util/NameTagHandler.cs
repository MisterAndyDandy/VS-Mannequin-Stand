using System.Xml.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace MannequinStand.Util
{
    internal class NameTagHandler
    {

        /// <summary>
        /// Handles the command to set a name tag on the entity mannequin or an item.
        /// </summary>
        /// <param name="args">The text command calling arguments.</param>
        /// <returns>The result of the command execution.</returns>
        public TextCommandResult SetNameTagCommand(TextCommandCallingArgs args)
        {
            IPlayer player = args.Caller.Player;
            EntityMannequin entityMannequin = player.CurrentEntitySelection?.Entity as EntityMannequin;
            ItemStack itemStack = player.Entity.ActiveHandItemSlot?.Itemstack;

            if (entityMannequin != null && !HasBuildOrBreakPermission(player, args.Caller.Pos.AsBlockPos))
            {
                return TextCommandResult.Success(Lang.GetMatching("mannequins:command-nametag-set-name-missing-permission: {0}", entityMannequin.GetName()));
            }

            if (HasAdminPrivilege(player) && SetEntityNameTag(entityMannequin, args.Parsers[0].GetValue().ToString()))
            {
                return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-set-name-entity"));
            }

            if (HasNameTagInActiveHand(itemStack) && !itemStack.Attributes.HasAttribute("name"))
            {
                if (!HasInkAndQuillInOffHand(player.Entity.LeftHandItemSlot))
                {
                    return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-set-name-missing-item"));
                }

                if (SetItemNameTag(player, itemStack, args.Parsers[0].GetValue().ToString()))
                {
                    return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-set-name-item"));
                }
            }

            return TextCommandResult.Success();
        }

        /// <summary>
        /// Handles the command to remove the name tag from the entity mannequin or an item.
        /// </summary>
        /// <param name="args">The text command calling arguments.</param>
        /// <returns>The result of the command execution.</returns>
        public TextCommandResult RemoveNameTagCommand(TextCommandCallingArgs args)
        {
            IPlayer player = args.Caller.Player;
            EntityMannequin entityMannequin = player.CurrentEntitySelection?.Entity as EntityMannequin;
            ItemStack itemStack = player.Entity.ActiveHandItemSlot?.Itemstack;

            if (entityMannequin != null && !HasBuildOrBreakPermission(player, args.Caller.Pos.AsBlockPos))
            {
                return TextCommandResult.Success(Lang.GetMatching("mannequins:command-nametag-remove-name-missing-permission: {0}", entityMannequin.GetName()));
            }

            if (HasAdminPrivilege(player) && HasEntityNameTag(entityMannequin))
            {
                if (RemoveAttribute(entityMannequin, player))
                {
                    return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-remove-name-entity"));
                }
                else
                {
                    return TextCommandResult.Success(Lang.GetMatching("mannequins:command-nametag-remove-name-missing: {0}", itemStack?.Item.Tool is EnumTool.Knife or EnumTool.Shears));
                }
            }

            return TextCommandResult.Success();
        }

        /// <summary>
        /// Removes the name tag attribute from the specified entity mannequin and gives back the associated item to the player.
        /// </summary>
        /// <param name="mannequin">The entity mannequin from which to remove the attribute.</param>
        /// <param name="player">The player to receive the associated item.</param>
        /// <returns>True if the attribute was successfully removed; otherwise, false.</returns>
        private bool RemoveAttribute(EntityMannequin mannequin, IPlayer player)
        {
            SyncedTreeAttribute attributes = mannequin.WatchedAttributes;
     
            if (attributes.HasAttribute("nameTagItemStack"))
            {
                ItemStack itemStack = attributes.GetItemstack("nameTagItemStack");
                itemStack.StackSize = 1;
                itemStack.ResolveBlockOrItem(player.Entity.Api.World);
                itemStack.Attributes.RemoveAttribute("name");
                player.InventoryManager.TryGiveItemstack(itemStack, true);
                player.InventoryManager.BroadcastHotbarSlot();
              
                return true;
            }

            mannequin.WatchedAttributes.RemoveAttribute("name");
            return false;
        }

        /// <summary>
        /// Checks if the player has build or break permission at a specific position.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <param name="pos">The position to check.</param>
        /// <returns><c>true</c> if the player has build or break permission, <c>false</c> otherwise.</returns>
        private bool HasBuildOrBreakPermission(IPlayer player, BlockPos pos)
        {
            return player.Entity.World.Claims.TryAccess(player, pos, EnumBlockAccessFlags.BuildOrBreak);
        }

        /// <summary>
        /// Checks if the player has admin privilege.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><c>true</c> if the player has admin privilege, <c>false</c> otherwise.</returns>
        private bool HasAdminPrivilege(IPlayer player)
        {
            return player.Role.Code.StartsWith("admin");
        }

        /// <summary>
        /// Checks if the player has a name tag in the active hand.
        /// </summary>
        /// <param name="itemStack">The item stack to check.</param>
        /// <returns><c>true</c> if the player has a name tag in the active hand, <c>false</c> otherwise.</returns>
        private bool HasNameTagInActiveHand(ItemStack itemStack)
        {
            return itemStack != null && itemStack.Collectible.Code.FirstCodePart().Equals("nametag");
        }

        /// <summary>
        /// Checks if the player has ink and quill in the off hand.
        /// </summary>
        /// <param name="leftHand">The left hand slot to check.</param>
        /// <returns><c>true</c> if the player has ink and quill in the off hand, <c>false</c> otherwise.</returns>
        private bool HasInkAndQuillInOffHand(ItemSlot leftHand)
        {
            return !leftHand.Empty && leftHand.Itemstack.Collectible.Code.FirstCodePart().Equals("inkandquill");
        }

        /// <summary>
        /// Checks if the entity mannequin has a name tag attribute.
        /// </summary>
        /// <param name="mannequin">The entity mannequin to check.</param>
        /// <returns>True if the entity mannequin has a name tag attribute; otherwise, false.</returns>
        private bool HasEntityNameTag(EntityMannequin mannequin)
        {
            return mannequin != null && mannequin.WatchedAttributes.HasAttribute("name");
        }

        /// <summary>
        /// Sets the name tag for the mannequin entity.
        /// </summary>
        /// <param name="mannequin">The mannequin entity.</param>
        /// <param name="name">The name to set.</param>
        /// <returns><c>true</c> if the name tag was successfully set, <c>false</c> otherwise.</returns>
        private bool SetEntityNameTag(EntityMannequin mannequin, string name)
        {
            if (mannequin != null)
            {
                mannequin.WatchedAttributes.SetString("name", name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the name tag attribute on one item from the specified item stack and gives it back to the player if there is enough space in the inventory.
        /// </summary>
        /// <param name="player">The player to receive the modified item stack.</param>
        /// <param name="itemStack">The item stack to modify.</param>
        /// <param name="name">The name to set as the name tag.</param>
        /// <returns>True if the name tag was successfully set and the modified item stack was given back to the player; otherwise, false.</returns>
        private bool SetItemNameTag(IPlayer player, ItemStack itemStack, string name)
        {
            if(itemStack == null || itemStack.StackSize <= 0)
{
                return false;
            }

            ItemStack modifiedStack = itemStack.Clone();
            modifiedStack.StackSize = 1;
            modifiedStack.Attributes.SetString("name", name);

            player.InventoryManager.ActiveHotbarSlot.TakeOut(1);

            if (player.InventoryManager.TryGiveItemstack(modifiedStack))
            {   // Mark the active hotbar slot as dirty

                player.InventoryManager.ActiveHotbarSlot.MarkDirty();

                // Broadcast the hotbar slot to update the client
                player.InventoryManager.BroadcastHotbarSlot();

                return true;
            }

            return false;

        }

    }
}
