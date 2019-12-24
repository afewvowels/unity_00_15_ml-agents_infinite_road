using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{
    private CarAcademy carAcademy;
    public RoadSceneManager sceneManager;
    private Rigidbody rb;
    private BoxCollider bc;
    private float forceMultiplier;

    private void Start()
    {
        forceMultiplier = 500.0f;
        carAcademy = FindObjectOfType<CarAcademy>();
        bc = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -10.0f)
        {
            AddReward(-0.5f);
            Done();
        }
    }
    
    public override void AgentAction(float[] vectorAction)
    {
        var moveAction = 0.0f;
        var turnAction = 0.0f;

        if (IsOnRoad())
        {
            moveAction = Mathf.FloorToInt(vectorAction[0]);
            turnAction = Mathf.FloorToInt(vectorAction[1]);
        }

        switch (moveAction)
        {
            case 1:
                rb.AddForce(transform.forward * forceMultiplier, ForceMode.Force);
                break;
            case 2:
                rb.AddForce(transform.forward * forceMultiplier * 1.5f, ForceMode.Force);
                break;
            case 3:
                rb.AddForce(transform.forward * -forceMultiplier, ForceMode.Force);
                break;
        }

        if (rb.velocity.magnitude > 0.1f)
        {
            switch (turnAction)
            {
                case 1:
                    rb.AddTorque(transform.up * forceMultiplier * 0.5f, ForceMode.Force);
                    break;
                case 2:
                    rb.AddTorque(transform.up * -forceMultiplier * 0.5f, ForceMode.Force);
                    break;
            }
        }
    }

    public override float[] Heuristic()
    {
        float[] action = new float[2];

        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1.0f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            action[0] = 3.0f;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            action[0] = 2.0f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            action[1] = 2.0f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            action[1] = 1.0f;
        }

        return action;
    }

    public override void AgentReset()
    {
        transform.localRotation = Quaternion.identity;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        bool horizontal = carAcademy.resetParameters["horizontal"] == 1.0f ? true : false;
        bool vertical = carAcademy.resetParameters["vertical"] == 1.0f ? true : false;
        int pathLength = Random.Range((int)carAcademy.resetParameters["minLength"], (int)carAcademy.resetParameters["maxLength"]);
        float horizontalDistance = carAcademy.resetParameters["hDist"];
        float verticalDistance = carAcademy.resetParameters["vDist"];
        float nodeMinDistance = carAcademy.resetParameters["nMin"];
        float nodeMaxDistance = carAcademy.resetParameters["nMax"];
        float roadMinWidth = carAcademy.resetParameters["rMin"];
        float roadMaxWidth = carAcademy.resetParameters["rMax"];

        RoadSceneManager.PathInfo pathInfo = new RoadSceneManager.PathInfo(horizontal, vertical, pathLength,
                                                                            horizontalDistance, verticalDistance, nodeMinDistance,
                                                                            nodeMaxDistance, roadMinWidth, roadMaxWidth);

        sceneManager.UpdateParameters(pathInfo);
        sceneManager.GenerateNewPath();
    }

    private bool IsOnRoad()
    {
        Debug.DrawRay(bc.bounds.center, -transform.up * 0.4f, Color.magenta);
        return Physics.Raycast(bc.bounds.center, -transform.up, 0.4f);
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.CompareTag("checkpoint"))
        {
            AddReward(0.1f);
            Destroy(c.gameObject);
        }

        if (c.gameObject.CompareTag("goal"))
        {
            AddReward(1.0f);
            Done();
        }
    }
}