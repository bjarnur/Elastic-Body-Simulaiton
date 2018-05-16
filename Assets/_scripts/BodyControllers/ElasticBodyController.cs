using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElasticBodyController : MonoBehaviour {

    public Vector3 topLeft;
    public Vector3 bottomRight;
    public float spread;
    public float weight_coefficient = 10;
    public float avg_weight = 1;
    public GameObject particle;
    public float coefficientOfRepulsion = 1.0f;
    public float groundCoefficientOfRepulsion = 15.0f;
    public float energyLeakUponBounce = 0.95f;

    private List<GameObject> particles = new List<GameObject>();
    
    // Use this for initialization
    void Start () {        
        for(float x = topLeft.x; x < bottomRight.x; x += spread)
        {
            for(float y = topLeft.y; y > bottomRight.y; y -= spread)
            {
                float x_use = x;
                if (y % 2 == 0) x_use += (spread / 2);
                GameObject particleInstance = Instantiate(particle, new Vector3(x_use, y, 0), Quaternion.identity);
                if (y % 2 == 0)
                {
                    Renderer renderer = particleInstance.GetComponent<Renderer>();
                    Material m = renderer.material;
                    m.color = new Color(0, 1, 0);
                }
                particles.Add(particleInstance);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        foreach (GameObject particleInstance in particles)
        {
            ParticleController pc = particleInstance.GetComponent<ParticleController>();
        }        
    }
    
    public List<GameObject> getParticles()
    {
        return particles;
    }
}
