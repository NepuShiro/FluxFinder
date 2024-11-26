using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using ResoniteModLoader;
using System.ComponentModel;

namespace FluxFinder
{
	public class FluxFinder : ResoniteMod
	{
		public override string Name => "FluxFinder";
		public override string Author => "NepuShiro";
		public override string Version => "1.0.0";
		public override string Link => "https://github.com/NepuShiro/FluxFinder/";

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("enabled", "Should FluxFinder be Enabled?", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<string> EQUIPEDDV = new ModConfigurationKey<string>("Equiped DV", "The DynamicVariable<bool> to set if the Tip is Equipped", () => "World/nepu.istipequiped");
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<string> COLORDV = new ModConfigurationKey<string>("Color DV", "The DynamicVariable<colorX> for the Hovered Color", () => "World/nepu.allhovercolor");

		private static ModConfiguration config;
		private static string colorDV;
		private static string equipedDV;

		public override void OnEngineInit()
		{
			config = GetConfiguration();
			config.Save(true);

			Harmony harmony = new("net.NepuShiro.FluxFinder");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(ProtoFluxTool))]
		private class ProtoFluxToolPatch
		{
			static Slot fluxFinderSlot;

			[HarmonyPrefix, HarmonyPatch("OnEquipped")]
			static void OnEquippedPatch(ProtoFluxTool __instance)
			{
				if (!config.GetValue(ENABLED)) return;

				var userSlot = __instance.LocalUser.Root.Slot;
				var funnySlot = userSlot.FindChildOrAdd("Injected - FluxFinder", false);
				
				colorDV = config.GetValue(COLORDV);
				equipedDV = config.GetValue(EQUIPEDDV);
				
				if (funnySlot != null)
				{
					fluxFinderSlot = funnySlot;

					var funnyvar = funnySlot.GetComponentsInChildren<DynamicValueVariable<colorX>>();
					if (funnyvar.Count == 0)
					{
						var fluxvar1 = funnySlot.AttachComponent<DynamicValueVariable<colorX>>();
						fluxvar1.Persistent = false;
						fluxvar1.VariableName.Value = colorDV;
						var equippedVar2 = funnySlot.AttachComponent<DynamicValueVariable<bool>>();
						equippedVar2.Persistent = false;
						equippedVar2.VariableName.Value = equipedDV;
						equippedVar2.Value.Value = true;
					}

					funnySlot.WriteDynamicVariable(equipedDV, true);
					return;
				};
			}

			[HarmonyPrefix, HarmonyPatch("OnDequipped")]
			static void OnDequippedPatch(ProtoFluxTool __instance)
			{
				if (fluxFinderSlot == null || !config.GetValue(ENABLED)) return;

				colorDV = config.GetValue(COLORDV);
				equipedDV = config.GetValue(EQUIPEDDV);
				
				fluxFinderSlot.WriteDynamicVariable(equipedDV, false);
				fluxFinderSlot.WriteDynamicVariable(colorDV, new colorX(0f, 0f, 0f, 0f));
			}

			[HarmonyPrefix, HarmonyPatch("Update")]
			static void UpdatePatch(ProtoFluxTool __instance)
			{
				if (fluxFinderSlot == null || !config.GetValue(ENABLED)) return;

				fluxFinderSlot.WriteDynamicVariable(colorDV, __instance.HoveringElementColor.Value);
			}
		}
	}
}
