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

    static void SaveManyObjectsTest( int numObjects )
    {
      if ( numObjects > 50000 )
      {
        Console.WriteLine( "Let's think about this: is this the best way to do it? Y/N" );
        var answer = Console.ReadLine();
        if ( answer == "N" )
        {
          return;
        }
        else
        {
          Console.WriteLine( "oh well, ok... will go ahead." );
        }
      }

      var nodes = new List<Node>();
      for ( int i = 0; i < numObjects; i++ )
      {
        nodes.Add( new Node { x = i, y = i % 2, z = i % 3 } );
      }

      var account = GetAccount();
      var savedObjects = SaveManyObjects( account, nodes );

      var client = new SpeckleApiClient( account.RestApi, false, "console_app" );
      client.AuthToken = account.Token;

      Console.WriteLine( "Please enter a stream name:" );
      var name = Console.ReadLine();

      var myStream = new SpeckleStream()
      {
        Objects = savedObjects.Cast<SpeckleObject>().ToList(),
        Name = name != "" ? name : "Console test stream",
        Description = "This stream was created from a .net console program. Easy peasy.",
        Tags = new List<string> { "example", "console-test" },
      };

      try
      {
        // save the stream.
        var result = client.StreamCreateAsync( myStream ).Result;

        // profit
        Console.WriteLine( String.Format( "Succesfully created a stream! It's id is {0}. Rock on! Check it out at {1}/streams/{0}", result.Resource.StreamId, account.RestApi ) );
        Console.WriteLine( "Press any key to continue." );

        System.Diagnostics.Process.Start( String.Format( "{1}/streams/{0}", result.Resource.StreamId, account.RestApi ) );
        Console.ReadLine();
      }
      catch ( Exception e )
      {
        Console.WriteLine( "Bummer - something went wrong:" );
        Console.WriteLine( e.Message );
        Console.WriteLine( "Press any key to continue." );
        Console.ReadLine();
      }

    }

    /// <summary>
    /// Orchestrates the saving of many objects. 
    /// </summary>
    /// <param name="account"></param>
    /// <param name="objects"></param>
    /// <returns></returns>
    static IEnumerable<SpecklePlaceholder> SaveManyObjects( Account account, IEnumerable<SpeckleObject> objects )
    {
      Console.WriteLine( String.Format( "Saving {0} objects.", objects.Count() ) );
      Console.WriteLine();
      Console.WriteLine();

      // Step 1: Payload creation.
      // The approach below keeps request sizes around 500kb each.
      // NOTE: Will change in Speckle 2.0

      var objectUpdatePayloads = new List<List<SpeckleObject>>();
      long totalBucketSize = 0, currentBucketSize = 0;
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
