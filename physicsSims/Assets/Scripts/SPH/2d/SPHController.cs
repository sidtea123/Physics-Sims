using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics;

public struct Particle2D
{
    public Vector2 r;
    public Vector2 v;
    public Vector2 a;
    public float rho;

    public Particle2D(Vector2 r)
    {
        this.r = r;
        v = Vector2.zero;
        rho = 0.0f;
        a = Vector2.zero;
    }
}

public class SPHController
{
    public Particle2D[] particles;
    private SPHSettings sph;
    private float maxX;
    private float maxY;
    private float WFactor;
    private float GradientWSpikyFactor;

    public SPHController(Particle2D[] particles, SPHSettings sphSettings, float maxX, float maxY)
    {
        this.particles = particles;
        this.maxX = maxX;
        this.maxY = maxY;
        sph = sphSettings;

        WFactor = 4 / (Mathf.PI * Mathf.Pow(sph.h, 8));
        GradientWSpikyFactor = 30 / (Mathf.PI * Mathf.Pow(sph.h, 5));
    }

    public void CalculateDensities()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            float sum = 0.0f;
            for (int j = 0; j < particles.Length; j++)
            {
                float sqrDst = Vector2.SqrMagnitude(particles[i].r - particles[j].r);
                sum += WPoly6(sqrDst);
            }
            particles[i].rho = sum * sph.m;
        }
    }
    
    public void UpdateAccelerations()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].a = FNet(i);
        }
    }

    public void UpdateVelocitiesAndPositions()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].v += particles[i].a / particles[i].rho * Time.deltaTime;
            BoundaryCheck(i);
            particles[i].r += particles[i].v * Time.deltaTime;
        }
    }

    Vector2 FNet(int i)
    {
        Vector2 sum = Vector2.zero;

        for (int j = 0; j < particles.Length; j++)
        {
            float dst = Vector2.Distance(particles[i].r, particles[j].r);
            if (dst >= sph.h || dst == 0)
            {
                continue;
            }
            
            //FPressure
            float pressureQuotient = (P(i) + P(j)) / (2 * particles[j].rho);
            sum -= pressureQuotient * GradientWSpiky(dst) * (particles[i].r - particles[j].r).normalized;

            //FViscosity
            Vector2 velocityTerm = (particles[j].v - particles[i].v) / particles[j].rho;
            sum += Laplacian(dst) * sph.mu * velocityTerm;
        }

        return sph.m * sum + Fg(i);
    }

    float P(int i)
    {
        return sph.k * (particles[i].rho - sph.rho0);
    }

    Vector2 Fg(int i)
    {
        return particles[i].rho * sph.g * Vector2.down;
    }

    float WPoly6(float sqrDst)
    {
        if (sqrDst < sph.h * sph.h)
        {
            return WFactor * Mathf.Pow(sph.h * sph.h - sqrDst, 3);
        }
        return 0;
    }

    float GradientWSpiky(float dst)
    {
        if (dst < sph.h)
        {
            return -GradientWSpikyFactor * Mathf.Pow(sph.h - dst, 2);
        }
        return 0;
    }

    float Laplacian(float dst)
    {
        if (dst < sph.h)
        {
            return 1.4f * (sph.h - dst);
        }
        return 0;
    }
    
    void BoundaryCheck(int i)
    {
        Vector2 rNew = particles[i].r + particles[i].v * Time.deltaTime;
        if (Mathf.Abs(rNew.x) >= maxX)
        {
            particles[i].v.x *= -sph.restitution;
        }
        else if (Mathf.Abs(rNew.y) >= maxY)
        {
            particles[i].v.y *= -sph.restitution;
        }
    }
}