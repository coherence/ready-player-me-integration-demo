using UnityEngine;

public class Movement : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        transform.Translate(movement * Time.deltaTime * 5, Space.World);
        
        transform.forward = Vector3.Slerp(transform.forward, movement, Time.deltaTime * 5);
    }
}
