using Vintagestory.API.Common;

namespace MannequinStand.Common
{
    public class InventoryMannequin : InventoryGeneric
    {
        // In order they appear on the character screen
        public static readonly int[] ClothingSlotIds = new int[6] { 0, 1, 2, 11, 3, 4 };

        // In order they appear on the character screen
        public static readonly int[] AccessorySlotIds = new int[6] { 6, 7, 8, 10, 5, 9 };

        public const int HeadArmorSlotId = 12;
        public const int BodyArmorSlotId = 13;
        public const int LegsArmorSlotId = 14;
        public const int LeftHandSlotId = 15;
        public const int RightHandSlotId = 16;
        public const int BackpackSlotId = 17;

        protected static readonly int quantitySlots = 18;

        public ItemSlot LeftHandItemSlot => this[LeftHandSlotId];
        public ItemSlot RightHandItemSlot => this[RightHandSlotId];
        public ItemSlot BackpackItemSlot => this[BackpackSlotId];

        public InventoryMannequin(string className, string instanceId, ICoreAPI api) : base(quantitySlots, className, instanceId, api) { }

        public InventoryMannequin(string invId, ICoreAPI api) : base(quantitySlots, invId, api) { }

        public override float GetSuitability(ItemSlot sourceSlot, ItemSlot targetSlot, bool isMerge)
        {
            return (!isMerge) ? (baseWeight + 1f) : (baseWeight + 3f);
        }

        protected override ItemSlot NewSlot(int slotId)
        {
            switch (slotId)
            {
                case LeftHandSlotId:
                    return new ItemSlotMannequinHand(this, isOffhandSlot: true);
                case RightHandSlotId:
                    return new ItemSlotMannequinHand(this);
                case BackpackSlotId:
                    return new ItemSlotMannequinBackpack(this);
                default:
                    return new ItemSlotMannequinWearable(this, (EnumCharacterDressType)slotId);
            }
        }
    }
}
