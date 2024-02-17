using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace MannequinStand
{
    /// <summary>
    /// Represents a name tag item used to label entities or objects.
    /// </summary>
    public class ItemNameTag : Item
    {
        /// <summary>
        /// Handles the start of interaction with the held name tag item.
        /// </summary>
        /// <param name="slot">The item slot containing the name tag.</param>
        /// <param name="byEntity">The entity holding the name tag.</param>
        /// <param name="blockSel">The block selected during interaction.</param>
        /// <param name="entitySel">The entity selected during interaction.</param>
        /// <param name="firstEvent">Indicates if this is the first interaction event.</param>
        /// <param name="handling">The handling mode of the interaction.</param>
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null) return;

            IPlayer player = byEntity.World.PlayerByUid((byEntity as EntityPlayer)?.PlayerUID);

            if (!byEntity.World.Claims.TryAccess(player, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
            {
                
                slot.MarkDirty();
                return;
            }

            handling = EnumHandHandling.PreventDefaultAction;
        }

        /// <summary>
        /// Gets the display name of the held name tag item.
        /// </summary>
        /// <param name="itemStack">The ItemStack representing the name tag.</param>
        /// <returns>The display name of the name tag item.</returns>
        public override string GetHeldItemName(ItemStack itemStack)
        {
            ITreeAttribute attribute = itemStack.Attributes;

            if (attribute.HasAttribute("nametag"))
            {
                return Lang.GetMatching("mannequins:item-nametag: {0}", $"({attribute.GetAsString("nametag")})");
            }

            return Lang.GetMatching("mannequins:item-nametag: {0}", "".RemoveDiacritics());
        }

        /// <summary>
        /// Gets the list of world interactions provided by the held name tag item.
        /// </summary>
        /// <param name="inSlot">The slot containing the name tag item.</param>
        /// <returns>An array of WorldInteraction instances.</returns>
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
           
            WorldInteraction placeInteraction = new WorldInteraction
            {
                ActionLangCode = "heldhelp-place",
                MouseButton = EnumMouseButton.Right
            };

            return new WorldInteraction[] { placeInteraction }.Append(base.GetHeldInteractionHelp(inSlot)).ToArray();
        }
    }
}