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
    /// <summary>
    /// Gets a stream and its objects, in bulk. This is NOT recommended for large streams.
    /// </summary>
    /// <param name="_streamId"></param>
    static void GetStream( string _streamId = null )
    {
      Console.WriteLine( "Hello Speckle! We will now try and get a stream." );
      var account = LocalContext.GetAccountByEmailAndRestApi( "didimitrie@gmail.com", "https://hestia.speckle.works/api" );

      var streamId = "";

      if ( _streamId == null )
      {
        Console.WriteLine( "What stream id should we get?" );
        streamId = Console.ReadLine();
      }
      else
      {
        streamId = _streamId;
      }

      var client = new SpeckleApiClient( account.RestApi, false, "console_app" );
      client.AuthToken = account.Token;

      var stream = client.StreamGetAsync( streamId, "" ).Result.Resource;
      var objects = client.StreamGetObjectsAsync( streamId, "fields=hash,type" ).Result; // restricting to just a few things

      Console.WriteLine( "This is the stream:" );
      Console.WriteLine( stream.ToJson() );
      Console.WriteLine();
      Console.WriteLine( "Press any key to continue." );
      Console.ReadLine();
      Console.WriteLine( "These are the objects:" );
      Console.WriteLine( objects.ToJson() );
      Console.WriteLine();
      Console.WriteLine( "Press any key to continue." );
      Console.ReadLine();
    }

    /// <summary>
    /// <para>Creates a stream.</para>
    /// <para>Please note this is not the best way to do it for large payloads!</para>
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    static string CreateStream( Account account )
    {
      SpeckleCore.SpeckleInitializer.Initialize();

      Console.WriteLine( "Hello Speckle! We will now create a sample stream." );

      Console.WriteLine( "Please enter a stream name:" );
      var name = Console.ReadLine();

      var myStream = new SpeckleStream()
      {
        Objects = new List<SpeckleObject>(),
        Name = name != "" ? name : "Console test stream",
        Description = "This stream was created from a .net console program. Easy peasy.",
        Tags = new List<string> { "example", "console-test" }
      };


      Console.WriteLine( "Creating and converting sample data... " );
      var sampleSize = 10;

      var myNodes = new List<Node>();
      for ( int i = 0; i < sampleSize; i++ )
      {
        myNodes.Add( new Node { id = i, x = i, y = i + 1, z = i + 2 } );
        myStream.Objects.Add( myNodes[ i ] );
        Console.WriteLine( "Added " + i + " nodes" );
      }

      var myQuads = new List<Quad>();
      for ( int i = 0; i < sampleSize; i++ )
      {
        myQuads.Add( new Quad
        {
          id = i,
          nodes = myNodes.GetRange( 0, i ).ToArray(),
          vx = 42,
          iteration = 42,
          tau = 1337 // obviously bogus numbers
        } );

        myStream.Objects.Add( myQuads[ i ] );
        Console.WriteLine( "Added " + i + " quads" );
      }

      Console.WriteLine( "Done.\n" );
      Console.WriteLine( String.Format( "Saving stream to {0} using {1}'s account. This might take a bit.", account.RestApi, account.Email ) );

      // create an api client
      var client = new SpeckleApiClient( account.RestApi, false, "console_app" );
      client.AuthToken = account.Token;

      try
      {
        // save the stream.
        var result = client.StreamCreateAsync( myStream ).Result;

        // profit
        Console.WriteLine( String.Format( "Succesfully created a stream! It's id is {0}. Rock on! Check it out at {1}/streams/{0}", result.Resource.StreamId, account.RestApi ) );
        Console.WriteLine( "Press any key to continue." );

        System.Diagnostics.Process.Start( String.Format( "{1}/streams/{0}", result.Resource.StreamId, account.RestApi ) );
        Console.ReadLine();
        return result.Resource.StreamId;
      }
      catch ( Exception e )
      {
        Console.WriteLine( "Bummer - something went wrong:" );
        Console.WriteLine( e.Message );
        Console.WriteLine( "Press any key to continue." );
        Console.ReadLine();
        return null;
      }
    }
  }

}
