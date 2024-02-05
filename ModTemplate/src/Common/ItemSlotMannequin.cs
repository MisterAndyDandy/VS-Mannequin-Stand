using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace MannequinStand.Common
{
    public abstract class ItemSlotMannequin : ItemSlot
    {
        public ItemSlotMannequin(InventoryMannequin inventory) : base(inventory)
        {
            MaxSlotStackSize = 1;
        }
    }


    public class ItemSlotMannequinHand : ItemSlotMannequin
    {
        public ItemSlotMannequinHand(InventoryMannequin inventory, bool isOffhandSlot = false) : base(inventory)
        {
            if (isOffhandSlot)
            {
                StorageType = EnumItemStorageFlags.Offhand;
                BackgroundIcon = "offhand";
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            return IsAcceptable(sourceSlot)
                   && base.CanTakeFrom(sourceSlot, priority);
        }

        public override bool CanHold(ItemSlot sourceSlot)
        {
            return IsAcceptable(sourceSlot)
                   && base.CanHold(sourceSlot);
        }

        private bool IsAcceptable(ItemSlot sourceSlot)
        {
            var collectible = sourceSlot?.Itemstack?.Collectible;
            return CanBePlacedOnToolRack(collectible)
                   || EmitsLight(collectible);
        }

        protected static bool CanBePlacedOnToolRack(CollectibleObject collectible)
        {
            return collectible?.Attributes?["toolrackTransform"]?.Exists ?? false;
        }

        protected static bool EmitsLight(CollectibleObject collectible)
        {
            if (collectible.StorageFlags == EnumItemStorageFlags.Outfit && !CanBePlacedOnToolRack(collectible) == true || collectible.StorageFlags == EnumItemStorageFlags.Backpack)
            {
                return false; // don't want anything that shouldn't be in the hand slot.
            }

            foreach (var value in collectible?.LightHsv)
            {
                if (value != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }


    public class ItemSlotMannequinBackpack : ItemSlotMannequin
    {
        public ItemSlotMannequinBackpack(InventoryMannequin inventory) : base(inventory)
        {
            BackgroundIcon = "basket";
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            return IsAcceptable(sourceSlot)
                   && base.CanTakeFrom(sourceSlot, priority);
        }

        public override bool CanHold(ItemSlot sourceSlot)
        {
            return IsAcceptable(sourceSlot)
                   && base.CanHold(sourceSlot);
        }

        private bool IsAcceptable(ItemSlot sourceSlot)
        {
            return CollectibleObject.IsEmptyBackPack(sourceSlot.Itemstack);
        }

        public override EnumItemStorageFlags StorageType => EnumItemStorageFlags.Backpack;
    }


    public class ItemSlotMannequinWearable : ItemSlotMannequin
    {
        protected static Dictionary<EnumCharacterDressType, string> iconByDressType = new Dictionary<EnumCharacterDressType, string> {
            { EnumCharacterDressType.Foot, "boots" },
            { EnumCharacterDressType.Hand, "gloves" },
            { EnumCharacterDressType.Shoulder, "cape" },
            { EnumCharacterDressType.Head, "hat" },
            { EnumCharacterDressType.LowerBody, "trousers" },
            { EnumCharacterDressType.UpperBody, "shirt" },
            { EnumCharacterDressType.UpperBodyOver, "pullover" },
            { EnumCharacterDressType.Neck, "necklace" },
            { EnumCharacterDressType.Arm, "bracers" },
            { EnumCharacterDressType.Waist, "belt" },
            { EnumCharacterDressType.Emblem, "medal" },
            { EnumCharacterDressType.Face, "mask" }
        };

        protected EnumCharacterDressType dressType;
        public bool IsArmorSlot { get; protected set; }
        public override EnumItemStorageFlags StorageType => EnumItemStorageFlags.Outfit;

        public ItemSlotMannequinWearable(InventoryMannequin inventory, EnumCharacterDressType dressType) : base(inventory)
        {
            this.dressType = dressType;
            IsArmorSlot = IsArmor(dressType);
            iconByDressType.TryGetValue(dressType, out BackgroundIcon);
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            return IsAcceptable(sourceSlot)
                   && base.CanTakeFrom(sourceSlot, priority);
        }

        public override bool CanHold(ItemSlot sourceSlot)
        {
            return IsAcceptable(sourceSlot)
                   && base.CanHold(sourceSlot);
        }

        private bool IsAcceptable(ItemSlot sourceSlot)
        {
            return ItemSlotCharacter.IsDressType(sourceSlot?.Itemstack, dressType);
        }

        public static bool IsArmor(EnumCharacterDressType dressType)
        {
            switch (dressType)
            {
                case EnumCharacterDressType.ArmorHead:
                case EnumCharacterDressType.ArmorBody:
                case EnumCharacterDressType.ArmorLegs:
                    return true;
            }
            return false;
        }
    }
}
