using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using System.Collections.Generic;
using System.Text;
using Mannequins.Util;
using Mannequins.Client;
using Mannequins.Entities;

namespace Mannequins.Items
{
    /// <summary>
    /// Represents a name tag item used to label entities or objects.
    /// </summary>
    public class ItemNameTag : Item
    {
        private ModSystemEditableNameTag nameTagModSys;

        private ICoreClientAPI capi;

        private WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            capi = api as ICoreClientAPI;
            nameTagModSys = api.ModLoader.GetModSystem<ModSystemEditableNameTag>();
            interactions = ObjectCacheUtil.GetOrCreate(api, "nameTagInteractions", delegate
            {
                List<ItemStack> list = new List<ItemStack>();
                foreach (CollectibleObject collectible in api.World.Collectibles)
                {
                    if (collectible.Attributes != null && collectible.Attributes.IsTrue("writingTool"))
                    {
                        list.Add(new ItemStack(collectible));
                    }
                }

                return new WorldInteraction[1]
                {
                new WorldInteraction
                {
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = list.ToArray(),
                    ActionLangCode = "heldhelp-write",
                    GetMatchingStacks = (WorldInteraction wi, BlockSelection bs, EntitySelection es) => (capi.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetString("signedby") == null) ? wi.Itemstacks : null
                }
                };
            });
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (byEntity.Controls.Sneak)
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
                return;
            }

            if (entitySel?.Entity.HasBehavior("") ?? false)
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
                return;
            }

            IPlayer player = (byEntity as EntityPlayer).Player;
            if (isWritingTool(byEntity.LeftHandItemSlot))
            {
                nameTagModSys.BeginEdit(player, slot);
                if (api.Side == EnumAppSide.Client)
                {
                    GuiDialogEditableNameTag dlg = new GuiDialogEditableNameTag(slot.Itemstack, api as ICoreClientAPI);
                    dlg.OnClosed += delegate
                    {
                        if (dlg.DidSave)
                        {
                            nameTagModSys.EndEdit(player, dlg.Name);
                        }
                        else
                        {
                            nameTagModSys.CancelEdit(player);
                        }
                    };
                    dlg.TryOpen();
                }

                handling = EnumHandHandling.PreventDefault;
            }
            else
            {
                (api as ICoreClientAPI)?.TriggerIngameError(!isWritingTool(byEntity.LeftHandItemSlot), "noink", Lang.Get("Need ink and quill in my off hand"));
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            }
        }

        public override string GetHeldItemName(ItemStack itemStack)
        {
            string @string = itemStack.Attributes.GetString("name");
            if (@string != null && @string.Length > 0)
            {
                return @string;
            }

            return base.GetHeldItemName(itemStack);
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }

        public static bool isWritingTool(ItemSlot slot)
        {
            ItemStack itemstack = slot.Itemstack;
            if (itemstack == null)
            {
                return false;
            }

            return itemstack.Collectible.Attributes?.IsTrue("writingTool") == true;
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }
    }
}