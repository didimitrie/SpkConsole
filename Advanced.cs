using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;

namespace SpkConsole
{
  partial class Program
  {

    public IEnumerable<SpecklePlaceholder> SaveManyObjects(Account account, IEnumerable<SpeckleObject> objects)
    {
      Console.WriteLine( String.Format( "Saving {0} objects.", objects.Count() ) );
      Console.WriteLine();
      Console.WriteLine();

      // Step 1: Payload creation.
      // The approach below keeps request sizes around 500kb each.
      // NOTE: Will change in Speckle 2.0

      var objectUpdatePayloads = new List<List<SpeckleObject>>();
      long totalBucketSize = 0,  currentBucketSize = 0;
      var currentBucketObjects = new List<SpeckleObject>();
      var allObjects = new List<SpeckleObject>();

      foreach ( var obj in objects )
      {
        long size = Converter.getBytes( obj ).Length;
        currentBucketSize += size;
        totalBucketSize += size;
        currentBucketObjects.Add( obj );

        if ( currentBucketSize > 5e5 ) // restrict max to ~500kb;
        {
          Console.Write( String.Format( "Made {0} payloads.", objectUpdatePayloads.Count ) );
          Console.CursorLeft =  0;

          objectUpdatePayloads.Add( currentBucketObjects );
          currentBucketObjects = new List<SpeckleObject>();
          currentBucketSize = 0;
        }
      }

      // add in the last bucket
      if ( currentBucketObjects.Count > 0 )
      {
        objectUpdatePayloads.Add( currentBucketObjects );
      }

      Console.WriteLine();
      Console.WriteLine( String.Format( "Done making payloads ({0} total).", objectUpdatePayloads.Count ) );


      // Step 2: Actually persist the objects, and return placeholders with their ObjectIds.
      // This might take a while.
      var client = new SpeckleApiClient( account.RestApi, false, "console_app" );
      client.AuthToken = account.Token;

      var i = 0;
      foreach ( var payload in objectUpdatePayloads )
      {
        Console.Write( String.Format( "Sending payload #{0} ({1} objects) ...", i++, payload.Count ) );

        var result = client.ObjectCreateAsync( payload ).Result.Resources;
        foreach ( var placehholder in result )
          yield return placehholder as SpecklePlaceholder;

        Console.Write( " done." );
        Console.CursorLeft = 0;
      }
    }
  }


}
