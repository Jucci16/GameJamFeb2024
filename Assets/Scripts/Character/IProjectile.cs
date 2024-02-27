using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class IProjectile : NetworkBehaviour
{
    public bool isDestroyed = false;

    public override void OnDestroy () {
        isDestroyed = true;
        base.OnDestroy();
    }
}
