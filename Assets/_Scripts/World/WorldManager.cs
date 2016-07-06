using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HcpVr
{
   /// <summary>
   /// Controls creation of VrEntities in the World
   /// </summary>
   public class WorldManager : MonoBehaviour
   {
      public GameObject VrEntityTemplate;

      private ConfigurationVr _configurationVr;
      private CloudDataMassList _cloudDataMassList;
      private int _entitiesCreatedSoFar;

      void Start()
      {
         print("RenderManager starting...");
         ConnManager.Instance.OnCloudDataChangedMassList += OnCloudDataChangedMassList;
         _configurationVr = ConnManager.Instance.ConfigurationVr;
         ConnManager.Instance.GetCloudData_MassList();
      }

      private void OnCloudDataChangedMassList(object sender, EventArgs e)
      {
         print("RenderManager has received event OnCloudDataChangedMassList");
         _cloudDataMassList = ConnManager.Instance.GetCloudResponse_MassList();

         GameObject go = GameObject.Find("VrTextShowStuff");
         //(Text)
         if (go != null && go.GetComponent<Text>())
         {
            go.GetComponent<Text>().text = _cloudDataMassList.TestStr;
         }

         DoProductionRun();
      }

      private void DoProductionRun()
      { 
         if (_cloudDataMassList == null || _cloudDataMassList.SensorPhoneRecords == null || 
            _cloudDataMassList.SensorPhoneRecords.Count == 0)
         {
            print("RenderManager production run - nothing to do");
            return;
         }

         print("RenderManager production run starting...");
         _entitiesCreatedSoFar = 0;
         CreateVrEntity();
      }

      private void CreateVrEntity()
      {
         //-------------------------------------------------------------------------
         // Instantiate gameobject
         //-------------------------------------------------------------------------
         SensorPhoneRecord spr = _cloudDataMassList.SensorPhoneRecords[_entitiesCreatedSoFar];

         GameObject gParent = GameObject.FindWithTag("VrEntityBucket");
         Vector3 spawnPosition = new Vector3(
            (float)(spr.World.LatitudeN * (float)_configurationVr.MaxXWidth),
            (float)(spr.World.AltitudeN * (float)_configurationVr.MaxYHeight + 0.6f),
            (float)(spr.World.LongitudeN * (float)_configurationVr.MaxZDistance));
         //print("RenderManager Creating VR Entity " + _entitiesCreatedSoFar + " at X Y Z " + spawnPosition.x + " " + spawnPosition.y + " " + spawnPosition.z);

         // TODO CLOUD - if spawn position is very close to another point, skip it to avoid artifacts

         Quaternion spawnRotation = Quaternion.identity;
         spawnRotation.SetLookRotation(new Vector3(0, 270, 0));
         GameObject g = (GameObject)Instantiate(VrEntityTemplate, spawnPosition, spawnRotation);
         if (gParent != null)
         {
            g.transform.parent = gParent.transform;
         }

         // Scale
         //float f = UnityEngine.Random.Range(0.85f, 1.15f);
         //Vector3 adjustedScale = new Vector3(f, f, f);
         //g.transform.localScale = adjustedScale;

         //-------------------------------------------------------------------------
         // Fill entity with its sensorphone data
         //-------------------------------------------------------------------------         
         VrEntityManager vrem = g.GetComponent<VrEntityManager>();
         vrem.SensorPhoneRecord = spr;
                  
         ////-------------------------------------------------------------------------
         //// Change its texture
         ////-------------------------------------------------------------------------
         //Texture2D tex = PoTexturesMaster.GetPoTextureForProductId(powi.PoItems[0].ProductId);
         //if (tex == null)
         //{
         //   print("WARNING could not find PO texture called " + powi.PoItems[0].ProductId);
         //   tex = PoTexturesMaster.HT_1000; // just default to something
         //}
         //g.GetComponent<Renderer>().material.mainTexture = tex;
         //// Set the tint
         //Color tint = PoProdCategoriesMaster.GetTintForProductCategory(powi.PoItems[0].ProductCategory);
         //g.GetComponent<Renderer>().material.color = tint;

         ////-------------------------------------------------------------------------
         //// PO behaviour
         ////-------------------------------------------------------------------------
         //EpmPoBehaviour pob = new EpmPoBehaviour()
         //{
         //   movementType = _posToCreate[_posCreatedSoFar].MoveType,
         //   PreRocketWarmUpTime = 2f
         //};
         //pom.PoBehaviour = pob;

         //// Tell Gui
         //if (_gameGui != null)
         //{
         //   _gameGui.PoCreated();
         //}

         // Have we created all the entities?
         _entitiesCreatedSoFar++;         
         if (_entitiesCreatedSoFar < _cloudDataMassList.SensorPhoneRecords.Count)
         {
            spr = _cloudDataMassList.SensorPhoneRecords[_entitiesCreatedSoFar];
            float delayInSeconds;
            //if (_configurationVr.SmoothTime)
            //{
               // constant time interval
               delayInSeconds = (float)_configurationVr.MaxTimeSeconds / (float)_cloudDataMassList.SensorPhoneRecords.Count;
            //}
            //else
            //{
               // use time from timestamp
               // not implemented
               // this calc needs to subtract prior/next record to get delay
               //delayInSeconds = _configurationVr.MaxTimeSeconds * (float)spr.World.TimeStampSecondsN;
            //}
            
            //print("RenderManager will invoke CreateVrEntity " + _entitiesCreatedSoFar + " in " + delayInSeconds + " seconds");
            Invoke("CreateVrEntity", delayInSeconds);
         }
         //else
         //{
         //   // factory is idle
         //   _factoryState = FactoryState.Idle;
         //}
      }
   }
}
