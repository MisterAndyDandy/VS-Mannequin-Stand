using System.Linq;
using Mannequins.Entities;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Mannequins.Items
{
    /// <summary>
    /// Represents an item for placing mannequins in the world.
    /// </summary>
    public class ItemMannequin : Item
    {

        /// <summary>
        /// Handles the start of an interaction when the item is held.
        /// </summary>
        /// <param name="slot">The item slot containing the item.</param>
        /// <param name="byEntity">The entity performing the interaction.</param>
        /// <param name="blockSel">The block selection.</param>
        /// <param name="entitySel">The entity selection.</param>
        /// <param name="firstEvent">Indicates whether it's the first event in a sequence.</param>
        /// <param name="handling">The handling mode for the event.</param>
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null) return;
            IPlayer player = byEntity.World.PlayerByUid((byEntity as EntityPlayer)?.PlayerUID);

            if (!byEntity.World.Claims.TryAccess(player, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
            {
                slot.MarkDirty();
                return;
            }

            ItemStack itemStack = slot.Itemstack;
            if (!(byEntity is EntityPlayer) || player.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                itemStack = slot.TakeOut(1);
                slot.MarkDirty();
            }

            string name = this.FirstCodePart();
            EntityProperties type = byEntity.World.GetEntityType(new AssetLocation(this.Code.Domain, name));

            if (type == null) return;

            Entity entity = byEntity.World.ClassRegistry.CreateEntity(type);

            if (entity == null)
            {
                return;
            }

            if (entity is EntityMannequin entityMannequin)
            {
                entityMannequin.PlacedByItemStack = itemStack.Clone();
            }

            entity.ServerPos.X = blockSel.Position.X + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.X) + 0.5f;
            entity.ServerPos.Y = blockSel.Position.Y + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.Y);
            entity.ServerPos.Z = blockSel.Position.Z + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.Z) + 0.5f;
            entity.ServerPos.Yaw = byEntity.SidedPos.Yaw + GameMath.PI;
            if (player?.PlayerUID != null)
            {
                entity.WatchedAttributes.SetString("ownerUid", player.PlayerUID);
            }

            entity.Pos.SetFrom(entity.ServerPos);

            byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/block/planks"), entity, player);
            byEntity.World.SpawnEntity(entity);
            handling = EnumHandHandling.PreventDefaultAction;
        }

        /// <summary>
        /// Gets the name of the held item.
        /// </summary>
        /// <param name="itemStack">The item stack.</param>
        /// <returns>The name of the held item.</returns>
        public override string GetHeldItemName(ItemStack itemStack)
        {
            if (itemStack?.Attributes == null)
            {
                return string.Empty;
            }

            ITreeAttribute attribute = itemStack.Attributes;

            ICoreClientAPI capi = api as ICoreClientAPI;
            bool value = capi?.Settings?.String?.Get("language") == "en";

            if (attribute.HasAttribute("name"))
            {
                return attribute.GetString("name");
            }

            string materialName = Lang.GetMatching("game:material-" + Code.EndVariant());

            if (Code.EndVariant() == "baldcypress" && value)
            {
                return Lang.GetMatching(Code.Domain + ":item-mannequinstand" + ": {0}", "Bald Cypress");
            }

            return Lang.GetMatching(Code.Domain + ":item-mannequinstand" + ": {0}", materialName); 
        }

        /// <summary>
        /// Gets the interactions available when holding the item.
        /// </summary>
        /// <param name="inSlot">The item slot.</param>
        /// <returns>The interactions available when holding the item.</returns>
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction
                {
                    ActionLangCode = "heldhelp-place",
                    MouseButton = EnumMouseButton.Right
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }
    }
}