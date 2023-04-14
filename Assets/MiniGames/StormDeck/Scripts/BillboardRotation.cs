using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardRotation : MonoBehaviour
{
	public bool yawOnly = true;

    void Update()
    {
		Quaternion newRotation;
		if (yawOnly) {
			newRotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
		}
		else {
			newRotation = Camera.main.transform.rotation;
		}
		transform.rotation = newRotation;
    }
}