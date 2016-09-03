using UnityEngine;
using System.Collections;

public class HelicopterFlightCam : MonoBehaviour {

    //public FlyCamV2 flycam;
    /*public HelicopterFlight heliflight;
    private bool hasStarted = false;
    private bool pushOut = false;
    private bool isRunning = true;
    public float startTime = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.J) && !hasStarted && isRunning)
        {
            hasStarted = true;
            // StartCoroutine(StartFlightRoutine());
            heliflight.GoHeliStart();
            flycam.StartFlight();
            isRunning = false;
        }
	}

    void FixedUpdate()
    {
        if (isRunning)
        {
            Vector3 flightVector = new Vector3(heliflight.flightSpeed, 0, 0);
            transform.Translate(flightVector);
            if (hasStarted)
            {
                transform.Translate(Vector3.forward * Time.deltaTime * heliflight.startSpeed);
            }
        }
    }

    IEnumerator StartFlightRoutine()
    {
        yield return new WaitForSeconds(startTime);
        heliflight.GoHeliStart();
        flycam.StartFlight();
        isRunning = false;
    }*/
}
