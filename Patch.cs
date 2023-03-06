using HarmonyLib;
using UnityEngine;
using static LoveLamp.Plugin;

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

        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.OnPlaced)), HarmonyPostfix]
        public static void WearNTearOnPlaced(WearNTear __instance)
        {
            InitLamp(__instance);
        }

        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Awake)), HarmonyPostfix]
        public static void WearNTearAwake(WearNTear __instance)
        {
            if(__instance.m_nview.m_ghost == false) InitLamp(__instance);
        }

        private static void InitLamp(WearNTear __instance)
        {
            if(__instance.m_piece && __instance.m_piece.m_name == "$piece_JF_LoveLamp")
            {
                LoveLamp loveLamp = __instance.GetComponent<LoveLamp>();
                if(!LoveLamp.all.Contains(loveLamp)) LoveLamp.all.Add(loveLamp);
                if(!loveLamp) return;
                loveLamp.ConnectChest();

                maxFuel = maxFuelConfig.Value;
                secPerFuel = secPerFuelConfig.Value;
                loveLamp.radius = radius;
                loveLamp.m_areaMarker.m_radius = radius;
                loveLamp.m_maxFuel = maxFuel;
                loveLamp.m_secPerFuel = secPerFuel;
            }
        }

        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText)), HarmonyPrefix]
        public static bool FireplaceGetHoverText(Fireplace __instance, ref string __result)
        {
            if(__instance is LoveLamp loveLamp)
            {
                bool haveChest = loveLamp.container != null;
                __result += Localization.instance.Localize(__instance.m_name + " ( $piece_fire_fuel " + 
                    Mathf.Ceil(__instance.m_nview.GetZDO().GetFloat("fuel")).ToString() + "/" + ((int)__instance.m_maxFuel).ToString() + ")" 
                    + "\n[<color=yellow><b>1-8</b></color>] $piece_useitem " + __instance.m_fuelItem.m_itemData.m_shared.m_name); ;
                __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $LoveLamp_ShowHideArea");
                __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $LoveLamp_LinkChest");
                __result += "\n";
                if(haveChest)
                {
                    __result += Localization.instance.Localize("\n$LoveLamp_HaveChest");
                    // if(loveLamp.noFood) __result += "\n<color=red>Not enough food</color>";
                    // if(!loveLamp.noFood) __result += "\nThere's enough food";s
                }
                if(!haveChest) __result += Localization.instance.Localize("\n<color=red>$LoveLamp_NoHaveChest</color>");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText)), HarmonyPostfix]
        public static void TameableGetHoverText(Tameable __instance, ref string __result)
        {
            if(Game.instance.IsShuttingDown()) return;
            if(__instance.m_character.m_nview.GetZDO().GetBool("Boosted", false))
            {
                __result += "\n";
                __result += Localization.instance.Localize("\n$LoveLamp_Boosts:");
                __result += Localization.instance.Localize($"\n$LoveLamp_MaxCreatures <color=orange>X{maxCreatures}</color>");
                __result += Localization.instance.Localize($"\n$LoveLamp_RequiredLovePoints <color=orange>-X{requiredLovePoints}</color>");
                __result += Localization.instance.Localize($"\n$LoveLamp_PregnancyChance <color=orange>100%</color>");
                __result += Localization.instance.Localize($"\n$LoveLamp_TamingTime <color=orange>-X{tamingTime}</color>");
                __result += Localization.instance.Localize($"\n$LoveLamp_LevelUpFactor <color=orange>X{levelUpFactor}</color>");
                __result += Localization.instance.Localize($"\n$LoveLamp_Health <color=orange>X{health}</color>");
                __result += Localization.instance.Localize($"\n$LoveLamp_Speed <color=orange>X{speed}</color>");
                __result += Localization.instance.Localize($"\n$LoveLamp_JumpForce <color=orange>X{jumpForce}</color>");
                __result += Localization.instance.Localize($"\n$LoveLamp_Level <color=orange>+{boostLevel}</color>");
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
            if(!__instance.IsDead() && __instance.IsTamed() && !Game.IsPaused()) LoveLamp.CheckBoost(__instance);
        }

        [HarmonyPatch(typeof(Character), nameof(Character.GetHoverName)), HarmonyPostfix]
        public static void CharacterGetHoverName(Character __instance, ref string __result)
        {
            if(__instance.m_nview.GetZDO().GetBool("Boosted", false)) __result += $" <color=yellow>{namePostfix}</color>";
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Awake)), HarmonyPostfix]
        public static void CharacterAwake(Character __instance)
        {
            if(__instance.m_tameableMonsterAI == null) __instance.m_tameableMonsterAI = __instance.GetComponent<MonsterAI>();
            if(__instance.m_tameable == null) __instance.m_tameable = __instance.GetComponent<Tameable>();
        }
    }
}