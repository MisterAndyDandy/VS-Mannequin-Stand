using Mannequins.Entities;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Mannequins.Util
{
    public class NameTagHandler
    {

        /// <summary>
        /// Handles the command to set a name tag on the entity mannequin or an item.
        /// </summary>
        /// <param name="args">The text command calling arguments.</param>
        /// <returns>The result of the command execution.</returns>
        public TextCommandResult SetNameTagCommand(TextCommandCallingArgs args)
        {
            ICoreAPI api = args.Caller.Entity.Api;
            if (api == null) return TextCommandResult.Success();

            IPlayer player = args.Caller.Player;
            if(player == null) return TextCommandResult.Success(); 
            
            EntityMannequin entityMannequin = player.CurrentEntitySelection?.Entity as EntityMannequin;

            if (SetEntityNameTag(entityMannequin, args.Parsers[0].GetValue().ToString()))
            {
                return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-set-name-entity"));
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
            ICoreAPI api = args.Caller.Entity.Api;
            if (api == null) return TextCommandResult.Success();

            IPlayer player = args.Caller.Player;
            if (player == null) return TextCommandResult.Success();

            EntityMannequin entityMannequin = player.CurrentEntitySelection?.Entity as EntityMannequin;

            if (HasEntityNameTag(entityMannequin))
            {
                if (RemoveAttribute(api, entityMannequin, player))
                {
                    return TextCommandResult.Success(Lang.Get("mannequins:command-nametag-remove-name-entity"));
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
        private bool RemoveAttribute(ICoreAPI api, EntityMannequin mannequin, IPlayer player)
        {
            SyncedTreeAttribute entityattributes = mannequin.WatchedAttributes;

            if (entityattributes.HasAttribute("itemNametag"))
            {
                ItemStack itemStack = entityattributes.GetItemstack("itemNametag");
                itemStack.Attributes.RemoveAttribute("name");
                itemStack.StackSize = 1;
                itemStack.ResolveBlockOrItem(api.World);

                if (!player.InventoryManager.TryGiveItemstack(itemStack, slotNotifyEffect: true))
                {
                    api.World.SpawnItemEntity(itemStack, mannequin.Pos.AsBlockPos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

               
                entityattributes.RemoveAttribute("itemNametag");
            }

            mannequin.WatchedAttributes.RemoveAttribute("name");
            return true;
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

    }
}
