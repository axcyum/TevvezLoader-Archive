using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VRClient.SDK.VRCs
{
    internal class SINameplateText
    {
        public GameObject gameObject { get; private set; }
        public TMP_Text text { get; private set; }
        public SINameplate parent { get; }
        public Action<SINameplateText>? updateAction { get; set; }
        public SINameplateText? sep { get; set; }
        public bool IsSeparator { get; }

        /// <summary>
        /// Erstellt einen neuen Namensschild-Text (oder Separator).
        /// </summary>
        /// <param name="np">Das übergeordnete SINameplate.</param>
        /// <param name="initialText">Initialer Text (für Separator einfach " - " verwenden).</param>
        /// <param name="updateAction">Optional: Wird bei jedem Frame aufgerufen, um den Text zu aktualisieren.</param>
        /// <param name="startAction">Optional: Wird sofort nach der Erstellung ausgeführt.</param>
        /// <param name="sep">Optional: Ein zugehöriges Separator-Objekt, das vor diesem Text angezeigt wird.</param>
        /// <param name="isSeparator">Gibt an, ob dieses Objekt selbst ein Separator ist.</param>
        public SINameplateText(SINameplate np, string initialText,
            Action<SINameplateText>? updateAction = null,
            Action<SINameplateText>? startAction = null,
            SINameplateText? sep = null,
            bool isSeparator = false)
        {
            parent = np ?? throw new ArgumentNullException(nameof(np));
            this.updateAction = updateAction;
            this.sep = sep;
            IsSeparator = isSeparator;

            // Text-Preset des Parents klonen
            if (np.textPreset == null)
            {
                Debug.LogError("[SINameplateText] textPreset is null – cannot create nameplate text.");
                return;
            }

            gameObject = UnityEngine.Object.Instantiate(np.textPreset.gameObject, np.customNameplate);
            gameObject.SetActive(true);

            var pg = gameObject.GetComponent<PedalGraphic>();
            if (pg != null) pg.color = Color.black;

            text = gameObject.GetComponent<TMP_Text>();
            if (text == null)
                throw new InvalidOperationException("TMP_Text component missing in preset!");

            text.color = Color.white;
            text.richText = true;
            text.text = initialText;

            // Sofort nach Erstellung die Start-Action aufrufen
            startAction?.Invoke(this);
        }

        /// <summary>Führt die Update-Action aus (falls vorhanden).</summary>
        public void UpdateText()
        {
            updateAction?.Invoke(this);
        }

        /// <summary>
        /// Zerstört diesen Namensschild-Text und den zugehörigen Separator (falls vorhanden).
        /// </summary>
        public void Destroy()
        {
            // Zuerst den Separator entfernen (falls vorhanden)
            if (sep != null)
            {
                sep.DestroyInternal();
                sep = null;
            }

            DestroyInternal();
        }

        /// <summary>
        /// Interne Zerstörung: zerstört nur dieses Objekt (nicht rekursiv).
        /// </summary>
        private void DestroyInternal()
        {
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
                gameObject = null;
            }

            // Aus der Eltern-Liste entfernen
            if (parent != null && parent.texts != null)
            {
                int index = parent.texts.IndexOf(this);
                if (index >= 0)
                    parent.texts.RemoveAt(index);
            }
        }
    }

    internal class SINameplate : MonoBehaviour
    {
        public Player player { get; private set; }
        private Transform _playerNameplateContent;
        public Transform customNameplate { get; private set; }
        public Transform textPreset { get; private set; }
        public List<SINameplateText> texts { get; } = new List<SINameplateText>();

        // Offset über der ursprünglichen Namensschild-Position
        private float _offset = 28.3f;

        private void Update()
        {
            // Nur Texte mit Update-Action aktualisieren (Spart CPU-Zeit)
            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i] != null && !texts[i].IsSeparator)
                    texts[i].UpdateText();
            }
        }

        /// <summary>
        /// Initialisiert das benutzerdefinierte Namensschild für einen Spieler.
        /// </summary>
        public void Init(Player p, float offset = 28.3f)
        {
            player = p ?? throw new ArgumentNullException(nameof(p));
            _offset = offset;

            // Zugriff auf das originale Namensschild des Spielers
            var playerNameplate = p.vrcplayer?.field_Public_PlayerNameplate_0;
            if (playerNameplate == null)
            {
                Debug.LogError($"[SINameplate] PlayerNameplate is null for player {p.username}");
                return;
            }

            _playerNameplateContent = playerNameplate.field_Public_GameObject_0?.transform;
            if (_playerNameplateContent == null)
            {
                Debug.LogError("[SINameplate] playerNameplateContent is null");
                return;
            }

            // "Quick Stats" finden (als Vorlage für unser Custom-Nameplate)
            Transform quickStats = null;
            for (int i = 0; i < _playerNameplateContent.childCount; i++)
            {
                var child = _playerNameplateContent.GetChild(i);
                if (child.name == "Quick Stats")
                {
                    quickStats = child;
                    break;
                }
            }

            if (quickStats == null)
            {
                Debug.LogError("[SINameplate] Quick Stats not found – CCNameplate not created");
                return;
            }

            // Benutzerdefiniertes Namensschild aus der Vorlage klonen
            customNameplate = UnityEngine.Object.Instantiate(quickStats, _playerNameplateContent);
            customNameplate.name = $"CustomNameplate_{Guid.NewGuid()}";
            customNameplate.gameObject.SetActive(true);
            customNameplate.localPosition += new Vector3(0f, _offset, 0f);

            // Alle Kinder deaktivieren (behalten nur die Struktur)
            for (int i = 0; i < customNameplate.childCount; i++)
                customNameplate.GetChild(i).gameObject.SetActive(false);

            // Text-Preset bestimmen (bevorzugt das zweite Kind, falls vorhanden)
            textPreset = customNameplate.childCount > 1
                ? customNameplate.GetChild(1)
                : customNameplate.GetChild(0);

            if (textPreset == null)
            {
                Debug.LogError("[SINameplate] textPreset is null after initialization");
            }
        }

        /// <summary>
        /// Fügt einen neuen Text hinzu (mit automatischem Separator, falls nicht der erste Eintrag).
        /// </summary>
        public SINameplateText AddText(string initialText = "",
            Action<SINameplateText>? updateAction = null,
            Action<SINameplateText>? startAction = null)
        {
            // Separator nur hinzufügen, wenn bereits andere Texte existieren
            SINameplateText? sep = null;
            if (texts.Count > 0)
            {
                sep = new SINameplateText(this, " - ", isSeparator: true);
                texts.Add(sep);
            }

            var newText = new SINameplateText(this, initialText, updateAction, startAction, sep);
            texts.Add(newText);
            return newText;
        }

        /// <summary>
        /// Fügt einen bereits existierenden SINameplateText hinzu (z. B. aus einem Pool).
        /// </summary>
        public void AddExistingText(SINameplateText text)
        {
            if (text == null) return;

            // Separator vor dem neuen Text
            if (texts.Count > 0)
            {
                var sep = new SINameplateText(this, " - ", isSeparator: true);
                texts.Add(sep);
                text.sep = sep;
            }

            texts.Add(text);
        }

        private void OnDestroy()
        {
            // Alle Texte aufräumen
            for (int i = texts.Count - 1; i >= 0; i--)
            {
                texts[i]?.Destroy();
            }
            texts.Clear();

            // Custom Nameplate zerstören
            if (customNameplate != null)
            {
                Destroy(customNameplate.gameObject);
            }
        }
    }
}