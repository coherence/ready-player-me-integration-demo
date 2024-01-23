using UnityEngine;
using ReadyPlayerMe.Core;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private BasicMovement basicMovement;
    
    public void LoadAvatar(string shortcode)
    {
        AvatarObjectLoader avatarObjectLoader = new AvatarObjectLoader();
        avatarObjectLoader.LoadAvatar(shortcode);
        avatarObjectLoader.OnCompleted += OnCompleted;
    }

    private void OnCompleted(object sender, CompletionEventArgs e)
    {
        GameObject avatar = e.Avatar;
        
        avatar.transform.SetParent(transform);
        avatar.transform.localPosition = Vector3.zero;
        avatar.transform.localRotation = Quaternion.identity;
        avatar.transform.localScale = Vector3.one;
        
        avatar.GetComponent<Animator>().runtimeAnimatorController = animatorController;
        basicMovement.animator = avatar.GetComponent<Animator>();
    }
}
