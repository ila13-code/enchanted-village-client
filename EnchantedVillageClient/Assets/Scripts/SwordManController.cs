using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordManController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Ottiene il componente Animator
        animator = GetComponent<Animator>();

        // Setta l'animazione iniziale su "idle"
        animator.Play("idle");
    }
}
