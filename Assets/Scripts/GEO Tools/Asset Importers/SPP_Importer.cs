using SILVO.GEO_Tools.SPP;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SILVO.GEO_Tools.Asset_Importers
{
    /// <summary>
    /// .spp File Importer
    /// (Silvo-Pastoralismo Pírico Positions)
    ///
    /// SPP file format:
    ///   device_id,sent_time,received_time,msg_type,lon,lat
    ///
    /// Converts data in SPP_Signals:
    ///  id, sentTime, receivedTime, type, position
    ///
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
