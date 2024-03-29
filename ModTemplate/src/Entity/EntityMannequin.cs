﻿using System;
using System.IO;
using Mannequins.Client;
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
    public class EntityMannequin : EntityHumanoid
    {
        protected const int packetId_OpenInventory = 1000;
        protected const int packetId_CloseInventory = 1001;
        protected const string CurPoseKey = "curPose";
        protected const string InventoryTreeKey = "inventory";
        protected string MannequinMaterial = "oak";
        protected const string MannequinTreeKey = "mannequin";
        protected const string BaseSkin = "baseskin";

        private InventoryMannequin gearInv;

        private string[] poses = new string[5] { "idle", "lefthandup", "righthandup", "holdbothhandsup", "shieldup" };

        public override IInventory GearInventory => gearInv;

        protected virtual string inventoryId => "gear-" + EntityId;

        protected int CurPose
        {
            get
            {
                return WatchedAttributes.GetInt(CurPoseKey);
            }
            set
            {
                WatchedAttributes.SetInt(CurPoseKey, value % poses.Length);
            }
        }

        protected int PrevPose => (CurPose + poses.Length - 1) % poses.Length;

        public WorldInteraction[] interactions;

        public override byte[] LightHsv
        {
            get
            {
                var light = new byte[3] { 0, 0, 0 };
                for (int i = 0; i < gearInv.Count; i++)
                {
                    if (gearInv[i].Empty)
                    {
                        continue;
                    }

                    AdjustLightLevelForItemSlot(i, ref light);
                }


                return light;
            }
        }

        public string Name => WatchedAttributes.GetAsString("name");

        public ItemStack PlacedByItemStack { get; set; }

        protected TreeAttribute mannequinTreeKey => WatchedAttributes.GetOrAddTreeAttribute(MannequinTreeKey) as TreeAttribute;

        protected GuiInventoryDialog GuiInventoryDialog { get; set; }

        public override ItemSlot LeftHandItemSlot => gearInv.LeftHandItemSlot;

        public override ItemSlot RightHandItemSlot => gearInv.RightHandItemSlot;

        public ItemSlot BackpackItemSlot => gearInv.BackpackItemSlot;

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            if (gearInv == null)
            {
                InitializeInventory(api);
            }
            else
            {
                gearInv.LateInitialize(inventoryId, api);
            }

            if (api.Side == EnumAppSide.Client)
            {
                WatchedAttributes.RegisterModifiedListener(MannequinTreeKey, readMannequinPartsFromAttributes);
                WatchedAttributes.RegisterModifiedListener(InventoryTreeKey, readInventoryFromAttributes);
                WatchedAttributes.RegisterModifiedListener(CurPoseKey, RefreshPose);
            }

 
            readMannequinPartsFromAttributes();
            readInventoryFromAttributes();
            RefreshPose();
        }

        protected virtual void InitializeInventory(ICoreAPI api)
        {
            gearInv = new InventoryMannequin(inventoryId, api);
            gearInv.SlotModified += GearInv_SlotModified;
        }

        protected virtual void readMannequinPartsFromAttributes()
        {
           GetBehavior<EntityBehaviorExtraSkinnable>()?.selectSkinPart(BaseSkin, mannequinTreeKey.GetString(BaseSkin, MannequinMaterial));
        }

        private void readInventoryFromAttributes()
        {
            ITreeAttribute treeAttribute = WatchedAttributes["inventory"] as ITreeAttribute;
            if (gearInv != null && treeAttribute != null)
            {
                gearInv.FromTreeAttributes(treeAttribute);
            }

            (base.Properties.Client.Renderer as EntitySkinnableShapeRenderer)?.MarkShapeModified();
           
        }

        private void GearInv_SlotModified(int slotid)
        {
            ITreeAttribute treeAttribute = new TreeAttribute();
            WatchedAttributes["inventory"] = treeAttribute;
            gearInv.ToTreeAttributes(treeAttribute);
            WatchedAttributes.MarkPathDirty("inventory");
        }

        public override void FromBytes(BinaryReader reader, bool forClient)
        {
            base.FromBytes(reader, forClient);

            if (WatchedAttributes[InventoryTreeKey] is ITreeAttribute inventoryTree)
            {
                if (gearInv == null)
                {
                    InitializeInventory(Api);
                }

                gearInv.FromTreeAttributes(inventoryTree);
            }
        }

        public override void OnReceivedClientPacket(IServerPlayer player, int packetid, byte[] data)
        {
            base.OnReceivedClientPacket(player, packetid, data);

            switch (packetid)
            {
                case packetId_OpenInventory:
                    player.InventoryManager.OpenInventory(GearInventory);
                    break;
                case packetId_CloseInventory:
                    player.InventoryManager.CloseInventory(GearInventory);
                    break;
            }
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            base.OnReceivedServerPacket(packetid, data);

            if (packetid == packetId_CloseInventory)
            {
                (World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(GearInventory);
                GuiInventoryDialog?.TryClose();
            }
        }

        public override void OnEntitySpawn()
        {
            base.OnEntitySpawn();

            if (PlacedByItemStack == null)
            {
                return;
            }

            var itemStackTree = PlacedByItemStack.Attributes.GetOrAddTreeAttribute(MannequinTreeKey);

            mannequinTreeKey.SetString(BaseSkin, itemStackTree.GetString(BaseSkin, PlacedByItemStack.Collectible.Code.EndVariant()));

            if(PlacedByItemStack.Attributes.HasAttribute("name")) 
            {
              WatchedAttributes.SetItemstack("entityNametag", PlacedByItemStack.Attributes.GetItemstack("entityNametag"));
              WatchedAttributes.SetString("name", PlacedByItemStack.Attributes.GetAsString("name"));
            }

            WatchedAttributes.MarkPathDirty(MannequinTreeKey);
        }

        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);

            GuiInventoryDialog?.TryClose();
        }

        public override void OnInteract(EntityAgent byEntity, ItemSlot slot, Vec3d hitPosition, EnumInteractMode mode)
        {
            IPlayer player = (byEntity as EntityPlayer)?.Player;
            if (player != null && !byEntity.World.Claims.TryAccess(player, Pos.AsBlockPos, EnumBlockAccessFlags.Use))
            {
                player.InventoryManager.ActiveHotbarSlot.MarkDirty();
                WatchedAttributes.MarkAllDirty();
                return;
            }

            if (mode == EnumInteractMode.Interact)
            {
                if (byEntity.Controls.Sneak)
                {
                    OnInteractWhileSneaking(byEntity, slot, hitPosition, mode);
                }
                else
                {
                    OnInteractWhileStanding(byEntity, slot, hitPosition, mode);
                }
            }

            if (Alive && World.Side != EnumAppSide.Client && mode != 0)
            {
                base.OnInteract(byEntity, slot, hitPosition, mode);
            }
        }

        protected virtual void OnInteractWhileSneaking(EntityAgent byEntity, ItemSlot withSlot, Vec3d hitPosition, EnumInteractMode mode)
        {
            if (withSlot?.Empty ?? true)
            {
                OnInteractWhileSneakingWithoutItem(byEntity, withSlot, hitPosition, mode);
            }
            else
            {
                OnInteractWhileSneakingWithItem(byEntity, withSlot, hitPosition, mode);
            }
        }

        protected virtual void OnInteractWhileStanding(EntityAgent byEntity, ItemSlot withSlot, Vec3d hitPosition, EnumInteractMode mode)
        {
            if (withSlot?.Empty ?? true)
            {
                OnInteractWhileStandingWithoutItem(byEntity, withSlot, hitPosition, mode);
            }
            else
            {
                OnInteractWhileStandingWithItem(byEntity, withSlot, hitPosition, mode);
            }
        }

        protected virtual void OnInteractWhileSneakingWithoutItem(EntityAgent byEntity, ItemSlot withSlot, Vec3d hitPosition, EnumInteractMode mode)
        {
            if (!TryTakeItemFromEntity(byEntity, withSlot))
            {
                TryPickUpEntityAsItem(byEntity);
            }
        }

        protected virtual void OnInteractWhileSneakingWithItem(EntityAgent byEntity, ItemSlot withSlot, Vec3d hitPosition, EnumInteractMode mode)
        {
            TryGiveItemToEntity(byEntity, withSlot);
        }

        protected virtual void OnInteractWhileStandingWithoutItem(EntityAgent byEntity, ItemSlot withSlot, Vec3d hitPosition, EnumInteractMode mode)
        {
            if (byEntity is EntityPlayer entityPlayer)
            {
                ToggleInventoryDialog(entityPlayer.Player);
            }
        }

        protected virtual void OnInteractWhileStandingWithItem(EntityAgent byEntity, ItemSlot withSlot, Vec3d hitPosition, EnumInteractMode mode)
        {
            if (IsWrench(withSlot))
            {
                ChangeToNextPose();
            }

            if (IsNameTagItem(withSlot) && !EntityHasAttribute())
            {
                HandleNameTagInteraction(withSlot);
            }

            if(HasTool(withSlot))
            {
                HandleRemoveNameTagInteraction(byEntity as EntityPlayer);
            }
        }

        private bool HasTool(ItemSlot slot) {
            return slot.Itemstack?.Collectible?.Tool is EnumTool.Knife or EnumTool.Shears;
        }

        private bool IsWrench(ItemSlot slot)
        {
            return slot?.Itemstack?.Collectible is ItemWrench;
        }

        private bool IsNameTagItem(ItemSlot slot)
        {
            return slot.Itemstack.Attributes.HasAttribute("name");
        }

        private void HandleRemoveNameTagInteraction(EntityPlayer entityPlayer) 
        {
            SyncedTreeAttribute entityattributes = WatchedAttributes;

            if (EntityHasAttribute())
            {
                ItemStack itemStack = WatchedAttributes.GetItemstack("entityNametag");
                itemStack.Attributes.RemoveAttribute("name");
                itemStack.StackSize = 1;
                itemStack.ResolveBlockOrItem(Api.World);

                if (!entityPlayer.Player.InventoryManager.TryGiveItemstack(itemStack))
                {
                    Api.World.SpawnItemEntity(itemStack, entityPlayer.Pos.AsBlockPos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                entityattributes.RemoveAttribute("name");
                entityattributes.RemoveAttribute("entityNametag");
            }

        }

        private void HandleNameTagInteraction(ItemSlot slot)
        {
            WatchedAttributes.SetItemstack("entityNametag", slot.Itemstack);
            WatchedAttributes.SetString("name", slot.Itemstack.Attributes.GetAsString("name"));
            slot.TakeOut(1);
            slot.MarkDirty();
        }


        private bool EntityHasAttribute() {
            if (WatchedAttributes.HasAttribute("name") && WatchedAttributes.HasAttribute("entityNametag"))
            {
                return true;
            }

            return false;
        }

        protected virtual bool TryTakeItemFromEntity(EntityAgent byEntity, ItemSlot intoSlot)
        {
            for (int i = 0; i < GearInventory.Count; i++)
            {
                if (GearInventory[i].Empty)
                {
                    continue;
                }

                if (GearInventory[i].TryPutInto(World, intoSlot) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool TryPickUpEntityAsItem(EntityAgent byEntity)
        {
            if (!GearInventory.Empty)
            {
                return false;
            }

            AssetLocation entityAsItemCode = GetAssetLocation("mannequins", $"mannequinstand-{mannequinTreeKey.GetAsString("baseskin", "oak")}")
               ?? GetAssetLocation("mannequins", "woondenmannequinstand");

            Item entityAsItem = World.GetItem(entityAsItemCode);

            if (entityAsItem == null)
            {
                World.Logger.Error("Could not pick up entity ({0}, id: {1}). No such Item: {2}", GetType(), EntityId, entityAsItemCode);
                return false;
            }

            ItemStack itemStack = new ItemStack(entityAsItem);
            itemStack.Attributes[MannequinTreeKey] = mannequinTreeKey.Clone();

            if (EntityHasAttribute())
            {
                itemStack.Attributes.SetItemstack("entityNametag", WatchedAttributes.GetItemstack("entityNametag"));
                itemStack.Attributes.SetString("name", WatchedAttributes.GetAsString("name"));
            }

            itemStack.Collectible.Code.EndVariant().Replace(itemStack.Collectible.Code.EndVariant(), MannequinMaterial);

            if (!byEntity.TryGiveItemStack(itemStack))
            {
                World.SpawnItemEntity(itemStack, ServerPos.XYZ);
            }

            Die(EnumDespawnReason.PickedUp);
            return true;
        }

        private AssetLocation GetAssetLocation(string domain, string path)
        {
            return new AssetLocation(domain, path);
        }

        protected virtual bool TryGiveItemToEntity(EntityAgent byEntity, ItemSlot fromSlot)
        {
            WeightedSlot weightedSlot = GearInventory.GetBestSuitedSlot(fromSlot);
            ItemSlot sinkSlot = weightedSlot.slot;

            if (sinkSlot == null || weightedSlot.weight <= 0f)
            {
                return false;
            }

            return fromSlot.TryPutInto(World, weightedSlot.slot) > 0;
        }

        protected virtual void ToggleInventoryDialog(IPlayer player)
        {
            TryOpenInventory(player);
        }

        protected virtual bool TryOpenInventory(IPlayer player)
        {
            if (Api.Side != EnumAppSide.Client)
            {
                return false;
            }

            var capi = (ICoreClientAPI)Api;
            if (GuiInventoryDialog == null)
            {
                GuiInventoryDialog = new GuiInventoryDialog(gearInv, this, capi);
                GuiInventoryDialog.OnClosed += OnGuiInventoryDialogClosed;
            }

            if (!GuiInventoryDialog.TryOpen())
            {
                return false;
            }

            player.InventoryManager.OpenInventory(GearInventory);
            capi.Network.SendEntityPacket(EntityId, packetId_OpenInventory);
            return true;
        }

        public virtual void ChangeToNextPose()
        {
            CurPose++;
        }

        protected virtual void RefreshPose()
        {
            AnimManager.StopAnimation(poses[PrevPose]);
            AnimManager.StartAnimation(new AnimationMetaData
            {
                Animation = poses[CurPose],
                Code = poses[CurPose]
            }.Init());
            return;
        }

        protected virtual void OnGuiInventoryDialogClosed()
        {
            var capi = (ICoreClientAPI)Api;
            capi.World.Player.InventoryManager.CloseInventory(GearInventory);
            capi.Network.SendEntityPacket(EntityId, packetId_CloseInventory);
            GuiInventoryDialog?.Dispose();
            GuiInventoryDialog = null;
        }

        public override bool ReceiveDamage(DamageSource damageSource, float damage)
        {
            return base.ReceiveDamage(damageSource, damage);
        }

        public override void Die(EnumDespawnReason reason = EnumDespawnReason.Death, DamageSource damageSourceForDeath = null)
        {
            gearInv?.DropAll(insidePos.ToVec3d());
            base.Die(reason, damageSourceForDeath);
        }

        #region Lights
        private void AdjustLightLevelForItemSlot(int slotNumber, ref byte[] __result)
        {
            byte[] clipon = gearInv?[slotNumber].Itemstack.Collectible.LightHsv;
            if (clipon == null) return;

            if (__result == null)
            {
                __result = clipon;
                return;
            }

            float totalval = __result[2] + clipon[2];
            float t = clipon[2] / totalval;

            __result = new byte[]
            {
                    (byte)(clipon[0] * t + __result[0] * (1-t)),
                    (byte)(clipon[1] * t + __result[1] * (1-t)),
                    Math.Max(clipon[2], __result[2])
            };
        }
        #endregion

        public override string GetName()
        {
            ICoreClientAPI capi = Api as ICoreClientAPI;

            bool value = false;

            if (capi != null) { 

                ISettingsClass<string> en = capi.Settings.String;
                value = en.Get("language") == "en";
            }

            if (Code == GetAssetLocation("mannequins", "woondenmannequinstand"))
            {
                return Lang.Get("item-creature-mannequinstand-error");
            }

            if (WatchedAttributes.HasAttribute("name"))
            {
                return Name;
            }
       
            if (mannequinTreeKey["baseskin"].GetValue().Equals("baldcypress") && value)
            {
                return Lang.GetMatching(Code.Domain + ":item-creature-mannequinstand" + ": {0}", "Bald Cypress");
            }

            
            return Lang.GetMatching(Code.Domain + ":item-creature-mannequinstand" + ": {0}", Lang.GetMatching("game:material-" + mannequinTreeKey.GetString(BaseSkin)));
        }

        public override WorldInteraction[] GetInteractionHelp(IClientWorldAccessor world, EntitySelection es, IClientPlayer player)
        {
            var gearEmpty = gearInv.Empty ? interactions : null;

            interactions = ObjectCacheUtil.GetOrCreate(world.Api, "mannequinInteractions", () =>
            {
                return new WorldInteraction[1]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "game:blockhelp-behavior-rightclickpickup",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "shift",
                        RequireFreeHand = true,

                    }

                };
            });

            return gearEmpty;
        }
    }
}
