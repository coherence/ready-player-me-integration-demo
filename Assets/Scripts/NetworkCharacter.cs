using System;
using Coherence.Toolkit;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkCharacter : MonoBehaviour
{
    public Transform animatedAvatar;
    public Transform rootBone;
    public Animator animator;
    
    [Header("Resources")]
    public Avatar masculineAvatar;
    public Avatar feminineAvatar;

    [Header("Synced properties")]
    [Sync, OnValueSynced(nameof(ReloadAvatar))] public string avatarModelID;

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

        // Use the right Animator Avatar definition
        OutfitGender outfitGender = downloadedAvatar.GetComponent<AvatarData>().AvatarMetadata.OutfitGender;
        animator.avatar = outfitGender switch
        {
            OutfitGender.Masculine => masculineAvatar,
            OutfitGender.Feminine => feminineAvatar,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Remove previous SkinnedMeshRenderers in this object
        SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMesh in skinnedMeshRenderers) Destroy(skinnedMesh.gameObject);

        // Recreate new SkinnedMeshRenderers with the new avatar data
        skinnedMeshRenderers = downloadedAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer downloadedSkinnedMesh in skinnedMeshRenderers)
        {
            GameObject newMeshObject = new GameObject(downloadedSkinnedMesh.gameObject.name);
            newMeshObject.transform.SetParent(animatedAvatar, false);
            newMeshObject.transform.localPosition = Vector3.zero;
            SkinnedMeshRenderer newSkinnedMeshRenderer = newMeshObject.AddComponent<SkinnedMeshRenderer>();
            newSkinnedMeshRenderer.sharedMesh = downloadedSkinnedMesh.sharedMesh;
            newSkinnedMeshRenderer.sharedMaterial = downloadedSkinnedMesh.sharedMaterial;
            newSkinnedMeshRenderer.rootBone = rootBone;
            newSkinnedMeshRenderer.bones = Utilities.MapBones(rootBone, downloadedSkinnedMesh.bones);
            newSkinnedMeshRenderer.quality = downloadedSkinnedMesh.quality;
            newSkinnedMeshRenderer.updateWhenOffscreen = downloadedSkinnedMesh.updateWhenOffscreen;
            newSkinnedMeshRenderer.localBounds = downloadedSkinnedMesh.localBounds;

            // Is this even needed? Only if RPM uses blend shapes
            for (int i = 0; i < downloadedSkinnedMesh.sharedMesh.blendShapeCount; i++)
            {
                float blendShapeWeight = downloadedSkinnedMesh.GetBlendShapeWeight(i);
                newSkinnedMeshRenderer.SetBlendShapeWeight(i, blendShapeWeight);
            }
        }
        
        animator.Rebind(); // Necessary to "restart" the Animator, otherwise it will look frozen
        
        Destroy(downloadedAvatar);
    }

    private void OnAvatarLoadFailed(object sender, FailureEventArgs e)
    {
        Debug.LogError($"Avatar download failed. {e.Message}");
    }
}
