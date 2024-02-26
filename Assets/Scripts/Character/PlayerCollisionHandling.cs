using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerCollisionHandling : NetworkBehaviour
{   
    [SerializeField]
    // Used for testing purposes only.
    public string overridePlayerId = null;

    [SerializeField]
    private GameObject _explosionPrefab; 

    private PlayerData _playerData;

    void Start()
    {
        _playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
    }

    void OnTriggerEnter(Collider projectile)
    {
        if(IsOwner) {
            if(projectile.gameObject.tag == ("Projectile"))
            {
                var projectileObject = projectile.gameObject.GetComponent<IProjectile>();
                // Only take action if the bullet was not shot by the current player
                string playerId = string.IsNullOrEmpty(overridePlayerId) ? _playerData.PlayerId.ToString() : overridePlayerId;
                if(projectileObject.originPlayerId != playerId) {
                    KaboomServerRpc(projectile.gameObject);
                }
            }
        }
    }

    [ServerRpc]
    public void KaboomServerRpc(NetworkObjectReference projectile) {
        ((GameObject) projectile).GetComponent<NetworkObject>().Despawn();
        var explosion = Instantiate(_explosionPrefab, transform.position, transform.rotation);
        explosion.GetComponent<NetworkObject>().Spawn();

        // Destroy player object and respawn
        PlayerDeathClientRpc();
    }

    [ClientRpc]
    private void PlayerDeathClientRpc() {
        // remove the object's rigidbody so the camera is locked in place
        var rigidBody = gameObject.GetComponent<Rigidbody>();
        Destroy(rigidBody);

        // remove the player visual from the object, since they exploded.
        var playerVisual = gameObject.transform.GetChild(1).gameObject;
        Destroy(playerVisual);
    }
}
