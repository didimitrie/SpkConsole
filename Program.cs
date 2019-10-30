using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;

namespace SpkConsole
{
  class Program
  {
    static void Main( string[ ] args )
    {
      SpeckleCore.SpeckleInitializer.Initialize();

      Console.WriteLine( "Hello Speckle!" );

      var account = LocalContext.GetAccountByEmailAndRestApi( "didimitrie@gmail.com", "https://hestia.speckle.works/api" );

      if ( account == null )
      {
        Console.WriteLine( "Meep. No account found. Press  any key to exit." );
        Console.ReadLine();
        return;
      }

      var myStream = new SpeckleStream()
      {
        Objects = new List<SpeckleObject>(),
        Name = "Console test stream",
        Description = "Wowoww!",
        Tags = new List<string> { "what", "console-test" }
      };


      Console.WriteLine( "Creating and converting sample data... " );
      var sampleSize = 1;

      var myNodes = new List<Node>();
      for ( int i = 0; i < sampleSize; i++ )
      {
        myNodes.Add( new Node { id = i, x = i, y = i + 1, z = i + 2 } );
        myStream.Objects.Add( ( SpeckleObject ) Converter.Serialise( myNodes[ i ] ) ); // this adds objects to the stream
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

        myStream.Objects.Add( ( SpeckleObject ) Converter.Serialise( myQuads[ i ] ) );
        Console.WriteLine( "Added " + i + " quads" );
      }

      Console.WriteLine( "Done.\n" );
      Console.WriteLine( String.Format( "Saving stream to {0} using {1}'s account. This might take a bit.", account.RestApi, account.Email ) );

      // create an api client
      var client = new SpeckleApiClient( account.RestApi, false, "console_app" );
      client.AuthToken = account.Token;

      // save the stream
      var result = client.StreamCreateAsync( myStream ).Result;

      // profit
      Console.WriteLine( String.Format( "Succesfully created a stream! It's id is {0}. Rock on! Check it out at {1}/streams/{0}", result.Resource.StreamId, account.RestApi ) );
      Console.WriteLine( "Press any key to exit." );
      Console.ReadLine();

    }
  }
}


public class Base
{
  public int id;
}

public class Node : Base
{
  public float x; public float y; public float z;
  public Node( ) { }
}

public class Quad : Base
{
  public Node[ ] nodes;
  public int group;
  public float vx;
  public float vy;
  public float nx;
  public float ny;
  public float nxy;
  public float sigx;
  public float sigy;
  public float tau;
  public int iteration;

  public Quad( ) { }
}
