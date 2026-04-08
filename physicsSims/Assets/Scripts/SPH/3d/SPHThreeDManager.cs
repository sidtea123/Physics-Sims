using UnityEngine;

public class SPHThreeDManager : MonoBehaviour
{
    public int resolution = 5;
    public SPHSettings sphSettings;
    public float spacing = 0.5f;
    public float radius = 0.2f;
    public float maxX;
    public float maxY;
    public float maxZ;
    public ComputeShader computeShader;
    private SPHThreeDController sphController;
    private ParticleSystem.Particle[] _particleBuffer;
    public ParticleSystem partSystem;
    public Color particleColor;

    void Start()
    {
        InitSimulation();
    }

    void Update()
    {
        TickFrame();
        RenderFrame();

        if (Input.GetKeyDown(KeyCode.R))
        {
            InitSimulation();
        }
    }

    void TickFrame()
    {
        sphController.TickSim();
    }

    void RenderFrame()
    {
        for (int i = 0; i < resolution * resolution * resolution; i++)
        {
            _particleBuffer[i].position = sphController.positions[i];
        }
        partSystem.SetParticles(_particleBuffer, resolution * resolution * resolution);
    }

    void InitSimulation()
    {
        _particleBuffer = new ParticleSystem.Particle[resolution * resolution * resolution];
        Vector3[] positions = new Vector3[resolution * resolution * resolution];
        float offset = -resolution / 2 * spacing;

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                for (int k = 0; k < resolution; k++)
                {
                    Vector3 pos = new Vector3(offset + j * spacing, offset + i * spacing, offset + k * spacing);
                    positions[i * resolution * resolution + k * resolution + j] = pos;

                    //var particle = _particleBuffer[i * rows + j];
                    ParticleSystem.Particle particle = new ParticleSystem.Particle();

                    particle.position = pos;
                    particle.startColor = particleColor;
                    particle.startSize = radius;

                    // Keep this particle alive forever.
                    particle.startLifetime = float.MaxValue;
                    particle.remainingLifetime = float.MaxValue;

                    _particleBuffer[i * resolution * resolution + j * resolution + k] = particle;
                }
            }
        }
        // sphController = new SPHController(particles, sphSettings, maxX, maxY);
        sphController = new SPHThreeDController(positions, sphSettings, computeShader, maxX, maxY, maxZ);
    }
}