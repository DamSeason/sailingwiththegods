using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSpawnerController : MonoBehaviour
{
    // public Transform arrowToSpawn;
    public GameObject arrowToSpawn;
	//public int targetScore;
    public  GameObject spawnLoc;

    public float minTimeBetweenSpawn;
    public float maxTimeBetweenSpawn;

    //private Transform arrow;
    private GameObject arrow;
    public Transform spawnParent;
    Vector3 spawnPosition;

    private bool callFunction ;
    private bool startFunction;
    private float startFunctionTimer;
    public float startFunctionEndTime;

    // Start is called before the first frame update
    void Start()
    {
        startFunction = false;
        callFunction = true;
        startFunctionTimer = 0;
    }

    IEnumerator ArrowSpawn()
        {
        //while (GameManager.startPlaying == true && GameManager.currentScore < targetScore)
        // {

        //must be done to be able to set up GameObject as a child of the Canvas so our image renders
        // arrow = Instantiate(arrowToSpawn, spawnLoc.position, spawnLoc.rotation) as Transform;
        // arrow.parent = spawnParent;

      
		// josh's notes I moved the code down below line 45 so arrows would not spawn at the games start
		// spawn time is set publicly in the inspecter
			callFunction = false;
            yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawn, maxTimeBetweenSpawn));
			callFunction = true;
			arrow = Instantiate(arrowToSpawn, spawnLoc.transform.position, spawnLoc.transform.rotation) as GameObject;
			if (spawnParent != null) //get rid of null ref at the games end
			{ arrow.transform.SetParent(spawnParent.transform); }
		
		// }
	}

	void Update()
    {
		//so that the coroutine runs after timer
        if(SongGameController.startPlaying == true && SongGameController.endGameState == false && callFunction == true && startFunction == true)
        {
            StartCoroutine(ArrowSpawn());
        }

        startFunctionTimer += Time.deltaTime;
        if (startFunctionTimer >= startFunctionEndTime)
        {
            startFunction = true;
        }
        //Debug.Log(startFunction);
    }
}
