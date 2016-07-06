using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HcpVr
{
   [System.Serializable]
   public class SensorPhoneRecord
   {
      public Sensor Sensor;
      public World World;
   }

   /// <summary>
   /// Inbound SensorPhone record
   /// </summary>
   [System.Serializable]
   public struct Sensor
   {
      public string G_DEVICE; // "2ad39033-a0cc-4a19-b6ec-33f97a4f216f"
      public string G_CREATED; //"/Date(1465941656507)/"
      public string C_TIMESTAMP; // "/Date(1465941485000)/"
      public string C_DEVICE; // "iOS Device"
      public string C_GYROSCOPEX; // "-0.1265521794557571"
      public string C_GYROSCOPEY; // "0.08579760044813156"
      public string C_GYROSCOPEZ; // "-0.06093422323465347"
      public string C_ACCELEROMETERX; // "-0.0042724609375"
      public string C_ACCELEROMETERY; // "-0.7014465061947703"
      public string C_ACCELEROMETERZ; // "-0.673080462962389"
      public string C_ALTITUDE; // "12.53634262084961"
      public string C_LONGITUDE; // "-0.2245472464712565"
      public string C_LATITUDE; // "51.41115630050321"
      public string C_AUDIO; // "0"      
   }

   /// <summary>
   /// Post processed World record, which contains additional fields
   /// suitable for use when visualising in the VR world.
   /// Fields postfixed with N are normalised values 0..1 across whole data set.
   /// </summary>
   [System.Serializable]
   public struct World
   {
      public DateTime CreatedOn;
      public DateTime TimeStamp;
      public string ShortDate;
      public string ShortTime;
      public double TimeStampSeconds;
      public double TimeStampSecondsN;
      public double Altitude;
      public double AltitudeN;
      public double Longitude;
      public double LongitudeN;
      public double Latitude;
      public double LatitudeN;
      public double AccelX;
      public double AccelXN;
      public double AccelY;
      public double AccelYN;
      public double AccelZ;
      public double AccelZN;
      //public double GyroX;
      //public double GyroXN;
      //public double GyroY;
      //public double GyroYN;
      //public double GyroZ;
      //public double GyroZN;
      public Vector3 AccelVec;
      public double AccelMag;
      public double AccelMagN;
      public Color AccelColorMagN;
      public Color AccelColorVecXYZN;
      public Vector3 Gyroscope;
      //public Vector3 GyroscopeColor;
   }
      
   public class CloudDataMassList
   {
      public string TestStr;
      public List<SensorPhoneRecord> SensorPhoneRecords;

      public CloudDataMassList()
      {
         SensorPhoneRecords = new List<SensorPhoneRecord>();
      }

      /// <summary>
      /// Transform the list of SensorPhoneRecords to add in fields suitable for displaying data in VR world
      /// </summary>
      public void TransformToWorld()
      {
         // Step 0 Prepare fields - do simple conversions (no max/min or normalisation yet)
         foreach (SensorPhoneRecord spr in SensorPhoneRecords)
         {
            spr.World.CreatedOn = ConvertHcpStringToDate(spr.Sensor.G_CREATED);
            spr.World.TimeStamp = ConvertHcpStringToDate(spr.Sensor.C_TIMESTAMP);
            spr.World.ShortDate = spr.World.TimeStamp.ToShortDateString();
            spr.World.ShortTime = spr.World.TimeStamp.ToLongTimeString(); //ShortTimeString();
            spr.World.TimeStampSeconds = ConvertHcpStringToDouble(spr.Sensor.C_TIMESTAMP);
            spr.World.Altitude = Convert.ToDouble(spr.Sensor.C_ALTITUDE);
            spr.World.Longitude = Convert.ToDouble(spr.Sensor.C_LONGITUDE);
            spr.World.Latitude = Convert.ToDouble(spr.Sensor.C_LATITUDE);
            spr.World.AccelX = Convert.ToDouble(spr.Sensor.C_ACCELEROMETERX);
            spr.World.AccelY = Convert.ToDouble(spr.Sensor.C_ACCELEROMETERY);
            spr.World.AccelZ = Convert.ToDouble(spr.Sensor.C_ACCELEROMETERZ);          
            spr.World.AccelVec = new Vector3((float)spr.World.AccelX, (float)spr.World.AccelY, (float)spr.World.AccelZ);
            spr.World.AccelMag = spr.World.AccelVec.magnitude;
            spr.World.Gyroscope = new Vector3((float)Convert.ToDouble(spr.Sensor.C_GYROSCOPEX), (float)Convert.ToDouble(spr.Sensor.C_GYROSCOPEY), (float)Convert.ToDouble(spr.Sensor.C_GYROSCOPEZ));
         }

         // Step 1 calculate max and min values and the length between them
         World maxWorld = new World();
         maxWorld.Altitude = (SensorPhoneRecords.MaxBy(x => x.World.Altitude)).World.Altitude;
         maxWorld.Longitude = (SensorPhoneRecords.MaxBy(x => x.World.Longitude)).World.Longitude;
         maxWorld.Latitude = (SensorPhoneRecords.MaxBy(x => x.World.Latitude)).World.Latitude;         
         maxWorld.TimeStampSeconds = (SensorPhoneRecords.MaxBy(x => x.World.TimeStampSeconds)).World.TimeStampSeconds;
         maxWorld.AccelX = (SensorPhoneRecords.MaxBy(x => x.World.AccelX)).World.AccelX;
         maxWorld.AccelY = (SensorPhoneRecords.MaxBy(x => x.World.AccelY)).World.AccelY;
         maxWorld.AccelZ = (SensorPhoneRecords.MaxBy(x => x.World.AccelZ)).World.AccelZ;
         maxWorld.AccelMag = (SensorPhoneRecords.MaxBy(x => x.World.AccelMag)).World.AccelMag;

         World minWorld = new World();
         minWorld.Altitude = (SensorPhoneRecords.MinBy(x => x.World.Altitude)).World.Altitude;
         minWorld.Longitude = (SensorPhoneRecords.MinBy(x => x.World.Longitude)).World.Longitude;
         minWorld.Latitude = (SensorPhoneRecords.MinBy(x => x.World.Latitude)).World.Latitude;
         minWorld.TimeStampSeconds = (SensorPhoneRecords.MinBy(x => x.World.TimeStampSeconds)).World.TimeStampSeconds;
         minWorld.AccelX = (SensorPhoneRecords.MinBy(x => x.World.AccelX)).World.AccelX;
         minWorld.AccelY = (SensorPhoneRecords.MinBy(x => x.World.AccelY)).World.AccelY;
         minWorld.AccelZ = (SensorPhoneRecords.MinBy(x => x.World.AccelZ)).World.AccelZ;
         minWorld.AccelMag = (SensorPhoneRecords.MinBy(x => x.World.AccelMag)).World.AccelMag;

         World lenWorld = new World();
         lenWorld.Altitude = Math.Abs(maxWorld.Altitude - minWorld.Altitude);
         lenWorld.Longitude = Math.Abs(maxWorld.Longitude - minWorld.Longitude);
         lenWorld.Latitude = Math.Abs(maxWorld.Latitude - minWorld.Latitude);
         lenWorld.TimeStampSeconds = Math.Abs(maxWorld.TimeStampSeconds - minWorld.TimeStampSeconds);
         lenWorld.AccelX = Math.Abs(maxWorld.AccelX - minWorld.AccelX);
         lenWorld.AccelY = Math.Abs(maxWorld.AccelY - minWorld.AccelY);
         lenWorld.AccelZ = Math.Abs(maxWorld.AccelZ - minWorld.AccelZ);
         lenWorld.AccelMag = Math.Abs(maxWorld.AccelMag - minWorld.AccelMag);

         // Could any lengths ever be really completely zero? Dont know but protect anyway
         double nearZero = 0.000001D;
         lenWorld.Altitude = (lenWorld.Altitude == 0D) ? nearZero : lenWorld.Altitude;
         lenWorld.Longitude = (lenWorld.Longitude == 0D) ? nearZero : lenWorld.Longitude;
         lenWorld.Latitude = (lenWorld.Latitude == 0D) ? nearZero : lenWorld.Latitude;
         lenWorld.TimeStampSeconds = (lenWorld.TimeStampSeconds == 0D) ? nearZero : lenWorld.TimeStampSeconds;
         lenWorld.AccelX = (lenWorld.AccelX == 0D) ? nearZero : lenWorld.AccelX;
         lenWorld.AccelY = (lenWorld.AccelY == 0D) ? nearZero : lenWorld.AccelY;
         lenWorld.AccelZ = (lenWorld.AccelZ == 0D) ? nearZero : lenWorld.AccelZ;
         lenWorld.AccelMag = (lenWorld.AccelMag == 0D) ? nearZero : lenWorld.AccelMag;

         // Step 2 calculate normalised fields, and store normalised values 0..1
         foreach (SensorPhoneRecord spr in SensorPhoneRecords)
         {
            spr.World.AltitudeN = (spr.World.Altitude - minWorld.Altitude) / lenWorld.Altitude;
            spr.World.LongitudeN = (spr.World.Longitude - minWorld.Longitude) / lenWorld.Longitude;
            spr.World.LatitudeN = (spr.World.Latitude - minWorld.Latitude) / lenWorld.Latitude;
            spr.World.TimeStampSecondsN = (spr.World.TimeStampSeconds - minWorld.TimeStampSeconds) / lenWorld.TimeStampSeconds;
            spr.World.AccelXN = (spr.World.AccelX - minWorld.AccelX) / lenWorld.AccelX;
            spr.World.AccelYN = (spr.World.AccelY - minWorld.AccelY) / lenWorld.AccelY;
            spr.World.AccelZN = (spr.World.AccelZ - minWorld.AccelZ) / lenWorld.AccelZ;
            spr.World.AccelMag = (spr.World.AccelMag - minWorld.AccelMag) / lenWorld.AccelMag;

            // Normalised magnitude of accelerometer vector can be used to create a color
            spr.World.AccelColorMagN = new Color((float)spr.World.AccelMag, 0.5f, 0.5f);

            // Normalised accelerometer vector can be used to create a color
            spr.World.AccelColorVecXYZN = new Color((float)spr.World.AccelXN, (float)spr.World.AccelYN, (float)spr.World.AccelZN);
         }

         // Step 3 sort results
         SensorPhoneRecords.Sort((x, y) => x.World.TimeStampSeconds.CompareTo(y.World.TimeStampSeconds));

         // test
         TestStr = SensorPhoneRecords[0].Sensor.C_TIMESTAMP;
         TestStr += "\n" + SensorPhoneRecords[0].World.CreatedOn.ToShortDateString();
         TestStr += "\n" + SensorPhoneRecords.Count.ToString();
      }

      private double ConvertHcpStringToDouble(string hcpDateString)
      {
         double d;        
         // Very defensive and don't care about bad data, just do what we can
         try
         {
            string unixTimeStamp = hcpDateString.Substring(6, 13);
            d = Convert.ToDouble(unixTimeStamp) / 1000D;
         }
         catch
         {
            d = 1;
         }
         return d;
      }

      private DateTime ConvertHcpStringToDate(string hcpDateString)
      {
         DateTime dt;
         // Very defensive and don't care about bad data, just do what we can
         try
         {
            double unixTimeStampD = ConvertHcpStringToDouble(hcpDateString);
            dt = UnixTimeStampToDateTime(unixTimeStampD);
         }
         catch
         {
            //print("ConvertHcpStringToDate threw exception");
            dt = DateTime.UtcNow;
         }
         return dt;
      }

      private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
      {
         // Unix timestamp is seconds past epoch
         System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
         dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
         return dtDateTime;
      }
   }
}
