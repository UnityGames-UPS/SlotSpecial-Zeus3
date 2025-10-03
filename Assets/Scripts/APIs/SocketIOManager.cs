using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

public class SocketIOManager : MonoBehaviour
{
  [SerializeField]
  private SlotBehaviour slotManager;

  [SerializeField]
  private UIManager uiManager;

  internal GameData initialData = null;
  internal UIData initUIData = null;
  internal GameData resultData = null;
  internal PlayerData playerdata = null;
  internal List<List<int>> LineData = null;
  //WebSocket currentSocket = null;
  internal bool isResultdone = false;
  internal BonusData tempBonus;
  // protected string nameSpace="game"; //BackendChanges
  protected string nameSpace = ""; //BackendChanges
  private Socket gameSocket; //BackendChanges

  private SocketManager manager;

  protected string SocketURI = null;
  //protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
  //protected string TestSocketURI = "https://7p68wzhv-5000.inc1.devtunnels.ms/";
  //protected string TestSocketURI = "https://6f01c04j-5000.inc1.devtunnels.ms/";
  //protected string TestSocketURI = "https://c4xfw9cd-5002.inc1.devtunnels.ms/";
  protected string TestSocketURI = "http://localhost:5000/";
  //protected string TestSocketURI = "https://dingding-backend-nodejs-hamy.onrender.com/";
  [SerializeField] internal JSFunctCalls JSManager;
  [SerializeField]
  private string testToken;

  protected string gameID = "SL-Z3";
  //protected string gameID = "";

  internal bool isLoaded = false;

  internal bool SetInit = false;

  private const int maxReconnectionAttempts = 6;
  private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

  private void Awake()
  {
    //Debug.unityLogger.logEnabled = false;
    isLoaded = false;
    SetInit = false;

  }

  private void Start()
  {
    OpenSocket();
  }

  void ReceiveAuthToken(string jsonData)
  {
    Debug.Log("Received data: " + jsonData);

    // Parse the JSON data
    var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
    SocketURI = data.socketURL;
    myAuth = data.cookie;
    nameSpace = data.nameSpace;
    // Proceed with connecting to the server using myAuth and socketURL
  }

  string myAuth = null;

  private void OpenSocket()
  {
    //Create and setup SocketOptions
    SocketOptions options = new SocketOptions();
    options.ReconnectionAttempts = maxReconnectionAttempts;
    options.ReconnectionDelay = reconnectionDelay;
    options.Reconnection = true;
    options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket; //BackendChanges

    Application.ExternalCall("window.parent.postMessage", "authToken", "*");

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = testToken,
        gameId = gameID
      };
    };
    options.Auth = authFunction;
    // Proceed with connecting to the server
    SetupSocketManager(options);
