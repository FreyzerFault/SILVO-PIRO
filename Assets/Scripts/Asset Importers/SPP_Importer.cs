using System.Linq;
using SILVO.SPP;
using UnityEditor.AssetImporters;
using UnityEngine;

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
        [SerializeField]
        public SPP_CSV csv;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string path = ctx.assetPath;
            
            csv = new SPP_CSV(path);
            
            if (csv.IsEmpty)
            {
                Debug.LogError($"CSV file has no SPP Data: {path}");
                return;
            }
            
            SPP_Signal[] signals = csv.signals;
            signals = signals.Distinct().OrderBy(s => s.sentTime).ToArray();
            
            GameObject obj = new GameObject("SPP_Timeline");
            SPP_TimelineManager timelineManager = obj.AddComponent<SPP_TimelineManager>();
            timelineManager.animalTimelinePrefab = Resources.Load<GameObject>("Prefabs/AnimalTimeline");
            timelineManager.Signals = signals;
            
            ctx.AddObjectToAsset("Timeline Manager", obj);
            timelineManager.Timelines.ForEach(timeline => ctx.AddObjectToAsset(timeline.name, timeline.gameObject));
            
            ctx.SetMainObject(obj);
        }
    }
}
