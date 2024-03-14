using Coherence.Toolkit;
using ReadyPlayerMe.Core;
using UnityEngine;

public class NetworkCharacter : MonoBehaviour
{
    public GameObject avatar;

    [Header("Synced properties")]
    [Sync, OnValueSynced(nameof(ReloadAvatar)), Delayed] public string avatarModelID;

    private AvatarObjectLoader _avatarObjectLoader;

    private void OnEnable()
    {
        _avatarObjectLoader = new AvatarObjectLoader();
        _avatarObjectLoader.OnCompleted += OnAvatarLoaded;
        _avatarObjectLoader.OnFailed += OnAvatarLoadFailed;
    }

    private void OnDisable()
    {
        _avatarObjectLoader.OnCompleted -= OnAvatarLoaded;
        _avatarObjectLoader.OnFailed -= OnAvatarLoadFailed;
    }

    public void AssignModelID(string newID) => avatarModelID = newID;

    /// <summary>
    /// Triggers when the avatarURL string is synced from the remote authority,
    /// and forces this character to load a new avatar.
    /// </summary>
    private void ReloadAvatar(string oldID, string newID)
    {
        AssignModelID(newID);
        LoadRpmAvatar();
    }

    /// <summary>
    /// Requests loading of the RPM Avatar.
    /// </summary>
    public void LoadRpmAvatar()
    {
        string fullAvatarURL = Utilities.CompleteUrl(avatarModelID);
        _avatarObjectLoader.LoadAvatar(fullAvatarURL);
    }

    /// <summary>
    /// This method is triggered when the Avatar mesh has been obtained from the ReadyPlayerMe server
    /// (which automatically instantiates an avatar gameObject).
    /// It transfers all the mesh data to the existing MeshFilters that are bound with coherence,
    /// then destroys the auto-instantiated gameObject (downloadedAvatar).
    /// </summary>
    private void OnAvatarLoaded(object sender, CompletionEventArgs e)
    {
        GameObject downloadedAvatar = e.Avatar;
        AvatarMeshHelper.TransferMesh(downloadedAvatar, avatar);
        Destroy(downloadedAvatar);
    }

    private void OnAvatarLoadFailed(object sender, FailureEventArgs e)
    {
        Debug.LogError($"Avatar download failed. {e.Message}");
    }
    
    private void OnValidate()
    {
        // By tapping into OnValidate, we can paste a new avatarID directly in the Unity editor, and it will just load it straight away!
        if(!string.IsNullOrEmpty(avatarModelID))
            ReloadAvatar(avatarModelID, avatarModelID);
    }
}