#endif
  }


  private IEnumerator WaitForAuthToken(SocketOptions options)
  {
    // Wait until myAuth is not null
    while (myAuth == null)
    {
      Debug.Log("My Auth is null");
      yield return null;
    }
    while (SocketURI == null)
    {
      Debug.Log("My Socket is null");
      yield return null;
    }
    Debug.Log("My Auth is not null");
    // Once myAuth is set, configure the authFunction
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = myAuth,
        gameId = gameID
      };
    };
    options.Auth = authFunction;

    Debug.Log("Auth function configured with token: " + myAuth);

    // Proceed with connecting to the server
    SetupSocketManager(options);
  }

  private void SetupSocketManager(SocketOptions options)
  {
    // Create and setup SocketManager
#if UNITY_EDITOR
    this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif

    if (string.IsNullOrEmpty(nameSpace))
    {  //BackendChanges Start
      gameSocket = this.manager.Socket;
    }
    else
    {
      print("nameSpace: " + nameSpace);
      gameSocket = this.manager.GetSocket("/" + nameSpace);
    }
    // Set subscriptions
    gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
    gameSocket.On<string>(SocketIOEventTypes.Disconnect, OnDisconnected);
    gameSocket.On<string>(SocketIOEventTypes.Error, OnError);
    gameSocket.On<string>("message", OnListenEvent);
    gameSocket.On<bool>("socketState", OnSocketState);
    gameSocket.On<string>("internalError", OnSocketError);
    gameSocket.On<string>("alert", OnSocketAlert);

    // Start connecting to the server
  }

  // Connected event handler implementation
  void OnConnected(ConnectResponse resp)
  {
    Debug.Log("Connected!");
    SendPing();
  }

  private void OnDisconnected(string response)
  {
    Debug.Log("Disconnected from the server");
    //if (maxReconnectionAttempts <= this.manager.ReconnectAttempts)
    //{
    StopAllCoroutines();
    uiManager.DisconnectionPopup(false);
    //}
    //else
    //{
    //    uiManager.DisconnectionPopup(false);
    //}
  }

  private void OnError(string response)
  {
    Debug.LogError("Error: " + response);
  }

  private void OnListenEvent(string data)
  {
    Debug.Log("Received some_event with data: " + data);
    ParseResponse(data);
  }

  private void OnSocketState(bool state)
  {
    if (state)
    {
      Debug.Log("my state is " + state);
    }
    else
    {

    }
  }
  private void OnSocketError(string data)
  {
    Debug.Log("Received error with data: " + data);
  }
  private void OnSocketAlert(string data)
  {
    Debug.Log("Received alert with data: " + data);
  }

  private void SendPing()
  {
    InvokeRepeating("AliveRequest", 0f, 3f);
  }

  private void AliveRequest()
  {
    SendDataWithNamespace("YES I AM ALIVE");
  }

  private void SendDataWithNamespace(string eventName, string json = null)
  {
    // Send the message
    if (gameSocket != null && gameSocket.IsOpen) //BackendChanges
    {
      if (json != null)
      {
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);
      }
      else
      {
        gameSocket.Emit(eventName);
      }
    }
    else
    {
      Debug.LogWarning("Socket is not connected.");
    }
  }

  internal void ReactNativeCallOnFailedToConnect() //BackendChanges
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
  }


  internal void CloseSocket()
  {
    SendDataWithNamespace("EXIT");
  }

  private void ParseResponse(string jsonObject)
  {
    Debug.Log(jsonObject);
    Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

    string id = myData.id;

    switch (id)
    {
      case "InitData":
        {
          initialData = myData.message.GameData;
          initUIData = myData.message.UIData;
          playerdata = myData.message.PlayerData;
          LineData = myData.message.GameData.linesApiData;
          if (!SetInit)
          {
            Debug.Log(jsonObject);
            List<string> LinesString = ConvertListListIntToListString(initialData.linesApiData);
            PopulateSlotSocket(LinesString);
            SetInit = true;
          }
          else
          {
            RefreshUI();
          }
          break;
        }
      case "ResultData":
        {
          Debug.Log(jsonObject);
          myData.message.GameData.FinalResultReel = ConvertListOfListsToStrings(myData.message.GameData.ResultReel);
          myData.message.GameData.FinalsymbolsToEmit = TransformAndRemoveRecurring(myData.message.GameData.symbolsToEmit);
          resultData = myData.message.GameData;
          playerdata = myData.message.PlayerData;
          isResultdone = true;
          tempBonus = myData.message.GameData.bonusData;
          break;
        }
      case "ExitUser":
        {
          if (gameSocket != null) //BackendChanges
          {
            Debug.Log("Dispose my Socket");
            this.manager.Close();
          }
          //   Application.ExternalCall("window.parent.postMessage", "onExit", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("onExit");
#endif
          break;
        }
    }
  }

  private void RefreshUI()
  {
    uiManager.InitialiseUIData(initUIData.paylines);
  }

  private void PopulateSlotSocket(List<string> LineIds)
  {
    slotManager.shuffleInitialMatrix(true);
    for (int i = 0; i < LineIds.Count; i++)
    {
      slotManager.FetchLines(LineIds[i], i);
    }

    slotManager.SetInitialUI();

    isLoaded = true;
    // Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif

  }

  internal void AccumulateResult(double currBet)
  {
    isResultdone = false;
    MessageData message = new MessageData();
    message.data = new BetData();
    message.data.currentBet = currBet;
    message.data.spins = 1;
    message.data.currentLines = 192;
    message.id = "SPIN";
    // Serialize message data to JSON
    string json = JsonUtility.ToJson(message);
    SendDataWithNamespace("message", json);
  }

  private List<string> RemoveQuotes(List<string> stringList)
  {
    for (int i = 0; i < stringList.Count; i++)
    {
      stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
    }
    return stringList;
  }

  private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
  {
    List<string> resultList = new List<string>();

    foreach (List<int> innerList in listOfLists)
    {
      // Convert each integer in the inner list to string
      List<string> stringList = new List<string>();
      foreach (int number in innerList)
      {
        stringList.Add(number.ToString());
      }

      // Join the string representation of integers with ","
      string joinedString = string.Join(",", stringList.ToArray()).Trim();
      resultList.Add(joinedString);
    }

    return resultList;
  }

  private List<string> ConvertListOfListsToStrings(List<List<string>> inputList)
  {
    List<string> outputList = new List<string>();

    foreach (List<string> row in inputList)
    {
      string concatenatedString = string.Join(",", row);
      outputList.Add(concatenatedString);
    }

    return outputList;
  }

  private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
  {
    // Flattened list
    List<string> flattenedList = new List<string>();
    foreach (List<string> sublist in originalList)
    {
      flattenedList.AddRange(sublist);
    }

    // Remove recurring elements
    HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

    // Transformed list
    List<string> transformedList = new List<string>();
    foreach (string element in uniqueElements)
    {
      transformedList.Add(element.Replace(",", ""));
    }

    return transformedList;
  }
}

