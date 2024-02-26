using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Mannequins.Util
{
    public class Patches
    {
        [HarmonyPatch(typeof(Entity))]
        [HarmonyPatch("GetName")]
        public class Harmony_Entity_GetName_Patched
        {

            [HarmonyPostfix]
            static void Harmony_Entity_GetName_Postfix(Entity __instance, ref string __result)
            {
                if (__instance == null)
                        return;

                if (__instance is EntityPlayer)
                {
                    return;
                }

                SyncedTreeAttribute entityAttributes = __instance.WatchedAttributes;

                if (entityAttributes?.HasAttribute("name") ?? false)
                {
                    if (!__instance.Alive) {
                        __result = entityAttributes.GetAsString("name") + " (Dead)";
                    }
                    __result = entityAttributes.GetAsString("name");
                }

                return;
            }
        }

        [HarmonyPatch(typeof(Entity))]
        [HarmonyPatch("OnInteract")]
        public class Entity_OnInteract_Patched
        {

            [HarmonyPostfix]
            static void Harmony_Entity_OnInteract_Postfix(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
            {
                if (byEntity == null)
                    return;

                if (itemslot == null || itemslot.Empty)
                {
                    return;
                }

                SyncedTreeAttribute entityAttributes = __instance.WatchedAttributes;

                if (itemslot.Itemstack.Attributes?.HasAttribute("nametag") ?? false)
                {
                    if (!__instance.Alive)
                    {
                        __result = entityAttributes.GetAsString("name") + " (Dead)";
                    }
                    __result = entityAttributes.GetAsString("name");
                }

                return;
            }
        }
    }
}
