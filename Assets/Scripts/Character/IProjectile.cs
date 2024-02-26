using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class IProjectile : NetworkBehaviour
{
    public string originPlayerId;
    public bool isDestroyed = false;

    public void SetOriginPlayerId(string shotBy) {
        originPlayerId = shotBy;
    }

    public override void OnDestroy () {
        isDestroyed = true;
        base.OnDestroy();
    }
}
