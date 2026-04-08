using TMPro;
using UnityEngine;

public class SPHManager : MonoBehaviour
{
    public int rows = 3;
    public int columns = 3;
    public SPHSettings sphSettings;
    public float spacing = 0.5f;
    public float radius = 0.2f;
    public float maxX;
    public float maxY;
    public ComputeShader computeShader;
    //private SPHController sphController;
    private SPHComputeController sphController;
    private ParticleSystem.Particle[] _particleBuffer;
    public ParticleSystem partSystem;
    public GameObject circlePrefab;

    private bool debugMode = false;
    public Color particleColor;
    public TMP_Text simModeText;
    public TMP_Text gameSettings;
    public float inputDst;
    public float inputStrength;
    float fps = 60.0f;

    void Start()
    {
        InitSimulation();
    }

    void Update()
    {
        UpdateFPS();
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSimulation();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            debugMode = !debugMode;
            if (debugMode)
            {
                simModeText.text = "debug mode";
            }
            else
            {
                simModeText.text = "realtime mode";
            }
        }

        if (debugMode)
        {
            DebugUpdate();
        }
        else
        {
            RealtimeUpdate();
        }
    }

    void DebugUpdate()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TickFrame();
        }
        DrawAccelerations();
    }

    void RealtimeUpdate()
    {
        TickFrame();
    }

    void TickFrame()
    {
        sphController.TickSim();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                _particleBuffer[i * rows + j].position = sphController.positions[i * rows + j];
            }
        }
        partSystem.SetParticles(_particleBuffer, rows * columns);
    }

    void DrawAccelerations()
    {
        for (int i = 0; i < rows * columns; i++)
        {
            Debug.DrawRay(sphController.positions[i], sphController.accelerations[i], Color.green);
        }
    }

    void ResetSimulation()
    {
        InitSimulation();
    }

    void UpdateFPS()
    {
        float newFPS = 1.0f / Time.deltaTime;
        fps = Mathf.Lerp(fps, newFPS, 0.0005f);

        gameSettings.text = "particles: " + (rows * columns) + "\nfps: " + newFPS;
    }

    void InitSimulation()
    {
        _particleBuffer = new ParticleSystem.Particle[rows * columns];
        Vector2[] positions = new Vector2[rows * columns];
        float rx = -rows / 2 * spacing;
        float ry = -columns / 2 * spacing;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 pos = new Vector2(rx + j * spacing, ry + i * spacing);
                positions[i * rows + j] = pos;

                //var particle = _particleBuffer[i * rows + j];
                ParticleSystem.Particle particle = new ParticleSystem.Particle();

                particle.position = pos;
                particle.startColor = particleColor;
                particle.startSize = radius;

                // Keep this particle alive forever.
                particle.startLifetime = float.MaxValue;
                particle.remainingLifetime = float.MaxValue;

                _particleBuffer[i * rows + j] = particle;
            }
        }
        // sphController = new SPHController(particles, sphSettings, maxX, maxY);
        sphController = new SPHComputeController(positions, sphSettings, computeShader, maxX, maxY);
        sphController.inputDst = inputDst;
        sphController.inputStrength = inputStrength;
    }
}
