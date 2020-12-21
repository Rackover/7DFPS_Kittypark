using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class NetControllers : Dictionary<string, Action<NativeWebSocket.WebSocket, string>> {

    public const string PROTOCOL_MOVESTATE = "MOV";
    public const string PROTOCOL_CATCH = "CAT";
    public const string PROTOCOL_BIRD_UPDATE = "BRD";
    public const string PROTOCOL_UPDATE_SCORE = "SCO";
    public const string PROTOCOL_STATE = "STT";
    public const string PROTOCOL_ACKNOWLEDGE_STATE = "AKS";
    public const string PROTOCOL_KILL_PLAYER = "KIL";
    public const string PROTOCOL_MEOW = "MEW";

    public NetControllers() {
        Add(PROTOCOL_MOVESTATE, MovePlayer);
        Add(PROTOCOL_CATCH, null);
        Add(PROTOCOL_BIRD_UPDATE, UpdateBirds);
        Add(PROTOCOL_UPDATE_SCORE, UpdateScores);
        Add(PROTOCOL_ACKNOWLEDGE_STATE, null);
        Add(PROTOCOL_STATE, InitializeState);
        Add(PROTOCOL_KILL_PLAYER, KillPlayer);
        Add(PROTOCOL_MEOW, MakeMeow);
    }

    void MovePlayer(NativeWebSocket.WebSocket ws, string data) {
        var move = JsonConvert.DeserializeObject<PlayerMove>(data);
        var player = Game.i.GetPlayerById(move.id);
        var reDeser = new DeserializedPlayerMove(move);

        if (player == null) {
            Debug.Log("Spawning new unknown player " + move.id);
            player = Game.i.SpawnPlayer(move.id, reDeser.position, reDeser.rotation, isLocal: false);
        }

        if (player.IsLocal) {
            Debug.LogWarning("Received position for local player, should not happen!!");
            return;
        }

        player.UpdatePosition(reDeser);
    }

    void KillPlayer(NativeWebSocket.WebSocket ws, string data) {
        var id = Convert.ToInt32(data);

        Debug.Log("Killing player " + id);

        Game.i.KillPlayer(id);
    }

    void MakeMeow(NativeWebSocket.WebSocket ws, string data) {
        var id = Convert.ToInt32(data);

        Game.i.GetPlayerById(id)?.Meow();
    }

    void UpdateBirds(NativeWebSocket.WebSocket ws, string data) {
        var birdInfo = JsonConvert.DeserializeObject<BirdInfo>(data);
        Bird bird;

        if (Game.i.deadBirds.Contains(birdInfo.birdId)) {
            return;
        }

        BirdSpot spot = BirdSpot.GetSpotWithID(birdInfo.spotId);
        if (spot == null) {
            Debug.LogError("Found no spot with id " + birdInfo.spotId + ". Spots are (" + string.Join(", ", BirdSpot.spots.Select(o => o.id)) + ")");
            return;
        }

        if ((bird = Game.i.Birds.Find(o => o.id == birdInfo.birdId)) == null) {
            bird = UnityEngine.Object.Instantiate(Game.i.birdPrefab).GetComponent<Bird>();
            bird.id = birdInfo.birdId;

            bird.currentSpot = spot;
            bird.RefreshFromSpot();

            Game.i.Birds.Add(bird);
        }

        else if (birdInfo.isFlying) {
            bird.FlyTo(BirdSpot.GetSpotWithID(birdInfo.spotId));
            return;
        }

        if (birdInfo.isHeartbeat) {
            bird.Heartbeat();
            return;
        }

        if (birdInfo.isFlapping) {
            bird.Flap();
            return;
        }

        if (birdInfo.isHop) {
            bird.Hop();
            return;
        }

        if (birdInfo.isPickingGrain) {
            bird.PickGrain();
            return;
        }

    }
    void UpdateScores(NativeWebSocket.WebSocket ws, string data) {
        var state = JsonConvert.DeserializeObject<ScoreInfo>(data);

        if (state.deadBird > 0) {
            var bird = Game.i.Birds.Find(o => o.id == state.deadBird);

            if (bird && !Game.i.deadBirds.Contains(state.deadBird)) {
                bird.Kill(withFX: true);
            }
        }

        Game.i.SetScore(state.clientId, state.newScore);
    }

    void InitializeState(NativeWebSocket.WebSocket ws, string data) {
        var state = JsonConvert.DeserializeObject<GameState>(data);

        BirdSpot.ClearAll();

        Game.i.KillBowls();

        Game.i.DestroyAllPlayers();

        Game.i.deadBirds.Clear();

        foreach (var client in state.clients) {
            var splitRot = client.rotation.Split(' ');
            Debug.Log("Spawning client " + client.id+" (observer? "+client.isObserver+")");

            if (client.isObserver) {
                return;
            }

            Game.i.SpawnPlayer(
                client.id,
                new Vector3(client.position.x, client.position.y, client.position.z),
                new Quaternion(Convert.ToSingle(splitRot[0]), Convert.ToSingle(splitRot[1]), Convert.ToSingle(splitRot[2]), Convert.ToSingle(splitRot[3])),
                isLocal: client.isYou
            );
            
        }

        foreach (var spot in state.birdSpots) {
            Debug.Log("Loading bird spot " + spot.id);
            var realSpot = BirdSpot.GetUnusedSpot();
            realSpot.id = spot.id;
            realSpot.transform.position = new Vector3(spot.position.x, spot.position.y, spot.position.z);
            realSpot.isRestingSpot = spot.isSafe;
        }

        foreach (var score in state.scores) {
            Game.i.SetScore(Convert.ToInt32(score.Key), Convert.ToInt32(score.Value));
        }

        Debug.Log("Sending ACK");
        ws.SendText(PROTOCOL_ACKNOWLEDGE_STATE + (Game.i.IsMobile ? "observer" : ""));
    }

    [Serializable]
    public class ScoreInfo {
        public int deadBird = -1;
        public int newScore = 1;
        public int clientId = -1;
    }

    [Serializable]
    public class BirdInfo {
        public int birdId;
        public int spotId;
        public bool isPickingGrain = false;
        public bool isFlapping = false;
        public bool isHop = false;
        public bool isFlying = false;
        public bool isHeartbeat = false;
    }

    [Serializable]
    public class GameState {
        public List<Client> clients;
        public List<SerializedBird> birds;
        public List<DumpBirdSpot> birdSpots;
        public Dictionary<string, string> scores = new Dictionary<string, string>();
    }

    [Serializable]
    public class DumpBirdSpot {
        public int id;
        public Position position;
        public bool isSafe;
    }

    [Serializable]
    public class PlayerMove {
        public int id;
        public Position position;
        public string rotation;
        public bool isJumping = false;
        public bool isRunning = false;
        public bool isSneaking = false;
    }

    public class DeserializedPlayerMove {
        public Vector3 position;
        public Quaternion rotation;
        public bool isJumping = false;
        public bool isRunning = false;
        public bool isSneaking = false;

        public DeserializedPlayerMove() {

        }

        public DeserializedPlayerMove(PlayerMove move) {
            position = new Vector3(move.position.x, move.position.y, move.position.z);
            var splitRot = move.rotation.Split(' ');
            rotation = new Quaternion(Convert.ToSingle(splitRot[0]), Convert.ToSingle(splitRot[1]), Convert.ToSingle(splitRot[2]), Convert.ToSingle(splitRot[3]));

            isJumping = move.isJumping;
            isRunning = move.isRunning;
            isSneaking = move.isSneaking;
        }
    }

    [Serializable]
    public class Client {
        public int id;
        public Position position;
        public string rotation;
        public bool isYou = false;
        public bool isObserver = false;
    }

    [Serializable]
    public class Position {
        public float x = 0;
        public float y = 1;
        public float z = 0;
    }

    [Serializable]
    public class SerializedBird {
        public string id;
        public string spotId;
    }
}
