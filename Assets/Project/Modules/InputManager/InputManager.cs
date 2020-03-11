using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    private static InputManager instance;
    public static InputManager Instance => instance = instance ?? new InputManager();

    const string up = "w";
    const string left = "a";
    const string down = "s";
    const string right = "d";

    const string space = "space";


    public Vector2 AxisRaw {
        get {
            Vector2 axis = new Vector2();
            axis.y = (Input.GetKey(up) ? 1 : 0) + (Input.GetKey(down) ? -1 : 0);
            axis.x = (Input.GetKey(right) ? 1 : 0) + (Input.GetKey(left) ? -1 : 0);
            return axis;
        }
    }
    private Vector2 axis = Vector2.zero;
    public Vector2 Axis => axis = Vector2.Lerp(axis, AxisRaw, 0.2f);

    public InputButton Space;

    public InputButton LeftMouse;

    InputManager()
    {
        LeftMouse = new InputButton(() => Input.GetMouseButtonDown(0), () => Input.GetMouseButtonUp(0));
        Space = new InputButton(() => Input.GetKeyDown(space), () => Input.GetKeyUp(space));
    }
}
