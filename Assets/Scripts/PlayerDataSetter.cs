using UnityEngine;
using UnityEngine.UI;

public static class PlayerData
{
    public static string AvatarURL;
}

public class PlayerDataSetter : MonoBehaviour
{
    public InputField avatarURLInputField;

    private void Awake() => SetAvatarURL(avatarURLInputField.text);

    private void OnEnable() => avatarURLInputField.onValueChanged.AddListener(SetAvatarURL);

    private void OnDisable() => avatarURLInputField.onValueChanged.RemoveAllListeners();

    private void SetAvatarURL(string url)
    {
        PlayerData.AvatarURL = url;
    }
}