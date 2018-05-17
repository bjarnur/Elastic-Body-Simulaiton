using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour {

    PhysicsController physicsController;
    public List<GameObject> particles = new List<GameObject>();
    public List<GameObject> elasticBodies = new List<GameObject>();
    public List<GameObject> hardBodies = new List<GameObject>();

    void Start ()
    {
        //Alternative to using a prefab
        //Debug.DrawLine(new Vector3(-5, 5, 0), new Vector3(5, 5, 0), Color.white, 2);
        physicsController = new PhysicsController();
        physicsController.setParticles(particles);
        physicsController.setElasticBodies(elasticBodies);
        physicsController.setHardBodies(hardBodies);
	}

    void Update()
    {
        physicsController.DoUpdate();
    }
}
