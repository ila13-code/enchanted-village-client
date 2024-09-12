using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class SwordManController : MonoBehaviour
    {
        private Animator animator;
        private static readonly int IdleState = Animator.StringToHash("idle");
        private static readonly int WalkState = Animator.StringToHash("walk");
        private static readonly int AttackState = Animator.StringToHash("attack");

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayIdle()
        {
            animator.Play(IdleState);
        }

        public void PlayWalk()
        {
            animator.Play(WalkState);
        }

        public void PlayAttack()
        {
            animator.Play(AttackState);
        }
    }
}
