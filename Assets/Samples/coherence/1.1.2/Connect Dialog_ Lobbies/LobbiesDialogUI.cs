using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Coherence.Cloud;
using Coherence.Connection;
using Coherence.Runtime;
using Coherence.Toolkit;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Coherence.Samples.LobbiesDialog
{
    public class LobbiesDialogUI : MonoBehaviour
    {
#region References
        [Header("References")]
        public GameObject sampleUi;
        public GameObject connectDialog;
        public GameObject disconnectDialog;
        public GameObject createRoomPanel;
        public GameObject noCloudPlaceholder;
        public GameObject noLobbiesAvailable;
        public GameObject loadingSpinner;
        public LobbySessionUI lobbySessionUI;
        public Text joinLobbyTitleText; 
        public ConnectDialogLobbyView templateLobbyView;
        public InputField lobbyNameInputField;
        public InputField lobbyLimitInputField;
        public Dropdown regionDropdown;
        public Button refreshRegionsButton;
        public Button refreshLobbiesButton;
        public Button joinLobbyButton;
        public Button showCreateLobbyPanelButton;
        public Button hideCreateLobbyPanelButton;
        public Button createAndJoinLobbyButton;
        public Button disconnectButton;
        public GameObject popupDialog;
        public Text popupText;
        public Text popupTitleText;
        public Button popupDismissButton;
        public InputField nameText;
        public GameObject matchmakingRegionsContainer;
        public Toggle matchmakingRegionsTemplate;
        public Text matchmakingTag;
        public Button matchmakingButton;
        public GameObject matchmakingCreateRegionsContainer;
        public Toggle matchmakingCreateRegionsTemplate;
        public ToggleGroup matchmakingCreateRegionToggleGroup;
#endregion

        private CloudService cloudService;

        private int UserLobbyLimit => int.TryParse(lobbyLimitInputField.text, out var limit) ? limit : 10;

        private string initialJoinLobbyTitle;
        private ListView lobbiesListView;
        private string lastCreatedLobbyId;
        private Coroutine cloudServiceReady;
        private CoherenceBridge bridge;

        private string selectedRegion;

        private List<Toggle> instantiatedRegionToggles = new List<Toggle>();
        private List<Toggle> instantiatedCreateRegionToggles = new List<Toggle>();

#region Unity Events
        private void OnEnable()
        {
            if (bridge == null)
            {
                if (!CoherenceBridgeStore.TryGetBridge(gameObject.scene, out bridge))
                {
                    Debug.LogError(
                        $"Couldn't find a {nameof(CoherenceBridge)} in your scene. This dialog will not function properly.");
                    return;
                }
            }

            cloudService = bridge.CloudService;
            cloudService.GameServices.AuthService.OnLogin -= AuthServiceOnLogin;
            cloudService.GameServices.AuthService.OnLogin += AuthServiceOnLogin;

            disconnectDialog.SetActive(false);
            bridge.onConnected.AddListener(_ => UpdateDialogsVisibility());
            bridge.onDisconnected.AddListener((_, __) => UpdateDialogsVisibility());
            bridge.onConnectionError.AddListener(OnConnectionError);
            
            if (!string.IsNullOrEmpty(RuntimeSettings.instance.ProjectID))
            {
                cloudServiceReady = StartCoroutine(WaitForCloudService());
            }
            else if (regionDropdown.gameObject.activeInHierarchy)
            {
                noCloudPlaceholder.SetActive(true);
            }
        }

        private async void AuthServiceOnLogin(LoginResponse loginResponse)
        {
            if (loginResponse.LobbyIds != null && loginResponse.LobbyIds.Count > 0)
            {
                var id = loginResponse.LobbyIds[0];

                try
                {
                    var session = await cloudService.Rooms.LobbyService.GetActiveLobbySessionForLobbyId(id);

                    OnJoinedLobby(new RequestResponse<LobbySession>()
                    {
                        Status = RequestStatus.Success,
                        Exception = null,
                        Result = session
                    });
                }
                catch (Exception e)
                {
                    OnJoinedLobby(new RequestResponse<LobbySession>()
                    {
                        Status = RequestStatus.Fail,
                        Exception = e,
                        Result = null
                    });
                }
            }
        }

        private void OnDisable()
        {
            if (cloudServiceReady != null)
            {
                StopCoroutine(cloudServiceReady);
                cloudServiceReady = null;
            }
        }

        void Start()
        {
            matchmakingRegionsTemplate.gameObject.SetActive(false);
            matchmakingCreateRegionsTemplate.gameObject.SetActive(false);
            nameText.text = Environment.UserName;
            joinLobbyButton.onClick.AddListener(() => JoinLobby(lobbiesListView.Selection.LobbyData));
            showCreateLobbyPanelButton.onClick.AddListener(ShowCreateRoomPanel);
            hideCreateLobbyPanelButton.onClick.AddListener(HideCreateRoomPanel);
            createAndJoinLobbyButton.onClick.AddListener(CreateLobbyAndJoin);
            regionDropdown.onValueChanged.AddListener(OnRegionChanged);
            refreshRegionsButton.onClick.AddListener(RefreshRegions);
            refreshLobbiesButton.onClick.AddListener(RefreshLobbies);
            disconnectButton.onClick.AddListener(bridge.Disconnect);
            popupDismissButton.onClick.AddListener(HideError);
            matchmakingButton.onClick.AddListener(MatchmakingLobby);

            popupDialog.SetActive(false);
            noLobbiesAvailable.SetActive(false);
            joinLobbyButton.interactable = false;
            showCreateLobbyPanelButton.interactable = false;
            templateLobbyView.gameObject.SetActive(false);
            lobbyNameInputField.text = "My Lobby";
            
            lobbiesListView = new ListView
            {
                Template = templateLobbyView,
                onSelectionChange = view =>
                {
                    joinLobbyButton.interactable = view != default && view.LobbyData.Id != default(LobbyData).Id;
                }
            };

            initialJoinLobbyTitle = joinLobbyTitleText.text;
        }

        #endregion

#region Cloud & Replication Server Requests
        private IEnumerator WaitForCloudService()
        {
            ShowLoadingState();

            while (!cloudService.Rooms.IsLoggedIn)
            {
                yield return null;
            }

            HideLoadingState();

            RefreshRegions();
            cloudServiceReady = null;
        }

        private void RefreshLobbies()
        {
            if (cloudService == null)
            {
                return;
            }

            ShowLoadingState();
            noLobbiesAvailable.SetActive(false);
            refreshLobbiesButton.interactable = false;

            LobbyFilter fetchLobbyFilter = new LobbyFilter()
                .WithRegion(FilterOperator.Any, new List<string>() { selectedRegion });

            var lobbyFilters = new List<LobbyFilter>() { fetchLobbyFilter };
            
            cloudService.Rooms.LobbyService.FindLobbies(OnFetchLobbies, new FindLobbyOptions()
            {
                LobbyFilters = lobbyFilters
            });
        }

        private void CreateLobby()
        {
            ShowLoadingState();
            
            var playerAttribute = GetPlayerNameAttribute();

            var createOptions = new CreateLobbyOptions()
            {
                Region = matchmakingCreateRegionToggleGroup.GetFirstActiveToggle().GetComponentInChildren<Text>().text.ToLowerInvariant(),
                MaxPlayers = UserLobbyLimit,
                Name = lobbyNameInputField.text,
                PlayerAttributes = playerAttribute
            };

            cloudService.Rooms.LobbyService.CreateLobby(createOptions, OnCreatedLobby);
            HideCreateRoomPanel();
        }

        private List<CloudAttribute> GetPlayerNameAttribute()
        {
            var playerAttribute = new List<CloudAttribute>() { new CloudAttribute("player_name", nameText.text, true) };
            return playerAttribute;
        }

        private void JoinLobby(LobbyData lobbyData)
        {
            ShowLoadingState();
            
            var playerAttribute = GetPlayerNameAttribute();
            
            cloudService.Rooms.LobbyService.JoinLobby(lobbyData, OnJoinedLobby, playerAttribute);
        }

        private void CreateLobbyAndJoin()
        {
            CreateLobby();
        }

        private void RefreshRegions()
        {
            cloudService.Rooms.RefreshRegions(OnRegionsChanged);
        }
        
        private void MatchmakingLobby()
        {
            ShowLoadingState();

            var regions = new List<string>();

            foreach (var regionToggle in instantiatedRegionToggles)
            {
                if (regionToggle.isOn)
                {
                    regions.Add(regionToggle.GetComponentInChildren<Text>().text.ToLowerInvariant());
                }
            }

            LobbyFilter filter = new LobbyFilter()
                .WithAnd()
                .WithRegion(FilterOperator.Any, regions)
                .WithTag(FilterOperator.Any, new List<string>() { matchmakingTag.text });

            var findOptions = new FindLobbyOptions()
            {
                LobbyFilters = new List<LobbyFilter>() { filter }
            };
            
            var playerNameAttribute = GetPlayerNameAttribute();
            var region = matchmakingCreateRegionToggleGroup.GetFirstActiveToggle().GetComponentInChildren<Text>().text.ToLowerInvariant();
       
            var createOptions = new CreateLobbyOptions()
            {
                Tag = matchmakingTag.text,
                Region = region,
                MaxPlayers = UserLobbyLimit,
                PlayerAttributes = playerNameAttribute
            };
            
            cloudService.Rooms.LobbyService.FindOrCreateLobby(findOptions, createOptions, OnJoinedLobby);
            HideCreateRoomPanel();
        }

        #endregion

#region Request Callbacks
        private void OnRegionsChanged(RequestResponse<IReadOnlyList<string>> requestResponse)
        {
            HideLoadingState();

            if (!AssertRequestResponse("Error while fetching room regions", requestResponse.Status, requestResponse.Exception))
            {
                return;
            }

            var options = new List<Dropdown.OptionData>();
            var regions = requestResponse.Result;
            foreach (var region in regions)
            {
                options.Add(new Dropdown.OptionData(region));
            }

            regionDropdown.options = options;

            if (regions.Count > 0)
            {
                selectedRegion = regions[0];
                regionDropdown.captionText.text = regions[0];
                RefreshLobbies();
            }
        }

        private void OnFetchLobbies(RequestResponse<IReadOnlyList<LobbyData>> requestResponse)
        {
            var lobbies = requestResponse.Result;

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ReactivateRefreshButton());
            }
            else
            {
                refreshLobbiesButton.interactable = true;
            }
            
            loadingSpinner.SetActive(false);
            HideLoadingState();

            joinLobbyTitleText.text = initialJoinLobbyTitle + " (0)";
            noLobbiesAvailable.SetActive(requestResponse.Status != RequestStatus.Success || requestResponse.Result.Count == 0);

            if (!AssertRequestResponse("Error while fetching available lobbies", requestResponse.Status, requestResponse.Exception))
            {
                lobbiesListView.Clear();
                return;
            }

            if (lobbies.Count == 0)
            {
                lobbiesListView.Clear();
                return;
            }

            lobbiesListView.SetSource(lobbies, lastCreatedLobbyId);
            lastCreatedLobbyId = default; // selection was already set.
            joinLobbyTitleText.text = $"{initialJoinLobbyTitle} ({lobbies.Count})";

            joinLobbyButton.interactable = lobbiesListView.Selection != default;
        }

        private IEnumerator ReactivateRefreshButton()
        {
            while (cloudService.Rooms.LobbyService.GetFindLobbiesCooldown() > TimeSpan.Zero)
            {
                yield return null;
            }
            
            refreshLobbiesButton.interactable = true;
        }
        
        private void OnCreatedLobby(RequestResponse<LobbySession> response)
        {
            HideLoadingState();
            
            if (!AssertRequestResponse("Error while creating and joining lobby", response.Status, response.Exception))
            {
                return;
            }
            
            ActivateLobbySessionUI(response);
        }

        private void ActivateLobbySessionUI(RequestResponse<LobbySession> response)
        {
            connectDialog.SetActive(false);
            lobbySessionUI.gameObject.SetActive(true);
            lobbySessionUI.Initialize(response.Result);
        }

        private void OnJoinedLobby(RequestResponse<LobbySession> response)
        {
            HideLoadingState();
            
            if (!AssertRequestResponse("Error while joining lobby", response.Status, response.Exception))
            {
                return;
            }
            
            ActivateLobbySessionUI(response);
        }
