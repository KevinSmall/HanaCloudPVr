using UnityEngine;
using System;
using System.Collections;
using System.Text;

namespace HcpVr
{
   /// <summary>
   /// Login screen to get HANA login info, create singleton ConnManager, and start initial scene.  
   /// Usually attached in the logingui scene. HANA login info stored in PlayerPrefs.
   /// Script can be attached to a dummy game object in any scene to have values defaulted in during 
   /// testing (allows any scene to be run directly without actually going via login screen) to do this
   /// see _isRunImmediatelySkipGui
   /// </summary>
   public class LoginGui : MonoBehaviour
   {
      /// <summary>
      /// Desire to run script immediately during testing, set in the inspector.
      /// This can be attached to a dummy object in a scene to allow the whole login screen 
      /// to be skipped immediately - and all defaults are for offline
      /// </summary>
      public bool IsRunImmediatelySkipGui;

      /// <summary>
      /// Flag if the script was called from the proper main GameLogin screen (scene)
      /// </summary>
      public static bool IsCalledFromLoginScreen;

      /// <summary>
      /// The scene to load when finished (ignored if _isRunImmediatelySkipGui is true)
      /// </summary>
      public string SceneToLoadWhenFinished;
      public GUISkin ThisGUISkin;

      // GuiSpace is abstract coord system, ScreenSpace is actual device pixels
      private Rect _mainWindowGuiSpace;
      private Rect _mainWindowScreenSpace;
      private float _mainWindowAspectRatio;
      /// <summary>
      /// x factor to convert Gui space to Screen space along x axis
      /// </summary>
      private float _xf;
      /// <summary>
      /// y factor to convert Gui space to Screen space along y axis
      /// </summary>
      private float _yf;
      /// <summary>
      /// Offset in screenspace for x positions
      /// </summary>
      private float _xo;
      /// <summary>
      /// Offset in screenspace for y positions
      /// </summary>
      private float _yo;

      private string _gameTitle = "HCP System Details";
      private string _gameVersion = "1.10";

      private const string _epmServiceUrlDefault = @"https://iotmmsp123456trial.hanatrial.ondemand.com/com.sap.iotservices.mms/v1/api/http/app.svc/T_IOT_49DB33E129EAE6C5E44BB";
      private const string _usernameDefault = "p123456"; //"SYSTEM";
      private const string _passwordDefault = "password";            
      private string _epmServiceUrlToEdit = _epmServiceUrlDefault;      
      private string _usernameToEdit = _usernameDefault;
      private string _passwordToEdit = _passwordDefault;
      private bool _epmIsOfflineToEdit = false;
      private bool _isItOkToPressPlayButton = false;
      /// <summary>
      /// connection log displayed on gui
      /// </summary>
      private string _connLog;

      private enum ConnCheckState
      {
         Idle,
         NetCheck,
         LoginCred,
         CheckPassed,
         CheckFailed
      }
      private ConnCheckState _connCheckState;

      //---- CONFIG SETTINGS ----
      // Max records to retrieve from HCP
      private static int _maxRecordsRetrievedDefault = 300;
      private int _maxRecordsRetrievedToEdit = _maxRecordsRetrievedDefault;
      
      // World space time (replay time), x y and z
      private static int _maxTimeSecondsDefault = 20;
      private int _maxTimeSecondsToEdit = _maxTimeSecondsDefault;

      private static int _maxYHeightDefault = 6;
      private int _maxYHeightToEdit = _maxYHeightDefault;

      private static int _maxZDistanceDefault = 20;
      private int _maxZDistanceToEdit = _maxZDistanceDefault;

      private static int _maxXWidthDefault = 10;
      private int _maxXWidthToEdit = _maxXWidthDefault;

      private static bool _smoothTimeDefault = true;
      private static bool _smoothTimeToEdit = _smoothTimeDefault;

      //---- CONFIG SETTINGS ----

      void Awake()
      {
         _mainWindowGuiSpace = new Rect(20, 20, 640, 520);
         _mainWindowAspectRatio = _mainWindowGuiSpace.width / _mainWindowGuiSpace.height;
      }

