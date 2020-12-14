using NativeWebSocket;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Game : MonoBehaviour {
    public static Game i { private set; get; }

    public List<Player> Players { get; set; } = new List<Player>();
    public List<Bird> Birds { get; set; } = new List<Bird>();

    public GameObject birdPrefab;
    [SerializeField] private GameObject playerPrefab;

    Player localPlayer;
    WebSocket websocket;
    NetControllers controllers = new NetControllers();
    E_ConnectionState connectionState;

    enum E_ConnectionState {CONNECTING, ERROR, OK};

    void Awake() {
        i = this;

        ConnectSocket();
    }

    private void Start() {
#if UNITY_EDITOR
        int i = 0;
        Debug.Log(
            Newtonsoft.Json.JsonConvert.SerializeObject(BirdSpot.spots.Select(o => new NetControllers.DumpBirdSpot() {
                id = ++i,
                position = new NetControllers.Position() {
                    x = o.transform.position.x,
                    y = o.transform.position.y,
                    z = o.transform.position.z
                },
                isSafe = o.isRestingSpot
            }).ToList())
        );
#endif
    }

    async void ConnectSocket() {
        while (true) {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) {
                break;
            }
#endif

            if (connectionState == E_ConnectionState.OK) {
                await Task.Delay(1000);
                continue;
            }

            await InitWebSock();

        }
    }

    async Task InitWebSock() {
        connectionState = E_ConnectionState.CONNECTING;
        websocket = new WebSocket("ws://racksberry:1234");

        websocket.OnOpen += () => {
            connectionState = E_ConnectionState.OK;
        };

        websocket.OnError += (e) => {
            connectionState = E_ConnectionState.ERROR;
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) => {
            connectionState = E_ConnectionState.ERROR;
            Debug.Log("Connection closed! " + e);
        };

        websocket.OnMessage += (bytes) => {
            try {
                string message = System.Text.Encoding.UTF8.GetString(bytes);
                string controller = message.Substring(0, 3);
                message = message.Substring(3);

                Debug.Log(controller + " => " + message);

                if (controllers.ContainsKey(controller)) {
                    controllers[controller].Invoke(websocket, message);
                }
                else {
                    Debug.LogWarning("Received junk controller " + controller + " with message " + message);
                }
            }
            catch (System.Exception e) {
                Debug.LogError(e);
            }


            // Debug.Log("OnMessage! " + message);
        };

        await websocket.Connect();
    }

    public Player SpawnPlayer(int id, Vector3 position, Quaternion rotation, bool isLocal = false) {
        var player = Object.Instantiate(playerPrefab).GetComponent<Player>();
        player.id = id;
        player.transform.position = position;
        player.transform.rotation = rotation;
        player.IsLocal = isLocal;

        if (player.IsLocal) {
            localPlayer = player;
        }

        Players.Add(player);

        return player;
    }

    public void DestroyAllPlayers() {
        foreach (var player in Players) {
            Destroy(player);
        }

        Players.Clear();
    }

    public Player GetPlayerById(int id) {
        return Players.Find(o => o.id == id);
    }

    public void SendMyPosition(Vector3 position, Quaternion rotation, PlayerMovement infos) {
        websocket.SendText(NetControllers.PROTOCOL_MOVESTATE + Newtonsoft.Json.JsonConvert.SerializeObject(new NetControllers.PlayerMove() {
            id = localPlayer.id,
            isJumping = infos.IsJumping,
            isRunning = infos.IsSprinting,
            isSneaking = infos.IsCrouching,
            position = new NetControllers.Position() {
                x = position.x,
                y = position.y,
                z = position.z
            },
            rotation = rotation.x + " " + rotation.y + " " + rotation.z + " " + rotation.w
        }));
    }

    void Update() {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif

    }
}
