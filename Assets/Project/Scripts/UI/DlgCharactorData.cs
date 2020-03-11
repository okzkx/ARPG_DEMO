using OKZKX.UnityTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DlgCharactorData : DlgBase
{
    [AutoSet("")] Slider HPSlider;

    public void SetHpSlider(float percent)
    {
        HPSlider.value = percent;
    }
}
