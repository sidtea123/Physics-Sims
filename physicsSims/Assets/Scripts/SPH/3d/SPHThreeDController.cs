using UnityEngine;

public class SPHThreeDController
{
    public Vector3[] positions;
    public Vector3[] velocities;
    public Vector3[] accelerations;
    public float[] densities;
    private SPHSettings sph;
    private float maxX;
    private float maxY;
    private float maxZ;
    private ComputeBuffer _positionBuffer;
    private ComputeBuffer _velocityBuffer;
    private ComputeBuffer _accelerationBuffer;
    private ComputeBuffer _densityBuffer;
    private ComputeShader sphComputeShader;
    private int numParts;
    private int resolution;
    private int accelerationsKernel;
    private int densitiesKernel;

    public SPHThreeDController(Vector3[] positions, SPHSettings sphSettings, ComputeShader computeShader, float maxX, float maxY, float maxZ)
    {
        this.positions = positions;
        numParts = positions.Length;
        resolution = (int)Mathf.Pow(numParts, 1.0f/3.0f);
        velocities = new Vector3[numParts];
        accelerations = new Vector3[numParts];
        densities = new float[numParts];
        sph = sphSettings;
        this.maxX = maxX;
        this.maxY = maxY;
        this.maxZ = maxZ;
        sphComputeShader = computeShader;
        InitComputeBuffer();
    }

    public void TickSim()
    {
        CalculateDensities();
        int groups = Mathf.CeilToInt(resolution * resolution / 8f);
        sphComputeShader.Dispatch(accelerationsKernel, groups, groups, 1);
        _accelerationBuffer.GetData(accelerations);

        UpdateVelocitiesAndPositions();
        sphComputeShader.SetFloat("_dt", Time.deltaTime);
    }

    public void UpdateVelocitiesAndPositions()
    {
        for (int i = 0; i < resolution * resolution * resolution; i++)
        {
            velocities[i] += accelerations[i] * Time.deltaTime;
            BoundaryCheck(i);
            positions[i] += velocities[i] * Time.deltaTime;
        }

        _velocityBuffer.SetData(velocities);
        _positionBuffer.SetData(positions);
    }

    void BoundaryCheck(int i)
    {
        Vector3 rNew = positions[i] + velocities[i] * Time.deltaTime;
        if (Mathf.Abs(rNew.x) >= maxX)
        {
            velocities[i].x *= -sph.restitution;
        }
        else if (Mathf.Abs(rNew.y) >= maxY)
        {
            velocities[i].y *= -sph.restitution;
        }
        else if (Mathf.Abs(rNew.z) >= maxZ)
        {
            velocities[i].z *= -sph.restitution;
        }
    }

    public void CalculateDensities()
    {
        int groups = Mathf.CeilToInt(resolution * resolution / 8f);
        sphComputeShader.Dispatch(densitiesKernel, groups, groups, 1);
        _densityBuffer.GetData(densities);
    }

    private void InitComputeBuffer()
    {
        accelerationsKernel = sphComputeShader.FindKernel("CalcAccelerations");
        densitiesKernel = sphComputeShader.FindKernel("CalcDensities");

        _positionBuffer = new ComputeBuffer(numParts, sizeof(float) * 3);
        _velocityBuffer = new ComputeBuffer(numParts, sizeof(float) * 3);
        _accelerationBuffer = new ComputeBuffer(numParts, sizeof(float) * 3);
        _densityBuffer = new ComputeBuffer(numParts, sizeof(float) * 1);

        sphComputeShader.SetBuffer(accelerationsKernel, "_positions", _positionBuffer);
        sphComputeShader.SetBuffer(accelerationsKernel, "_velocities", _velocityBuffer);
        sphComputeShader.SetBuffer(accelerationsKernel, "_densities", _densityBuffer);
        sphComputeShader.SetBuffer(accelerationsKernel, "_accelerations", _accelerationBuffer);

        sphComputeShader.SetBuffer(densitiesKernel, "_positions", _positionBuffer);
        sphComputeShader.SetBuffer(densitiesKernel, "_densities", _densityBuffer);

        _positionBuffer.SetData(positions);
        _velocityBuffer.SetData(velocities);
        _accelerationBuffer.SetData(accelerations);
        _densityBuffer.SetData(densities);

        sphComputeShader.SetInt("resolution", resolution);
        sphComputeShader.SetFloat("g", sph.g);
        sphComputeShader.SetFloat("h", sph.h);
        sphComputeShader.SetFloat("rho0", sph.rho0);
        sphComputeShader.SetFloat("k", sph.k);
        sphComputeShader.SetFloat("m", sph.m);
        sphComputeShader.SetFloat("mu", sph.mu);
        sphComputeShader.SetFloat("WPoly6Factor", 4 / (Mathf.PI * Mathf.Pow(sph.h, 8)));
        sphComputeShader.SetFloat("GradientWSpikyFactor", 30 / (Mathf.PI * Mathf.Pow(sph.h, 5)));
        sphComputeShader.SetFloat("_dt", Time.deltaTime);
    }
}
