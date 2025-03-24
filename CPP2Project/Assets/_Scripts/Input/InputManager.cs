using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InputManager : Singleton<InputManager>
{
    public PlayerController controller;
    private ThirdPersonInputs inputs;

    protected override void Awake()
    {
        base.Awake();
        inputs = new ThirdPersonInputs();
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable called.");
        Debug.Log($"Inputs: {inputs != null}, Controller: {controller != null}");
        if (inputs != null)
        {
            inputs.Enable();
            if (controller != null)
                inputs.Overworld.SetCallbacks(controller);
        }
    }

    private void OnDisable()
    {
        if (inputs != null)
        {
            inputs.Disable();
            if (controller != null)
                inputs.Overworld.RemoveCallbacks(controller);
        }
    }
}





