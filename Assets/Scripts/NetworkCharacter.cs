using System;
using Coherence.Toolkit;
using ReadyPlayerMe.Core;
using UnityEngine;

public class NetworkCharacter : MonoBehaviour
{
    public Transform animatedAvatar;
    public Transform rootBone;
    public Animator animator;
    
    [Header("Resources")]
    public Avatar masculineAvatar;
    public Avatar feminineAvatar;

    [Header("Synced properties")]
    [Sync, OnValueSynced(nameof(ReloadAvatar))] public string avatarURL;

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

    private void ReloadAvatar(string oldAvatarURL, string newAvatarURL)
    {
        avatarURL = newAvatarURL;
        LoadRPMAvatar();
    }

    public void SetAvatarURL(string newAvatarUrl) => avatarURL = newAvatarUrl;
    
    public void LoadRPMAvatar() => _avatarObjectLoader.LoadAvatar(avatarURL);

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
            newSkinnedMeshRenderer.bones = MapBones(rootBone, downloadedSkinnedMesh.bones);
            newSkinnedMeshRenderer.quality = downloadedSkinnedMesh.quality;
            newSkinnedMeshRenderer.updateWhenOffscreen = downloadedSkinnedMesh.updateWhenOffscreen;
            newSkinnedMeshRenderer.localBounds = downloadedSkinnedMesh.localBounds;

            // Is this needed?
            for (int i = 0; i < downloadedSkinnedMesh.sharedMesh.blendShapeCount; i++)
            {
                float blendShapeWeight = downloadedSkinnedMesh.GetBlendShapeWeight(i);
                newSkinnedMeshRenderer.SetBlendShapeWeight(i, blendShapeWeight);
            }
        }
        
        animator.Rebind(); // Necessary to "restart" the Animator
        
        Destroy(downloadedAvatar);
    }
    
    Transform[] MapBones(Transform destinationRoot, Transform[] sourceBones)
    {
        Transform[] mappedBones = new Transform[sourceBones.Length];
        for (int i = 0; i < sourceBones.Length; i++)
        {
            mappedBones[i] = FindDeepChild(destinationRoot, sourceBones[i].name);
            
            if (mappedBones[i] == null) Debug.LogError("Bone mapping failed for: " + sourceBones[i].name);
        }
        return mappedBones;
    }

    Transform FindDeepChild(Transform parent, string boneName)
    {
        if (parent.name == boneName) return parent;

        foreach (Transform child in parent)
        {
            if (child.name == boneName)
                return child;

            Transform result = FindDeepChild(child, boneName);
            if (result != null)
                return result;
        }

        return null;
    }

    private void OnAvatarLoadFailed(object sender, FailureEventArgs e)
    {
        Debug.LogError($"Avatar download failed. {e.Message}");
    }
}