      // Use this for initialization
      void Start()
      {
         ConnManager.Instance.OnConnNetCheckReturned += OnConnNetCheckReturned;
         //ConnManager.Instance.OnServerUpReturned += OnServerUpReturned;
         ConnManager.Instance.OnLoginCredReturned += OnLoginCredReturned;         
         _connCheckState = ConnCheckState.Idle;

         // if this flag in inspector is false, we must have been called from real GameLogin scene
         if (IsRunImmediatelySkipGui == false)
         {
            IsCalledFromLoginScreen = true;
         }

         // these defaults also set by the "set to defaults" button
         SetDefaults();

         // Sort out scaling factors for screen space vs GUI space
         // Report GUI space

         _mainWindowGuiSpace = new Rect(20, 20, 640, 520);
         string s = "Main Window Gui Space x y width height: " + _mainWindowGuiSpace.x.ToString() + " " + _mainWindowGuiSpace.y.ToString() + " " +
            _mainWindowGuiSpace.width.ToString() + " " + _mainWindowGuiSpace.height.ToString();
         //ConnLog(s);
         //print(s);

         // Get device size
         s = "Device Screen width height: " + Screen.width.ToString() + " " + Screen.height.ToString();
         //ConnLog(s);
         //print(s);

         // Work out the scaling
         Rect deviceScreenSpace = new Rect(0, 0, Screen.width, Screen.height);
         _mainWindowScreenSpace = deviceScreenSpace.GetLargestRectangle(_mainWindowAspectRatio, 1f);
         s = "Main Window Screen Space x y width height: " + _mainWindowScreenSpace.x.ToString() + " " + _mainWindowScreenSpace.y.ToString() + " " +
            _mainWindowScreenSpace.width.ToString() + " " + _mainWindowScreenSpace.height.ToString();
         //ConnLog(s);
         //print(s);

         _xf = _mainWindowScreenSpace.width / _mainWindowGuiSpace.width;
         _yf = _mainWindowScreenSpace.height / _mainWindowGuiSpace.height;
         _xo = _mainWindowScreenSpace.x;
         _yo = _mainWindowScreenSpace.y;
         s = "xf yf xo yo: " + _xf.ToString() + " " + _yf + " " + _xo + " " + _yo;
         //ConnLog(s);
         //print(s);

         // game version contains version info
#if UNITY_EDITOR
         _gameVersion += ": Editor";
#elif UNITY_WEBPLAYER
      _gameVersion += ": Web Player";
#elif UNITY_STANDALONE
      _gameVersion += ": Standalone";
#elif UNITY_IOS
      _gameVersion += ": iOS";
#elif UNITY_ANDROID
      _gameVersion += ": Android";
#elif UNITY_WEBGL
      _gameVersion += ": WebGL";
#else
      _gameVersion += ": Unknown";
#endif

         if (IsRunImmediatelySkipGui)
         {
            // cant set this, it means later calls appear to be offline _epmIsOfflineToEdit = true;
            SceneToLoadWhenFinished = null;  // if we are skipping gui, leave scene alone
            StartGame();
         }
      }

      private void SetDefaults()
      {
         _connLog = "";

         // get values from PlayerPrefs if possible
         _epmServiceUrlToEdit = PlayerPrefs.GetString("service", _epmServiceUrlDefault);
         _usernameToEdit = PlayerPrefs.GetString("username", _usernameDefault);
         _passwordToEdit = PlayerPrefs.GetString("password", _passwordDefault);

#if !UNITY_EDITOR
      _epmIsOfflineToEdit = false;
#endif
      }

      // Startscreen GUI
      // OnGUI is called for rendering and handling GUI events.
      // This means that your OnGUI implementation might be called several times per frame (one call per event). 
      // http://docs.unity3d.com/Documentation/Manual/GUIScriptingGuide.html
      void OnGUI()
      {
         if (IsRunImmediatelySkipGui)
            return;

         GUI.skin = ThisGUISkin;
         _mainWindowScreenSpace = GUI.Window(0, _mainWindowScreenSpace, DisplayWindow, _gameTitle, GUI.skin.GetStyle("window"));
      }

