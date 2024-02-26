
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace Mannequins.Client
{
    public class GuiDialogEditableNameTag : GuiDialogReadonlyBook
    {
        public bool DidSave;

        public bool DidSign;

        public string Name;

        public string GetTitle()
        {
            return Name;
        }

        public GuiDialogEditableNameTag(ItemStack nametagStack, ICoreClientAPI capi)
            : base(nametagStack, capi)
        {
        }

        protected override void Compose()
        {
            FontExtents fontExtents = font.GetFontExtents();
            double num = fontExtents.Height * font.LineHeightMultiplier / RuntimeEnv.GUIScale;
            ElementBounds elementBounds = ElementBounds.Fixed(0.0, 30.0, 400, 24.0);
            ElementBounds elementBounds2 = ElementBounds.Fixed(0.0, 0.0, 100, -50).FixedUnder(elementBounds, 5.0);
            ElementBounds elementBounds3 = ElementBounds.FixedSize(60.0, 30.0).FixedUnder(elementBounds2, 5.0).WithAlignment(EnumDialogArea.LeftFixed)
                .WithFixedPadding(10.0, 2.0);
            ElementBounds bounds = ElementBounds.FixedSize(80.0, 30.0).FixedUnder(elementBounds2, 17.0).WithAlignment(EnumDialogArea.CenterFixed)
                .WithFixedPadding(10.0, 2.0);
            ElementBounds elementBounds4 = ElementBounds.FixedSize(60.0, 30.0).FixedUnder(elementBounds2, 5.0).WithAlignment(EnumDialogArea.RightFixed)
                .WithFixedPadding(10.0, 2.0);
            ElementBounds elementBounds5 = ElementBounds.FixedSize(0.0, 0.0).FixedUnder(elementBounds3, 25.0).WithAlignment(EnumDialogArea.LeftFixed)
                .WithFixedPadding(10.0, 2.0);
            ElementBounds bounds2 = ElementBounds.FixedSize(0.0, 0.0).FixedUnder(elementBounds3, 25.0).WithAlignment(EnumDialogArea.CenterFixed)
                .WithFixedPadding(10.0, 2.0);
            ElementBounds elementBounds6 = ElementBounds.FixedSize(0.0, 0.0).FixedUnder(elementBounds4, 25.0).WithAlignment(EnumDialogArea.RightFixed)
                .WithFixedPadding(10.0, 2.0);
            ElementBounds elementBounds7 = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            elementBounds7.BothSizing = ElementSizing.FitToChildren;
            elementBounds7.WithChildren(elementBounds5, elementBounds6);
            ElementBounds bounds3 = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle).WithFixedAlignmentOffset(0.0 - GuiStyle.DialogToScreenPadding, 0.0);
            SingleComposer = capi.Gui.CreateCompo("blockentitytexteditordialog", bounds3).AddShadedDialogBG(elementBounds7).AddDialogTitleBar(Lang.Get("Edit paper"), OnTitleBarClose)
                .BeginChildElements(elementBounds7)
                .AddTextInput(elementBounds, null, CairoFont.TextInput().WithFontSize(18f), "name")
                .AddSmallButton(Lang.Get("Cancel"), OnButtonCancel, elementBounds5)
                .AddSmallButton(Lang.Get("Save"), OnButtonSave, elementBounds6)
                .EndChildElements()
                .Compose();

            SingleComposer.GetTextInput("name").SetPlaceHolderText(Lang.Get("Name Tag"));
            SingleComposer.GetTextInput("name").SetValue(GetTitle());
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
        }

        private void OnTitleBarClose()
        {
            OnButtonCancel();
        }

        private bool OnButtonSave()
        {
            Name = SingleComposer.GetTextInput("name").GetText();
            DidSave = true;
            TryClose();
            return true;
        }

        private bool OnButtonCancel()
        {
            DidSave = false;
            TryClose();
            return true;
        }
    }
}
