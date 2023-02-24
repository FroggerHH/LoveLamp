using HarmonyLib;
using UnityEngine;

namespace LoveLamp
{
    [HarmonyPatch]
    internal class Patch
    {
        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Interact)), HarmonyPrefix]
        public static bool FireplaceInteract(Humanoid user, bool hold, bool alt, Fireplace __instance, ref bool __result)
        {
            if(__instance is LoveLamp loveLamp)
            {
                __result = loveLamp.OnInteract(user, hold, alt);
                return false;
            }
            else return true;
        }
        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Awake)), HarmonyPrefix]
        public static void FireplaceAwake(Fireplace __instance)
        {
            if(__instance is LoveLamp loveLamp)
            {
                LoveLamp.all.Add(loveLamp);
            }
        }

        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText)), HarmonyPrefix]
        public static bool FireplaceGetHoverText(Fireplace __instance, ref string __result)
        {
            if(__instance is LoveLamp)
            {
                __result += Localization.instance.Localize(__instance.m_name + " ( $piece_fire_fuel " + Mathf.Ceil(__instance.m_nview.GetZDO().GetFloat("fuel")).ToString() + "/" + ((int)__instance.m_maxFuel).ToString() + ")" + "\n[<color=yellow><b>1-8</b></color>] $piece_useitem " + __instance.m_fuelItem.m_itemData.m_shared.m_name); ;
                __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] To show/hide area");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText)), HarmonyPostfix]
        public static void TameableGetHoverText(Tameable __instance, ref string __result)
        {
            if(__instance.m_character.m_nview.GetZDO().GetBool("Boosted", false))
            {
                __result += Localization.instance.Localize("\n");
                __result += Localization.instance.Localize("\nBoosts:");
                __result += Localization.instance.Localize("\nMax Creatures <color=orange>X2</color>");
                __result += Localization.instance.Localize("\nRequired Love Points <color=orange>-X2</color>");
                __result += Localization.instance.Localize("\nPregnancy Chance <color=orange>100%</color>");
                __result += Localization.instance.Localize("\nTaming Time <color=orange>-X2</color>");
                __result += Localization.instance.Localize("\nLevelUp Factor <color=orange>X2</color>");
                __result += Localization.instance.Localize("\nHealth <color=orange>X2</color>");
                __result += Localization.instance.Localize("\nWalk Speed <color=orange>X2</color>");
                __result += Localization.instance.Localize("\nRun Speed <color=orange>X2</color>");
                __result += Localization.instance.Localize("\nSwim Speed <color=orange>X2</color>");
                __result += Localization.instance.Localize("\nTurn Speed <color=orange>X2</color>");
                __result += Localization.instance.Localize("\nJumpForce <color=orange>X2</color>");
                __result += Localization.instance.Localize("\nlevel <color=orange>+1</color>");
            }
        }

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake)), HarmonyPostfix]
        public static void ZNetSceneAwake(ZNetScene __instance)
        {
            Fireplace loveLamp = __instance.GetPrefab("JF_LoveLamp").GetComponent<Fireplace>();
            ItemDrop honey = __instance.GetPrefab("Honey").GetComponent<ItemDrop>();

            loveLamp.m_fuelItem = honey;
        }

        [HarmonyPatch(typeof(Character), nameof(Character.FixedUpdate)), HarmonyPostfix]
        public static void CharacterFixedUpdate(Character __instance)
        {
            if(!__instance.IsDead()) LoveLamp.CheckBoost(__instance);
        }

        [HarmonyPatch(typeof(Character), nameof(Character.GetHoverName)), HarmonyPostfix]
        public static void CharacterGetHoverName(Character __instance, ref string __result)
        {
            if(__instance.m_nview.GetZDO().GetBool("Boosted", false)) __result += "<color=yellow>Boosted</color>";
        }
    }
}
