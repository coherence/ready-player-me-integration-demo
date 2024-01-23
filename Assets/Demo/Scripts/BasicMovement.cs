using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    [SerializeField] private new GameObject camera;
    
    public Animator animator;
    
    private readonly static int WALK_ANIM = Animator.StringToHash("Walking");
    
    private void Update()
    {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
    
        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        animator.SetBool(WALK_ANIM, z != 0);
    }
}
