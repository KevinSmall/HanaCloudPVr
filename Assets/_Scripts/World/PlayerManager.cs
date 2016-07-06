using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace HcpVr
{
   /// <summary>
   /// Controls player movement, screen overlay
   /// </summary>
   public class PlayerManager : MonoBehaviour
   {
      public Text VrHud;

      // Use this for initialization
      void Start()
      {
         GvrViewer.Instance.OnTrigger += OnGvrTriggered;

         // hide screen overlay if we're not in VR mode
         if (!GvrViewer.Instance.VRModeEnabled)
         {
            VrHud.enabled = false;
         }
      }

      private void OnGvrTriggered()
      {
         MoveForwards();
      }

      public void MoveForwards()
      {
         Vector3 fwd = Camera.main.transform.forward;
         //Vector3 actualForceDir = new Vector3(desiredTerrainDir.x, 2f, desiredTerrainDir.z);
         GetComponent<Rigidbody>().AddForce(fwd * 12f);

         //print("moving");
      }

      // Update is called once per frame
      void Update()
      {
      }
   }
}