using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private List<AnimationMapping> animationMappings;
    private Animator animator;
    private Dictionary<AnimationType, AnimationMapping> mappingDict;

    void Awake()
    {
        animator = GetComponent<Animator>();

        mappingDict = new Dictionary<AnimationType, AnimationMapping>();
        foreach (var mapping in animationMappings)
        {
            if (!mappingDict.ContainsKey(mapping.type))
            {
                mappingDict.Add(mapping.type, mapping);
            }
        }
    }

    // --- CÁC HÀM GỌI ANIMATION ---

    // Dành cho các animation đơn giản như Die (chỉ cần Trigger)
    public void Play(AnimationType type)
    {
        if (mappingDict.TryGetValue(type, out AnimationMapping mapping))
        {
            if (mapping.primaryParameterType == ParameterType.Trigger)
            {
                animator.SetTrigger(mapping.primaryParameterName);
            }
        }
    }

    // Dành cho các animation như Walk (chỉ cần Bool)
    public void Play(AnimationType type, bool value)
    {
        if (mappingDict.TryGetValue(type, out AnimationMapping mapping))
        {
            if (mapping.primaryParameterType == ParameterType.Bool)
            {
                animator.SetBool(mapping.primaryParameterName, value);
            }
        }
    }
    public void Play(AnimationType type, float value)
    {
        if (mappingDict.TryGetValue(type, out AnimationMapping mapping))
        {
            if (mapping.primaryParameterType == ParameterType.Float)
            {
                animator.SetFloat(mapping.primaryParameterName, value);
            }
        }
    }

    // Dành cho các animation phức tạp như Attack (cần cả Trigger và Int)
    public void PlaySpecialAnimation(AnimationType type)
    {
        if (mappingDict.TryGetValue(type, out AnimationMapping mapping))
        {
            // 1. Kích hoạt Trigger chính (nếu có)
            if (mapping.primaryParameterType == ParameterType.Trigger)
            {
                animator.SetTrigger(mapping.primaryParameterName);
            }

            // 2. Set parameter phụ (nếu có và đúng kiểu)
            if (mapping.hasSecondaryParameter && mapping.secondaryParameterType == ParameterType.Int)
            {
                int value = Random.Range(0, mapping.numberOfVariants);
                animator.SetInteger(mapping.secondaryParameterName, value);
            }
        }
    }
}

public enum AnimationType
{
    Walk,
    Attack,
    Die
}

public enum ParameterType
{
    Trigger, Bool, Int, Float
}

[System.Serializable]
public class AnimationMapping
{
    public AnimationType type;

    [Header("Primary Parameter")]
    public string primaryParameterName;
    public ParameterType primaryParameterType;

    [Header("Secondary Parameter (Optional)")]
    [Tooltip("Tick vào đây nếu animation này cần kích hoạt một parameter thứ hai.")]
    public bool hasSecondaryParameter;
    public string secondaryParameterName;
    public ParameterType secondaryParameterType;
    [Tooltip("Tick vào đây nếu animation này cần kích hoạt một parameter thứ hai.")]
    public int numberOfVariants; // Chỉ áp dụng nếu secondaryParameterType là Int
}