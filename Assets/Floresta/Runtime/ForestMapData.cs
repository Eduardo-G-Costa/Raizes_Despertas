using System;
using UnityEngine;

namespace Floresta {
 [Serializable] public class ForestMapData {
  public int width, height, tileSize;
  public int[] tiles;
  public ForestMarker[] markers;
 }
 [Serializable] public class ForestMarker {
  public string id, kind, text;
  public int x, y, stage;
 }
}
