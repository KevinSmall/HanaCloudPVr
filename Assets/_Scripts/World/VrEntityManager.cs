using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HcpVr
{
   /// <summary>
   /// Each data point from HCP is called a "VR entity" and there is one of these 
   /// scripts attached to each of them. It is the brain of the VrEntity.
   /// </summary>
   class VrEntityManager : MonoBehaviour, IGvrGazeResponder
   {
      public SensorPhoneRecord SensorPhoneRecord;

      private Color _startingColor;
      private Text _vrHud;
      //private Shader _shaderNotHighlighted;
      //private Shader _shaderHighlighted;

      public void Start()
      {         
         GetComponent<Renderer>().material.color = SensorPhoneRecord.World.AccelColorVecXYZN;
         //GetComponent<Renderer>().material.color = new Color(0.5f,0.5f, (float)SensorPhoneRecord.World.AccelZN);
         //GetComponent<Renderer>().material.color = SensorPhoneRecord.World.AccelColorMagN;

         _startingColor = GetComponent<Renderer>().material.color;
         MakeAppearanceNoise();

         GameObject go = GameObject.FindGameObjectWithTag("VrHud");
         if (go != null)
         {
            _vrHud = go.GetComponent<Text>();
         }

         //_shaderNotHighlighted = gameObject.GetComponent<Renderer>().material.shader; //Shader.Find(@"Diffuse");
         //_shaderHighlighted = Shader.Find(@"Self-Illumin/BumpedSpecular");
      }

      private void MakeAppearanceNoise()
      {
         // TODO CLOUD implement appearance noise
         //GetComponent<AudioSource>().clip = AudioHopping[UnityEngine.Random.Range(0, AudioHopping.Count)];
         //GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.9f, 1.1f);
         //GetComponent<AudioSource>().volume = 0.55f;
         //GetComponent<AudioSource>().Play();
      }

      public void SetGazedAt(bool gazedAt)
      {
         // Highlight color effect
         if (gazedAt)
         {
            //GetComponent<Renderer>().material.shader = _shaderHighlighted;
            GetComponent<Renderer>().material.color = Color.green;
         }
         
         else
         {
            //GetComponent<Renderer>().material.shader = _shaderNotHighlighted;
            GetComponent<Renderer>().material.color = _startingColor;
         }

         // VR HUD
         if (_vrHud != null)
         {
            if (gazedAt)
            {
               string s = SensorPhoneRecord.World.ShortDate + "\n" + SensorPhoneRecord.World.ShortTime;
               _vrHud.text = s;
            }
            else
            {
               _vrHud.text = "";
            }            
         }

         // AUDIO
         if (gazedAt)
         {
            GetComponent<GvrAudioSource>().Play();
         }
      }

      public void DoSomething()
      {

      }

      /// Called when the user is looking on a GameObject with this script,
      /// as long as it is set to an appropriate layer (see GvrGaze).
      public void OnGazeEnter()
      {
         SetGazedAt(true);
      }

      /// Called when the user stops looking on the GameObject, after OnGazeEnter
      /// was already called.
      public void OnGazeExit()
      {
         SetGazedAt(false);
      }

      /// Called when the viewer's trigger is used, between OnGazeEnter and OnGazeExit.
      public void OnGazeTrigger()
      {
         DoSomething();
      }
   }
}
