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

        List<List<GameObject>> particleMatrix = new List<List<GameObject>>();
        int particlesPerRow = 0;
        for (float y = topLeft.y; y > bottomRight.y; y -= spread)
        {
            List<GameObject> rowParticles = new List<GameObject>();
            for (float x = topLeft.x; x < bottomRight.x; x += spread)
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
                rowParticles.Add(particleInstance);
                if(y == topLeft.y)
                    particlesPerRow++;
            }
            particleMatrix.Add(rowParticles);
        }

        //Initialize a list of nearest neigbors and distances to them
        for (int i = 0; i < particleMatrix.Count; i++)
        {
            for (int j = 0; j < particleMatrix[i].Count; j++)
            {
                List<ParticleController> addedNeighbors = new List<ParticleController>();
                for (int x = -3; x <= 3; x++)
                {
                    ParticleController ctrl = particleMatrix[i][j].GetComponent<ParticleController>();
                    for (int y = -3; y <= 3; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        if (i + x >= 0 && i + x < particleMatrix.Count && j + y >= 0 && j+y < particleMatrix[i].Count)
                        {
                            ParticleController neighbor = particleMatrix[i + x][j + y].GetComponent<ParticleController>();
                            if (!addedNeighbors.Contains(neighbor))
                            {
                                ctrl.addNeighbor(neighbor, ctrl.getDistance(neighbor));
                                addedNeighbors.Add(neighbor);
                            }
                        }
                    }
                }
                if (i == 0)
                {
                    Debug.Log(addedNeighbors.Count);
                    Debug.Log("Our position: " + particleMatrix[i][j].GetComponent<ParticleController>().getCenter());
                    foreach (ParticleController neighboringParticle in addedNeighbors)
                    {
                        Debug.Log("Neighbor " + neighboringParticle.getCenter());
                    }

                }
                particles.Add(particleMatrix[i][j]);
            }
        }
    }
    
    public List<GameObject> getParticles()
    {
        return particles;
    }
}
