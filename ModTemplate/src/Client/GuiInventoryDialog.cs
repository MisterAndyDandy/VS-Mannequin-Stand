﻿using System;
using Mannequins.Entities;
using MannequinStand.Common;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Mannequins.Client
{
    public class GuiInventoryDialog : GuiDialog
    {
        protected InventoryMannequin inv;

        protected EntityMannequin owningEntity;

        protected Vec3d entityPos = new Vec3d();

        protected double FloatyDialogPosition => 0.6;

        protected double FloatyDialogAlign => 0.8;

        protected bool IsInRangeOfEntity => capi.World.Player.Entity.Pos.XYZ.Add(capi.World.Player.Entity.LocalEyePos).SquareDistanceTo(entityPos) <= Math.Pow(capi.World.Player.WorldData.PickingRange, 2);

        public override double DrawOrder => 0.2;

        public override bool UnregisterOnClose => true;

        public override bool PrefersUngrabbedMouse => false;

        public override bool DisableMouseGrab => false;

        public override string ToggleKeyCombinationCode => null;

        public GuiInventoryDialog(InventoryMannequin inv, EntityMannequin entityMannequin, ICoreClientAPI capi) : base(capi)
        {
            this.inv = inv;
            this.owningEntity = entityMannequin;

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
        
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.LeftMiddle).WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, 0.0);

            double pad = GuiElementItemSlotGridBase.unscaledSlotPadding;
            ElementBounds leftSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + pad, 1, 6).FixedGrow(0.0, pad);
            ElementBounds leftArmorSlotBoundsHead = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + pad, 1, 1).FixedGrow(0.0, pad);
            ElementBounds leftArmorSlotBoundsBody = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + pad + 102.0, 1, 1).FixedGrow(0.0, pad);
            ElementBounds leftArmorSlotBoundsLegs = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + pad + 204.0, 1, 1).FixedGrow(0.0, pad);
          
            leftSlotBounds.FixedRightOf(leftArmorSlotBoundsHead, 10.0).FixedRightOf(leftArmorSlotBoundsBody, 10.0).FixedRightOf(leftArmorSlotBoundsLegs, 10.0);
            
            ElementBounds rightSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + pad, 1, 6).FixedGrow(0.0, pad);
            rightSlotBounds.FixedRightOf(leftSlotBounds, 10.0);
            leftSlotBounds.fixedHeight -= 6.0;
            rightSlotBounds.fixedHeight -= 6.0;

            ElementBounds rightOtherSlotBoundsBackpack = leftArmorSlotBoundsHead.FlatCopy();
            ElementBounds rightOtherSlotBoundsLeftHand = leftArmorSlotBoundsBody.FlatCopy();
            ElementBounds rightOtherSlotBoundsRightHand = leftArmorSlotBoundsLegs.FlatCopy();
            rightOtherSlotBoundsBackpack.FixedRightOf(rightSlotBounds, 10.0);
            rightOtherSlotBoundsLeftHand.FixedRightOf(rightSlotBounds, 10.0);
            rightOtherSlotBoundsRightHand.FixedRightOf(rightSlotBounds, 10.0);

            SingleComposer = capi.Gui.CreateCompo("mannequincontents" + owningEntity.EntityId, dialogBounds).AddShadedDialogBG(bgBounds).AddDialogTitleBar(GetDialogName(entityMannequin), onClose: OnTitleBarClose);
            SingleComposer.BeginChildElements(bgBounds);

            SingleComposer
                .AddItemSlotGrid(inv, SendInvPacket, 1, new int[1] { InventoryMannequin.HeadArmorSlotId }, leftArmorSlotBoundsHead, "armorSlotsHead")
                .AddItemSlotGrid(inv, SendInvPacket, 1, new int[1] { InventoryMannequin.BodyArmorSlotId }, leftArmorSlotBoundsBody, "armorSlotsBody")
                .AddItemSlotGrid(inv, SendInvPacket, 1, new int[1] { InventoryMannequin.LegsArmorSlotId }, leftArmorSlotBoundsLegs, "armorSlotsLegs");

            SingleComposer
              .AddItemSlotGrid(inv, SendInvPacket, 1, InventoryMannequin.ClothingSlotIds, leftSlotBounds, "leftSlots")
              .AddItemSlotGrid(inv, SendInvPacket, 1, InventoryMannequin.AccessorySlotIds, rightSlotBounds, "rightSlots");

            SingleComposer.AddItemSlotGrid(inv, SendInvPacket, 1, new int[1] { InventoryMannequin.BackpackSlotId }, rightOtherSlotBoundsBackpack, "otherSlotsBackpack");

            SingleComposer
              .AddItemSlotGrid(inv, SendInvPacket, 1, new int[1] { InventoryMannequin.LeftHandSlotId }, rightOtherSlotBoundsLeftHand, "otherSlotsLeftHand")
              .AddItemSlotGrid(inv, SendInvPacket, 1, new int[1] { InventoryMannequin.RightHandSlotId }, rightOtherSlotBoundsRightHand, "otherSlotsRightHand")
              .EndChildElements();

            SingleComposer.Compose();
        }

        public string GetDialogName(EntityMannequin entityMannequin)
        {
            // Get the name of the entity mannequin
            string name = entityMannequin.GetName();

            // Ensure the returned name does not exceed the length of the original name
            return name.Length > 30 ? name.Substring(0, 30) : name;
        }

        public override void OnFinalizeFrame(float dt)
        {
            base.OnFinalizeFrame(dt);
            entityPos = owningEntity.Pos.XYZ.Clone();
            entityPos.Add(owningEntity.SelectionBox.X2 - owningEntity.OriginSelectionBox.X2, 0.0, owningEntity.SelectionBox.Z2 - owningEntity.OriginSelectionBox.Z2);
            if (!IsInRangeOfEntity)
            {
                capi.Event.EnqueueMainThreadTask(delegate
                {
                    TryClose();
                }, "closedmannequindlg");
            }
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();
            capi.Network.SendPacketClient(capi.World.Player.InventoryManager.CloseInventory(inv));
            SingleComposer.GetSlotGrid("armorSlotsHead")?.OnGuiClosed(capi);
            SingleComposer.GetSlotGrid("armorSlotsBody")?.OnGuiClosed(capi);
            SingleComposer.GetSlotGrid("armorSlotsLegs")?.OnGuiClosed(capi);
            SingleComposer.GetSlotGrid("leftSlots")?.OnGuiClosed(capi);
            SingleComposer.GetSlotGrid("rightSlots")?.OnGuiClosed(capi);
        }

        public override void OnRenderGUI(float deltaTime)
        {
            if (capi.Settings.Bool["immersiveMouseMode"])
            {
                double offX = owningEntity.SelectionBox.X2 - owningEntity.OriginSelectionBox.X2;
                double offZ = owningEntity.SelectionBox.Z2 - owningEntity.OriginSelectionBox.Z2;
                Vec3d pos = MatrixToolsd.Project(new Vec3d(owningEntity.Pos.X + offX, owningEntity.Pos.Y + FloatyDialogPosition, owningEntity.Pos.Z + offZ), capi.Render.PerspectiveProjectionMat, capi.Render.PerspectiveViewMat, capi.Render.FrameWidth, capi.Render.FrameHeight);
                if (pos.Z < 0.0)
                {
                    return;
                }
                SingleComposer.Bounds.Alignment = EnumDialogArea.None;
                SingleComposer.Bounds.fixedOffsetX = 0.0;
                SingleComposer.Bounds.fixedOffsetY = 0.0;
                SingleComposer.Bounds.absFixedX = pos.X - SingleComposer.Bounds.OuterWidth / 2.0;
                SingleComposer.Bounds.absFixedY = (double)capi.Render.FrameHeight - pos.Y - SingleComposer.Bounds.OuterHeight * FloatyDialogAlign;
                SingleComposer.Bounds.absMarginX = 0.0;
                SingleComposer.Bounds.absMarginY = 0.0;
            }
            base.OnRenderGUI(deltaTime);
        }

        protected void OnTitleBarClose()
        {
            TryClose();
        }

        protected void SendInvPacket(object packet)
        {
            capi.Network.SendPacketClient(packet);
        }
    }
}
