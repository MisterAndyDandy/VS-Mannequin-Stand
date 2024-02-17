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
    public class ItemNameTag : Item
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

            handling = EnumHandHandling.PreventDefaultAction;
        }

        public override string GetHeldItemName(ItemStack itemStack)
        {
            ITreeAttribute attribute = itemStack.Attributes;

            if (attribute.HasAttribute("nametag")) 
            {
                return Lang.GetMatching("mannequins:item-nametag: {0}", $"({attribute.GetAsString("nametag")})");
            }

            return Lang.GetMatching("mannequins:item-nametag: {0}", "".RemoveDiacritics());
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
