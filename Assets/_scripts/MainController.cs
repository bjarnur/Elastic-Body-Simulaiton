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