[Serializable]
public class BetData
{
  public double currentBet;
  public double currentLines;
  public double spins;
}

[Serializable]
public class AuthData
{
  public string GameID;
  //public double TotalLines;
}

[Serializable]
public class MessageData
{
  public BetData data;
  public string id;
}

[Serializable]
public class ExitData
{
  public string id;
}

[Serializable]
public class InitData
{
  public AuthData Data;
  public string id;
}

[Serializable]
public class GameData
{
  public List<List<int>> linesApiData { get; set; }
  public double baseBet { get; set; }
  public List<string> betMultiplier { get; set; }
  public List<List<string>> ResultReel { get; set; }
  public List<int> linesToEmit { get; set; }
  public List<List<string>> symbolsToEmit { get; set; }
  public double WinAmout { get; set; }
  public List<string> FinalsymbolsToEmit { get; set; }
  public List<string> FinalResultReel { get; set; }
  public bool isFreeSpin { get; set; }
  public int freeSpinCount { get; set; }
  public bool freeSpinAdded { get; set; }
  public BonusData bonusData { get; set; }
  public List<string> scatterWinningSymbols { get; set; }
  public List<List<int>> matchCountofLines { get; set; }
  public List<int> wildSymbolIndices { get; set; }
}

[Serializable]
public class BonusData
{
  public bool isBonus { get; set; }
  public int bonusWin { get; set; }
  public List<int> shuffledBonusValues { get; set; }
  public int selectedBonusMultiplier { get; set; }
  public List<string> trashForCashWinningSymbols { get; set; }
}

[Serializable]
public class Message
{
  public GameData GameData { get; set; }
  public UIData UIData { get; set; }
  public PlayerData PlayerData { get; set; }
}

[Serializable]
public class Root
{
  public string id { get; set; }
  public Message message { get; set; }
}

[Serializable]
public class UIData
{
  public Paylines paylines { get; set; }
  public List<string> spclSymbolTxt { get; set; }
}

[Serializable]
public class Paylines
{
  public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Symbol
{
  public int ID { get; set; }
  public string Name { get; set; }
  [JsonProperty("multiplier")]
  public object MultiplierObject { get; set; }

  // This property will hold the properly deserialized list of lists of integers
  [JsonIgnore]
  public List<List<double>> Multiplier { get; private set; }

  // Custom deserialization method to handle the conversion
  [OnDeserialized]
  internal void OnDeserializedMethod(StreamingContext context)
  {
    if (MultiplierObject == null)
    {
      // If the multiplier is null in the JSON, set an empty list
      Multiplier = new List<List<double>>();
    }
    else if (MultiplierObject is JObject)
    {
      // If the multiplier is an empty object ({}), treat it as an empty list
      Multiplier = new List<List<double>>();
    }
    else
    {
      try
      {
        // Attempt to deserialize the multiplier object as a list of lists of integers
        Multiplier = JsonConvert.DeserializeObject<List<List<double>>>(MultiplierObject.ToString());
      }
      catch (Exception ex)
      {
        // Handle unexpected format or deserialization errors
        Debug.Log($"Deserialization error: {ex.Message}");
        Multiplier = new List<List<double>>(); // Fallback to an empty list on error
      }
    }
  }
  public object defaultAmount { get; set; }
  public object symbolsCount { get; set; }
  public object description { get; set; }
  public int freeSpin { get; set; }
}
[Serializable]
public class PlayerData
{
  public double Balance { get; set; }
  public double haveWon { get; set; }
  public double currentWining { get; set; }
}
[Serializable]
public class AuthTokenData
{
  public string cookie;
  public string socketURL;
  public string nameSpace;
}


