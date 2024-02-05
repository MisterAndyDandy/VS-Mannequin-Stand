using Vintagestory.API.Common;

namespace MannequinStand
{
    class MannequinStandCore : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterEntity("EntityMannequinStand", typeof(EntityMannequin));
            api.RegisterItemClass("ItemMannequinStand", typeof(ItemMannequin));
        }

    }
}
