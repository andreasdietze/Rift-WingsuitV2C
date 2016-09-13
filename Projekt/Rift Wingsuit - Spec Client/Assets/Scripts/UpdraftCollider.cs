using UnityEngine;
using System.Collections;

public class UpdraftCollider : MonoBehaviour {

    public float UpdraftStrength = 40;

	// Use this for initialization
	void OnTriggerEnter(Collider other)
    {
        Rigidbody rBody = other.GetComponentInChildren<Rigidbody>();
        if(rBody == null)
        {
            Debug.LogWarning("Rigidbody in collider is null!");
        } else
        {
            Vector3 vel = new Vector3(rBody.velocity.x, UpdraftStrength, rBody.velocity.z);
            rBody.velocity = vel;
        }
    }
}