      void DisplayWindow(int windowID)
      {
         bool IsItOkToPressCheckButton = true;

         // screen space (ss prefix) coords and sizes
         int ssx_col1 = 20;
         int ssx_col2 = 120;
         int ssx_col3 = 445;
         int ssy_top = 30;
         int ssy_lh = 30; // line height
         int ssy_lg = 8; // line gap
         int ssbut_h = 36; // button height
         int ssbut_w = 150; // button width
         int ssyit = 0; // screen space y iterator runs down window top to bottom

         // scale font
         int fontSize = (int)(16 * ((_xf + _yf) / 2));
         GUI.skin.textField.fontSize = fontSize;
         GUI.skin.textArea.fontSize = fontSize;
         GUI.skin.label.fontSize = fontSize;
         GUI.skin.button.fontSize = fontSize;
         GUI.skin.toggle.fontSize = fontSize;

         // Service
         ssyit = ssy_top;
         GUI.Label(new Rect(ssx_col1 * _xf, ssyit * _yf, 100 * _xf, 30 * _yf), "Service URL:");
         _epmServiceUrlToEdit = GUI.TextArea(new Rect(ssx_col2 * _xf, ssyit * _yf, 470 * _xf, 50 * _yf), _epmServiceUrlToEdit);
         ssyit += (2 * ssy_lh) + ssy_lg;

         // User
         GUI.Label(new Rect(ssx_col1 * _xf, ssyit * _yf, 100 * _xf, 30 * _yf), "Username:");
         _usernameToEdit = GUI.TextField(new Rect(ssx_col2 * _xf, ssyit * _yf, 120 * _xf, 26 * _yf), _usernameToEdit, 25);
         ssyit += ssy_lh + ssy_lg;

         // Password
         GUI.Label(new Rect(ssx_col1 * _xf, ssyit * _yf, 100 * _xf, 30 * _yf), "Password:");
         _passwordToEdit = GUI.PasswordField(new Rect(ssx_col2 * _xf, ssyit * _yf, 120 * _xf, 26 * _yf), _passwordToEdit, "*"[0], 25);
         ssyit += ssy_lh + ssy_lg;

         // Tech check, are there enough fields to be able to check?
         if (string.IsNullOrEmpty(_usernameToEdit) || string.IsNullOrEmpty(_passwordToEdit) ||
            string.IsNullOrEmpty(_epmServiceUrlToEdit))
         {
            IsItOkToPressCheckButton = false;
         }

         // Process check, are we already busy?
         if (_connCheckState == ConnCheckState.NetCheck ||
             _connCheckState == ConnCheckState.LoginCred)
         {
            IsItOkToPressCheckButton = false;
         }

         // ---[ CHECK CONNECTION ]---
         // disable button if fields not all filled
         GUI.enabled = IsItOkToPressCheckButton;
         if (GUI.Button(new Rect(ssx_col2 * _xf, ssyit * _yf, ssbut_w * _xf, ssbut_h * _yf), "Check Connection"))
         {
            // tidy strings when CHECK is pressed
            _usernameToEdit = _usernameToEdit.Trim();
            // service should not end with /
            if (_epmServiceUrlToEdit.EndsWith("/"))
            {
               _epmServiceUrlToEdit = _epmServiceUrlToEdit.Trim('/');
            }            
            // store prefs
            PlayerPrefs.SetString("service", _epmServiceUrlToEdit);
            PlayerPrefs.SetString("username", _usernameToEdit);
            PlayerPrefs.SetString("password", _passwordToEdit);
            CheckConnection();
         }
         GUI.enabled = true;

         // ---[ OFFLINE ]---
         //if (Debug.isDebugBuild)
         {
            _epmIsOfflineToEdit = GUI.Toggle(new Rect(ssx_col3 * _xf, ssyit * _yf, ssbut_w * _xf, ssbut_h * _yf), _epmIsOfflineToEdit, "Offline");
         }
         ssyit += ssy_lh + ssy_lg + ssy_lg;

         // ---[ START ]---
         if (_epmIsOfflineToEdit == true)
         {
            _isItOkToPressPlayButton = true;
         }
         GUI.enabled = _isItOkToPressPlayButton;
         if (GUI.Button(new Rect(ssx_col2 * _xf, ssyit * _yf, ssbut_w * _xf, ssbut_h * _yf), "Start"))
         {            
            StartGame();
         }
         GUI.enabled = true;

         // ---[ RESET ]---
         if (GUI.Button(new Rect(ssx_col3 * _xf, ssyit * _yf, ssbut_w * _xf, ssbut_h * _yf), "Reset to Defaults"))
         {
            PlayerPrefs.DeleteKey("service");
            PlayerPrefs.DeleteKey("username");
            PlayerPrefs.DeleteKey("password");
            SetDefaults();
         }
         ssyit += ssy_lh + ssy_lg + ssy_lg;

         // CONNECTION CHECK LOG
         GUI.TextArea(new Rect(ssx_col2 * _xf, ssyit * _yf, 473 * _xf, 200 * _yf), _connLog);

         // Version label (hardcoded y location)
         GUI.Label(new Rect(ssx_col3 * _xf, 480 * _yf, 180 * _xf, 30 * _yf), "Version " + _gameVersion);
      }
   

