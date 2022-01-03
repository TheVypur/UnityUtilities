using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Check to see if we're about to be destroyed.
    private static bool m_ShuttingDown = false;
    private static object m_Lock = new object();
    private static CameraShake m_Instance;

    public float fTrauma = 0;
    public float fTraumaFalloffRate = 2f;

    private float fSeed = 1;
    private float fTimeCounter = 0;

    public float fRotationPower = 5;
    public float fRotationMag = 5f;

    public float fDampening = 0.1f;

    private Quaternion q4Rot;

    public void AddTrauma(float fVal)
    {
        fTrauma += fVal;
        if (fTrauma > 1)
            fTrauma = 1;
    }


    // Start is called before the first frame update
    void Start()
    {
        q4Rot = this.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (fTrauma > 0)
        {
            fTrauma -= fTraumaFalloffRate * Time.deltaTime;
            fTimeCounter += Time.deltaTime * fTrauma * fTrauma * fRotationPower;
            float fEffectStrength = fTrauma * fTrauma;
            float fPerlinX = ((Mathf.PerlinNoise(fSeed, fTimeCounter) * 2) - 1) * fRotationMag;
            float fPerlinY = ((Mathf.PerlinNoise(fSeed * 10, fTimeCounter) * 2) -1) * fRotationMag;
            float fPerlinZ = ((Mathf.PerlinNoise(fSeed * 100, fTimeCounter) * 2) - 1) * fRotationMag;
            fPerlinZ *= 0.3f;

            Quaternion rot = Quaternion.Euler(fPerlinX, fPerlinY, fPerlinZ);
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, rot, fDampening);

        }
        else
        {
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, q4Rot, fDampening);
        }
    }

    public static CameraShake Instance
    {
        get
        {
            if (m_ShuttingDown)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(CameraShake) +
                    "' already destroyed. Returning null.");
                return null;
            }

            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    // Search for existing instance.
                    m_Instance = (CameraShake)FindObjectOfType(typeof(CameraShake));
                }

                return m_Instance;
            }
        }
    }


    private void OnApplicationQuit()
    {
        m_ShuttingDown = true;
    }


    private void OnDestroy()
    {
        m_ShuttingDown = true;
    }
}
