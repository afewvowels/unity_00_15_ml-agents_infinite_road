using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{
    private CarAcademy carAcademy;
    public RoadSceneManagerVariant sceneManager;
    private Rigidbody rb;
    private BoxCollider bc;
    private float forceMultiplier;
    private int agentStepCount;
    public GameObject[] sensors;
    public GameObject goal;

    private void Start()
    {
        agentStepCount = 0;
        forceMultiplier = 500.0f;
        carAcademy = FindObjectOfType<CarAcademy>();
        bc = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
    }

    public override void AgentAction(float[] vectorAction)
    {
        agentStepCount++;

        if (transform.position.y < -10.0f || IsTippedOver())
        {
            AddReward(-0.25f);
            Done();
        }

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
                rb.AddForce(transform.forward * forceMultiplier * 2.0f, ForceMode.Force);
                break;
            case 3:
                rb.AddForce(transform.forward * -forceMultiplier * 1.5f, ForceMode.Force);
                break;
        }

        if (rb.velocity.magnitude > 0.1f)
        {
            switch (turnAction)
            {
                case 1:
                    rb.AddTorque(transform.up * forceMultiplier, ForceMode.Force);
                    break;
                case 2:
                    rb.AddTorque(transform.up * -forceMultiplier, ForceMode.Force);
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
        agentStepCount = 0;

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

        RoadSceneManagerVariant.PathInfo pathInfo = new RoadSceneManagerVariant.PathInfo(horizontal, vertical, pathLength,
                                                                            horizontalDistance, verticalDistance, nodeMinDistance,
                                                                            nodeMaxDistance, roadMinWidth, roadMaxWidth);

        sceneManager.UpdateParameters(pathInfo);
        sceneManager.GenerateNewPath();
    }

    public override void CollectObservations()
    {
        float distance = -9999.9f;
        if (goal)
        {
            distance = Vector3.Distance(goal.transform.position, transform.position);
        }
        Debug.Log("distance: " + distance.ToString());

        AddVectorObs(distance);

        RaycastHit hit;
        Vector3 castPosition = transform.position + transform.TransformDirection(Vector3.up);
        Vector3 castDirection = transform.TransformDirection(Vector3.forward);
        LayerMask hitLayer = 1 << 2;
        hitLayer = ~hitLayer;

        if (Physics.Raycast(castPosition, castDirection, out hit, 15.0f, hitLayer))
        {
            Debug.DrawRay(castPosition, castDirection * hit.distance, Color.magenta);
            AddVectorObs(hit.distance);
            Debug.Log("Raycast distance: " + hit.distance.ToString());
        }
        else
        {
            Debug.DrawRay(castPosition, castDirection * 5.0f, Color.cyan);
            AddVectorObs(-9999.9f);
        }
    }

    private bool IsTippedOver()
    {
        Transform sensor = sensors[0].transform;

        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.right), 0.15f))
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.right) * 0.15f, Color.red, 0.15f);
            return true;
        }
        else
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.right) * 0.15f, Color.green, 0.15f);
        }

        sensor = sensors[1].transform;

        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.left), 0.15f))
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.left) * 0.15f, Color.red, 0.15f);
            return true;
        }
        else
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.left) * 0.15f, Color.green, 0.15f);
        }

        sensor = sensors[2].transform;

        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.forward), 0.15f))
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.forward) * 0.15f, Color.red, 0.15f);
            return true;
        }
        else
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.forward) * 0.15f, Color.green, 0.15f);
        }

        sensor = sensors[3].transform;

        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.back), 0.15f))
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.back) * 0.15f, Color.red, 0.15f);
            return true;
        }
        else
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.back) * 0.15f, Color.green, 0.15f);
        }

        sensor = sensors[4].transform;

        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.up), 0.15f))
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.up) * 0.15f, Color.red, 0.15f);
            return true;
        }
        else
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.up) * 0.15f, Color.green, 0.15f);
        }

        return false;
    }

    private bool IsOnRoad()
    {
        for (int i = 0; i < sensors.Length; i++)
        {
            Transform sensor = sensors[i].transform;

            if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.down), 0.5f))
            {
                Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.down) * 0.5f, Color.green, 0.15f);
                return true;
            }
            else
            {
                Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.down) * 0.5f, Color.red, 0.15f);
            }
        }

        return false;
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
            float stepReward = (float)(15000 - agentStepCount) / 15000.0f;
            AddReward(stepReward);
            AddReward(1.0f);
            Done();
        }
    }
}
