using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace LegacyCore.Utilities
{
    public class BeatmapUtil
    {
        public static string ConvertBeatmapToV2(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            if (json.Contains("\"_version\"")) return json; // v2
            if (!json.Contains("\"version\"")) return json; // other

            V3Beatmap v3;

            try
            {
                v3 = JsonConvert.DeserializeObject<V3Beatmap>(json);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Failed to parse v3 beatmap JSON: " + ex);
                return json;
            }

            V2Beatmap v2 = new V2Beatmap();

            v2._version = "2.0.0";
            v2._customData = new V2CustomData();

            if (v3.bpmEvents != null)
            {
                v2._customData._BPMChanges = new V2BpmChange[v3.bpmEvents.Length];

                for (int i = 0; i < v3.bpmEvents.Length; i++)
                {
                    v2._customData._BPMChanges[i] = new V2BpmChange();
                    v2._customData._BPMChanges[i]._BPM = v3.bpmEvents[i].m;
                    v2._customData._BPMChanges[i]._time = v3.bpmEvents[i].b;
                }
            }
            else v2._customData._BPMChanges = Array.Empty<V2BpmChange>();

            int colorCount = v3.colorNotes?.Length ?? 0;
            int bombCount = v3.bombNotes?.Length ?? 0;

            v2._notes = new V2Note[colorCount + bombCount];

            int noteIndex = 0;

            for (int i = 0; i < colorCount; i++)
            {
                V3ColorNote source = v3.colorNotes[i];
                V2Note note = new V2Note();

                note._time = source.b - 0.5f;
                note._lineIndex = source.x;
                note._lineLayer = source.y;
                note._type = source.c;
                note._cutDirection = source.d;

                v2._notes[noteIndex] = note;

                noteIndex++;
            }

            for (int i = 0; i < bombCount; i++)
            {
                V3BombNote source = v3.bombNotes[i];
                V2Note note = new V2Note();

                note._time = source.b - 0.5f;
                note._lineIndex = source.x;
                note._lineLayer = source.y;
                note._type = 3; // v2 bomb
                note._cutDirection = 0; // bombs don't have a cut direction lol

                v2._notes[noteIndex] = note;
                noteIndex++;
            }

            int obstacleCount = v3.obstacles != null ? v3.obstacles.Length : 0;
            List<V2Obstacle> obstacles = new List<V2Obstacle>();

            for (int i = 0; i < obstacleCount; i++)
            {
                V3Obstacle source = v3.obstacles[i];

                if (source.x < 0 || source.x > 3)
                {
                    LogUtil.Error("Skipping obstacle with invalid x=" + source.x + ", time=" + source.b);
                    continue;
                }

                V2Obstacle obstacle = new V2Obstacle();
                obstacle._time = source.b - 0.5f;
                obstacle._lineIndex = source.x;
                obstacle._duration = source.d;
                obstacle._width = source.w;
                obstacle._type = source.h < 3 ? 1 : 0;

                obstacles.Add(obstacle);
            }
            v2._obstacles = obstacles.ToArray();

            int eventCount = v3.basicBeatmapEvents != null ? v3.basicBeatmapEvents.Length : 0;
            v2._events = new V2Event[eventCount];

            for (int i = 0; i < eventCount; i++)
            {
                V3BasicEvent source = v3.basicBeatmapEvents[i];
                V2Event evt = new V2Event();

                evt._time = source.b;
                evt._type = source.et;
                evt._value = source.i;

                v2._events[i] = evt;
            }

            v2._waypoints = Array.Empty<V2Waypoint>(); // waypoints don't exist in v3
            v2._customData._bookmarks = Array.Empty<V2Bookmark>();

            return JsonConvert.SerializeObject(v2);
        }
    }

    [Serializable]
    public class V3Beatmap
    {
        public string version;
        public V3BpmEvent[] bpmEvents;
        public V3BasicEvent[] basicBeatmapEvents;
        public V3ColorNote[] colorNotes;
        public V3BombNote[] bombNotes;
        public V3Obstacle[] obstacles;
    }

    [Serializable]
    public class V3BpmEvent
    {
        public float b;
        public float m;
    }

    [Serializable]
    public class V3BasicEvent
    {
        public float b;
        public int et;
        public int i;
    }

    [Serializable]
    public class V3ColorNote
    {
        public float b;
        public int x;
        public int y;
        public int c;
        public int d;
    }

    [Serializable]
    public class V3BombNote
    {
        public float b;
        public int x;
        public int y;
    }

    [Serializable]
    public class V3Obstacle
    {
        public float b;
        public int x;
        public int y;
        public float d;
        public int w;
        public int h;
    }

    [Serializable]
    public class V3Bookmark
    {
        public float b;
        public string n;
        public float[] c;
    }

    [Serializable]
    public class V2Beatmap
    {
        public string _version;
        public V2CustomData _customData;
        public V2Event[] _events;
        public V2Note[] _notes;
        public V2Obstacle[] _obstacles;
        public V2Waypoint[] _waypoints;
    }

    [Serializable]
    public class V2CustomData
    {
        public V2BpmChange[] _BPMChanges;
        public V2Bookmark[] _bookmarks;
    }

    [Serializable]
    public class V2BpmChange
    {
        public float _BPM;
        public float _time;
    }

    [Serializable]
    public class V2Bookmark
    {
        public float _time;
        public string _name;
        public float[] _color;
    }

    [Serializable]
    public class V2Event
    {
        public float _time;
        public int _type;
        public int _value;
    }

    [Serializable]
    public class V2Note
    {
        public float _time;
        public int _lineIndex;
        public int _lineLayer;
        public int _type;
        public int _cutDirection;
    }

    [Serializable]
    public class V2Obstacle
    {
        public float _time;
        public int _lineIndex;
        public int _type;
        public float _duration;
        public int _width;
    }

    [Serializable]
    public class V2Waypoint
    {
        public float _time;
        public int _lineIndex;
        public int _lineLayer;
        public int _offsetDirection;
    }
}