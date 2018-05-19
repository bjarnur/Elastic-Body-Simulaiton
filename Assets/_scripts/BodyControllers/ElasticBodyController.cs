using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElasticBodyController : MonoBehaviour {

    public Vector3 topLeft;
    public Vector3 bottomRight;
    public float spread;
    public GameObject particle;
    public Color color = new Color(1, 0, 0);

    float[,] initialDistances;

    private List<GameObject> particles = new List<GameObject>();
    
    void Start () {

        int particlesPerRow = 0;
        for(float x = topLeft.x; x < bottomRight.x; x += spread)
        {
            particlesPerRow++;
            for (float y = topLeft.y; y > bottomRight.y; y -= spread)
            {
                float x_use = x;
                //if (y % 2 == 0) x_use += (spread / 2);
                GameObject particleInstance = Instantiate(particle, new Vector3(x_use, y, 0), Quaternion.identity);
                Renderer renderer = particleInstance.GetComponent<Renderer>();
                Material m = renderer.material;
                m.color = color;
                /*
                if (y % 2 == 0)
                {
                    Renderer renderer = particleInstance.GetComponent<Renderer>();
                    Material m = renderer.material;
                    m.color = new Color(0, 1, 0);
                }*/
                particles.Add(particleInstance);
            }
        }

        //Initialize a list of nearest neigbors and distances to them
        for (int i = 0; i < particles.Count; i++)
        {
            for(int x = -1; x <= 1; x++)
            {
                ParticleController ctrl = particles[i].GetComponent<ParticleController>();
                for (int y = -1; y <= 1; y++)
                {
                    int idx = (i + (particlesPerRow * x) + y);
                    if (idx >= 0 && idx < (particles.Count) && idx != i)
                    {
                        ParticleController neighbor = particles[idx].GetComponent<ParticleController>();
                        ctrl.addNeighbor(neighbor, ctrl.getDistance(neighbor));
                    }
                }
            }
        }
    }
    
    public List<GameObject> getParticles()
    {
        return particles;
    }
}
