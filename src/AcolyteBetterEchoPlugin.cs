using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace AcolyteBetterEcho
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class AcolyteBetterEchoPlugin : BaseUnityPlugin
    {
        public const string PluginAuthor = "EviLord";
        public const string PluginName = "AcolyteBetterEcho";
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        // Generated from <Version> in the csproj, the single source for the version
        public const string PluginVersion = BuildInfo.Version;

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<int> EchoSacrificeConvictions;

        private void Awake()
        {
            Log = Logger;

            Enabled = Config.Bind("General", "Enabled", true, "Enable the Echo changes. Requires a restart.");
            EchoSacrificeConvictions = Config.Bind("General", "EchoSacrificeConvictions", 3,
                new ConfigDescription("Convictions granted by a Sacrifice hit when Anima Echo is selected (vanilla: 2). Kills still grant 1 extra. " +
                    "Gameplay uses the new value immediately; skill descriptions update after a restart.",
                    new AcceptableValueRange<int>(1, 7)));

            if (Enabled.Value)
            {
                EchoChanges.Init();
                EchoDescriptions.Init();
            }

            Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }
    }
}
