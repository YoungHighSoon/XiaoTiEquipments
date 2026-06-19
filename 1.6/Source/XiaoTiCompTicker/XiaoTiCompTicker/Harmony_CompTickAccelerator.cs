using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace XiaoTiCompTicker
{
    [StaticConstructorOnStartup]
    public static class Harmony_CompTickAccelerator
    {
        [ThreadStatic]
        private static int _recursionGuard;

        private static MethodInfo _doTickMethod;

        static Harmony_CompTickAccelerator()
        {
            _doTickMethod = AccessTools.Method(typeof(Thing), nameof(Thing.DoTick));

            var harmony = new Harmony("XiaoTiCompTicker.CompTickAccelerator");
            harmony.Patch(_doTickMethod,
                prefix: new HarmonyMethod(typeof(Harmony_CompTickAccelerator), nameof(Prefix_DoTick)));

            Log.Message("[XiaoTiCompTicker] Harmony patch applied (Thing.DoTick)");
        }

        static bool Prefix_DoTick(Thing __instance)
        {
            if (_recursionGuard > 0) return true;
            if (__instance == null || !__instance.Spawned) return true;

            var map = __instance.Map;
            if (map == null) return true;

            var manager = map.GetComponent<MapComponent_AccelerationManager>();
            if (manager == null) return true;

            float factor;
            if (__instance is Pawn)
            {
                // Pawns use independent factor, only boosted when within range of an accelerator with Pawn Boost enabled
                factor = manager.GetPawnFactor(__instance);
                if (factor <= 1f) return true;
            }
            else
            {
                factor = manager.GetFactor(__instance);
            }

            int totalCalls = Mathf.Clamp(Mathf.FloorToInt(factor), 1, 16);
            if (totalCalls <= 1) return true;

            _recursionGuard++;
            try
            {
                for (int i = 1; i < totalCalls; i++)
                {
                    if (__instance.Destroyed) break;
                    _doTickMethod.Invoke(__instance, null);
                }
                return true;
            }
            finally
            {
                _recursionGuard--;
            }
        }
    }
}
