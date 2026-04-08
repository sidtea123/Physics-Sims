using System;
using TMPro;
using UnityEngine;

public class Particle
{
    public Vector2 a;
    public Vector2 v;
    public Vector2 r;

    public Particle(Vector2 r)
    {
        this.a = Vector2.zero;
        this.v = Vector2.zero;
        this.r = r;
    }
}

public class Spring
{
    public Particle p1;
    public Particle p2;

    public Spring(Particle p1, Particle p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }
}

public class SoftbodyMananger : MonoBehaviour
{
    public int rows = 5;
    public float rest = 0.4f;
    public float restScale = 1.3f;
    public Color pointColor = Color.red;
    public float pointRadius = 0.1f;
    public float m;
    public float k;
    public float g = -0.5f;
    public float damping = 0.1f;
    public float pressureForce = 0.3f;
    public float restitution;
    private ParticleSystem.Particle[] _particleBuffer;
    public ParticleSystem partSystem;
    private Particle[] particles;
    private Spring[] springs;
    public GameObject[] colliders;
    public Camera cam;
    private Vector2 pAvg = Vector2.zero;
    private int numSprings;
    private float fps = 60.0f;
    public TMP_Text gameSettings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitParticles();
    }

    // Update is called once per frame
    void Update()
    {
        //ResetAccelerations();
        CalculateAccelerations();
        UpdateParticleSystem();

        UpdateFPS();
    }

    void UpdateFPS()
    {
        float newFPS = 1.0f / Time.deltaTime;
        fps = Mathf.Lerp(fps, newFPS, 0.0005f);

        gameSettings.text = "particles: " + (rows * rows) + "\nsprings: " + (4 * rows * rows - 6 * rows + 2) + "\nfps: " + (int)fps;
    }

    void InitParticles()
    {
        // four springs per particle,
        // three for first column
        // two for last column
        // one for last row
        numSprings = 4 * rows * rows - 6 * rows + 2;
    
        float spacing = rest * restScale;
        springs = new Spring[numSprings];
        particles = new Particle[rows * rows];
        _particleBuffer = new ParticleSystem.Particle[rows * rows];
        float rx = -rows / 2 * spacing;
        float ry = -rows / 2 * spacing;

        int si = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector2 pos = new Vector2(rx + j * spacing, ry + i * spacing);

                ParticleSystem.Particle particle = new ParticleSystem.Particle();

                particle.position = pos;
                particle.startColor = pointColor;
                particle.startSize = pointRadius;

                // Keep this particle alive forever.
                particle.startLifetime = float.MaxValue;
                particle.remainingLifetime = float.MaxValue;

                _particleBuffer[i * rows + j] = particle;
                particles[i * rows  + j] = new Particle(pos);
            }
        }

        // adds springs for each particle
        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                // bottom
                springs[si] = new Spring(particles[i * rows + j], particles[(i + 1) * rows + j]);
                si++;
                if (j == 0)
                {
                    // right
                    springs[si] = new Spring(particles[i * rows + j], particles[i * rows + j + 1]);
                    si++;
                    // bottom right
                    springs[si] = new Spring(particles[i * rows + j], particles[(i + 1) * rows + j + 1]);
                    si++;
                }
                else if (j != rows - 1)
                {
                    // right
                    springs[si] = new Spring(particles[i * rows + j], particles[i * rows + j + 1]);
                    si++;
                    // bottom right
                    springs[si] = new Spring(particles[i * rows + j], particles[(i + 1) * rows + j + 1]);
                    si++;
                    // bottom left
                    springs[si] = new Spring(particles[i * rows + j], particles[(i + 1) * rows + j - 1]);
                    si++;
                }
                else
                {
                    // bottom left
                    springs[si] = new Spring(particles[i * rows + j], particles[(i + 1) * rows + j - 1]);
                    si++;
                }
            }
        }

        // bottom row
        for (int k = 0; k < rows - 1; k++)
        {
            // right
            springs[si] = new Spring(particles[(rows - 1) * rows + k], particles[(rows - 1) * rows + k + 1]);
            si++;
        }
        Debug.Log(si);
        Debug.Log(numSprings);
    }

    void CalculateAccelerations()
    {
        Vector2 posSum = Vector2.zero;
        for (int i = 0; i < particles.Length; i++)
        {
            Particle p = particles[i];
            posSum += p.r;

            // resets force, gravity
            p.a = new Vector2(0, m * g);
            // damping force
            p.a += -damping * p.v;
            // internal pressure
            Vector2 dir = pAvg - p.r;
            p.a += -pressureForce * dir;
        }

        pAvg = posSum / particles.Length;

        for (int i = 0; i < springs.Length; i++)
        {
            Spring s = springs[i];
            Vector2 disp = s.p2.r - s.p1.r;
            float mag = disp.magnitude;
            float stretch = rest - mag;

            // spring forces
            s.p1.a += -k * stretch * disp.normalized;
            s.p2.a += k * stretch * disp.normalized;
        }
    }

    void UpdateParticleSystem()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Particle p = particles[i * rows + j];
                p.v += p.a * Time.deltaTime;

                Vector2 test = p.r + p.v * Time.deltaTime;
                for (int k = 0; k < colliders.Length; k++)
                {
                    if (colliders[k].GetComponent<Collider2D>().OverlapPoint(test))
                    {
                        //v_final = v_initial - 2x normal*(v_initial.normal)
                        float angle = (float)((90 + colliders[k].transform.eulerAngles.z) * Math.PI / 180);
                        Vector2 normal = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                        p.v -= 2 * (restitution * normal * p.v) * normal;
                    }
                }

                p.r += p.v * Time.deltaTime;
                _particleBuffer[i * rows + j].position = p.r;
                cam.transform.position = new Vector3(pAvg.x, pAvg.y, cam.transform.position.z);
            }
        }
        partSystem.SetParticles(_particleBuffer, rows * rows);
    }
}