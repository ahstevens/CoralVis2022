using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Flashlight : MonoBehaviour
{
    [SerializeField]
    InputActionProperty toggleFlashlight;
    [SerializeField]
    InputActionProperty hapticAction;

    bool lightIsOn;

    // Start is called before the first frame update
    void Start()
    {
        toggleFlashlight.action.started += ctx => ToggleLight();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        toggleFlashlight.action.Enable();

        ShowBulb();
        LightOff();
    }

    void OnDisable()
    {
        toggleFlashlight.action.Disable();

        HideBulb();
        LightOff();
    }

    public void ShowBulb()
    {
        GetComponent<MeshRenderer>().enabled = true;
    }

    public void HideBulb()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void LightOn()
    {
        GetComponent<Light>().enabled = true;

        GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.white);

        UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse(hapticAction.action, 0.5f, 500f, 0.025f, XRController.rightHand); 

        lightIsOn = true;
    }

    public void LightOff()
    {
        GetComponent<Light>().enabled = false;

        GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
        GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);

        UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse(hapticAction.action, 0.25f, 400f, 0.01f, XRController.rightHand);

        lightIsOn = false;
    }

    public void ToggleLight()
    {
        if(lightIsOn)
            LightOff();
        else
            LightOn();
    }
}
