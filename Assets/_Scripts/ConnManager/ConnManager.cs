using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Text;
using SimpleJSON;

namespace HcpVr
{
   public struct WwwResponse
   {
      public string WwwText;
      public string WwwError;
   }

   /// <summary>
   /// Connection manager used to store persistent data, communicate with external server.
   /// There is no UI here. Handles security and comms with HANA and all JSON parsing.
   /// Usage to get data:
   /// - Other objects call GetCloudData_* to request some specific data
   /// - Once the data arrives the event OnCloud*DataChanged is raised 
   /// - Other objects can then call GetCloudResponse_* to get the actual data as an object 
   /// </summary>
   public partial class ConnManager : MonoBehaviour
   {
      public event EventHandler<EventArgs> OnCloudDataChangedMassList = delegate { };
      public event EventHandler<EventArgs> OnConnNetCheckReturned = delegate { };
      //public event EventHandler<EventArgs> OnServerUpReturned = delegate {};
      public event EventHandler<EventArgs> OnLoginCredReturned = delegate { };

      public ConfigurationVr ConfigurationVr { get { return _configurationVr; } }
      private ConfigurationVr _configurationVr;

      private enum WwwDataType
      {
         CloudMassList,
         ConnNetCheck,
         //ConnServerUp,
         ConnLoginCred,
      }

      private static ConnManager _instance;

      /// <summary>
      /// Flag true if connection manager has already been created
      /// </summary>
      private bool _isCreated = false;

      private string _serviceUrl;
      private string _username;
      private string _password;
      private bool _isOffline;

      private string _cloudDataMassList = "empty";

      private WwwResponse _connDataNetCheckResponse;
      //private WwwResponse _connDataServerUpResponse;
      private WwwResponse _connDataLoginCredResponse;

      /// <summary>
      /// Creates an instance of gamestate as a gameobject if an instance does not exist
      /// you'll see the gamestate instance appear in the hierarchy.
      /// </summary>
      public static ConnManager Instance
      {
         get
         {
            if (_instance == null)
            {
               // create a gamestate instance as a GameObject if one does not exist (this prevents you creating multiple
               // instances which you don't want), and AddComponent this script component (gamestate) to it
               _instance = new GameObject("gamestate").AddComponent<ConnManager>();
            }
            return _instance;
         }
      }

      void Awake()
      {
      }

      void OnApplicationPause()
      {
      }

      /// <summary>
      /// Calls server and asynchronously gets data, raising appropriate event On*Changed when done.
      /// Results can later be retrieved using GetCloudResponse_MassList (which does parsing on demand).
      /// </summary>
      public void GetCloudData_MassList()
      {         
         string url = _serviceUrl + @"?$top=" + _configurationVr.MaxHcpRecordsToRetrieve + "&$format=json";
         // Asynch call will eventually populate _cloudDataMassList
         GetWwwData(url, WwwDataType.CloudMassList);
      }

      public void GetConnData_NetCheck()
      {
         //print("Net check requested);
         string url = @"http://www.google.com";
         // Asynch call will eventually populate _connDataNetCheckResponse
         GetWwwData(url, WwwDataType.ConnNetCheck);
      }

      //public void GetConnData_ServerUp(string host, string instance)
      //{
      //   string url = @"http://" + host + @":80" + instance + @"/";
      //   // Asynch call will eventually populate _connDataServerUpResponse
      //   GetWwwData(url, WwwDataType.ConnServerUp);
      //}

      public void GetConnData_LoginCred()
      {
         // Call OData service for just one record to see if login works
         string url = _serviceUrl + @"?$top=1&$format=json";
         // Asynch call will eventually populate _connDataLoginCredResponse
         GetWwwData(url, WwwDataType.ConnLoginCred);
      }

