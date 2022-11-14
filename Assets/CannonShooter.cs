using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonShooter : MonoBehaviour
{
    public GameObject projectile;
    public float shootForce;
    public float attackCoolDown = 1;

    private float timer;

    private float distanceToTarget;
    private Vector3 targetDirection;
    private float targetSpeed;

    public GameObject target;

    public float shootOffset;
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackCoolDown)  // if using ammo, add (&& ammo >= 1)
        {
            ShootProjectile();
        }
    }

    void ShootProjectile()
    {
        // Get target info
        Vector3 velocity = target.GetComponent<Rigidbody>().velocity;
        targetSpeed = velocity.magnitude;
        distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        targetDirection = target.transform.forward;

        float projectileSpeed = shootForce;
        float projectileTimeToTarget = distanceToTarget / projectileSpeed;
        float projectedTargetTravelDistance = targetSpeed * projectileTimeToTarget;
        Vector3 projectedTarget = target.transform.position + targetDirection * projectedTargetTravelDistance;
        projectedTarget.y += shootOffset; //aim at center of target if 2m high

        GameObject go = Instantiate(projectile, transform.position, Quaternion.identity);
        go.transform.LookAt(projectedTarget);
        Rigidbody rb = go.GetComponent<Rigidbody>();
        rb.velocity = go.transform.forward * shootForce;
        rb.useGravity = true;

        timer = 0f;
    }
}