#endregion

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

            var message = exception?.Message;

            if (exception is RequestException requestEx && requestEx.ErrorCode == ErrorCode.FeatureDisabled)
            {
                message += "\n\nMake sure Persisted Accounts is enabled in your coherence Dashboard.";
            }

            ShowError(title, message);

            return false;
        }

        private void OnConnectionError(CoherenceBridge bridge, ConnectionException exception)
        {
            HideLoadingState();
            RefreshLobbies();
            ShowError("Error connecting to Room", exception?.Message);
        }
#endregion

#region Update UI
        private void ShowCreateRoomPanel()
        {
            createRoomPanel.SetActive(true);

            InstantiateRegionToggles(instantiatedRegionToggles, matchmakingRegionsTemplate, matchmakingRegionsContainer.transform);
            InstantiateRegionToggles(instantiatedCreateRegionToggles, matchmakingCreateRegionsTemplate, matchmakingCreateRegionsContainer.transform);
        }

        private void InstantiateRegionToggles(List<Toggle> instantiatedToggles, Toggle template, Transform parent)
        {
            foreach (var toggle in instantiatedToggles)
            {
                Destroy(toggle.gameObject);
            }

            instantiatedToggles.Clear();

            foreach (var region in regionDropdown.options)
            {
                var instantiatedToggle = Instantiate(template, parent);
                instantiatedToggle.gameObject.SetActive(true);
                instantiatedToggle.GetComponentInChildren<Text>().text = region.text.ToUpperInvariant();
                instantiatedToggles.Add(instantiatedToggle);
            }
        }

        private void HideCreateRoomPanel()
        {
            createRoomPanel.SetActive(false);
        }

        private void UpdateDialogsVisibility()
        {
            sampleUi.SetActive(!bridge.isConnected);
            disconnectDialog.SetActive(bridge.isConnected);

            if (!bridge.isConnected)
            {
                RefreshLobbies();
            }
        }

        private void HideLoadingState()
        {
            loadingSpinner.SetActive(false);
            showCreateLobbyPanelButton.interactable = true;
            joinLobbyButton.interactable = lobbiesListView != null && lobbiesListView.Selection != default
                                                                && lobbiesListView.Selection.LobbyData.Id != default(LobbyData).Id;
        }

        private void ShowLoadingState()
        {
            loadingSpinner.SetActive(true);
            showCreateLobbyPanelButton.interactable = false;
            joinLobbyButton.interactable = false;
        }

        private void OnRegionChanged(int region)
        {
            if (!cloudService.Rooms.IsLoggedIn)
            {
                return;
            }

            selectedRegion = regionDropdown.options[region].text;
            
            RefreshLobbies();
        }
