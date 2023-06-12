using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using TMPro;

public class MeasureLine : MonoBehaviour
{
    [SerializeField]
    InputActionProperty measure;
    [SerializeField]
    InputActionProperty clearMeasures;
    [SerializeField]
    InputActionProperty haptic;

    [SerializeField]
    GameObject measureSpherePrefab;
    [SerializeField]
    GameObject measureLinePrefab;
    [SerializeField]
    GameObject measureTextPrefab;

    GameObject firstMeasurePoint;
    GameObject secondMeasurePoint;
    GameObject measureLine;

    float lastDistance;

    Vector3 measurePointOffsetFromController;

    int measurePointCounter;

    bool measuring;

    List<GameObject> listOfMeasurements;

    void OnEnable()
    {
        measure.action.Enable();
        clearMeasures.action.Enable();

        CreateFirstMeasurePoint();
    }

    void OnDisable()
    {
        measure.action.Disable();
        clearMeasures.action.Disable();

        Destroy(firstMeasurePoint);

        if(measuring)
        {
            Destroy(measureLine);
            Destroy(secondMeasurePoint);

            measuring = false;
        }
    }

    void Awake()
    {
        listOfMeasurements = new List<GameObject>();
        measurePointOffsetFromController = new Vector3(0, -0.02f, 0.05f);
        measurePointCounter = 0;
        lastDistance = 0f;
        measuring = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        measure.action.started += ctx => LineMeasureAction();

        clearMeasures.action.started += ctx =>
        {
            ClearAllMeasurements();
            CreateFirstMeasurePoint();
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (measuring)
        {
            LineRenderer measureLineLineRenderer = measureLine.GetComponent<LineRenderer>();
            measureLineLineRenderer.SetPosition(1, secondMeasurePoint.transform.position);

            float distance = (secondMeasurePoint.transform.position - firstMeasurePoint.transform.position).magnitude;

            if (Mathf.Abs(distance - lastDistance) > 0.01f)
            {
                UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse(haptic.action, 0.1f, 100f + distance * 400f, 0.025f, XRController.rightHand);
                lastDistance = distance;
            }
        }
    }

    public void LineMeasureAction()
    {
        if (!measuring)
        {
            LockFirstMeasurePoint();
            CreateSecondMeasurePoint();
            CreateConnectingLine();
        }
        else
        {
            LockSecondMeasurePoint();
            LockConnectingLine();
            CreateFirstMeasurePoint();
        }
    }

    void CreateFirstMeasurePoint()
    {
        firstMeasurePoint = Instantiate(measureSpherePrefab);//, new Vector3(0, 0, 0), Quaternion.identity);
        listOfMeasurements.Add(firstMeasurePoint);
        firstMeasurePoint.name = "Measure Point " + measurePointCounter++;
        firstMeasurePoint.transform.SetParent(transform);
        firstMeasurePoint.transform.localPosition = measurePointOffsetFromController;
    }

    void CreateSecondMeasurePoint()
    {
        //spawn second measure point
        secondMeasurePoint = Instantiate(measureSpherePrefab);
        listOfMeasurements.Add(secondMeasurePoint);
        secondMeasurePoint.name = "Measure Point " + measurePointCounter++;
        GameObject controller = this.gameObject;
        secondMeasurePoint.transform.SetParent(controller.transform);
        secondMeasurePoint.transform.localPosition = measurePointOffsetFromController;
    }

    void LockFirstMeasurePoint()
    {
        firstMeasurePoint.transform.SetParent(null, true);

        measuring = true;
    }

    void LockSecondMeasurePoint()
    {
        secondMeasurePoint.transform.SetParent(null, true);

        measuring = false;
    }

    void CreateConnectingLine()
    {
        measureLine = Instantiate(measureLinePrefab);
        measureLine.name = "Measure Line (" + (measurePointCounter - 2) + " to " + (measurePointCounter - 1) + ")";
        LineRenderer measureLineLineRenderer = measureLine.GetComponent<LineRenderer>();
        measureLineLineRenderer.SetPosition(0, firstMeasurePoint.transform.position);
        measureLineLineRenderer.SetPosition(1, secondMeasurePoint.transform.position);
    }

    void LockConnectingLine()
    {
        //remove lights from points (otherwise too many of them slow the app down after just a few measurements)
        Destroy(firstMeasurePoint.transform.Find("MeasureSphereLight").gameObject);
        Destroy(secondMeasurePoint.transform.Find("MeasureSphereLight").gameObject);

        //spawn measure line label
        float distance = (secondMeasurePoint.transform.position - firstMeasurePoint.transform.position).magnitude;
        Vector3 midpoint = (secondMeasurePoint.transform.position + firstMeasurePoint.transform.position) / 2;

        GameObject measureText = Instantiate(measureTextPrefab, midpoint, Quaternion.identity);
        measureText.transform.position = midpoint + new Vector3(0, 0.01f, 0);

        TextMeshPro thisText = measureText.GetComponent<TextMeshPro>();
        thisText.text = (distance * 1000).ToString("0.00") + "mm";

        //add to measurments list
        listOfMeasurements.Add(measureLine);
        listOfMeasurements.Add(measureText);
    }

    public void ClearAllMeasurements()
    {
        if (measuring)
            return;

        foreach (GameObject aMeasurement in listOfMeasurements)
        {
            Destroy(aMeasurement);
        }
    }
}
