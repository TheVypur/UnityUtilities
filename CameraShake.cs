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
    public float fTraumaFalloffRate = 15f;

    private float fSeed = 1;
    private float fTimeCounter = 0;

    public float fRotationPower = 8f;
    public float fRotationMag = 2f;

    public float fDampening = 0.2f;

    private Quaternion q4Rot;

    public bool bZooming = false;
    private float fZoomTimer = 0;
    private float fZoomMax = 0.3f;

    private float fBaseFoV = 80;
    private float fCurrentFoV = 80;
    private float fDynamicFoV = 92;
    private float fFovZoomed = 65;

    private float fADSZoomTime = 0;
    private float fADSZoomMax = 0.45f;
    int iZoomDirection = -1;
    public AnimationCurve acZoomCurve;
    public bool bADSZoom = false;

    public bool bDynamicMode = false;

    private SmoothFollow m_MainCam;
    public Vector3 v3DynamicOffset = new Vector3(0, 0, -1f);

    public void AddTrauma(float fVal)
    {
        fTrauma += fVal;
    }


    // Start is called before the first frame update
    void Start()
    {
        q4Rot = this.transform.localRotation;
        m_MainCam = GetComponentInParent<SmoothFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        fTimeCounter += Time.deltaTime;
        if (fTrauma > 0)
        {
            fTrauma -= fTraumaFalloffRate * Time.deltaTime;
            fTimeCounter += Time.deltaTime * fTrauma * fTrauma * fRotationPower;
            float fEffectStrength = fTrauma * fTrauma;
            float fPerlinX = ((Mathf.PerlinNoise(fSeed, fTimeCounter) * 2) - 1) * fRotationMag;
            float fPerlinY = ((Mathf.PerlinNoise(fSeed * 10, fTimeCounter) * 2) - 1) * fRotationMag;
            float fPerlinZ = ((Mathf.PerlinNoise(fSeed * 100, fTimeCounter) * 2) - 1) * fRotationMag;
            fPerlinZ *= 0.3f;

            Quaternion rot = Quaternion.Euler(fPerlinX * fRotationMag, fPerlinY * fRotationMag, fPerlinZ * fRotationMag);
            rot *= this.transform.localRotation;
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, rot, 0.6f);
        }
        else
        {
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, q4Rot, fDampening);
            fTrauma = 0;
        }

        CheckZoom();
    }

    void CheckZoom()
    {

        if (bADSZoom)
        {
            if (fADSZoomTime < fADSZoomMax)
            {
                fADSZoomTime += Time.deltaTime;

                if (iZoomDirection < 0)
                {
                    fCurrentFoV = fFovZoomed + ((1 - acZoomCurve.Evaluate(fADSZoomTime / fADSZoomMax)) * (fBaseFoV - fFovZoomed));
                }
                else
                {
                    fCurrentFoV = fFovZoomed + (acZoomCurve.Evaluate(fADSZoomTime / fADSZoomMax) * (fBaseFoV - fFovZoomed));
                }
                
            }
            else
            {
                fADSZoomTime = fADSZoomMax;
                if (iZoomDirection > 0)
                {
                    fCurrentFoV = fBaseFoV;
                }
                else
                {
                    fCurrentFoV = fFovZoomed;
                }
            }
            Camera.main.fieldOfView = fCurrentFoV;
        }

        if (bZooming)
        {
            fZoomTimer += Time.deltaTime;

            if (fZoomTimer > fZoomMax)
            {
                bZooming = false;
                fZoomTimer = 0;
                if (fCurrentFoV != fBaseFoV && !bADSZoom)
                {
                    fCurrentFoV = fBaseFoV;
                }
                Camera.main.fieldOfView = fCurrentFoV;
            }
            else
            {
                fCurrentFoV = Camera.main.fieldOfView;
                float fProgress = fZoomTimer / fZoomMax;

                float fFinalFoV = Mathf.Lerp(fBaseFoV + fFovZoomed, fBaseFoV, fProgress);
                Camera.main.fieldOfView = fFinalFoV;

            }
        }
        else if (bDynamicMode)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fDynamicFoV, 0.05f);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fBaseFoV, 0.05f);
        }
    }

    public void ADSZoom(float fZoomFoV = 30)
    {
        bADSZoom = true;
        fFovZoomed = fZoomFoV;
        iZoomDirection = -1;
        fADSZoomTime = 0;
    }

    public void ADSUnZoom()
    {
        bADSZoom = true;
        iZoomDirection = 1;
        fADSZoomTime = 0f;
    }

    public void Zoom(float fDuration, float fDesiredDelta = -10)
    {
        bZooming = true;
        fFovZoomed = fDesiredDelta;
        fZoomMax = fDuration;
        fZoomTimer = 0;
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

    public void SetDynamicMode()
    {
        bDynamicMode = true;
        m_MainCam.v3BasePivotOffset = v3DynamicOffset;
    }

    public void ClearDynamicMode()
    {
        bDynamicMode = false;
        m_MainCam.v3BasePivotOffset = Vector3.zero;
    }
}