      private void GetWwwData(string url, WwwDataType wwwDataType)
      {
         // offline handling, fakes population of _epmData* values and raises events
         if (_isOffline)
         {
            // and raise events
            switch (wwwDataType)
            {
               case WwwDataType.CloudMassList:
                  // Cloud mass list offline www call
                  _cloudDataMassList = GetOfflineData_CloudMassList();
                  OnCloudDataChangedMassList(null, EventArgs.Empty);
                  break;

               case WwwDataType.ConnNetCheck:
                  _connDataNetCheckResponse.WwwText = "some google text";
                  _connDataNetCheckResponse.WwwError = null;
                  OnConnNetCheckReturned(this, null);
                  break;

               //case WwwDataType.ConnServerUp:
               //   _connDataServerUpResponse.WwwText = "some server text";
               //   _connDataServerUpResponse.WwwError = null;
               //   OnServerUpReturned(this, null);
               //   break;

               case WwwDataType.ConnLoginCred:
                  _connDataLoginCredResponse.WwwText = "some login cred text";
                  _connDataLoginCredResponse.WwwError = null;
                  OnLoginCredReturned(this, null);
                  break;

               default:
                  break;
            }
            return;
         }

         // normal online use
         try
         {
            WWW www = null;
            System.Collections.Generic.Dictionary<string, string> d = null;

            if (wwwDataType == WwwDataType.ConnNetCheck)
            {
               //---------------------------------------------------------
               // Without login
               //---------------------------------------------------------
               print("WWW ------------ calling now: " + url);
               www = new WWW(url, null, d);
            }
            else
            {
               //---------------------------------------------------------
               // With login 
               //---------------------------------------------------------
               // prep headers
               //Hashtable headers = new Hashtable();
               //headers["Accept-Language"] = "en-US,en;q=0.8"; // prevents a 500 error saying "Multiple resources found. Inconsistency between data model and service description found"
               //string userAndPasswordCombo = _username + ":" + _password;
               //byte[] bytesToEncode = Encoding.UTF8.GetBytes(userAndPasswordCombo);
               //string encodedText = Convert.ToBase64String(bytesToEncode);
               //headers["Authorization"] = "Basic " + encodedText;
               //print("WWW ------------ calling now: " + url);
               //www = new WWW(url, null, headers);
               d = new System.Collections.Generic.Dictionary<string, string>();
               d.Add("Accept-Language", "en-US,en;q=0.8"); // prevents a 500 error saying "Multiple resources found. Inconsistency between data model and service description found"
               string userAndPasswordCombo = _username + ":" + _password;
               byte[] bytesToEncode = Encoding.UTF8.GetBytes(userAndPasswordCombo);
               string encodedText = Convert.ToBase64String(bytesToEncode);
               d.Add("Authorization", "Basic " + encodedText);

               print("WWW ------------ calling now: " + url);
               www = new WWW(url, null, d);
            }
            StartCoroutine(WaitForRequest(www, wwwDataType));
         }
         catch (Exception ex)
         {
            print("An error occurred starting url: " + ex.Message);
         }
      }

      string TruncateWwwAsRequired(string fullText, WwwDataType wwwDataType)
      {
         string s = "";
         switch (wwwDataType)
         {
            case WwwDataType.ConnNetCheck:
               // the google response is quite long
               s = fullText.Substring(0, 20) + " (string truncated)";
               break;

            //case WwwDataType.ConnServerUp:
            //   // pinging the web ide is long
            //   s = fullText.Substring(0,20) + " (string truncated)";
            //   break;

            //case WwwDataType.ConnLoginCred:
            //   // this is pro a hana login page
            //   s = fullText.Substring(0,20) + " (string truncated)";
            //   break;

            default:
               // no truncations
               s = fullText;
               break;
         }
         return s;
      }

      /// <summary>
      /// Waits for request, then stores raw string result in _cloudData* and raises event
      /// No parsing of results happens here, the results are parsed on demand when the
      /// GetCloudResponse_* methods are called.
      /// </summary>
      IEnumerator WaitForRequest(WWW www, WwwDataType wwwDataType)
      {
         yield return www;
         print("WWW ------------ returned");
         string wwwResult = "";
         bool isWwwAdditionalError = false;
         string wwwAdditionalError = "";
         bool isWwwOk = false;

         //-------------------------------------------------------------------------
         // Insert our own errors
         //-------------------------------------------------------------------------
         // check for failed login, it is a www ok request, but body contains a login page
         // the login page would be acceptable for a server ping check
         // if (wwwDataType != WwwDataType.ConnServerUp && www.text.Contains(@"<title>HANA Login</title>"))
         if (www.text.Contains(@"<title>HANA Login</title>"))
         {
            isWwwAdditionalError = true;
            wwwAdditionalError = "HCP Login Failed - User or Pwd wrong";
         }

         // Error check
         if (www.error == null && !isWwwAdditionalError)
         {
            // all is ok
            string s = TruncateWwwAsRequired(www.text, wwwDataType);
            print("WWW Ok: " + s);
            wwwResult = www.text;
            isWwwOk = true;
         }
         else if (!isWwwAdditionalError)
         {
            // standard error (eg field name is wrong case)
            print("WWW Error: " + www.error);
            wwwResult = www.error;
            isWwwOk = false;
         }
         else
         {
            // own additional error (eg login failed) 
            print("WWW Additional Error: " + wwwAdditionalError);
            wwwResult = wwwAdditionalError;
            isWwwOk = false;
         }

         //-------------------------------------------------------------------------
         // use dataType to decide where to put data
         //-------------------------------------------------------------------------
         switch (wwwDataType)
         {
            case WwwDataType.CloudMassList:
               if (isWwwOk)
               {
                  _cloudDataMassList = wwwResult;
                  OnCloudDataChangedMassList(null, EventArgs.Empty);
               }
               break;

            case WwwDataType.ConnNetCheck:
               _connDataNetCheckResponse.WwwText = wwwResult;
               _connDataNetCheckResponse.WwwError = www.error;
               OnConnNetCheckReturned(this, null);
               break;

            //case WwwDataType.ConnServerUp:
            //   _connDataServerUpResponse.WwwText = wwwResult;
            //   _connDataServerUpResponse.WwwError = www.error;
            //   OnServerUpReturned(this, null);
            //   break;

            case WwwDataType.ConnLoginCred:
               _connDataLoginCredResponse.WwwText = wwwResult;
               _connDataLoginCredResponse.WwwError = www.error;
               if (isWwwAdditionalError)
               {
                  _connDataLoginCredResponse.WwwError = wwwResult;
               }
               OnLoginCredReturned(this, null);
               break;

            default:
               break;
         }
      }

