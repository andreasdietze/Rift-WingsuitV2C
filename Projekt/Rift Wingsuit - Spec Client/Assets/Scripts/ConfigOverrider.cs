using UnityEngine;
using System.Collections;

public class ConfigOverrider : MonoBehaviour {

    public long detailDistance = 3000;
    private float detailObjectDistance;
    public bool overrideStartPos = false;
    public Vector3 startPosition = new Vector3(5000, 5000, 5000);

	// Use this for initialization
    void Start()
    {
        detailObjectDistance = detailDistance / 2;
        Terrain.activeTerrain.basemapDistance = detailDistance;
        Terrain.activeTerrain.detailObjectDistance = detailObjectDistance;

        if (overrideStartPos)
        {
            Camera.main.transform.position = startPosition;
        }
	}
	
	// Update is called once per frame
    void Update()
    {
        Terrain.activeTerrain.basemapDistance = detailDistance;
	}
}
