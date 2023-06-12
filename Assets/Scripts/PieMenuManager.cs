using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using TMPro;

public class PieMenuManager : MonoBehaviour
{
    [SerializeField]
    InputActionProperty showMenu;
    [SerializeField]
    InputActionProperty navigate;
    [SerializeField]
    InputActionProperty select;
    [SerializeField]
    InputActionProperty haptic;

    public Color enabledColor;
    public Color disabledColor;

    [SerializeField]
    private MeshRenderer pieMenuPlaneMeshRenderer;

    [SerializeField]
    private GameObject pieMenu;

    [SerializeField]
    Texture defaultMenuTexture;
    [SerializeField]
    private Texture EightSliceNTexture;
    [SerializeField]
    private Texture EightSliceNETexture;
    [SerializeField]
    private Texture EightSliceETexture;
    [SerializeField]
    private Texture EightSliceSETexture;
    [SerializeField]
    private Texture EightSliceSTexture;
    [SerializeField]
    private Texture EightSliceSWTexture;
    [SerializeField]
    private Texture EightSliceWTexture;
    [SerializeField]
    private Texture EightSliceNWTexture;

    [SerializeField]
    private TextMeshPro PieTextN;
    [SerializeField]
    private TextMeshPro PieTextNE;
    [SerializeField]
    private TextMeshPro PieTextE;
    [SerializeField]
    private TextMeshPro PieTextSE;
    [SerializeField]
    private TextMeshPro PieTextS;
    [SerializeField]
    private TextMeshPro PieTextSW;
    [SerializeField]
    private TextMeshPro PieTextW;
    [SerializeField]
    private TextMeshPro PieTextNW;

    private bool selectionMade;

    [SerializeField]
    Annotate annotateScript;
    [SerializeField]
    MeasureLine measureLineScript;
    [SerializeField]
    Flashlight flashlightScript;

    enum PieSlice
    {
        None = 0,
        N,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW
    }

    PieSlice onSlice;

    private void OnEnable()
    {
        showMenu.action.Enable();
        navigate.action.Enable();
        select.action.Enable();
    }

    private void OnDisable()
    {
        showMenu.action.Disable();
        navigate.action.Disable();
        select.action.Disable();

        onSlice = PieSlice.None;
    }

    private void Awake()
    {
        selectionMade = false;
        onSlice = PieSlice.None;
    }

