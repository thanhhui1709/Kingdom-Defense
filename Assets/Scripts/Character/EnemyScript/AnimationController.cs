using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnimationController : MonoBehaviour
{

    [SerializeField]
    private List<AnimationData> animData;
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
    /// 
    public void PlayAnimation(AnimationType type, ParameterType paramType, object value = null)
    {
        switch (type) { 
            case AnimationType.Walk:
                string animName = animData.FirstOrDefault(a => a.type == AnimationType.Walk)?.names.FirstOrDefault();
                if (!string.IsNullOrEmpty(animName))
                {
                    TriggerAnimation(animName, paramType, value);
                }
                break;
            case AnimationType.Run:
                string animNameRun = animData.FirstOrDefault(a => a.type == AnimationType.Run)?.names.FirstOrDefault();
                if (!string.IsNullOrEmpty(animNameRun))
                {
                    TriggerAnimation(animNameRun, paramType, value);
                }
                break;
            case AnimationType.Attack:
                string animNameAttack = animData.FirstOrDefault(a => a.type == AnimationType.Attack)?.names.FirstOrDefault();
                if (!string.IsNullOrEmpty(animNameAttack))
                {
                    TriggerAnimation(animNameAttack, paramType, value);
                }

                break;
            case AnimationType.Die:
                string animNameDie = animData.FirstOrDefault(a => a.type == AnimationType.Die)?.names.FirstOrDefault();
                if (!string.IsNullOrEmpty(animNameDie))
                {
                    TriggerAnimation(animNameDie, paramType, value);
                }
                break;


        }
    }
    private void TriggerAnimation(string name, ParameterType type, object value = null)
    {
        string findName = animData.SelectMany(a => a.names).FirstOrDefault(n => n.Equals(name));
        if (string.IsNullOrEmpty(findName))
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
public enum AnimationType
{
    Idle, Walk, Run, Attack, Die
}
public enum ParameterType
{
    Trigger, Bool, Int, Float
}
[System.Serializable]
public class AnimationData
{
    public AnimationType type;
    public List<string> names;
}

