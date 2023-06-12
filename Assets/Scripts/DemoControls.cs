using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DemoControls : MonoBehaviour
{
    [SerializeField]
    GameObject coralModel;

    [SerializeField]
    Annotate annotateScript;
    [SerializeField]
    MeasureLine measureLineScript;
    [SerializeField]
    Flashlight flashlightScript;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 coralPos = Vector3.zero;

        if (PlayerPrefs.HasKey($"{SceneManager.GetActiveScene().name} Side Position"))
            coralPos.x = PlayerPrefs.GetFloat($"{SceneManager.GetActiveScene().name} Side Position");

        if (PlayerPrefs.HasKey($"{SceneManager.GetActiveScene().name} Forward Position"))
            coralPos.z = PlayerPrefs.GetFloat($"{SceneManager.GetActiveScene().name} Forward Position");

        coralModel.transform.position = coralPos;
    }

    // Update is called once per frame
    void Update()
    {
        bool shiftPressed = Keyboard.current.shiftKey.isPressed;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            coralModel.transform.position += Vector3.right * (shiftPressed ? 0.1f : 0.01f);
            PlayerPrefs.SetFloat($"{SceneManager.GetActiveScene().name} Side Position", coralModel.transform.position.x);
        }
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            coralModel.transform.position += Vector3.left * (shiftPressed ? 0.1f : 0.01f);
            PlayerPrefs.SetFloat($"{SceneManager.GetActiveScene().name} Side Position", coralModel.transform.position.x);
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            coralModel.transform.position += Vector3.forward * (shiftPressed ? 0.1f : 0.01f);
            PlayerPrefs.SetFloat($"{SceneManager.GetActiveScene().name} Forward Position", coralModel.transform.position.z);
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            coralModel.transform.position += Vector3.back * (shiftPressed ? 0.1f : 0.01f);
            PlayerPrefs.SetFloat($"{SceneManager.GetActiveScene().name} Forward Position", coralModel.transform.position.z);
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            StartCoroutine(LoadNewScene("Coral Head"));
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            StartCoroutine(LoadNewScene("Coral Reef"));
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            annotateScript.ClearAnnotations();
            annotateScript.enabled = false;
            measureLineScript.ClearAllMeasurements();
            measureLineScript.enabled = false;
            flashlightScript.enabled = false;

            if (shiftPressed)
                coralModel.transform.position = Vector3.zero;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            measureLineScript.enabled = false;
            flashlightScript.enabled = false;

            annotateScript.enabled = !annotateScript.enabled;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            annotateScript.enabled = false;
            measureLineScript.enabled = false;

            flashlightScript.enabled = !flashlightScript.enabled;
        }

        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            annotateScript.enabled = false;
            flashlightScript.enabled = false;

            measureLineScript.enabled = !measureLineScript.enabled;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Application.Quit();
    }

    IEnumerator LoadNewScene(string sceneName)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone) 
        {
            yield return null;
        }
    }
}
