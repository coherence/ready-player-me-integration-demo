using System;
using Coherence.Cloud;

namespace Coherence.Samples.LobbiesDialog
{
    using UnityEngine;
    using UnityEngine.UI;

    public class LobbySessionPlayersView : MonoBehaviour
    {
        [SerializeField] private Text playersNameText;
        [SerializeField] private GameObject hostIconObject;
        [SerializeField] private Button optionsButton;
        [SerializeField] private GameObject optionsGameObject;
        [SerializeField] private Button kickPlayerButton;
        [SerializeField] private Button fadeSectionButton;
        [SerializeField] private Font regularPlayerFont;
        [SerializeField] private Font boldPlayerFont;
        [SerializeField] private Text lobbyNameText;
        [SerializeField] private Text playersColumnText;
        [SerializeField] private GameObject popupDialog;
        [SerializeField] private Text popupTitleText;
        [SerializeField] private Text popupText;
        [SerializeField] private Button popupDismissButton;
        
        public Player PlayerData => playerData;

        private Player playerData;
        private LobbySession lobbySession;

        public Action onKickedPlayer;

        public void Initialize(Player player, LobbySession lobbySession)
        {
            playerData = player;
            this.lobbySession = lobbySession;

            lobbyNameText.text = lobbySession.LobbyData.Name;
            playersColumnText.text =
                $"Players ({lobbySession.LobbyData.Players.Count}/{lobbySession.LobbyData.MaxPlayers})";

            var playerAttribute = playerData.Attributes[0];
            
            playersNameText.text = TruncateName(playerAttribute.GetStringValue());

            var isMe = lobbySession.MyPlayer == player;
            var isHost = lobbySession.LobbyOwnerActions != null;

            playersNameText.font = isMe ? boldPlayerFont : regularPlayerFont;
            
            var image = optionsButton.GetComponent<Image>();
            var color = image.color;    
            color.a = isMe || !isHost ? 0f : 1f;
            image.color = color;

            optionsButton.onClick.RemoveAllListeners();

            if (!isMe && isHost)
            {
                optionsButton.onClick.AddListener(() =>
                {
                    fadeSectionButton.gameObject.SetActive(true);
                    fadeSectionButton.onClick.RemoveAllListeners();
                    fadeSectionButton.onClick.AddListener(() =>
                    {
                        fadeSectionButton.gameObject.SetActive(false);
                        optionsGameObject.SetActive(false);
                    });
                    optionsGameObject.SetActive(true);
                    optionsGameObject.transform.position = optionsButton.transform.position;

                    kickPlayerButton.onClick.RemoveAllListeners();

                    if (kickPlayerButton.interactable)
                    {
                        kickPlayerButton.onClick.AddListener(KickPlayer);
                    }
                });
            }
            
            hostIconObject.SetActive(player.UserId == lobbySession.LobbyData.OwnerId);
        }

        private void KickPlayer()
        {
            lobbySession.LobbyOwnerActions.KickPlayer(playerData, OnPlayerKicked);
        }

        private void OnPlayerKicked(RequestResponse<bool> response)
        {
            fadeSectionButton.gameObject.SetActive(false);
            optionsGameObject.SetActive(false);

            if (!AssertRequestResponse("Error while kicking player", response.Status, response.Exception))
            {
                return;
            }
            
            onKickedPlayer?.Invoke();
        }

        private static string TruncateName(string name, int maxLength = 30)
        {
            string newName = name;

            if (newName.Length > maxLength)
            {
                newName = newName.Substring(0, maxLength) + "...";
            }

            return newName;
        }
        
#region Error Handling
        private void ShowError(string title, string message = "Unknown Error")
        {
            popupDialog.SetActive(true);
            popupTitleText.text = title;
            popupText.text = message;
            Debug.LogError(message);
        }

        private void HideError()
        {
            popupDialog.SetActive(false);
        }

        private bool AssertRequestResponse(string title, RequestStatus status, Exception exception)
        {
            if (status == RequestStatus.Success)
            {
                return true;
            }
        
            ShowError(title, exception?.Message);

            return false;
        }
#endregion
    }
}