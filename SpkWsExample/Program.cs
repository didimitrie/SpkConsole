using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using SpeckleCore;
using SpkConsole;
using WebSocketSharp;

namespace SpkWsExample
{
  class Program
  {
    public static Account SelectedAccount;
    public static string DummyStreamId;

    static void Main(string[] args)
    {
      Console.WriteLine("Hello Smelly Sockets");

      SelectedAccount = SpkConsole.Program.GetAccount();

      var spkClient_A = new SpeckleApiClient(SelectedAccount.RestApi, false, "console application");
      var spkClient_B = new SpeckleApiClient(SelectedAccount.RestApi, false, "console application");
      spkClient_A.AuthToken = SelectedAccount.Token;
      spkClient_B.AuthToken = SelectedAccount.Token;

      //gen streamid
      DummyStreamId = spkClient_A.StreamCreateAsync(new SpeckleStream() { Name = "WS Test" }).Result.Resource.StreamId;
      Console.WriteLine($"Created dummy stream: {DummyStreamId}. Press any key to continue stuff.");

      // Add event handlers and setup streamId on both ws clients. The event handlers just spit out what they get. 
      spkClient_A.StreamId = DummyStreamId;
      spkClient_A.SetupWebsocket();
      spkClient_A.OnWsMessage += SpkClient_A_OnWsMessage;

      spkClient_B.StreamId = DummyStreamId;
      spkClient_B.SetupWebsocket();
      spkClient_B.OnWsMessage += SpkClient_B_OnWsMessage;


      Console.WriteLine("Waiting for 200ms, ensure connection actually happened. This is an issue with core, it doesn't expose a 'onwsconnection' event :/");
      Thread.Sleep(200);

      // Flop them in a room - this is important if you want to broadcast messages. 
      spkClient_A.JoinRoom("stream", DummyStreamId);
      spkClient_B.JoinRoom("stream", DummyStreamId);

      // Same hack as above. 
      Thread.Sleep(200);

      // Send some dummy broadcasts
      spkClient_A.BroadcastMessage("stream", DummyStreamId, new { customEventType = "update-mesh", data = "42" });
      spkClient_A.BroadcastMessage("stream", DummyStreamId, new { customEventType = "update-mesh-other", data = "wow" });

      spkClient_B.BroadcastMessage("stream", DummyStreamId, new { customEventType = "update-mesh-other", data = "wow" });
      spkClient_B.SendMessage(spkClient_A.ClientId, new { what = "This is a direct, 1-1 message from B to A." });


      Console.WriteLine("Press any key to continue testing the barebones approach!");

      BareBonesApproach();

      Console.WriteLine("Done. Press any key to continue.");
      spkClient_A.StreamDeleteAsync(DummyStreamId);
      Console.ReadLine();
    }

    private static void SpkClient_B_OnWsMessage(object source, SpeckleEventArgs e)
    {
      // Note, the dynamic object with the "parsed" event data is in e.EventObject.
      Console.WriteLine();
      Console.WriteLine($"(Client B) Event name: {e.EventName}; Raw: {e.EventData}.");
    }

    private static void SpkClient_A_OnWsMessage(object source, SpeckleEventArgs e)
    {
      Console.WriteLine();
      Console.WriteLine($"(Client A) Event name: {e.EventName}; Raw: {e.EventData}.");
    }

    public static void BareBonesApproach()
    {
      string ClientId = Guid.NewGuid().ToString();
      
      
      WebSocket WebsocketClient = new WebSocket("wss://hestia.speckle.works?access_token=" + SelectedAccount.Token + "&stream_id=" + DummyStreamId + "&client_id=" + ClientId);
      WebsocketClient.OnMessage += (sender, e) => Console.WriteLine("computer says: " + e.Data);

      WebsocketClient.OnOpen += (sender, e) =>
      {
        Dictionary<string, object> message = new Dictionary<string, object>();

        var eventData = new
        {
          eventName = "broadcast",
          senderId = ClientId,
          resourceType = "stream",
          resourceId = DummyStreamId,
          args = new
          {
            hello="world",
            source="random client"
          }
        };


        WebsocketClient.Send(JsonConvert.SerializeObject(eventData));
        Console.WriteLine("just sent: " + JsonConvert.SerializeObject(eventData));
      };
      WebsocketClient.OnError += (sender, e) => Console.WriteLine("Error: " + e.Message);
      WebsocketClient.OnClose += (sender, e) => Console.WriteLine("Closing: " + e.Reason);
      WebsocketClient.Connect();
      Console.ReadLine();
    }
  }
}

