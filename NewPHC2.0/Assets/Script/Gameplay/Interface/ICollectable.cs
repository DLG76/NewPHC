using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollectable
{
    void MoveTo(PlayerCombat obj);
    void Collect();
}
