using System;
using System.Threading;
using SpeckleCore;
using SpkConsole;

namespace SpkWsExample
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello Smelly Sockets");

      var account = SpkConsole.Program.GetAccount();

      var spkClient_A = new SpeckleApiClient(account.RestApi, false, "console application");
      var spkClient_B = new SpeckleApiClient(account.RestApi, false, "console application");
      spkClient_A.AuthToken = account.Token;
      spkClient_B.AuthToken = account.Token;

      //gen streamid
      var streamId = spkClient_A.StreamCreateAsync(new SpeckleStream() { Name = "WS Test" }).Result.Resource.StreamId;
      Console.WriteLine($"Created dummy stream: {streamId}. Press any key to continue stuff.");

      // Add event handlers and setup streamId on both ws clients. The event handlers just spit out what they get. 
      spkClient_A.StreamId = streamId;
      spkClient_A.SetupWebsocket();
      spkClient_A.OnWsMessage += SpkClient_A_OnWsMessage;

      spkClient_B.StreamId = streamId;
      spkClient_B.SetupWebsocket();
      spkClient_B.OnWsMessage += SpkClient_B_OnWsMessage;


      Console.WriteLine("Waiting for 200ms, ensure connection actually happened. This is an issue with core, it doesn't expose a 'onwsconnection' event :/");
      Thread.Sleep(200);

      // Flop them in a room - this is important if you want to broadcast messages. 
      spkClient_A.JoinRoom("stream", streamId);
      spkClient_B.JoinRoom("stream", streamId);
      
      // Same hack as above. 
      Thread.Sleep(200);

      // Send some dummy broadcasts
      spkClient_A.BroadcastMessage("stream", streamId, new { customEventType = "update-mesh", data = "42" });
      spkClient_A.BroadcastMessage("stream", streamId, new { customEventType = "update-mesh-other", data = "wow" });

      spkClient_B.BroadcastMessage("stream", streamId, new { customEventType = "update-mesh-other", data = "wow" });
      spkClient_B.SendMessage(spkClient_A.ClientId, new { what = "This is a direct, 1-1 message from B to A." });


      Console.WriteLine("Press any key to continue.");
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
  }
}
