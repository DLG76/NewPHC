using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{
    [TextArea(10, int.MaxValue)] public string description;
    [TextArea(10, int.MaxValue)] public string exampleOutput;
    //public string hint;
}
