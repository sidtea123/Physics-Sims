using UnityEngine;

[CreateAssetMenu(fileName = "SPHSettings", menuName = "Scriptable Objects/SPHSettings")]
public class SPHSettings : ScriptableObject
{
    public float g = 1.2f;
    public float restitution = 0.8f;
    public float m = 1.2f;
    public float h = 0.6f;
    public float rho0 = 2.1f;
    public float k = 1.2f;
    public float mu = 0.5f;
}
