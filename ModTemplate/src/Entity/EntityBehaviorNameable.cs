using System;
using System.IO;
using Mannequins.Client;
using Mannequins.Items;
using MannequinStand.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Mannequins.Entities
{
    public class EntityBehaviorNameable : EntityBehavior
    {

        public EntityBehaviorNameable(Entity entity)
            : base(entity)
        {
        }

        public override void Initialize(EntityProperties entityType, JsonObject attributes)
        {
            base.Initialize(entityType, attributes);
        }


        public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode, ref EnumHandling handled)
        {
            if(itemslot.Empty) { return; }

            ITreeAttribute itemAttribute = itemslot?.Itemstack.Attributes.GetTreeAttribute("nametag");

            if (itemAttribute.HasAttribute("nametag")) 
            {
                if (entity.WatchedAttributes.HasAttribute("name")) return;

                SetName(itemAttribute.GetString("nametag"));
            }

            base.OnInteract(byEntity, itemslot, hitPosition, mode, ref handled);
        }

        public override void OnEntitySpawn()
        {
            base.OnEntitySpawn();
        }

        //
        // Summary:
        //     Sets the display name of the entitys.
        //
        // Parameters:
        //   playername:
        public void SetName(string entitys)
        {
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("nametag");
            if (treeAttribute == null)
            {
                entity.WatchedAttributes.SetAttribute("nametag", treeAttribute = new TreeAttribute());
            }

            treeAttribute.SetString("name", entitys);
            entity.WatchedAttributes.MarkPathDirty("nametag");
        }

        public override string PropertyName()
        {
            return "nameable";
        }
    }
}
