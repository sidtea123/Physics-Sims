using UnityEngine;

[System.Serializable]
public struct GravityObject
{
    public Vector3 r;
    public Vector3 v;
    public float m;
    public float radius;
    public Color color;
}

public class GravityManager : MonoBehaviour
{
    public GravityObject[] objects;
    public float G = 0.0003f;
    private GameObject[] gameObjects;

    void Start()
    {
        InitObjects();
    }

    void Update()
    {
        UpdateVelocities();
        UpdatePositions();
    }
    
    void InitObjects()
    {
        gameObjects = new GameObject[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.position = objects[i].r;
            obj.transform.localScale *= objects[i].radius;
            obj.GetComponent<Renderer>().material.color = objects[i].color;

            gameObjects[i] = obj;
        }
    }

    void UpdateVelocities()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            Vector3 a = Vector3.zero;

            for (int j = 0; j < objects.Length; j++)
            {
                if (j != i)
                {
                    // commnents are for people with two hands
                    Vector3 dir = objects[j].r - objects[i].r;
                    a += G * objects[j].m * dir.normalized / dir.sqrMagnitude;
                }
            }

            objects[i].v += a * Time.deltaTime;
        }
    }
    
    void UpdatePositions()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].r += objects[i].v * Time.deltaTime;

            gameObjects[i].transform.position = objects[i].r;
        }
    }
}
