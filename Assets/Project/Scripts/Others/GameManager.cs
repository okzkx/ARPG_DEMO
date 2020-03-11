using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static DlgManager DlgManager;

    private void Awake()
    {
        DlgManager = new DlgManager();
        
    }

    void Start()
    {
        DlgManager.ResourcePath = "UIPanel/";
        DlgManager.Switch<DlgCharactorData>();
    }
}