#endregion
    }

    internal class ListView
    {
        public ConnectDialogLobbyView Template;
        public Action<ConnectDialogLobbyView> onSelectionChange;

        public ConnectDialogLobbyView Selection
        {
            get => selection;
            set
            {
                if (selection != value)
                {
                    selection = value;
                    lastSelectedId = selection == default ? default : selection.LobbyData.Id;
                    onSelectionChange?.Invoke(Selection);
                    foreach (var viewRow in Views)
                    {
                        viewRow.IsSelected = selection == viewRow;
                    }
                }
            }
        }

        public List<ConnectDialogLobbyView> Views { get; }
        private ConnectDialogLobbyView selection;
        private HashSet<string> displayedIds = new HashSet<string>();
        private string lastSelectedId;

        public ListView(int capacity = 50)
        {
            Views = new List<ConnectDialogLobbyView>(capacity);
        }

        public void SetSource(IReadOnlyList<LobbyData> dataSource, string idToSelect = default)
        {
            displayedIds = new HashSet<string>(dataSource.Select(d => d.Id));

            Clear();

            if (dataSource.Count <= 0)
            {
                return;
            }

            var sortedData = dataSource.ToList();
            sortedData.Sort((lobbyA, lobbyB) => String.CompareOrdinal(lobbyA.Name, lobbyA.Name));

            if (idToSelect == default && lastSelectedId != default)
            {
                idToSelect = lastSelectedId;
            }

            foreach (var data in sortedData)
            {
                var view = MakeViewItem(data);
                Views.Add(view);
                if (data.Id == idToSelect)
                {
                    Selection = view;
                }
            }
        }

        private ConnectDialogLobbyView MakeViewItem(LobbyData data, bool isSelected = false)
        {
            ConnectDialogLobbyView view = Object.Instantiate(Template, Template.transform.parent);
            view.LobbyData = data;
            view.IsSelected = isSelected;
            view.OnClick = () => Selection = view;
            view.gameObject.SetActive(true);
            return view;
        }

        public void Clear()
        {
            Selection = default;
            foreach (var view in Views)
            {
                Object.Destroy(view.gameObject);
            }
            Views.Clear();
        }
    }
}
