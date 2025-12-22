using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExitGames.Client.Photon;
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
        
        internal static ConfigEntry<string> PhotonAppIdRealtime;
        internal static ConfigEntry<string> PhotonAppIdVoice;
        internal static ConfigEntry<string> PhotonServerAddress;
        internal static ConfigEntry<int> PhotonServerPort;
        internal static ConfigEntry<int> PhotonServerVersion;
        internal static ConfigEntry<string> PhotonConnectionProtocol;

        private void Awake()
        {
            if (initialized) return;
            initialized = true;

            StaticLogger = Logger;
            StaticConfig = Config;
            
            PhotonAppIdRealtime = Config.Bind("Photon", "AppId Realtime", "", new ConfigDescription("Photon Realtime App ID"));
            PhotonAppIdVoice = Config.Bind("Photon", "AppId Voice", "", new ConfigDescription("Photon Voice App ID"));
            PhotonServerAddress = Config.Bind("Photon", "Server", "", new ConfigDescription("Photon Server Address"));
            PhotonServerPort = Config.Bind("Photon", "Server Port", 0, new ConfigDescription("Photon Server Port", new AcceptableValueRange<int>(0, 65535)));
            PhotonServerVersion = Config.Bind("Photon", "Server Version", 5, new ConfigDescription("Photon Server Version", new AcceptableValueRange<int>(4, 5)));
            PhotonConnectionProtocol = Config.Bind("Photon", "Protocol", "Udp", new ConfigDescription("Photon Protocol", new AcceptableValueList<string>(Enum.GetNames(typeof(ConnectionProtocol)))));

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
            PhotonNetwork.PhotonServerSettings.AppSettings.Port = 0;
            PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
            PhotonNetwork.NetworkingClient.SerializationProtocol = SerializationProtocol.GpBinaryV18;
            
            if(!string.IsNullOrEmpty(PluginLoader.PhotonServerAddress.Value)){
                PhotonNetwork.PhotonServerSettings.AppSettings.Server = PluginLoader.PhotonServerAddress.Value;
                PluginLoader.StaticLogger.LogInfo($"Photon: Changed Server Address");
                
                if(PluginLoader.PhotonServerVersion.Value == 4){
                    PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = false;
                    PhotonNetwork.NetworkingClient.SerializationProtocol = SerializationProtocol.GpBinaryV16;
                }
                
                if(PluginLoader.PhotonServerPort.Value > 0){
                    PhotonNetwork.PhotonServerSettings.AppSettings.Port = PluginLoader.PhotonServerPort.Value;
                    PluginLoader.StaticLogger.LogInfo($"Photon: Changed Server Port");
                }
            }
            
            if(Enum.TryParse<ConnectionProtocol>(PluginLoader.PhotonConnectionProtocol.Value, out var protocol)){
                PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = protocol;
                PluginLoader.StaticLogger.LogInfo($"Photon: Changed Protocol ({protocol})");
            }else{
                PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = ConnectionProtocol.Udp;
            }
            
            if(!string.IsNullOrEmpty(PluginLoader.PhotonAppIdRealtime.Value)){
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = PluginLoader.PhotonAppIdRealtime.Value;
                PluginLoader.StaticLogger.LogInfo($"Photon: Changed AppIdRealtime");
            }
            
            if(!string.IsNullOrEmpty(PluginLoader.PhotonAppIdVoice.Value)){
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = PluginLoader.PhotonAppIdVoice.Value;
                PluginLoader.StaticLogger.LogInfo($"Photon: Changed AppIdVoice");
            }
        }
        
        [HarmonyPatch(typeof(DataDirector), nameof(DataDirector.PhotonSetRegion))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void DataDirector_PhotonSetRegion_Postfix(){
            if(!string.IsNullOrEmpty(PluginLoader.PhotonServerAddress.Value) && PluginLoader.PhotonServerVersion.Value == 4){
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "";
                PluginLoader.StaticLogger.LogInfo("Photon: Cleared Region");
            }
        }

        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.SendSteamAuthTicket))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void SteamManager_SendSteamAuthTicket_Postfix(){
            if(!string.IsNullOrEmpty(PluginLoader.PhotonServerAddress.Value) || !string.IsNullOrEmpty(PluginLoader.PhotonAppIdRealtime.Value) || !string.IsNullOrEmpty(PluginLoader.PhotonAppIdVoice.Value)){
                PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.None;
                PluginLoader.StaticLogger.LogInfo("Photon: Cleared Auth Type");
            }
        }
    }
}