using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong ClientId;
    public int ColorId;
    public FixedString64Bytes PlayerName;
    public FixedString64Bytes PlayerId;
    public PlayerMatchState MatchState;
    

    public bool Equals(PlayerData other)
    {
        return
            ClientId == other.ClientId &&
            ColorId == other.ColorId &&
            PlayerName == other.PlayerName &&
            PlayerId == other.PlayerId &&
            MatchState == other.MatchState;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref ColorId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref PlayerId); 
        serializer.SerializeValue(ref MatchState); 
    }
}

public enum PlayerMatchState {
    active,
    gameOver
}