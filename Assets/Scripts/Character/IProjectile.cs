using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class IProjectile : MonoBehaviour
{
    public FixedString64Bytes originPlayerId;

    public void SetOriginPlayerId(FixedString64Bytes shotBy) {
        originPlayerId = shotBy;
    }
}
