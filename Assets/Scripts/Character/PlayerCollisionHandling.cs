using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCollisionHandling : NetworkBehaviour
{ 
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
                if(!projectile.gameObject.GetComponent<NetworkObject>().IsOwner) {
                    KaboomServerRpc(projectile.gameObject);
                }
            }
        }
    }

    [ServerRpc]
    public void KaboomServerRpc(NetworkObjectReference projectile) {
        ((GameObject) projectile).GetComponent<NetworkObject>().Despawn();

        // Destroy player object and respawn
        PlayerDeathClientRpc();
    }

    [ClientRpc]
    private void PlayerDeathClientRpc() {
        Instantiate(_explosionPrefab, transform.position, transform.rotation);

        var spawnController = gameObject.GetComponent<PlayerSpawnController>();
        spawnController.UpdatePlayerDisplayState(PlayerDisplayState.hide);

        // Start respawn timer
        StartCoroutine(spawnController.StartPlayerRespawnTimer());
    }
}
