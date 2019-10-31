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
    static void Main( string[ ] args )
    {
      // always call the initialisation function to make sure kits are loaded.
      SpeckleCore.SpeckleInitializer.Initialize();

      //var account = GetAccount();

      //var streamId = CreateStream( account );

      //GetStream( streamId );

      SaveManyObjectsTest( 1000 );
    }

    /// <summary>
    /// Gets an account from the existing ones.
    /// </summary>
    /// <returns></returns>
    static Account GetAccount( )
    {
      Console.WriteLine( "What email address should we use to search for local accounts?" );
      var email = Console.ReadLine();
      try
      {
        return LocalContext.GetDefaultAccount();
      }
      catch
      {
        var accounts = LocalContext.GetAccountsByEmail( email );
        if ( accounts.Count == 0 )
        {
          Console.WriteLine( "No account found. Please try again!" );
          return GetAccount();
        }

        Console.WriteLine( String.Format( "Found {0} accounts. Which one should we use? (0 - {0})", accounts.Count ) );
        int i = 0;
        foreach ( var acc in accounts )
        {
          Console.WriteLine( String.Format( "{0} : {1} at {2}", i++, acc.Email, acc.RestApi ) );
        }

        var selIndex = System.Convert.ToInt32( Console.ReadLine() );
        return accounts[ selIndex ];
      }
    }


  }
}
