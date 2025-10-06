using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour
{
    public enum ParameterType
    {
        Trigger, Bool, Int, Float
    }

    [SerializeField]
    private List<string> parameterNames;
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("trig_Attack");
            animator.SetInteger("AttackIndex", Random.Range(0, 2));
        }

    }
    /// <summary>
    /// Play animation bằng cách set parameter
    /// </summary>
    /// <param name="name">Tên parameter trong Animator</param>
    /// <param name="type">Kiểu parameter</param>
    /// <param name="value">Giá trị (nếu cần)</param>
    public void PlayAnimation(string name, ParameterType type, object value = null)
    {
        if (!parameterNames.Contains(name))
        {
            Debug.LogWarning("Animation name not found in list: " + name);
            return;
        }

        switch (type)
        {
            case ParameterType.Trigger:
                animator.SetTrigger(name);
                break;

            case ParameterType.Bool:
                if (value is bool boolVal)
                {
                    animator.SetBool(name, boolVal);
                }
                else
                {
                    Debug.LogWarning($"Expected bool for parameter '{name}', but got {value?.GetType()}");
                }
                break;

            case ParameterType.Int:
                if (value is int intVal)
                {
                    animator.SetInteger(name, intVal);
                }
                else
                {
                    Debug.LogWarning($"Expected int for parameter '{name}', but got {value?.GetType()}");
                }
                break;

            case ParameterType.Float:
                if (value is float floatVal)
                {
                    animator.SetFloat(name, floatVal);
                }
                else
                {
                    Debug.LogWarning($"Expected float for parameter '{name}', but got {value?.GetType()}");
                }
                break;
        }
    }
}

