// Copyright (c) coherence ApS.
// See the license file in the package root for more information.

using Coherence.Cloud;

namespace Coherence.Samples.LobbiesDialog
{
    using UnityEngine;
    using UnityEngine.UI;
    using System;

    public class ConnectDialogLobbyView: MonoBehaviour
    {
        [SerializeField] protected Button selectButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text roomNameText;
        [SerializeField] private Text roomPlayersText;
        [SerializeField] protected Color defaultColor = new Color(243, 247, 250);
        [SerializeField] protected Color selectedColor = new Color(122, 184, 240);

        public LobbyData LobbyData
        {
            get => lobbyData;
            set
            {
                lobbyData = value;
                roomNameText.text = !string.IsNullOrEmpty(lobbyData.Name) ? TruncateName(lobbyData.Name) : lobbyData.ToString();
                roomPlayersText.text = $"{lobbyData.Players.Count}/{lobbyData.MaxPlayers}";
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                backgroundImage.color = isSelected ? selectedColor : defaultColor;
            }
        }

        public Action OnClick
        {
            set
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => value?.Invoke());
            }
        }

        [SerializeField, HideInInspector] private bool isSelected;
        private LobbyData lobbyData;

        private static string TruncateName(string name, int maxLength = 30)
        {
            string newName = name;

            if (newName.Length > maxLength)
            {
                newName = newName.Substring(0, maxLength) + "...";
            }

            return newName;
        }
    }
}
