using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFuseData", menuName = "InventoryUI/New Fuse Data")]
public class FuseData : ScriptableObject
{
    [SerializeField] private VoidItem fuseItem1;
    [SerializeField] private VoidItem fuseItem2;
    [SerializeField] private VoidItem resultItem;

    public void Fuse()
    {
        if (fuseItem1 != null && fuseItem2 != null && resultItem != null)
        {
            // check have fuseItem1 and fuseItem2

            // remove fuseItem1
            // remove fuseItem2
            // add resultItem
        }
    }
}
