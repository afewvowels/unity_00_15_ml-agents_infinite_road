using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using System.Collections.Generic;

public class RoadSceneManager : MonoBehaviour
{
    public GameObject carAgent;
    public GameObject goalPrefab;
    public GameObject instantiatedGoal;
    public PathCreator pathCreator;
    public RoadMeshCreator roadMeshCreator;
    public GameObject checkpointPrefab;
    public List<GameObject> checkpoints;
    public GameObject roadRoot;

    public struct PathInfo
    {
        private int pathLength;
        private Vector3[] path;
        private bool horizontal;
        private bool vertical;
        private float horizontalDistance;
        private float verticalDistance;
        private float nodeMinDistance;
        private float nodeMaxDistance;
        private float roadMinWidth;
        private float roadMaxWidth;

        public PathInfo(bool horizontal, bool vertical, int pathLength, float horizontalDistance,
                        float verticalDistance, float nodeMinDistance, float nodeMaxDistance,
                        float roadMinWidth, float roadMaxWidth)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
            this.pathLength = pathLength;
            path = new Vector3[pathLength];
            this.horizontalDistance = horizontalDistance;
            this.verticalDistance = verticalDistance;
            this.nodeMinDistance = nodeMinDistance;
            this.nodeMaxDistance = nodeMaxDistance;
            this.roadMinWidth = roadMinWidth;
            this.roadMaxWidth = roadMaxWidth;
        }

        public PathInfo(PathInfo pathInfo)
        {
            pathLength = pathInfo.pathLength;
            path = pathInfo.path;
            horizontal = pathInfo.horizontal;
            vertical = pathInfo.vertical;
            horizontalDistance = pathInfo.horizontalDistance;
            verticalDistance = pathInfo.verticalDistance;
            nodeMinDistance = pathInfo.nodeMinDistance;
            nodeMaxDistance = pathInfo.nodeMaxDistance;
            roadMinWidth = pathInfo.roadMinWidth;
            roadMaxWidth = pathInfo.roadMaxWidth;
        }

        public void SetPathLength(int length)
        {
            pathLength = length;
        }

        public int GetPathLength()
        {
            return pathLength;
        }

        public void IncrementPathLength()
        {
            pathLength++;
        }

        public void GenerateNewPath()
        {
            path = new Vector3[pathLength];
        }

        public float GetNodeMin()
        {
            return nodeMinDistance;
        }

        public float GetNodeMax()
        {
            return nodeMaxDistance;
        }

        public Vector3[] GetPath()
        {
            return path;
        }

        public Vector3 GetPathNode(int index)
        {
            return path[index];
        }

        public void SetPathNode(int index, Vector3 inNode)
        {
            path[index] = inNode;
        }

        public bool GetHorizontal()
        {
            return horizontal;
        }

        public bool GetVertical()
        {
            return vertical;
        }

        public float GetHorizontalDistance()
        {
            return horizontalDistance;
        }

        public float GetVerticalDistance()
        {
            return verticalDistance;
        }

        public float GetRandomNodeDistance()
        {
            return Random.Range(nodeMinDistance, nodeMaxDistance);
        }

