using System;
using System.Collections.Generic;
using System.Linq;
using Coherence.Cloud;
using Coherence.Samples.LobbiesDialog;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class LobbySessionUI : MonoBehaviour
{
    [Header("References")] 
    public LobbySessionPlayersView templatePlayersView;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject lobbiesListObject;
    [SerializeField] private Button refreshLobbyButton;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private GameObject popupDialog;
    [SerializeField] private Text popupTitleText;
    [SerializeField] private Text popupText;
    [SerializeField] private Button popupDismissButton;

    private PlayersListView playersListView;
    private LobbySession lobbySession;
    
    private void Awake()
    {
        popupDismissButton.onClick.AddListener(HideError);
    }

    private void InitPlayersListView()
    {
        if (playersListView != null)
        {
            return;
        }
        
        templatePlayersView.gameObject.SetActive(false);
        
        playersListView = new PlayersListView
        {
            Template = templatePlayersView
        };
    }

    public void Initialize(LobbySession lobbySession)
    {
        if (this.lobbySession != null)
        {
            this.lobbySession.OnLobbyUpdated -= OnLobbyUpdated;
            this.lobbySession.OnLobbyDisposed -= OnLobbyUpdated;
        }
        
        this.lobbySession = lobbySession;
        this.lobbySession.OnLobbyUpdated += OnLobbyUpdated;
        this.lobbySession.OnLobbyDisposed += OnLobbyUpdated;

        RefreshViews();

        leaveLobbyButton.onClick.RemoveAllListeners();
        leaveLobbyButton.onClick.AddListener(LeaveLobby);
        
        refreshLobbyButton.onClick.RemoveAllListeners();
        refreshLobbyButton.onClick.AddListener(RefreshLobby);
        
        startGameButton.onClick.RemoveAllListeners();
        startGameButton.onClick.AddListener(StartGame);
    }

    private void OnLobbyUpdated(LobbySession lobby)
    {
        if (lobby.MyPlayer == null)
        {
            OnLobbyDispose();
        }
        else
        {
            RefreshViews();
        }
    }

    private void StartGame()
    {
        ShowLoadingState();

        lobbySession.LobbyOwnerActions.StartGameSession(OnGameSessionStarted);
    }

    private void OnGameSessionStarted(RequestResponse<bool> response)
    {
        HideLoadingState();

        AssertRequestResponse("Error while starting game session", response.Status, response.Exception);
    }

    private void RefreshViews()
    {
        if (lobbySession.IsDisposed)
        {
            OnLobbyDispose();
        }
        
        InitPlayersListView();
        playersListView.SetSource(lobbySession.LobbyData.Players, lobbySession, RefreshLobby);
        var isHost = lobbySession.LobbyOwnerActions != null;
        startGameButton.gameObject.SetActive(isHost);
    }

    private void RefreshLobby()
    {
        ShowLoadingState();
        
        lobbySession.RefreshLobby(OnLobbyRefreshed);
    }

    private void OnLobbyRefreshed()
    {
        HideLoadingState();
        RefreshViews();
    }

    private void LeaveLobby()
    {
        ShowLoadingState();

        lobbySession.LeaveLobby(OnLeftLobby);
    }

    private void OnLeftLobby(RequestResponse<bool> response)
    {
        HideLoadingState();

        AssertRequestResponse("Error while leaving lobby", response.Status, response.Exception);
    }

    private void OnLobbyDispose()
    {
        lobbiesListObject.SetActive(true);
        gameObject.SetActive(false);
    }

    private void HideLoadingState()
    {
        loadingSpinner.SetActive(false);
        startGameButton.interactable = true;
        leaveLobbyButton.interactable = true;
        refreshLobbyButton.interactable = true;
    }

    private void ShowLoadingState()
    {
        loadingSpinner.SetActive(true);
        startGameButton.interactable = false;
        leaveLobbyButton.interactable = false;
        refreshLobbyButton.interactable = false;
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


internal class PlayersListView
{
    public LobbySessionPlayersView Template;

    public List<LobbySessionPlayersView> Views { get; }
    private HashSet<string> displayedIds = new HashSet<string>();
    private string lastSelectedId;

    public PlayersListView(int capacity = 50)
    {
        Views = new List<LobbySessionPlayersView>(capacity);
    }

    public void SetSource(IReadOnlyList<Player> dataSource, LobbySession lobbySession, Action onKickedPlayer)
    {
        displayedIds = new HashSet<string>(dataSource.Select(d => d.UserId));

        Clear();

        if (dataSource.Count <= 0)
        {
            return;
        }

        var sortedData = dataSource.ToList();
        sortedData.Sort((playerA, playerB) =>
        {
            if (playerA == lobbySession.MyPlayer)
            {
                return 1;
            }
            
            if (playerB == lobbySession.MyPlayer)
            {
                return -1;
            }

            return 0;
        });

        foreach (var data in sortedData)
        {
            var view = MakeViewItem(data, lobbySession);
            view.onKickedPlayer += onKickedPlayer;
            Views.Add(view);
        }
    }

    private LobbySessionPlayersView MakeViewItem(Player data, LobbySession lobbySession)
    {
        LobbySessionPlayersView view = Object.Instantiate(Template, Template.transform.parent);
        view.Initialize(data, lobbySession);
        view.gameObject.SetActive(true);
        return view;
    }

    public void Clear()
    {
        foreach (var view in Views)
        {
            view.onKickedPlayer = null;
            Object.Destroy(view.gameObject);
        }
        Views.Clear();
    }
}