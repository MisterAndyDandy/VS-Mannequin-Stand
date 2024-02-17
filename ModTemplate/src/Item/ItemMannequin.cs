using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace MannequinStand
{

    public class ItemMannequin : Item
    {
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

        public override string GetHeldItemName(ItemStack itemStack)
        {
            ITreeAttribute attribute = itemStack.Attributes;

            ISettingsClass<string> en = (api as ICoreClientAPI)?.Settings.String;
            bool value = en.Get("language") == "en";

            if (attribute.HasAttribute("nametag"))
            {
                return attribute.GetAsString("nametag");
            }
     
            if (Code.EndVariant() == "baldcypress" && value) 
            {
                return Lang.GetMatching(Code.Domain + ":item-mannequinstand" + ": {0}", "Bald Cypress");
            }

            return Lang.GetMatching(Code.Domain + ":item-mannequinstand" + ": {0}", Lang.GetMatching("game:material-" + Code.EndVariant()));
        }

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
