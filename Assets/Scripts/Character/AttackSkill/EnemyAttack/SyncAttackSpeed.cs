using UnityEngine;

public class SyncAttackSpeed : StateMachineBehaviour
{
    Stats stats; // Cache component Stats
    [SerializeField] private float originalSpeed = 1.0f;

    // OnStateEnter được gọi MỘT LẦN khi animation TẤN CÔNG BẮT ĐẦU
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 1. Lấy component Stats (chỉ một lần)
        if (stats == null)
            stats = animator.GetComponent<Stats>();

        // 2. Đồng bộ tốc độ của Animator với Tốc độ đánh của Stats
        // (Giả sử 1.0 là tốc độ gốc, 2.0 là nhanh gấp đôi)
        if (stats != null)
            animator.speed = stats.AttackSpeed;
    }

    // OnStateExit được gọi MỘT LẦN khi animation TẤN CÔNG KẾT THÚC
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 3. Reset tốc độ của Animator về 1.0
        // (Rất quan trọng, nếu không animation "Walk" và "Idle" cũng sẽ bị nhanh)
        animator.speed = 1.0f;
    }
}