      // Sets the instance to null when the application quits
      public void OnApplicationQuit()
      {
         _instance = null;
      }

      /// <summary>
      /// Starts new game state. sceneName is an actual scene to start with, not a login screen
      /// </summary>
      public void StartGameNewState(string serviceUrl,
                                    string username, string password,
                                    ConfigurationVr cvr,
                                    bool isOffline, string sceneName)
      {
         if (_isCreated)
            return;

         Debug.Log("Starting game... " +
                   " serviceUrl: " + serviceUrl +
                   " user: " + username + " offline: " + isOffline, this);

         //_verTranslator = new VersionTranslator(hanaVersion, host, instance);
         _isCreated = true;
         _username = username;
         _password = password;
         _isOffline = isOffline;
         _configurationVr = cvr;

         // Load scene (an actual scene, not a login screen)
         if (!string.IsNullOrEmpty(sceneName))
         {
            //Cursor.visible = false;
            SceneManager.LoadScene(sceneName);
         }
      }

      public void SetOffineMode(bool isOffline)
      {
         _isOffline = isOffline;
      }

      /// <summary>
      /// Set some minimum fields so can login with www calls to check login id works
      /// </summary>
      public void StartGamePartial(string serviceUrl, string username, string password)
      {
         _serviceUrl = serviceUrl;
         _username = username;
         _password = password;
      }

      public CloudDataMassList GetCloudResponse_MassList()
      {
         if (string.IsNullOrEmpty(_cloudDataMassList) || _cloudDataMassList == "empty")
         {
            return null;
         }
         else
         {
            var N = JSON.Parse(_cloudDataMassList);

            // if server not up, then _cloudDataMassList will contain error string, not parsable JSON
            if (N == null)
            {
               print("ConnManager.GetCloudResponse_MassList failed to parse: " + _cloudDataMassList);
               // only log above once:
               _cloudDataMassList = null;
               return null;
            }

            // Build Sensorphone records
            CloudDataMassList cloudDataMass = new CloudDataMassList();
            for (int i = 0; i < _configurationVr.MaxHcpRecordsToRetrieve; i++)
            {
               string scout = N["d"]["results"][i]["G_DEVICE"].Value;
               if (String.IsNullOrEmpty(scout))
               {
                  // We didnt get all the records we asked for, leave early
                  break;
               }
               else
               {
                  SensorPhoneRecord spr = new SensorPhoneRecord()
                  {
                     Sensor = new Sensor()
                     {
                        G_DEVICE = N["d"]["results"][i]["G_DEVICE"].Value, //"2ad39033-a0cc-4a19-b6ec-33f97a4f216f"
                        G_CREATED = N["d"]["results"][i]["G_CREATED"].Value, //"/Date(1465941656507)/"
                        C_TIMESTAMP = N["d"]["results"][i]["C_TIMESTAMP"].Value, //"/Date(1465941485000)/"
                        C_DEVICE = N["d"]["results"][i]["C_DEVICE"].Value, //"iOS Device"
                        C_GYROSCOPEX = N["d"]["results"][i]["C_GYROSCOPEX"].Value, //"-0.1265521794557571"
                        C_GYROSCOPEY = N["d"]["results"][i]["C_GYROSCOPEY"].Value, //"0.08579760044813156"
                        C_GYROSCOPEZ = N["d"]["results"][i]["C_GYROSCOPEZ"].Value, //"-0.06093422323465347"
                        C_ACCELEROMETERX = N["d"]["results"][i]["C_ACCELEROMETERX"].Value, //"-0.0042724609375"
                        C_ACCELEROMETERY = N["d"]["results"][i]["C_ACCELEROMETERY"].Value, //"-0.7014465061947703"
                        C_ACCELEROMETERZ = N["d"]["results"][i]["C_ACCELEROMETERZ"].Value, //"-0.673080462962389"
                        C_ALTITUDE = N["d"]["results"][i]["C_ALTITUDE"].Value, //"12.53634262084961"
                        C_LONGITUDE = N["d"]["results"][i]["C_LONGITUDE"].Value, //"-0.2245472464712565"
                        C_LATITUDE = N["d"]["results"][i]["C_LATITUDE"].Value, //"51.41115630050321"
                        C_AUDIO = N["d"]["results"][i]["C_AUDIO"].Value //"0"
                     },
                     World = new World() { }
                  };
                  cloudDataMass.SensorPhoneRecords.Add(spr);
               }
            }

            // Transform sensor records to produce values suitable for VR world space
            cloudDataMass.TransformToWorld();

            return cloudDataMass;
         }
      }

      public WwwResponse GetConnResponse_NetCheck()
      {
         return _connDataNetCheckResponse;
      }

      //public WwwResponse GetConnResponse_ServerUp()
      //{
      //   return _connDataServerUpResponse;
      //}

      public WwwResponse GetConnResponse_LoginCred()
      {
         return _connDataLoginCredResponse;
      }
   }
}