      private void StartGame()
      {
         // tell Unity not to destroy our gamestate instance when we load a new scene, so that it will persist between scene transitions.
         // calling gamestate.Instance creates the singleton
         DontDestroyOnLoad(ConnManager.Instance);

         // force offline if we're running gameplay scene directly, and not going via login screen
         if (IsRunImmediatelySkipGui && !LoginGui.IsCalledFromLoginScreen)
         {
            print("WARNING: Forcing offline mode, scene run without GameLogin scene");
            _epmIsOfflineToEdit = true;
            ConnManager.Instance.SetOffineMode(true);
         }
         // start new game state
         ConfigurationVr cvr = new ConfigurationVr()
         {
            MaxHcpRecordsToRetrieve = _maxRecordsRetrievedToEdit,
            MaxTimeSeconds = _maxTimeSecondsToEdit,
            SmoothTime = _smoothTimeToEdit,
            MaxYHeight = _maxYHeightToEdit,
            MaxZDistance = _maxZDistanceToEdit,
            MaxXWidth = _maxXWidthToEdit,
         };
         ConnManager.Instance.StartGameNewState(_epmServiceUrlToEdit,
                                                _usernameToEdit, _passwordToEdit,
                                                cvr,
                                                _epmIsOfflineToEdit, SceneToLoadWhenFinished);
      }

      private void CheckConnection()
      {
         _connLog = "";

         if (_epmIsOfflineToEdit)
         {
            ConnManager.Instance.SetOffineMode(_epmIsOfflineToEdit);
            _connCheckState = ConnCheckState.CheckPassed;
            ConnLog("No checks needed since Offline mode selected.");
            return;
         }
         else
         {
            ConnManager.Instance.StartGamePartial(_epmServiceUrlToEdit, _usernameToEdit, _passwordToEdit);
         }

         // Trigger the first connection check
         _connCheckState = ConnCheckState.NetCheck;
         ConnLog("Checking internet connection...");

         // This asynchronously raises event when NetCheck done, and we've hooked event OnConnNetCheckReturned
         // to process results and trigger next check
         ConnManager.Instance.GetConnData_NetCheck();

         // this just for testing
         //_isItOkToPressPlayButton = true;
      }

      private void OnConnNetCheckReturned(object sender, EventArgs e)
      {
         WwwResponse r = ConnManager.Instance.GetConnResponse_NetCheck();

         //print("GameLogin has received event OnConnNetCheckReturned, it contained" + r.WwwError);
         if (String.IsNullOrEmpty(r.WwwError))
            ConnLog("Internet connection ok.");
         else
         {
            ConnLog("Internet connection not ok.");
            ConnLog("Error: " + r.WwwError);
         }

         // Trigger a next connection check
          _connCheckState = ConnCheckState.LoginCred;
         ConnLog("Checking to see if specified HCP OData service can be accessed...");
         ConnManager.Instance.GetConnData_LoginCred();
      }

      //private void OnServerUpReturned(object sender, EventArgs e)
      //{
      //   WwwResponse r = GameManager.Instance.GetConnResponse_ServerUp();
      //   //print("GameLogin has received event OnConnNetCheckReturned, it contained" + r.WwwError);
      //   if (String.IsNullOrEmpty(r.WwwError))
      //   {
      //      ConnLog("HANA server pinged ok");
      //      // Trigger the next connection check
      //      _connCheckState = ConnCheckState.LoginCred;
      //      ConnLog("Checking login credentials...");
      //      GameManager.Instance.GetConnData_LoginCred();
      //   }
      //   else
      //   {
      //      ConnLog("HANA server ping failed, error: " + r.WwwError);
      //      _connCheckState = ConnCheckState.CheckFailed;
      //      ConnLog("If the internet connection check was ok, this means host or instance is wrong, or server is down");
      //   }
      //}

      private void OnLoginCredReturned(object sender, EventArgs e)
      {
         WwwResponse r = ConnManager.Instance.GetConnResponse_LoginCred();
         if (String.IsNullOrEmpty(r.WwwError))
         {
            ConnLog("Specified HCP OData service can be accessed ok.");
            // Trigger further connection checks here, else all done
            _connCheckState = ConnCheckState.CheckPassed;
            ConnLog("Checks completed ok.");
            ConnLog("---------------------------------------------");
            ConnLog("Press Start Button, then insert phone into the Cardboard headset.");
            _isItOkToPressPlayButton = true;            
         }
         else
         {
            ConnLog("Specified HCP OData Service could not be accessed.");
            ConnLog("Error: " + r.WwwError);
            //ConnLog("Cause may be server down, incorrect username or password, or missing authorisations.");
            _connCheckState = ConnCheckState.CheckFailed;
         }
      }

      private void ConnLog(string s)
      {
         if (String.IsNullOrEmpty(_connLog))
            _connLog = s;
         else
            _connLog = _connLog + "\n" + s;
      }

      // Update is called once per frame
      void Update()
      {

      }
   }
}