        public float GetRandomRoadWidth()
        {
            return Random.Range(roadMinWidth, roadMaxWidth);
        }
    }

    private PathInfo pathInfo;

    private void Start()
    {

    }

    public void UpdateParameters(PathInfo pathInfo)
    {
        this.pathInfo = new PathInfo(pathInfo);
    }

    public void ResetPathLength()
    {
        pathInfo.SetPathLength(2);
    }

    public void IncrementPathLength()
    {
        pathInfo.IncrementPathLength();
    }

    public void GenerateNewPath()
    {
        if (instantiatedGoal != null)
        {
            Destroy(instantiatedGoal);
        }

        if (checkpoints.Count > 0)
        {
            foreach (GameObject checkpoint in checkpoints)
            {
                Destroy(checkpoint);
            }
        }

        checkpoints = new List<GameObject>();

        pathInfo.GenerateNewPath();

        float newZ = 0.0f;

        float randomizeOffsetX = Random.value < 0.5f ? 1.0f : -1.0f;

        for (int i = 0; i < pathInfo.GetPathLength(); i++)
        {
            float newX = 0.0f;
            float newY = 0.0f;

            if (pathInfo.GetHorizontal())
            {
                if (i % 2 == 0)
                {
                    newX = Random.Range(2.0f, pathInfo.GetHorizontalDistance()) * randomizeOffsetX;
                }
                else
                {
                    newX = Random.Range(-pathInfo.GetHorizontalDistance(), -2.0f) * randomizeOffsetX;
                }
            }

            if (pathInfo.GetVertical())
            {
                newY = Random.Range(-pathInfo.GetVerticalDistance(), pathInfo.GetVerticalDistance());
            }

            pathInfo.SetPathNode(i, new Vector3(newX, newY, newZ));
            newZ += pathInfo.GetRandomNodeDistance();
        }

        pathCreator.bezierPath = new BezierPath(transform.position);
        pathCreator.bezierPath = GeneratePath(pathInfo.GetPath());
        roadMeshCreator.flattenSurface = true;
        roadMeshCreator.roadWidth = pathInfo.GetRandomRoadWidth();
        // for(int i = 0; i < roadRoot.transform.childCount; i++)
        // {
        //     Destroy(roadRoot.transform.GetChild(i).gameObject);
        // }
        roadMeshCreator.TriggerUpdate();
        instantiatedGoal = (GameObject)Instantiate(goalPrefab);
        instantiatedGoal.transform.SetParent(transform, false);
        instantiatedGoal.GetComponent<BoxCollider>().size = new Vector3(roadMeshCreator.roadWidth + 5.0f, 4.0f, 5.0f);
        instantiatedGoal.transform.localRotation = pathCreator.path.GetRotationAtDistance(pathCreator.path.length);
        instantiatedGoal.transform.Rotate(0.0f, 0.0f, 90.0f);
        int furthestPoint = pathCreator.bezierPath.NumPoints - 1;
        instantiatedGoal.transform.localPosition = pathCreator.bezierPath.GetPoint(furthestPoint);
        carAgent.transform.localPosition = pathCreator.bezierPath.GetPoint(0);
        carAgent.transform.localRotation = pathCreator.path.GetRotation(0);
        carAgent.transform.Rotate(0.0f, 0.0f, 90.0f);
        carAgent.transform.position += transform.forward * 3.0f;
        carAgent.GetComponent<CarAgent>().goal = instantiatedGoal;
        // carAgent.transform.position += transform.TransformDirection(Vector3.up);
        carAgent.GetComponent<Rigidbody>().isKinematic = false;
        carAgent.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        roadRoot.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.0f, (float)pathInfo.GetPathLength() * 2.0f);
        roadRoot.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        for (int i = 0; i < roadRoot.transform.GetChild(0).gameObject.GetComponents<MeshCollider>().Length; i++)
        {
            Destroy(roadRoot.transform.GetChild(0).gameObject.GetComponents<MeshCollider>()[0]);
        }

        roadRoot.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
        roadRoot.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().sharedMesh =
            roadRoot.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;

        // roadRoot.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().sharedMesh = roadRoot.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().sharedMesh;
        // transform.GetChild(2).GetComponent<MeshCollider>().convex = true;

        float dst = 0.0f;
        float increment = 25.0f;

        while (dst < (pathCreator.path.length - increment))
        {
            if (dst < increment)
            {
                dst += increment;
                continue;
            }

            GameObject checkpoint = (GameObject)Instantiate(checkpointPrefab);
            checkpoint.transform.SetParent(transform, false);
            checkpoint.transform.position = pathCreator.path.GetPointAtDistance(dst);
            checkpoint.transform.localRotation = pathCreator.path.GetRotationAtDistance(dst);
            checkpoint.transform.Rotate(0.0f, 0.0f, 90.0f);
            checkpoints.Add(checkpoint);
            dst += increment;
        }
    }

    BezierPath GeneratePath(Vector3[] path)
    {
        BezierPath bPath = new BezierPath(path, false, PathSpace.xyz);

        return bPath;
    }
}
