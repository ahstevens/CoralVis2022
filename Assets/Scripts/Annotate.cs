using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Annotate : MonoBehaviour
{
    [SerializeField]
    PieMenuManager pieMenu;

    [SerializeField]
    InputActionProperty annotate;
    [SerializeField]
    InputActionProperty changeColor;
    [SerializeField]
    InputActionProperty clearAnnotations;

    bool annotating;

    [SerializeField]
    GameObject coralModel;
    int numCoralMaterials;
    Material[] coralMaterials;
    Texture2D[] coralTexturesUndos;
    Texture2D[] coralTexturesPainted;
    bool[] coralTexturesUndosModified;

    List<bool[]> circleArrayList = new List<bool[]>();

    [SerializeField]
    GameObject paintball;

    [SerializeField]
    GameObject paintLinePrefab;
    GameObject paintLine;
    LineRenderer paintLineLineRenderer;

    Quaternion sprayAngle1;
    Quaternion sprayAngle2;
    Quaternion sprayAngle3;
    Quaternion sprayAngle4;

    Color[] displaySprayColors = new[] {   
        new Color(0.42f, 0.24f, 0.60f),
        new Color(0.89f, 0.10f, 0.11f),
        new Color(1.00f, 0.50f, 0.00f),
        new Color(0.12f, 0.47f, 0.71f),
        new Color(0.20f, 0.63f, 0.17f),
        new Color(0.98f, 0.60f, 0.60f),
        new Color(0.99f, 0.75f, 0.44f),
        new Color(0.79f, 0.70f, 0.84f),
        new Color(0.70f, 0.87f, 0.54f),
        new Color(1.00f, 1.00f, 0.60f),
        new Color(0.69f, 0.35f, 0.16f),
        new Color(0.65f, 0.81f, 0.89f)
    };

    int currentSprayColorIndex;

    [SerializeField]
    int paintTextureWidth = 512;
    [SerializeField]
    int paintTextureHeight = 512;

    private void OnEnable()
    {
        annotate.action.Enable();
        changeColor.action.Enable();
        clearAnnotations.action.Enable();

        pieMenu.ShowAnnotationMenuText();

        paintball.GetComponent<MeshRenderer>().enabled = true;

        InitializeSprayWidget();
    }

    private void OnDisable()
    {
        annotate.action.Disable();
        changeColor.action.Disable();
        clearAnnotations.action.Disable();

        pieMenu.ShowMainMenuText();

        annotating = false;

        paintball.GetComponent<MeshRenderer>().enabled = false;
        Destroy(paintLine);
    }

    // Start is called before the first frame update
    void Start()
    {
        annotate.action.started += ctx => 
        {
            //save textures for undo later
            for (int i = 0; i < numCoralMaterials; i++)
            {
                coralTexturesUndosModified[i] = false;
            }

            annotating = true;
        };

        annotate.action.canceled += ctx =>
        {
            annotating = false;
        };

        changeColor.action.started += ctx => IncrementColor();

        clearAnnotations.action.started += ctx => ClearAnnotations();

        currentSprayColorIndex = 0;

        sprayAngle1 = Quaternion.AngleAxis(5, Vector3.forward);
        sprayAngle2 = Quaternion.AngleAxis(-5, Vector3.forward);
        sprayAngle3 = Quaternion.AngleAxis(5, Vector3.up);
        sprayAngle4 = Quaternion.AngleAxis(-5, Vector3.up);

        //create circular structuring elements
        //for each radius
        for (int thisWidth = 1; thisWidth < 100; thisWidth++)
        {
            bool[] thisArrayOfBools = new bool[thisWidth * thisWidth];
            float centerCoordXY = ((float)thisWidth) / 2.0f;
            for (int thisX = 0; thisX < thisWidth; thisX++)
            {
                for (int thisY = 0; thisY < thisWidth; thisY++)
                {
                    if (Mathf.Sqrt((((float)thisX - centerCoordXY) * ((float)thisX - centerCoordXY)) + (((float)thisY - centerCoordXY) * ((float)thisY - centerCoordXY))) < centerCoordXY)
                        thisArrayOfBools[(thisY * thisWidth) + thisX] = true;
                    else
                        thisArrayOfBools[(thisY * thisWidth) + thisX] = false;
                }
            }
            circleArrayList.Add(thisArrayOfBools);
        }

        coralMaterials = coralModel.GetComponent<MeshRenderer>().materials;
        numCoralMaterials = coralMaterials.Length;

        coralTexturesUndosModified = new bool[numCoralMaterials];
        coralTexturesUndos = new Texture2D[numCoralMaterials];

        for (int i = 0; i < numCoralMaterials; i++)
            coralTexturesUndos[i] = new Texture2D(paintTextureWidth, paintTextureHeight, TextureFormat.RGB24, false);

        coralTexturesPainted = new Texture2D[numCoralMaterials];

        for (int i = 0; i < numCoralMaterials; i++)
            coralTexturesPainted[i] = new Texture2D(paintTextureWidth, paintTextureHeight, TextureFormat.RGB24, false);

        ClearAnnotations();

        Debug.Log("Loaded " + numCoralMaterials + " textures from coral model.");
    }

    // Update is called once per frame
    void Update()
    {
        paintLineLineRenderer.SetPosition(0, transform.position);
        paintLineLineRenderer.SetPosition(1, transform.position + transform.forward);

        paintLineLineRenderer.SetPosition(2, transform.position);
        paintLineLineRenderer.SetPosition(3, transform.position + (sprayAngle1 * transform.forward));

        paintLineLineRenderer.SetPosition(4, transform.position);
        paintLineLineRenderer.SetPosition(5, transform.position + (sprayAngle2 * transform.forward));

        paintLineLineRenderer.SetPosition(6, transform.position);
        paintLineLineRenderer.SetPosition(7, transform.position + (sprayAngle3 * transform.forward));

        paintLineLineRenderer.SetPosition(8, transform.position);
        paintLineLineRenderer.SetPosition(9, transform.position + (sprayAngle4 * transform.forward));


        //if trigger down
        if (annotating)
        {
            bool[] textureModified = new bool[numCoralMaterials];

            for (int i = 0; i < numCoralMaterials; i++)
                textureModified[i] = false;

            for (int sprayRay = 0; sprayRay < 1; sprayRay++)
            {
                //calculate hit on model
                Vector3 sprayVector;
                if (sprayRay == 0)
                    sprayVector = transform.forward;
                else if (sprayRay == 1)
                    sprayVector = (sprayAngle1 * transform.forward);
                else if (sprayRay == 2)
                    sprayVector = (sprayAngle2 * transform.forward);
                else if (sprayRay == 3)
                    sprayVector = (sprayAngle3 * transform.forward);
                else
                    sprayVector = (sprayAngle4 * transform.forward);


                if (!Physics.Raycast(transform.position, sprayVector, out RaycastHit hit))
                {
                    Debug.Log("No hit on spray ray " + sprayRay);
                    continue;
                }
                Debug.Log("Hit on spray ray " + sprayRay);


                Renderer rend = hit.transform.GetComponent<Renderer>();
                MeshCollider meshCollider = hit.collider as MeshCollider;

                if (rend == null)
                {
                    Debug.Log("failed 1");
                    continue;
                }

                if (rend.sharedMaterial == null)
                {
                    Debug.Log("failed 2");
                    continue;
                }

                if (meshCollider == null)
                {
                    Debug.Log("failed 4");
                    continue;
                }

                Debug.Log("passed");

                Mesh hitSharedMesh = meshCollider.sharedMesh;

                //figure out which submesh was hit

                // There are 3 indices stored per triangle
                int limit = hit.triangleIndex * 3;
                int submesh;
                //bool maxxedout = false;
                for (submesh = 0; submesh < hitSharedMesh.subMeshCount; submesh++)
                {
                    //Debug.Log("Submesh #" + submesh + " has first last index: " + hitSharedMesh.GetTriangles(submesh)[0] + " "+ hitSharedMesh.GetTriangles(submesh)[hitSharedMesh.GetTriangles(submesh).Length-1]);
                    int numIndices = hitSharedMesh.GetTriangles(submesh).Length;
                    if (numIndices > limit)
                    {
                        //maxxedout = true;
                        break;
                    }

                    limit -= numIndices;
                }

                Vector2 pixelUV = hit.textureCoord;

                pixelUV.x *= coralTexturesPainted[submesh].width;
                pixelUV.y *= coralTexturesPainted[submesh].height;

                //save undo if needed:
                if (!coralTexturesUndosModified[submesh]) //if havent modified this submesh texture yet, save an undo of it
                {
                    coralTexturesUndos[submesh].SetPixels(coralTexturesPainted[submesh].GetPixels());
                    //prob dont need an apply() call here
                    coralTexturesUndosModified[submesh] = true;
                }


                //new circular splat
                int kernelSize;
                if (hit.distance < 0.02)
                    kernelSize = 0;
                else
                    kernelSize = (int)((hit.distance - 0.02f) / 0.01f);

                if (kernelSize > circleArrayList.Count)
                    kernelSize = circleArrayList.Count - 1;

                int splatWidth = Mathf.FloorToInt(Mathf.Sqrt(circleArrayList[kernelSize].Length));
                int halfKernelSize = splatWidth / 2;

                for (int thisX = 0; thisX < splatWidth; thisX++)
                {
                    for (int thisY = 0; thisY < splatWidth; thisY++)
                    {
                        if (circleArrayList[kernelSize][(thisY * splatWidth) + thisX])
                        {
                            int thisXinTex = ((int)pixelUV.x - halfKernelSize + thisX);
                            int thisYinTex = ((int)pixelUV.y - halfKernelSize + thisY);
                            if (thisXinTex >= 0 && thisXinTex < coralTexturesPainted[submesh].width)
                                if (thisYinTex >= 0 && thisYinTex < coralTexturesPainted[submesh].height)
                                    coralTexturesPainted[submesh].SetPixel(thisXinTex, thisYinTex, displaySprayColors[currentSprayColorIndex], 0); //works for square elements
                        }
                    }
                }

                textureModified[submesh] = true;

            }//end for each spray ray

            for (int i = 0; i < numCoralMaterials; i++)
            {
                if (textureModified[i])
                    coralTexturesPainted[i].Apply();
            }
        }
    }

    public void InitializeSprayWidget()
    {
        currentSprayColorIndex = 0;
        Color currentSprayColor = displaySprayColors[currentSprayColorIndex];

        paintball.GetComponent<MeshRenderer>().material.SetColor("_Color", currentSprayColor);
        paintball.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", currentSprayColor);

        paintLine = Instantiate(paintLinePrefab);
        paintLine.name = "Paint Line";
        paintLineLineRenderer = paintLine.GetComponent<LineRenderer>();
        paintLineLineRenderer.positionCount = 10;

        paintLineLineRenderer.material.SetColor("_Color", currentSprayColor);
        paintLineLineRenderer.material.SetColor("_EmissionColor", currentSprayColor);
    }

    public void IncrementColor()
    {
        currentSprayColorIndex = (currentSprayColorIndex + 1) % displaySprayColors.Length;
        
        Color newColor = displaySprayColors[currentSprayColorIndex];

        paintball.GetComponent<MeshRenderer>().material.SetColor("_Color", newColor);
        paintball.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", newColor);

        paintLineLineRenderer.material.SetColor("_Color", newColor);
        paintLineLineRenderer.material.SetColor("_EmissionColor", newColor);
    }

    public void UndoAnnotation()
    {
        //undo last spray
        for (int i = 0; i < numCoralMaterials; i++)
        {
            if (coralTexturesUndosModified[i]) //if have modified this submesh texture
            {
                coralTexturesPainted[i].SetPixels32(coralTexturesUndos[i].GetPixels32());
                coralTexturesPainted[i].Apply();
            }
        }
    }

    void ShowAnnotations()
    {
        for (int i = 0; i < numCoralMaterials; i++)
        {
            coralMaterials[i].EnableKeyword("_EMISSION");
            coralMaterials[i].SetColor("_EmissionColor", new Color(0.66f, 0.66f, 0.66f, 0.66f));

            coralModel.GetComponent<MeshRenderer>().UpdateGIMaterials();
        }
    }

    void HideAnnotations()
    {
        for (int i = 0; i < numCoralMaterials; i++)
        {
            coralMaterials[i].DisableKeyword("_EMISSION");
            coralMaterials[i].SetColor("_EmissionColor", Color.black);

            coralModel.GetComponent<MeshRenderer>().UpdateGIMaterials();
        }
    }

    public void ClearAnnotations()
    {
        Color[] blackArray = new Color[paintTextureWidth * paintTextureHeight];

        for (int j = 0; j < paintTextureWidth * paintTextureHeight; j++)
        {
            blackArray[j].r = 0.0f;
            blackArray[j].g = 0.0f;
            blackArray[j].b = 0.0f;
        }

        for (int i = 0; i < numCoralMaterials; i++)
        {
            coralTexturesPainted[i].SetPixels(0, 0, paintTextureWidth, paintTextureHeight, blackArray, 0);
            coralTexturesPainted[i].Apply();

            coralMaterials[i].EnableKeyword("_EMISSION");
            coralMaterials[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            coralMaterials[i].SetTexture("_EmissionMap", coralTexturesPainted[i]);
            coralMaterials[i].SetColor("_EmissionColor", new Color(0.66f, 0.66f, 0.66f, 0.66f));

            coralModel.GetComponent<MeshRenderer>().UpdateGIMaterials();
        }
    }

    public void SaveAnnotations()
    {
        var dirPath = Application.dataPath + "/Annotated";

        if (!System.IO.Directory.Exists(dirPath))
            System.IO.Directory.CreateDirectory(dirPath);

        for (int i = 0; i < numCoralMaterials; i++)
        {
            byte[] bytes = coralTexturesPainted[i].EncodeToPNG();

            System.IO.File.WriteAllBytes(dirPath + "/model_medium" + (i == 0 ? "" : i.ToString()) + ".png", bytes);

            Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
        }
    }
}
