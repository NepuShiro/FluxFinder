using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using ResoniteModLoader;

namespace FluxFinder
{
    public class FluxFinder : ResoniteMod
    {
        public override string Name => "FluxFinder";
        public override string Author => "NepuShiro";
        public override string Version => "1.0.1";
        public override string Link => "https://github.com/NepuShiro/FluxFinder/";

        [AutoRegisterConfigKey] private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("enabled", "Should FluxFinder be Enabled?", () => true);
        [AutoRegisterConfigKey] private static ModConfigurationKey<string> EQUIPEDDV = new ModConfigurationKey<string>("Equiped DV", "The DynamicVariable<bool> to set if the Tip is Equipped", () => "World/nepu.istipequiped");
        [AutoRegisterConfigKey] private static ModConfigurationKey<string> COLORDV = new ModConfigurationKey<string>("Color DV", "The DynamicVariable<colorX> for the Hovered Color", () => "World/nepu.allhovercolor");

        private static ModConfiguration _config;
        private static string ColorDv => _config.GetValue(COLORDV);
        private static string EquipedDv => _config.GetValue(EQUIPEDDV);

        public override void OnEngineInit()
        {
            _config = GetConfiguration();
            _config!.Save(true);

            Harmony harmony = new Harmony("net.NepuShiro.FluxFinder");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(ProtoFluxTool))]
        private class ProtoFluxToolPatch
        {
            private static Slot _fluxFinderSlot;

            [HarmonyPrefix, HarmonyPatch("OnEquipped")]
            private static void OnEquippedPatch(ProtoFluxTool __instance)
            {
                if (!_config.GetValue(ENABLED) || !__instance.Slot.ActiveUser.IsLocalUser) return;

                _fluxFinderSlot = __instance.LocalUser.Root.Slot.FindChildOrAdd("Injected - FluxFinder", false);
                if (_fluxFinderSlot == null) return;

                DynamicValueVariable<colorX> fluxVar1 = _fluxFinderSlot.GetComponentOrAttach<DynamicValueVariable<colorX>>();
                fluxVar1.Persistent = false;
                fluxVar1.VariableName.Value = ColorDv;
                fluxVar1.Value.Value = __instance.HoveringElementColor.Value;
                DynamicValueVariable<bool> equippedVar2 = _fluxFinderSlot.GetComponentOrAttach<DynamicValueVariable<bool>>();
                equippedVar2.Persistent = false;
                equippedVar2.VariableName.Value = EquipedDv;
                equippedVar2.Value.Value = true;
            }

            [HarmonyPrefix, HarmonyPatch("OnDequipped")]
            private static void OnDequippedPatch(ProtoFluxTool __instance)
            {
                if (!_config.GetValue(ENABLED) || !__instance.Slot.ActiveUser.IsLocalUser) return;

                _fluxFinderSlot?.WriteDynamicVariable(ColorDv, colorX.MinValue);
                _fluxFinderSlot?.WriteDynamicVariable(EquipedDv, false);
            }

            [HarmonyPrefix, HarmonyPatch("Update")]
            private static void UpdatePatch(ProtoFluxTool __instance)
            {
                if (!_config.GetValue(ENABLED) || !__instance.Slot.ActiveUser.IsLocalUser) return;

                _fluxFinderSlot?.WriteDynamicVariable(ColorDv, __instance.HoveringElementColor.Value);
            }
        }
    }
}