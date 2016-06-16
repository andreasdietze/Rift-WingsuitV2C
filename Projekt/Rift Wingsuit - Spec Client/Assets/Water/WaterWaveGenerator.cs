using UnityEngine;
using System.Collections;

public class WaterWaveGenerator : MonoBehaviour {
    
    public float basicScale = 0.1f;

    //Wie "schnell" sollen die Wellen sein? >= 2 wirkt wie ein fliegender Teppich
    public float waveSpeed = 2.0f;

    //Wie stark sollen die Wellen "brechen"?
    public float waveStrength = 1f;

    //Wie weit dürfen Wellenanfänge voneinander entfernt sein?
    public float waveLength = 2f;

    protected Vector3[] baseHeight;

	// Use this for initialization
	void Start () {
        baseHeight = GetComponent<MeshFilter>().mesh.vertices;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3[] vertices = new Vector3[baseHeight.Length];
        //Gehe alle vertices durch und berechne "Grundwelle"
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = baseHeight[i];
            vertex.y -= Mathf.Sin(Time.time * waveSpeed + baseHeight[i].x + baseHeight[i].y + baseHeight[i].z) * basicScale;

            //Zusätzliche Noise reinbringen
            vertex.y -= Mathf.PerlinNoise(baseHeight[i].x + waveLength, baseHeight[i].y + Mathf.Sin(Time.time * 0.1f)) * waveStrength;
            //vertex.y += (waveStrength * 0.5f);
            vertices[i] = vertex;
        }
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
