using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class IProjectile : NetworkBehaviour
{
    public string originPlayerId;

    public void SetOriginPlayerId(string shotBy) {
        originPlayerId = shotBy;
    }
}
