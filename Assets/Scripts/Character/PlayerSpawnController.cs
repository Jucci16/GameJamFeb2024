using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using System.Collections;

public class PlayerSpawnController : NetworkBehaviour
{
    void Start()
    {
        // Set initial spawn position
        ResetPlayerPosition(true);
    }

    public IEnumerator StartPlayerRespawnTimer() {
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

    private void ResetPlayerPosition(bool isInitialSpawn = false) {
        if(IsOwner) {
            System.Random r = new System.Random();
            int spawnIndex = isInitialSpawn ? (int)OwnerClientId : r.Next(0, MultiplayTestSceneManager.playerSpawnPositions.Count);
            var spawnPosition = MultiplayTestSceneManager.playerSpawnPositions[spawnIndex];
            var targetAngle = TransformUtils.GetYRotFromVec(new Vector2(0f,0f), new Vector2(spawnPosition.x, spawnPosition.z));
            gameObject.transform.position = spawnPosition;
            gameObject.transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            gameObject.GetComponent<TestPlayerController>().resetYRotation(targetAngle);
            if(!isInitialSpawn) {
                RespawnCountdown.Instance.StartRespawnCountdown();
            }
        }
    }

    // Probably can be done better but this was the only way I could successfully hide the body without causing the camera to fall due to gravity. 
    // Also, if you just set the entire "playerVisual" to inactive/active, it will jump across the screen and looks really bad (because some children have NetworkTransforms). Disabling the individual MeshRenderers worked though.
    public void UpdatePlayerDisplayState(PlayerDisplayState state) {
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
