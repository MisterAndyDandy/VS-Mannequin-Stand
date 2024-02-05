using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.Essentials;
using Vintagestory.GameContent;

namespace MannequinStand
{
    public class BlockMannequinPart : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible?.Code?.FirstPathPart(0) == "hammer" && byPlayer.Entity.LeftHandItemSlot.Empty)
            {
                ItemStack itemStack = new ItemStack(world.GetItem(new AssetLocation(blockSel.Block.Code.Domain, "mannequinstand" + "-" + blockSel.Block.Code.EndVariant())));
                world.SpawnItemEntity(itemStack, blockSel.FullPosition);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
