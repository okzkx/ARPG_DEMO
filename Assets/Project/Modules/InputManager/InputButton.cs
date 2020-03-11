using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class InputButton
{
    private Func<bool> downFunction;
    private Func<bool> upFunction;

    public bool Down {
        get {
            bool value = downFunction();
            if (value)
            {
                Hold = true;
            }
            return value;
        }
    }
    public bool Up {
        get {
            bool value = upFunction();
            if (value)
            {
                Hold = false;
            }
            return value;
        }
    }
    public bool Hold;
    public bool Click;
    public bool DoubleClick;

    private bool Holding;
    private bool PreHolding;
    private float timer;

    public InputButton(Func<bool> upFunction, Func<bool> downFunction)
    {
        this.upFunction = upFunction;
        this.downFunction = downFunction;
    }
}
