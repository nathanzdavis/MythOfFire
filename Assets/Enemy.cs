using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public Transform textSpawn;

    public void Damage(int damage)
    {
        health -= damage;
    }
}
