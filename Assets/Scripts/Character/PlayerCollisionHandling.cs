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

        // Destroy player object and respawn
        PlayerDeathClientRpc();
    }

    [ClientRpc]
    private void PlayerDeathClientRpc() {
        Instantiate(_explosionPrefab, transform.position, transform.rotation);
        UpdatePlayerDisplayState(PlayerDisplayState.hide);

        // Start respawn timer
        StartCoroutine(StartPlayerRespawnTimer());
    }

    private IEnumerator StartPlayerRespawnTimer() {
        yield return new WaitForSeconds(3);
        RespawnPlayer();
    }

    private void RespawnPlayer() {
        gameObject.transform.position = new Vector3(0f, 2f, 0f);
        UpdatePlayerDisplayState(PlayerDisplayState.show);
    }

    private void UpdatePlayerDisplayState(PlayerDisplayState state) {
        var rigidBody = gameObject.GetComponent<Rigidbody>();
        var playerVisual = gameObject.transform.GetChild(1).gameObject;
        switch(state) {
            case PlayerDisplayState.show:
                rigidBody.isKinematic = false;
                playerVisual.SetActive(true);
                break;
            case PlayerDisplayState.hide:
                rigidBody.isKinematic = true;
                playerVisual.SetActive(false);
                break;
        }
    }
}

enum PlayerDisplayState {
    show,
    hide
}
