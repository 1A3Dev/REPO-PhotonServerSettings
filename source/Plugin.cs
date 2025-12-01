using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;

namespace PhotonServerSettings
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal class PluginLoader : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private static bool initialized;

        internal static ManualLogSource StaticLogger { get; private set; }
        internal static ConfigFile StaticConfig { get; private set; }

        internal static ConfigEntry<int> SteamAppId;
        internal static ConfigEntry<string> PhotonAppIdRealtime;
        internal static ConfigEntry<string> PhotonAppIdVoice;
        internal static ConfigEntry<string> PhotonServerAddress;
        internal static ConfigEntry<int> PhotonServerPort;

        private void Awake()
        {
            if (initialized) return;
            initialized = true;

            StaticLogger = Logger;
            StaticConfig = Config;
            
            SteamAppId = Config.Bind("Steam", "AppId", 0, new ConfigDescription("Steam App ID"));
            PhotonAppIdRealtime = Config.Bind("Photon", "AppId Realtime", "", new ConfigDescription("Photon Realtime App ID"));
            PhotonAppIdVoice = Config.Bind("Photon", "AppId Voice", "", new ConfigDescription("Photon Voice App ID"));
            PhotonServerAddress = Config.Bind("Photon", "Server", "", new ConfigDescription("Photon Server Address"));
            PhotonServerPort = Config.Bind("Photon", "Server Port", 0, new ConfigDescription("Photon Server Port"));

            harmony.PatchAll(typeof(GeneralPatches));
            
            StaticLogger.LogInfo("Patches Loaded");
        }

#if DEBUG
        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            StaticLogger.LogInfo("Patches Unloaded");
        }
#endif
    }

    [HarmonyPatch]
    internal static class GeneralPatches
    {
        [HarmonyPatch(typeof(DataDirector), nameof(DataDirector.PhotonSetAppId))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void DataDirector_PhotonSetAppId_Postfix(){
            if(!string.IsNullOrEmpty(PluginLoader.PhotonServerAddress.Value)){
                PhotonNetwork.PhotonServerSettings.AppSettings.Server = PluginLoader.PhotonServerAddress.Value;
                PluginLoader.StaticLogger.LogInfo("Photon: Changed Server Address");
            }
            if(PluginLoader.PhotonServerPort.Value > 0){
                PhotonNetwork.PhotonServerSettings.AppSettings.Port = PluginLoader.PhotonServerPort.Value;
                PluginLoader.StaticLogger.LogInfo("Photon: Changed Server Port");
            }
            
            if(!string.IsNullOrEmpty(PluginLoader.PhotonAppIdRealtime.Value)){
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = PluginLoader.PhotonAppIdRealtime.Value;
                PluginLoader.StaticLogger.LogInfo("Photon: Changed AppIdRealtime");
            }
            
            if(!string.IsNullOrEmpty(PluginLoader.PhotonAppIdVoice.Value)){
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = PluginLoader.PhotonAppIdVoice.Value;
                PluginLoader.StaticLogger.LogInfo("Photon: Changed AppIdVoice");
            }
        }
        
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Awake))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        public static void SteamManager_Awake_Prefix(){
            if(!SteamManager.instance && PluginLoader.SteamAppId.Value > 0 && PluginLoader.SteamAppId.Value != 3241660){
                try {
                    SteamClient.Init((uint)PluginLoader.SteamAppId.Value);
                    PluginLoader.StaticLogger.LogInfo($"Steam: Changed AppId to {PluginLoader.SteamAppId.Value}");
                }catch(Exception ex){
                    PluginLoader.StaticLogger.LogError("Steamworks failed to initialize. Error: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.SendSteamAuthTicket))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void SteamManager_SendSteamAuthTicket_Postfix(){
            if(!string.IsNullOrEmpty(PluginLoader.PhotonAppIdRealtime.Value) || !string.IsNullOrEmpty(PluginLoader.PhotonAppIdVoice.Value)){
                PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.None;
                PluginLoader.StaticLogger.LogInfo("Photon: Cleared Steam Auth Ticket");
            }
        }
    }
}