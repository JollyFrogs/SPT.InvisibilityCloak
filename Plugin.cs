using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using EFT;
using EFT.Console.Core;
using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT.UI;
using EFT.Communications;
using SPT.Reflection.Utils;
using System.Security.Policy;
using static BotMemoryClass;


namespace InvisibilityCloak
{ 
    [BepInPlugin("com.Invisibility.Cloak", "InvisibilityCloak", "3.10.3.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ConfigEntry<bool> InvisibilityCloak_enabled;

        private void Awake()
        {
            Logger.LogInfo($"InvisibilityCloak has loaded, use console commands 'invisible_on' or 'invisible_off'");

            ConsoleScreen.Processor.RegisterCommand("invisible_on", new Action(invisible_on),"");
            ConsoleScreen.Processor.RegisterCommand("invisible_off", new Action(invisible_off),"");

            InvisibilityCloak_enabled = Config.Bind(
                "",
                "Tick to enable Invisibility",
                true,
                "Tick to enable invisibility");

            new BotMemoryClass_AddEnemy_Patch().Enable();
            new BotsGroup_AddEnemy_Patch().Enable();
            new BotsGroup_CheckAndAddEnemy_Patch().Enable();
            new EnemyInfo_CheckVisibility_Patch().Enable();
        }

        private void invisible_on()
        {
            InvisibilityCloak_enabled.Value = true;
            Notifier.DisplayWarningNotification($"InvisibilityCloak is ON", ENotificationDurationType.Long);
        }

        private void invisible_off()
        {
            InvisibilityCloak_enabled.Value = false;
            Notifier.DisplayWarningNotification($"InvisibilityCloak is OFF", ENotificationDurationType.Long);
        }
    }

    static class Notifier
    {
        private static readonly MethodInfo notifierMessageMethod;
        private static readonly MethodInfo notifierWarningMessageMethod;

        static Notifier()
        {
            var notifierType = PatchConstants.EftTypes.Single(x => x.GetMethod("DisplayMessageNotification") != null);
            notifierMessageMethod = notifierType.GetMethod("DisplayMessageNotification");
            notifierWarningMessageMethod = notifierType.GetMethod("DisplayWarningNotification");
        }

        public static void DisplayMessageNotification(string message, ENotificationDurationType duration = ENotificationDurationType.Default, ENotificationIconType iconType = ENotificationIconType.Default, Color? textColor = null)
        {
            notifierMessageMethod.Invoke(null, new object[] { message, duration, iconType, textColor });
        }

        public static void DisplayWarningNotification(string message, ENotificationDurationType duration = ENotificationDurationType.Default)
        {
            notifierWarningMessageMethod.Invoke(null, new object[] { message, duration });
        }
    }

    internal class BotMemoryClass_AddEnemy_Patch : ModulePatch
    {
        [HarmonyPriority(int.MaxValue)] // Load patch with absolute highest priority to override all other patches
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotMemoryClass), nameof(BotMemoryClass.AddEnemy));
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            BotMemoryClass __instance,
            BotOwner ___botOwner_0,
            BotsGroup ___botsGroup_0,
            Action<IPlayer> ___action_0,
            IPlayer enemy,
            BotSettingsClass groupInfo,
            bool onActivation
        )
        {
            if (!Plugin.InvisibilityCloak_enabled.Value)
            {
                return true; // If mod is not enabled, then skip our function and execute original function instead of ours
            }

            BotMemoryClass.Class978 Class978 = new BotMemoryClass.Class978();
            Class978.botMemoryClass = __instance;
            Class978.enemy = enemy;

            if (Class978.enemy.IsYourPlayer) // Do not add enemy if it is our player ;)
            {
                return false; // Skip original
            }
            if (Class978.enemy.Id == ___botOwner_0.GetPlayer.Id)
            {
                return false; // Skip original
            }
            for (int i = 0; i < ___botOwner_0.BotsGroup.MembersCount; i++)
            {
                if (___botOwner_0.BotsGroup.Member(i).GetPlayer.Id == Class978.enemy.Id)
                {
                    return false; // Skip original
                }
            }
            if (___botOwner_0.EnemiesController.EnemyInfos.ContainsKey(Class978.enemy))
            {
                return false; // Skip original
            }
            if (Class978.enemy.Transform == null || !Class978.enemy.HealthController.IsAlive)
            {
                return false; // Skip original
            }
            global::EnemyInfo enemyInfo = ___botOwner_0.EnemiesController.AddNew(___botsGroup_0, Class978.enemy, groupInfo);
            ___botOwner_0.EnemiesController.SetInfo(Class978.enemy, enemyInfo);
            ___botOwner_0.BotRequestController.RemoveAllRequestByRequester(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(Class978.enemy.ProfileId));
            Class978.enemy.HealthController.DiedEvent += Class978.method_0;
            float sqrMagnitude = (___botOwner_0.Position - Class978.enemy.Position).sqrMagnitude;
            if (!onActivation && sqrMagnitude < 625f && !___botOwner_0.Memory.HaveEnemy && global::GClass344.CanShoot(___botOwner_0, enemyInfo))
            {
                enemyInfo.SetVisible(true);
                ___botOwner_0.Memory.GoalEnemy = enemyInfo;
            }
            global::System.Action<global::EFT.IPlayer> action = ___action_0;
            if (action == null)
            {
                return false; // Skip original
            }
            action(Class978.enemy);

            return false; // Skip original
        }
    }

    internal class BotsGroup_AddEnemy_Patch : ModulePatch
    {
        [HarmonyPriority(int.MaxValue)] // Load patch with absolute highest priority to override all other patches
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.AddEnemy));
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            BotsGroup __instance,
            ref bool __result,
            IPlayer person, 
            EBotEnemyCause cause
        )
        {
            if (!Plugin.InvisibilityCloak_enabled.Value)
            {
                return true; 
            }

            if (person.IsYourPlayer)
            {
                __result = false;
                return false; 
            }

            __result = true;
            return false;
        }


    }

    internal class EnemyInfo_CheckVisibility_Patch : ModulePatch
    {
        [HarmonyPriority(int.MaxValue)] // Load patch with absolute highest priority to override all other patches
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.CheckVisibility));
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            ref bool __result,
            global::System.Collections.Generic.KeyValuePair<global::EnemyPart, global::EnemyPartData> part,
            float seenCoef,
            bool onSense,
            bool onSenceGreen,
            float addVisibility
        )
        {
            if (!Plugin.InvisibilityCloak_enabled.Value)
            {
                return true; // If mod is not enabled, then skip our function and execute original function instead of ours
            }

            if (part.Key.Owner.IsYourPlayer)
            {
                __result = false;
                return false; // if part belongs to our player, do not execute original function and simply return 'false'
            }

            return true; // execute original function
        }
    }

    internal class BotsGroup_CheckAndAddEnemy_Patch : ModulePatch
    {
        [HarmonyPriority(int.MaxValue)] // Load patch with absolute highest priority to override all other patches
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.CheckAndAddEnemy));
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            ref BotsGroup __instance,
            ref bool __result,
            global::EFT.IPlayer player,
            bool ignoreAI = false
        )
        {
            if (!Plugin.InvisibilityCloak_enabled.Value)
            {
                return true; // If mod is not enabled, then skip our function and execute original function instead of ours
            }

            __result = !player.IsYourPlayer && player.HealthController.IsAlive && (!player.AIData.IsAI || ignoreAI) && !__instance.Enemies.ContainsKey(player) && __instance.AddEnemy(player, global::EBotEnemyCause.checkAddTODO);
            return false; // Skip original
        }
    }
}
