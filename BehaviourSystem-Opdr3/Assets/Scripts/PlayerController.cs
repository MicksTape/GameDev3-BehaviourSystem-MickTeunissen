using UnityEngine;
using System.Collections;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    Plane groundPlane;
    [SerializeField] private float speed;
    [Range(0,1)]
    [SerializeField] private float straveMultiplier;
    CharacterController controller;
    [SerializeField] private Animator anim;
    [SerializeField] private 

    float AnimSmooth;
    float animSpeed = 0;
    float height;

    private void Start() {
        controller = GetComponent<CharacterController>();
    }

    void Update() {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        Vector3 mousePos = GetMousePos();

        if (Vector3.Distance(mousePos, transform.position) > 3)
        animSpeed = Mathf.SmoothDamp(animSpeed, input.y, ref AnimSmooth, 0.15f);
        transform.LookAt(new Vector3(mousePos.x, transform.position.y, mousePos.z));
       
        
        if(input.y < 0) {
            input.y *= straveMultiplier;
        }

        if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f) {
            anim.SetBool("isRun", true);
            anim.SetBool("isIdle", false);
        } else {
            anim.SetBool("isIdle", true);
            anim.SetBool("isRun", false);
        }

        if (Vector2.Distance(new Vector2(mousePos.x,mousePos.z), new Vector2(transform.position.x, transform.position.z)) > 0.5f) {
            controller.Move(((transform.forward * input.y)) * Time.deltaTime * speed);
        }
                
    }

    Vector3 GetMousePos() {
        Vector3 mousePos = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        groundPlane = new Plane(Vector3.up, Vector3.zero);

        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
            mousePos = ray.GetPoint(rayDistance);

        return mousePos;
    }
}