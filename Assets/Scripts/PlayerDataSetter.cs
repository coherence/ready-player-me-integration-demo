using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public static class PlayerData
{
    public static string AvatarModelID;
}

public class PlayerDataSetter : MonoBehaviour
{
    [FormerlySerializedAs("avatarURLInputField")] public InputField avatarIDInputField;

    private void Awake() => SetAvatarModelID(avatarIDInputField.text);

    private void OnEnable() => avatarIDInputField.onValueChanged.AddListener(SetAvatarModelID);

    private void OnDisable() => avatarIDInputField.onValueChanged.RemoveAllListeners();

    private void SetAvatarModelID(string id)
    {
        PlayerData.AvatarModelID = Utilities.StripUrl(id);
    }
}