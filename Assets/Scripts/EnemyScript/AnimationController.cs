using UnityEngine;

public class AnimationController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
       if(Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("trig_Attack");
            animator.SetInteger("AttackIndex", Random.Range(0, 2));
        }

    }
}
