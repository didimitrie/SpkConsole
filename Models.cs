using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;

namespace SpkConsole
{
  /// <summary>
  /// Base class. Inherits from a SpeckleObject.
  /// </summary>
  [Serializable]
  public class Base : SpeckleObject
  {
    public int id;
  }

  /// <summary>
  /// Just a sample node class. Has x, y, z float values.
  /// </summary>
  [Serializable]
  public class Node : Base
  {
    public override string Type { get => base.Type + "/" + "TestNodeClass"; } // Note how to define the type of a custom speckle object!
    public float x; public float y; public float z;
    public Node( ) { }
  }

  /// <summary>
  /// A quad class.
  /// </summary>
  [Serializable]
  public class Quad : Base
  {
    public override string Type { get => base.Type + "/" + "TestQuadClass"; } // Note how to define the type of a custom speckle object!

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
}
