using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HcpVr
{
   /// <summary>
   /// Config about VR World, its max dimensions in meters, the replay time for flythroughs, max records to retrieve etc.
   /// </summary>
   public struct ConfigurationVr
   {
      public int MaxHcpRecordsToRetrieve;
      public int MaxTimeSeconds;
      /// <summary>
      /// Use constant time between between entities appearing (ignore actual timestamp)
      /// </summary>
      public bool SmoothTime;
      public int MaxYHeight;
      public int MaxZDistance;
      public int MaxXWidth;      
   }
}
