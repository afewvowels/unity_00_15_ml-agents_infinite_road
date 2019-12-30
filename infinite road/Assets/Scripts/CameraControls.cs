using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    List<RoadSceneManager> sceneManagers;
    int activeIndex;
    bool isParentedToAgent;

    private void Start()
    {
        isParentedToAgent = false;
        activeIndex = 0;
        sceneManagers = new List<RoadSceneManager>();

        foreach (GameObject scene in GameObject.FindGameObjectsWithTag("carscene"))
        {
            sceneManagers.Add(scene.GetComponent<RoadSceneManager>());
        }

        ChangeActiveScene();
        isParentedToAgent = !isParentedToAgent;
    }

    private void Update()
    {
        if (isParentedToAgent)
        {
            transform.position = sceneManagers[activeIndex].carAgent.transform.position;
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.right * 5.0f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.right * 5.0f;
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.position += transform.forward * 5.0f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.position -= transform.forward * 5.0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeActiveScene();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isParentedToAgent = !isParentedToAgent;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0.0f, 5.0f, 0.0f);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0.0f, -5.0f, 0.0f);
        }
    }

    private void ChangeActiveScene()
    {
        activeIndex++;
        if (activeIndex >= sceneManagers.Count)
        {
            activeIndex = 0;
        }
        transform.position = sceneManagers[activeIndex].transform.position;
    }
}