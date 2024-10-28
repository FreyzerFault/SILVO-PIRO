using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Csv;
using DavidUtils.ExtensionMethods;
using SILVO.SPP;
using UnityEditor.AssetImporters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SILVO.Asset_Importers
{
    /// <summary>
    /// .spp File Importer
    /// (Silvo-Pastoralismo Pírico Positions)
    ///
    /// SPP file format:
    ///   time,device_id,msg_type,position_time,lat,lon
    ///
    /// Converts data in SPP_Signals:
    ///  id, receivedTime, sentTime, position, type
    ///
    /// time == receivedTime
    /// position_time == sentTime
    /// position == Vector2(lon, lat)
    /// 
    /// type can be:
    /// {Seq, Poll, Warn, Pulse, Unknown} 
    /// </summary>
    [ScriptedImporter(1, "spp")]
    public class SPP_Importer : ScriptedImporter
    {
        private bool parseOnImport = true;
        
        [SerializeField]
        public SPP_TimelineManager timelineManager;
        
        
        [SerializeField] int maxLines = 10000;
        [SerializeField] bool freeMemoryWhenParsed = true;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string path = ctx.assetPath;
            
            var obj = new GameObject();
            timelineManager = obj.AddComponent<SPP_TimelineManager>();
            timelineManager.animalTimelinePrefab = Resources.Load<GameObject>("Prefabs/AnimalTimeline");
            timelineManager.csv = new SPP_CSV(path, true, maxLines);
            
            if (parseOnImport)
            {
                timelineManager.ParseCSVFile(maxLines, freeMemoryWhenParsed);
                timelineManager.Timelines.ForEach(timeline => ctx.AddObjectToAsset(timeline.name, timeline.gameObject));
            }

            ctx.AddObjectToAsset("Timeline Manager Object", obj);
            ctx.AddObjectToAsset("Timeline Manager", timelineManager);
            ctx.SetMainObject(obj);
            
            Debug.Log($"Imported {path} with {timelineManager.csv.LineCount} lines => {timelineManager.Signals.Length} signals");;
        }
    }
}
