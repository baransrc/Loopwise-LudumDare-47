using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StripeManager : MonoBehaviour
{
    public void PlayStripes()
    {
        GetComponent<Animator>().Play("Stripe Animation");
    }
}
