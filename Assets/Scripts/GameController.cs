using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


public class GameController : MonoBehaviour
{
    public GameObject gameCamera;
    public Volume volume;
    public GameObject cityGenerator;
    public GameObject streetGenerator;
    public GameObject parkGenerator;
    public GameObject factoryGenerator;
    public GameObject wallGenerator;

    private InputAction generateActions;
    private InputActions actions;

    private GeneratorMode generatorMode = GeneratorMode.City;

    private float cityDistance = 150f;
    private float streetDistance = 75f;
    private float parkDistance = 75f;
    private float factoryDistance = 75f;
    private float wallDistance = 50f;

    private DepthOfField dof;
    private Fog fog;

    private void Awake()
    {
        actions = new InputActions();

        volume.profile.TryGet<DepthOfField>(out dof);
        volume.profile.TryGet<Fog>(out fog);
    }

    private void OnEnable()
    {
        generateActions = actions.Generation.Generate;
        generateActions.Enable();

        actions.Generation.Generate.performed += Generate;
        actions.Generation.Generate.Enable();

        actions.Generation.Clear.performed += Clear;
        actions.Generation.Clear.Enable();

        actions.Generation.Generate_City.performed += SwitchGenerationMode;
        actions.Generation.Generate_City.Enable();
        actions.Generation.Generate_Street.performed += SwitchGenerationMode;
        actions.Generation.Generate_Street.Enable();
        actions.Generation.Generate_Park.performed += SwitchGenerationMode;
        actions.Generation.Generate_Park.Enable();
        actions.Generation.Generate_Factory.performed += SwitchGenerationMode;
        actions.Generation.Generate_Factory.Enable();
        actions.Generation.Generate_Wall.performed += SwitchGenerationMode;
        actions.Generation.Generate_Wall.Enable();

        actions.Generation.Quit.performed += ctx => Application.Quit();
        actions.Generation.Quit.Enable();
    }

    private void OnDisable()
    {
        actions.Generation.Generate.Disable();
        actions.Generation.Clear.Disable();

        actions.Generation.Generate_City.Disable();
        actions.Generation.Generate_Street.Disable();
        actions.Generation.Generate_Park.Disable();
        actions.Generation.Generate_Factory.Disable();
        actions.Generation.Generate_Wall.Disable();

        actions.Generation.Quit.Disable();

        generateActions.Disable();
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        gameCamera.transform.LookAt(transform);
    }

    private void SwitchGenerationMode(InputAction.CallbackContext context)
    {
        Clear(generatorMode);

        switch(context.action.name)
        {
            case "Generate_City":
                generatorMode = GeneratorMode.City;
                dof.nearFocusEnd.value = 80f;
                fog.meanFreePath.value = 5f;
                transform.position = Vector3.zero;
                break;
            case "Generate_Street":
                generatorMode = GeneratorMode.Street;
                dof.nearFocusEnd.value = 25f;
                fog.meanFreePath.value = 25f;
                transform.position = new Vector3(0f, -20f, 0f);
                break;
            case "Generate_Park":
                generatorMode = GeneratorMode.Park;
                dof.nearFocusEnd.value = 25f;
                fog.meanFreePath.value = 25f;
                transform.position = new Vector3(0f, -20f, 0f);
                break;
            case "Generate_Factory":
                generatorMode = GeneratorMode.Factory;
                dof.nearFocusEnd.value = 0f;
                fog.meanFreePath.value = 25f;
                transform.position = new Vector3(0f, 10f, 0f);
                break;
            case "Generate_Wall":
                generatorMode = GeneratorMode.Wall;
                dof.nearFocusEnd.value = 0f;
                fog.meanFreePath.value = 25f;
                transform.position = new Vector3(0f, 10f, 0f);
                break;
            default:
                generatorMode = GeneratorMode.City;
                Debug.Log("Default with context name: " + context.action.name);
                break;
        }

        SetCameraDistance(generatorMode);
        Generate(generatorMode);
    }

    private void Generate(InputAction.CallbackContext context)
    {
        Generate(generatorMode);
    }

    private void Clear(InputAction.CallbackContext context)
    {
        Clear(generatorMode);
    }

    private void Generate(GeneratorMode mode)
    {
        switch (mode)
        {
            case GeneratorMode.City:
                cityGenerator.GetComponent<GenerateCity>().Generate();
                break;
            case GeneratorMode.Street:
                streetGenerator.GetComponent<GenerateStreet>().Generate();
                break;
            case GeneratorMode.Park:
                parkGenerator.GetComponent<GeneratePark>().Generate();
                break;
            case GeneratorMode.Factory:
                factoryGenerator.GetComponent<GenerateFactory>().Generate();
                break;
            case GeneratorMode.Wall:
                wallGenerator.GetComponent<GenerateWall>().Generate();
                break;
        }
    }

    private void Clear(GeneratorMode mode)
    {
        switch (mode)
        {
            case GeneratorMode.City:
                cityGenerator.GetComponent<GenerateCity>().Clear();
                break;
            case GeneratorMode.Street:
                streetGenerator.GetComponent<GenerateStreet>().Clear();
                break;
            case GeneratorMode.Park:
                parkGenerator.GetComponent<GeneratePark>().Clear();
                break;
            case GeneratorMode.Factory:
                factoryGenerator.GetComponent<GenerateFactory>().Clear();
                break;
            case GeneratorMode.Wall:
                wallGenerator.GetComponent<GenerateWall>().Clear();
                break;
        }
    }

    private void SetCameraDistance(GeneratorMode mode)
    {
        Vector3 angle = gameCamera.transform.position.normalized;

        switch (mode)
        {
            case GeneratorMode.City:
                gameCamera.transform.position = angle * cityDistance;
                break;
            case GeneratorMode.Street:
                gameCamera.transform.position = angle * streetDistance;
                break;
            case GeneratorMode.Park:
                gameCamera.transform.position = angle * parkDistance;
                break;
            case GeneratorMode.Factory:
                gameCamera.transform.position = angle * factoryDistance;
                break;
            case GeneratorMode.Wall:
                gameCamera.transform.position = angle * wallDistance;
                break;
        }
    }
}

public enum GeneratorMode
{
    City,
    Street,
    Park,
    Factory,
    Wall
}