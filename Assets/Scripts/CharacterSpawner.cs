using Cinemachine;
using Coherence.Toolkit;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public NetworkCharacter networkCharacterPrefab;
    public CinemachineVirtualCamera cinemachineVCam;
        
    private CoherenceBridge _bridge;
    
    private void OnEnable()
    {
        if (CoherenceBridgeStore.TryGetBridge(gameObject.scene, out _bridge))
            _bridge.onConnected.AddListener(OnConnection);
    }

    private void OnDisable() => _bridge.onConnected.RemoveAllListeners();

    private void OnConnection(CoherenceBridge _)
    {
        NetworkCharacter newCharacter = Instantiate(networkCharacterPrefab);
        newCharacter.AssignModelID(PlayerData.AvatarModelID);
        newCharacter.LoadRpmAvatar();

        cinemachineVCam.Follow = newCharacter.transform;
        cinemachineVCam.LookAt = newCharacter.transform;
    }
}