    // Start is called before the first frame update
    void Start()
    {
        showMenu.action.started += ctx => ShowMenu();
        showMenu.action.canceled += ctx => HideMenu();
        select.action.started += ctx => SelectMenuItem();

        ShowMainMenuText();

        pieMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!selectionMade)
        {
            PieSlice lastOnSlice = onSlice;
        
            //update selection
            float xPos = navigate.action.ReadValue<Vector2>().x;
            float yPos = navigate.action.ReadValue<Vector2>().y;
        
            //figure out which pie slice to highlight
            float angle = Mathf.Rad2Deg * Mathf.Atan2(yPos, xPos) + 180;
            //Debug.Log("x: " + xPos + " y: " +  yPos + " angle: " + angle + " ");

            if (Mathf.Abs(xPos) < 0.1f && Mathf.Abs(yPos) < 0.1f)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = defaultMenuTexture;
                onSlice = PieSlice.None;
            }
            else if (angle >= 337.5 || angle < 22.5)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = EightSliceWTexture;
                onSlice = PieSlice.W;
            }
            else if (angle >= 22.5 && angle < 67.5)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = EightSliceSWTexture;
                onSlice = PieSlice.SW;
            }
            else if (angle >= 67.5 && angle < 112.5)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = EightSliceSTexture;
                onSlice = PieSlice.S;
            }
            else if (angle >= 112.5 && angle < 157.5)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = EightSliceSETexture;
                onSlice = PieSlice.SE;
            }
            else if (angle >= 157.5 && angle < 202.5)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = EightSliceETexture;
                onSlice = PieSlice.E;
            }
            else if (angle >= 202.5 && angle < 247.5)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = EightSliceNETexture;
                onSlice = PieSlice.NE;
            }
            else if (angle >= 247.5 && angle < 292.5)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = EightSliceNTexture;
                onSlice = PieSlice.N;
            }
            else if (angle >= 292.5 && angle <= 337.5)
            {
                pieMenuPlaneMeshRenderer.material.mainTexture = EightSliceNWTexture;
                onSlice = PieSlice.NW;
            }
        
            //vibrate a bit if moved to another slice
            if (onSlice != lastOnSlice && onSlice != PieSlice.None)
            {
                HapticNudge();
            }
        }//end if trackpad touching
    }//end update()

    public void ShowMenu()
    {
        pieMenu.SetActive(true);
    }

    public void HideMenu()
    {
        pieMenu.SetActive(false);
        selectionMade = false;
    }

    void SelectMenuItem()
    {
        if (onSlice != PieSlice.None && !selectionMade)
        {
            //run menu command
            HideMenu();
            if (!annotateScript.enabled)
                DisableAllTools();

            selectionMade = true;

            //spray mode alternative menu
            if (annotateScript.enabled)
            {
                if (onSlice == PieSlice.SW)
                {
                    annotateScript.ClearAnnotations();
                    HapticConfirm();
                }
                else if (onSlice == PieSlice.S)
                {
                    annotateScript.enabled = false;
                    ShowMainMenuText();
                    HapticConfirm();
                }
                else if (onSlice == PieSlice.SE)
                {
                    annotateScript.IncrementColor();
                    HapticConfirm();
                }

                return;
            }

            //Normal Menu
            if (onSlice == PieSlice.W)
            {
                measureLineScript.enabled = true;
                HapticConfirm();
            }
            else if (onSlice == PieSlice.S)
            {
                annotateScript.enabled = true;
                HapticConfirm();
            }
            else if (onSlice == PieSlice.E)
            {
                measureLineScript.ClearAllMeasurements();
                HapticConfirm();
            }
            else if (onSlice == PieSlice.N)
            {
                flashlightScript.enabled = !flashlightScript.enabled;
                HapticConfirm();
            }
        }
    }

    private void HapticNudge()
    {
        UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse(haptic.action, 0.25f, 200f, 0.025f, XRController.rightHand);
    }

    private void HapticConfirm()
    {
        UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse(haptic.action, 0.5f, 250f, 0.05f, XRController.rightHand);
    }

    private void DisableAllTools()
    {
        annotateScript.enabled = false;
        measureLineScript.enabled = false;
        flashlightScript.enabled = false;
    }

    public void ShowMainMenuText()
    {
        PieTextE.SetText("Clear\nMeasures");
        PieTextE.color = enabledColor;

        PieTextNE.text = "SaveTags";
        PieTextNE.color = disabledColor;

        PieTextN.text = "Flashlight";
        PieTextN.color = enabledColor;

        PieTextNW.text = "Tag Species";
        PieTextNW.color = disabledColor;

        PieTextW.text = "Measure\nDistance";
        PieTextW.color = enabledColor;

        PieTextSW.text = "Measure\nVolume";
        PieTextSW.color = disabledColor;

        PieTextS.text = "Spray\nAnnotate";
        PieTextS.color = enabledColor;

        PieTextSE.text = "Toggle\nLens";
        PieTextSE.color = disabledColor;
    }

    public void ShowAnnotationMenuText()
    {
        PieTextE.text = "Tags";
        PieTextE.color = disabledColor;

        PieTextNE.text = "Dead\nCoral";
        PieTextNE.color = disabledColor;

        PieTextN.text = "Brain\nCoral";
        PieTextN.color = disabledColor;

        PieTextNW.text = "Rock";
        PieTextNW.color = disabledColor;

        PieTextW.text = "Sand";
        PieTextW.color = disabledColor;

        PieTextSW.text = "Clear";
        PieTextSW.color = enabledColor;

        PieTextS.text = "End\nSpray\nAnnotate";
        PieTextS.color = enabledColor;

        PieTextSE.text = "Next\nColor";
        PieTextSE.color = enabledColor;
    }
}
