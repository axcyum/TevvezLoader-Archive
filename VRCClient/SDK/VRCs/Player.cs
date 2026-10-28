using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

namespace VRClient.SDK.VRCs
{
    public class Player
    {
        // --- Rollen‑Sets (nicht readonly, damit sie extern befüllt werden können) ---
        public static HashSet<string> Developers = new HashSet<string>();
        public static HashSet<string> Managers = new HashSet<string>();
        public static HashSet<string> Users = new HashSet<string>();

        public static Player? localplayer;
        public static readonly List<Player> players = new List<Player>();

        // Instanz‑Felder (größtenteils als Feld belassen, um externe Schreibzugriffe zu erlauben)
        public readonly VRC.Player VRCPlayer;
        public readonly GameObject playerObject;
        public readonly VRCPlayer vrcplayer;
        public readonly VRCPlayerApi playerapi;
        public readonly IUser iuser;
        public readonly APIUser apiuser;
        public readonly ApiAvatar avatar;

        public string username { get; private set; }
        public string id { get; private set; }
        public string rank { get; private set; } = "Unknown";
        public bool isvr { get; private set; }
        public bool iscontroller;
        public bool islocal { get; private set; }
        public bool isfriend { get; private set; }
        public bool isquestie { get; private set; }
        public bool isadmin { get; private set; }
        public string tag { get; private set; } = "";
        public uint trustColorUint { get; private set; }
        public string userIdHash { get; private set; } = string.Empty;

        // Nameplate – jetzt internal, passend zu SINameplate (internal)
        internal SINameplate nameplate { get; private set; }

        private bool _initialized = false;

        public Player(VRC.Player ply)
        {
            VRCPlayer = ply;

            playerObject = ply.gameObject;
            vrcplayer = ply.prop_VRCPlayer_0;
            playerapi = ply.prop_VRCPlayerApi_0;
            iuser = ply.prop_IUser_0;
            apiuser = ply.prop_APIUser_0;
            avatar = ply.prop_ApiAvatar_0;

            id = iuser?.prop_String_0 ?? "unknown_id";
            username = iuser?.prop_String_1 ?? "unknown_player";

            islocal = playerapi?.isLocal ?? false;
            isvr = playerapi?.IsUserInVR() ?? false;
            isquestie = apiuser?.IsOnMobile ?? false;
        }

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // Freund
            if (APIUser.CurrentUser != null && apiuser != null)
            {
                try { isfriend = APIUser.CurrentUser.friendIDs.Contains(apiuser.id); }
                catch { isfriend = false; }
            }

            // Admin
            if (apiuser != null)
            {
                try
                {
                    isadmin = apiuser.hasModerationPowers || apiuser.hasSuperPowers
                              || (apiuser.tags?.Contains("admin_moderator") ?? false);
                }
                catch { isadmin = false; }
            }

            // Trust‑Farbe (statischer Aufruf!)
            try
            {
                if (apiuser != null)
                {
                    var col = global::VRCPlayer.Method_Public_Static_Color_APIUser_0(apiuser);
                    trustColorUint = ColorToUint(col);
                }
            }
            catch { trustColorUint = ColorToUint(Color.white); }

            // Rollen‑Hash
            try
            {
                if (Developers.Count > 0 || Managers.Count > 0 || Users.Count > 0)
                    userIdHash = SetStringSha256Hash(apiuser?.id ?? id);
            }
            catch { userIdHash = string.Empty; }

            // Tag
            tag = DetermineTag();

            // Rang (jetzt Instanzmethode)
            try { rank = GetRank(); } catch { rank = "Unknown"; }

            // Nameplate
            try { InitializeNameplate(); }
            catch (Exception e)
            {
                Logging.Log($"NAMEPLATE INIT FAILED for {username}: {e.Message}", Logging.LType.Error);
            }
        }

        public static Player CreateAndInitialize(VRC.Player ply)
        {
            var player = new Player(ply);
            player.Initialize();
            players.Add(player);
            if (player.islocal) localplayer = player;
            return player;
        }

        private string DetermineTag()
        {
            if (string.IsNullOrEmpty(userIdHash)) return "";
            if (Developers.Contains(userIdHash)) return "[Developer]";
            if (Managers.Contains(userIdHash)) return "[Manager]";
            if (Users.Contains(userIdHash)) return "[User]";
            return "";
        }

        private void InitializeNameplate()
        {
            if (playerObject == null) return;

            nameplate = playerObject.AddComponent<SINameplate>();
            var pg = nameplate.GetComponent<PedalGraphic>();
            if (pg != null) pg.color = Color.black;
            nameplate.Init(this);

            string platform = isquestie ? "[<color=green>Quest</color>]"
                            : isvr ? "[<color=yellow>PCVR</color>]"
                                   : "[<color=yellow>PC</color>]";
            nameplate.AddText(platform);

            if (!string.IsNullOrEmpty(tag))
            {
                if (tag == "[Developer]")
                    nameplate.AddText("[<color=#b768a2>Tevvez Cheats Developer</color>]");
                else if (tag == "[Manager]")
                    nameplate.AddText("[<color=#b768a2>Tevvez Cheats Manager</color>]");
                else if (tag == "[User]")
                    nameplate.AddText("[<color=#00755e>Tevvez Cheats User</color>]");
            }

            nameplate.AddText(UpdateNudityText());
        }

        public string UpdateNudityText()
        {
            var currentAvatar = vrcplayer?.field_Private_ApiAvatar_0;
            if (currentAvatar == null) return "";

            bool isPublic = currentAvatar.releaseStatus == "public";
            string color = isPublic ? "#228b22" : "#d9004c";
            string status = isPublic ? "Pub" : "Priv";
            return $"[<color={color}>{status} Avatar</color>]";
        }

        // Instanzmethode (nicht statisch)
        private string GetRank()
        {
            if (apiuser?.badges == null) return "Unknown";

            foreach (var badge in apiuser.badges)
            {
                if (badge?.badgeName != null &&
                    badge.badgeName.IndexOf("staff", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Admin/Moderator";
            }

            if (apiuser.HasTag("system_trust_veteran")) return "Veteran User";
            if (apiuser.HasTag("system_trust_trusted")) return "Trusted User";
            if (apiuser.HasTag("system_trust_known")) return "Known User";
            if (apiuser.HasTag("system_trust_basic")) return "User";
            if (apiuser.HasTag("system_trust_visitor")) return "Visitor";

            return "Unknown";
        }

        private static uint ColorToUint(Color c)
        {
            uint r = (uint)(c.r * 255f) & 0xFF;
            uint g = (uint)(c.g * 255f) & 0xFF;
            uint b = (uint)(c.b * 255f) & 0xFF;
            uint a = (uint)(c.a * 255f) & 0xFF;
            return (a << 24) | (b << 16) | (g << 8) | r;
        }

        private static string SetStringSha256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static void RemovePlayer(Player player)
        {
            if (player == null) return;

            if (player.nameplate != null)
            {
                try { UnityEngine.Object.Destroy(player.nameplate); }
                catch { }
            }

            players.Remove(player);
            if (player == localplayer) localplayer = null;
        }
    }
}