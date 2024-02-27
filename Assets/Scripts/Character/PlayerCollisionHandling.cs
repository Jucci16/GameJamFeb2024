using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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
        UpdatePlayerDisplayState(PlayerDisplayState.hide);

        // Start respawn timer
        StartCoroutine(StartPlayerRespawnTimer());
    }

    private IEnumerator StartPlayerRespawnTimer() {
        yield return new WaitForSeconds(2);
        bool isGameOver = false;
        // Decrement the life counter
        if(IsOwner) isGameOver = MatchUIManager.instance.DecrementLifeCount();
        // If a Game Over occurred for the current player, enable the new camera and despawn the player from the server.
        if(isGameOver) {
            MultiplayerManager.Instance.PlayerGameOverServerRpc(gameObject);
            MultiplayTestSceneManager.Instance.EnableSpectatorCamera();
        } 
        // If no Game Over occurred, reset the player position and respawn after some time.
        else {
            ResetPlayerPosition();
            yield return new WaitForSeconds(RespawnCountdown.respawnTimeSeconds);
            UpdatePlayerDisplayState(PlayerDisplayState.show);
        }
    }

    private void ResetPlayerPosition() {
        if(IsOwner) {
            System.Random r = new System.Random();
            var spawnPosition = MultiplayTestSceneManager.playerSpawnPositions[r.Next(0, MultiplayTestSceneManager.playerSpawnPositions.Count)];
            var targetAngle = TransformUtils.GetYRotFromVec(new Vector2(0f,0f), new Vector2(spawnPosition.x, spawnPosition.z));
            gameObject.transform.position = spawnPosition;
            gameObject.transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            gameObject.GetComponent<TestPlayerController>().resetYRotation(targetAngle);
            RespawnCountdown.Instance.StartRespawnCountdown();
        }
    }

    // Probably can be done better but this was the only way I could successfully hide the body without causing the camera to fall due to gravity. 
    // Also, if you just set the entire "playerVisual" to inactive/active, it will jump across the screen and looks really bad (because some children have NetworkTransforms). Disabling the individual MeshRenderers worked though.
    private void UpdatePlayerDisplayState(PlayerDisplayState state) {
        gameObject.GetComponent<TestPlayerController>().state = state;
        var rigidBody = gameObject.GetComponent<Rigidbody>();
        var playerVisual = gameObject.transform.GetChild(1).gameObject;
        switch(state) {
            case PlayerDisplayState.show:
                rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidBody.isKinematic = false;
                rigidBody.detectCollisions = true;
                playerVisual.transform.GetChild(0).gameObject.SetActive(true);
                playerVisual.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
                playerVisual.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
                playerVisual.transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                break;
            case PlayerDisplayState.hide:
                rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rigidBody.isKinematic = true;
                rigidBody.detectCollisions = false;
                playerVisual.transform.GetChild(0).gameObject.SetActive(false);
                playerVisual.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
                playerVisual.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
                playerVisual.transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                break;
            default:
                break;
        }
    }
}
