using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Mannequins.Util
{
    [ProtoContract]
    public class EditnametagPacket
    {
        [ProtoMember(1)]
        public bool DidSave;

        [ProtoMember(2)]
        public string Name;
    }

    [ProtoContract]
    public class NametagPacket
    {
        [ProtoMember(1)]
        public string Name;
    }

    public class ModSystemEditableNameTag : ModSystem
    {
        private Dictionary<string, ItemSlot> nowEditing = new Dictionary<string, ItemSlot>();

        private ICoreAPI api;
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public void BeginEdit(IPlayer player, ItemSlot slot)
        {
            nowEditing[player.PlayerUID] = slot;
        }
        public void EndEdit(IPlayer player, string nameTag)
        {
            if (nowEditing.TryGetValue(player.PlayerUID, out var value))
            {
                ItemStack itemStack = value.TakeOut(1);
                itemStack.Attributes.SetString("name", nameTag);
                value.MarkDirty();

                if (api is ICoreClientAPI coreClientAPI)
                {
                    coreClientAPI.Network.GetChannel("editablenametag").SendPacket(new EditnametagPacket
                    {
                        DidSave = true,
                        Name = nameTag
                    });
                }

                if (!player.InventoryManager.TryGiveItemstack(itemStack, slotNotifyEffect: true))
                {
                    api.World.SpawnItemEntity(itemStack, player.Entity.Pos.XYZ);
                }

                api.World.PlaySoundAt(new AssetLocation("game:sounds/effect/writing"), player.Entity);
            }

            nowEditing.Remove(player.PlayerUID);
        }

        public void CancelEdit(IPlayer player)
        {
            nowEditing.Remove(player.PlayerUID);
            if (api is ICoreClientAPI coreClientAPI)
            {
                coreClientAPI.Network.GetChannel("editablenametag").SendPacket(new EditnametagPacket
                {
                    DidSave = false
                });
            }
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.api = api;
            api.Network.RegisterChannel("editablenametag").RegisterMessageType<EditnametagPacket>().RegisterMessageType<NametagPacket>();
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Network.GetChannel("editablenametag").SetMessageHandler<EditnametagPacket>(onEditBookPacket);
        }

        private void onEditBookPacket(IServerPlayer fromPlayer, EditnametagPacket packet)
        {
            if (nowEditing.TryGetValue(fromPlayer.PlayerUID, out var _))
            {
                if (packet.DidSave)
                {
                    EndEdit(fromPlayer, packet.Name);
                }
                else
                {
                    CancelEdit(fromPlayer);
                }
            }
        }
    }
}
