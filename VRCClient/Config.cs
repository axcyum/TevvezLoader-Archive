using System.Text.Json.Serialization;
using UnityEngine;
using VRC.DataModel;

namespace VRClient
{
    public class ConfigDto
    {
        [JsonPropertyName("VRChat")]
        public Cfg VRChat = new Cfg();
    }

    public class Config
    {
        [JsonPropertyName("MenuKey")]
        public static int MenuKey = 0x2D;

        [JsonPropertyName("VRChat")]
        public static Cfg VRChat = new Cfg();
    }

    public class Cfg
    {
        [JsonPropertyName("Hud")]
        public static bool Hud = false;

        [JsonPropertyName("Fly")]
        public static bool Fly = false;

        [JsonPropertyName("BunnyHop")]
        public static bool BunnyHop = false;

        [JsonPropertyName("InfJump")]
        public static bool InfJump = false;

        [JsonPropertyName("StepHigh")]
        public static bool StepHigh = false;

        [JsonPropertyName("ClickTP")]
        public static bool ClickTP = false;

        [JsonPropertyName("GhostMode")]
        public static bool GhostMode = false;

        [JsonPropertyName("Invisibility")]
        public static bool Invisibility = false;

        [JsonPropertyName("UdonBlocker")]
        internal static bool UdonBlocker = false;

        [JsonPropertyName("blockSync")]
        internal static bool blockSync = false;

        [JsonPropertyName("E12Invis")]
        internal static bool E12Invis = false;

        [JsonPropertyName("Deserialize")]
        internal static bool Deserialize = false;

        [JsonPropertyName("UdonLogger")]
        internal static bool UdonLogger = false;

        [JsonPropertyName("AntiTeleport")]
        public static bool AntiTeleport = false;

        [JsonPropertyName("AntiRespawn")]
        public static bool AntiRespawn = false;

        [JsonPropertyName("NameESP")]
        public static bool NameESP = false;

        [JsonPropertyName("UseTrustColor")]
        public static bool UseTrustColor = true;

        [JsonPropertyName("Box3ESP")]
        public static bool Box3ESP = false;

        [JsonPropertyName("ItemBoxESP")]
        public static bool ItemBoxESP = false;

        [JsonPropertyName("ItemNameESP")]
        public static bool ItemNameESP = false;

        [JsonPropertyName("InteractableBoxESP")]
        public static bool InteractableBoxESP = false;

        [JsonPropertyName("CapsuleESP")]
        public static bool CapsuleESP = false;

        [JsonPropertyName("InteractableNameESP")]
        public static bool InteractableNameESP = false;

        [JsonPropertyName("SkeletonESP")]
        public static bool SkeletonESP = false;

        [JsonPropertyName("Box2ESP")]
        public static bool Box2ESP = false;

        [JsonPropertyName("UdonESP")]
        public static bool UdonESP = false;

        [JsonPropertyName("TriggersESP")]
        public static bool TriggersESP = false;

        [JsonPropertyName("StationESP")]
        public static bool StationESP = false;

        [JsonPropertyName("InteractableESP")]
        public static bool InteractableESP = false;

        [JsonPropertyName("InteractableRefresh")]
        public static bool InteractableRefresh = false;

        [JsonPropertyName("OwnerSpoof")]
        public static bool OwnerSpoof = false;

        [JsonPropertyName("MasterSpoof")]
        public static bool MasterSpoof = false;

        [JsonPropertyName("EventLog")]
        internal static bool EventLog = false;

        [JsonPropertyName("NpcNameESP")]
        public static bool NpcNameESP = false;

        [JsonPropertyName("NpcBoxESP")]
        public static bool NpcBoxESP = false;

        [JsonPropertyName("WorldBoxESP")]
        public static bool WorldBoxESP = false;

        [JsonPropertyName("WorldBoxLineWidth")]
        public static float WorldBoxLineWidth = 0.02f;

        [JsonPropertyName("BoxThickness")]
        public static float BoxThickness = 1.5f;

        [JsonPropertyName("SkeletonThickness")]
        public static float SkeletonThickness = 1.5f;

        [JsonPropertyName("CapsuleBlurIterations")]
        public static float CapsuleBlurIterations = 1;

        [JsonPropertyName("CapsuleBlurSize")]
        public static float CapsuleBlurSize = 0.5f;

        [JsonPropertyName("CapsuleBlurSize")]
        public static Vector3 WorldBoxSize = new Vector3(0.6f, 1.8f, 0.6f);

        public static float WorldNameFontSize = 0.3f;   // Größe des 3D-Textes
        public static float WorldNameOffset = 0.5f;

        [JsonPropertyName("CustomColor")]
        public static Color CustomColor = Color.green,
            BoxColor = new(0, 1, 0, 1),
            SkeletonColor = new(0, 1, 0, 1),
            ItemBoxColor = new(1, 0.5f, 0, 1),
            InteractableBoxColor = new(0, 1, 1, 1),
            CapsuleColor = new(0.4f, 0f, 0.4f, 1f),
            NpcColor = new(1f, 0.2f, 0.2f, 1f);

        [JsonPropertyName("ActionMenuColor")]
        public static Color ActionMenuColor = new Color(0f, 0f, 0f, 1f);

        [JsonPropertyName("PlayerSize")]
        public static float PlayerSize = 1f;

        [JsonPropertyName("SpeedHack")]
        public static float SpeedHack = 10f;

        [JsonPropertyName("TeleportRandom")]
        public static float TeleportRandom = 5000f;

        [JsonPropertyName("FlyBasicSpeed")]
        public static float FlyBasicSpeed = 0.1f;

        [JsonPropertyName("FlyShiftSpeed")]
        public static float FlyShiftSpeed = 0.5f;

        [JsonPropertyName("StepValue")]
        public static float StepValue = 111f;

        [JsonPropertyName("PlayerESPDistance")]
        public static float PlayerESPDistance = 50f;

        [JsonPropertyName("ItemESPDistance")]
        public static float ItemESPDistance = 50f;

        [JsonPropertyName("InteractableESPDistance")]
        public static float InteractableESPDistance = 50f;

        [JsonPropertyName("ZombieESPDistance")]
        public static float ZombieESPDistance = 50f;

        [JsonPropertyName("CapsuleESPDistance")]
        public static float CapsuleESPDistance = 50f;
    }
}