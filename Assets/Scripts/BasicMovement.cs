using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    public Animator animator;
    public float walkSpeed = 3f;
    public float rotateSpeed = 150.0f;
    
    private static readonly int WalkAnim = Animator.StringToHash("Walking");
    
    private void Update()
    {
        float rotate = Input.GetAxis("Horizontal") * Time.deltaTime * rotateSpeed;
        float forward = Mathf.Clamp01(Input.GetAxis("Vertical") * Time.deltaTime * walkSpeed); // Disallow negative movement
    
        transform.Rotate(0, rotate, 0);
        transform.Translate(0, 0, forward, Space.Self);

        animator.SetBool(WalkAnim, forward != 0);
    }
}
