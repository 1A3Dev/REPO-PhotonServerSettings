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
        
        internal static ConfigEntry<bool> PhotonAppIdEnabled;
        internal static ConfigEntry<string> PhotonAppIdRealtime;
        internal static ConfigEntry<string> PhotonAppIdVoice;
        
        internal static ConfigEntry<bool> PhotonServerEnabled;
        internal static ConfigEntry<string> PhotonServerAddress;
        internal static ConfigEntry<int> PhotonServerPort;
        internal static ConfigEntry<int> PhotonServerVersion;
        
        internal static ConfigEntry<string> PhotonConnectionProtocol;
        internal static ConfigEntry<bool> PhotonAlternativeUdpPorts;

        internal static ConfigEntry<int> PhotonObjectsInOneUpdate; 
        internal static ConfigEntry<int> PhotonSendRate;

        internal static int DefaultPhotonObjectsInOneUpdate = 10;
        internal static int DefaultPhotonSendRate = 25;

        private void Awake()
        {
            if (initialized) return;
            initialized = true;

            StaticLogger = Logger;
            StaticConfig = Config;
            
            PhotonAppIdEnabled = Config.Bind("Photon", "AppId Override", true, new ConfigDescription("Photon App ID Override"));
            PhotonAppIdRealtime = Config.Bind("Photon", "AppId Realtime", "", new ConfigDescription("Photon Realtime App ID"));
            PhotonAppIdVoice = Config.Bind("Photon", "AppId Voice", "", new ConfigDescription("Photon Voice App ID"));
            
            PhotonServerEnabled = Config.Bind("Photon", "Server Override", true, new ConfigDescription("Photon Server Override"));
            PhotonServerAddress = Config.Bind("Photon", "Server", "", new ConfigDescription("Photon Server Address"));
            PhotonServerPort = Config.Bind("Photon", "Server Port", 0, new ConfigDescription("Photon Server Port", new AcceptableValueRange<int>(0, 65535)));
            PhotonServerVersion = Config.Bind("Photon", "Server Version", 5, new ConfigDescription("Photon Server Version", new AcceptableValueRange<int>(4, 5)));
            
            PhotonConnectionProtocol = Config.Bind("Photon", "Protocol", "Udp", new ConfigDescription("Photon Protocol", new AcceptableValueList<string>(Enum.GetNames(typeof(ConnectionProtocol)))));
            PhotonAlternativeUdpPorts = Config.Bind("Photon", "Alternative Udp Ports", true, new ConfigDescription("Photon Alternative Ports (Udp)"));
            
            PhotonObjectsInOneUpdate = Config.Bind("Photon Networking", "Objects In One Update", 0, new ConfigDescription("Do not change this unless you know what you are doing. 0 = Vanilla", new AcceptableValueRange<int>(0, 100)));
            PhotonObjectsInOneUpdate.SettingChanged += (sender, args) => {
                if(PhotonObjectsInOneUpdate.Value > 0){
                    PhotonNetwork.ObjectsInOneUpdate = PhotonObjectsInOneUpdate.Value;
                    StaticLogger.LogInfo($"[Photon] Changed ObjectsInOneUpdate: {PhotonNetwork.ObjectsInOneUpdate}");
                }else{
                    PhotonNetwork.ObjectsInOneUpdate = DefaultPhotonObjectsInOneUpdate;
                    StaticLogger.LogInfo($"[Photon] Reset ObjectsInOneUpdate: {PhotonNetwork.ObjectsInOneUpdate}");
                }
            };
            PhotonSendRate = Config.Bind("Photon Networking", "Send Rate", 0, new ConfigDescription("Do not change this unless you know what you are doing. 0 = Vanilla", new AcceptableValueRange<int>(0, 100)));
            PhotonSendRate.SettingChanged += (sender, args) => {
                if(NetworkManager.instance){
                    if(PhotonSendRate.Value > 0){
                        PhotonNetwork.SendRate = PhotonSendRate.Value;
                        PhotonNetwork.SerializationRate = PhotonSendRate.Value;
                        StaticLogger.LogInfo($"[Photon] Changed SendRate and SerializationRate: {PhotonNetwork.SendRate}");
                    }else{
                        PhotonNetwork.SendRate = DefaultPhotonSendRate;
                        PhotonNetwork.SerializationRate = DefaultPhotonSendRate;
                        StaticLogger.LogInfo($"[Photon] Reset SendRate and SerializationRate: {PhotonNetwork.SendRate}");
                    }
                }
            };
            
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
        [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.Start))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void NetworkManager_Start(){
            PluginLoader.DefaultPhotonSendRate = PhotonNetwork.SendRate;
            if(PluginLoader.PhotonSendRate.Value > 0){
                PhotonNetwork.SendRate = PluginLoader.PhotonSendRate.Value;
                PhotonNetwork.SerializationRate = PluginLoader.PhotonSendRate.Value;
                PluginLoader.StaticLogger.LogInfo($"[Photon] Changed SendRate and SerializationRate: {PhotonNetwork.SendRate}");
            }
        }

        [HarmonyPatch(typeof(DataDirector), nameof(DataDirector.PhotonSetAppId))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void DataDirector_PhotonSetAppId_Postfix(){
            PhotonNetwork.PhotonServerSettings.AppSettings.Port = 0;
            PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
            PhotonNetwork.NetworkingClient.SerializationProtocol = SerializationProtocol.GpBinaryV18;
            
            PluginLoader.DefaultPhotonObjectsInOneUpdate = PhotonNetwork.ObjectsInOneUpdate;
            if(PluginLoader.PhotonObjectsInOneUpdate.Value > 0){
                PhotonNetwork.ObjectsInOneUpdate = PluginLoader.PhotonObjectsInOneUpdate.Value;
                PluginLoader.StaticLogger.LogInfo($"[Photon] Changed ObjectsInOneUpdate: {PhotonNetwork.ObjectsInOneUpdate}");
            }
            
            if(PluginLoader.PhotonServerEnabled.Value && !string.IsNullOrEmpty(PluginLoader.PhotonServerAddress.Value)){
                PhotonNetwork.PhotonServerSettings.AppSettings.Server = PluginLoader.PhotonServerAddress.Value;
                PluginLoader.StaticLogger.LogInfo($"[Photon] Changed Server Address: {PhotonNetwork.PhotonServerSettings.AppSettings.Server}");
                
                if(PluginLoader.PhotonServerPort.Value > 0){
                    PhotonNetwork.PhotonServerSettings.AppSettings.Port = PluginLoader.PhotonServerPort.Value;
                    PluginLoader.StaticLogger.LogInfo($"[Photon] Changed Server Port: {PhotonNetwork.PhotonServerSettings.AppSettings.Port}");
                }
                
                if(PluginLoader.PhotonServerVersion.Value == 4){
                    PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = false;
                    PhotonNetwork.NetworkingClient.SerializationProtocol = SerializationProtocol.GpBinaryV16;
                }
            }
            
            if(Enum.TryParse<ConnectionProtocol>(PluginLoader.PhotonConnectionProtocol.Value, out var protocol)){
                PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = protocol;
                PluginLoader.StaticLogger.LogInfo($"[Photon] Changed Protocol: {protocol}");
            }else{
                PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = ConnectionProtocol.Udp;
            }
            
            if(PluginLoader.PhotonAlternativeUdpPorts.Value && PhotonNetwork.PhotonServerSettings.AppSettings.Protocol == ConnectionProtocol.Udp){
                PhotonNetwork.ServerPortOverrides = PhotonPortDefinition.AlternativeUdpPorts;
            }else{
                PhotonNetwork.ServerPortOverrides = new PhotonPortDefinition();
            }
            
            if(PluginLoader.PhotonAppIdEnabled.Value){
                if(!string.IsNullOrEmpty(PluginLoader.PhotonAppIdRealtime.Value)){
                    PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = PluginLoader.PhotonAppIdRealtime.Value;
                    PluginLoader.StaticLogger.LogInfo($"[Photon] Changed AppIdRealtime: {PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime}");
                }
                
                if(!string.IsNullOrEmpty(PluginLoader.PhotonAppIdVoice.Value)){
                    PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = PluginLoader.PhotonAppIdVoice.Value;
                    PluginLoader.StaticLogger.LogInfo($"[Photon] Changed AppIdVoice: {PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice}");
                }
            }
        }
        
        [HarmonyPatch(typeof(DataDirector), nameof(DataDirector.PhotonSetRegion))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void DataDirector_PhotonSetRegion_Postfix(){
            if(!string.IsNullOrEmpty(PluginLoader.PhotonServerAddress.Value) && PluginLoader.PhotonServerVersion.Value == 4){
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "";
                PluginLoader.StaticLogger.LogInfo("[Photon] Changed Region: Empty");
            }
        }

        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.SendSteamAuthTicket))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void SteamManager_SendSteamAuthTicket_Postfix(){
            if(!string.IsNullOrEmpty(PluginLoader.PhotonServerAddress.Value) || !string.IsNullOrEmpty(PluginLoader.PhotonAppIdRealtime.Value) || !string.IsNullOrEmpty(PluginLoader.PhotonAppIdVoice.Value)){
                PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.None;
                PluginLoader.StaticLogger.LogInfo("[Photon] Changed AuthType: None");
            }
        }
    }
}