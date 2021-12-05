#region License
// Copyright (c) 2021 Himanshu Parchand (himan2104@gmail.com)
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion


using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Himan
{
    [System.Serializable]
    public enum CustomProfilerCategory
    {
        AI = 0,
        Animation = 1,
        Audio = 2,
        Gui = 3,
        Input = 4,
        Internal = 5,
        Lighting = 6,
        Loading = 7,
        Memory = 8,
        Network = 9,
        Particles = 10,
        Physics = 11,
        Render = 12,
        Scripts = 13,
        Video = 14,
        VirtualTexturing = 15,
        VR = 16
    }

    [System.Serializable]
    public class CustomProfilerRecorder
    {
        [SerializeField] public CustomProfilerCategory ProfilerCategory;
        [Tooltip("Name of the stat")] public string ProfilerMarker;
        [HideInInspector] public ProfilerRecorder ProfilerRecorder;
    }

    public class KrakenProfiler : MonoBehaviour
    {
        private KrakenProfiler instance;

        [Header("Controls")]
        [SerializeField][Tooltip("Toggle Build info i.e. <ProductName> v<version>")] private bool ShowBuildInfo = true;
        [SerializeField][Tooltip("Override the ProductName in Build Info")] private string ProductNameOverride;
        [SerializeField] private bool ShowStatsOnBoot = false;
        [Range(0, 100)] [SerializeField] private float TextSize = 20f;
        [SerializeField] private Color TextColor = Color.white;
        [SerializeField] private Color BackgroundColor = new Color(0,0,0,0.7843f);
        [SerializeField] [Tooltip("Delay (in seconds) for updating stats. Leave 0 if you want it to update every frame")] [Range(0,10)]private float UpdateDelay = 0f;
        [SerializeField] private int AverageFPSFrameRange = 60;

        [Header("Profiler Recorders")]
        [SerializeField] private bool DrawCalls = true;
        [SerializeField] private bool Batches = true;
        [SerializeField] private bool SetPassCalls = true;
        [SerializeField] private bool Triangles = true;
        [SerializeField] private bool Vertices = true;

        [SerializeField][Tooltip("You can add custom profiler recorders here")] private List<CustomProfilerRecorder> CustomRecorders = new List<CustomProfilerRecorder>();

        [Header("References (Don't bother with this)")]
        [SerializeField] private TextMeshProUGUI BuildInfo;
        [SerializeField] private GameObject DebugPanel;
        [SerializeField] private TextMeshProUGUI stats;

        private Queue<float> FramerateData = new Queue<float>();
        string Stats;
        Coroutine UpdateRoutineRef;

        //default recorders
        private ProfilerRecorder rSetPassCalls;
        private ProfilerRecorder rDrawCalls;
        private ProfilerRecorder rBatches;
        private ProfilerRecorder rTriangles;
        private ProfilerRecorder rVertices;

        private float framerate, frametime;

        private void Awake()
        {
            if (instance != this)
            {
                if (instance != null)
                    Destroy(instance.gameObject);
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
        }

        void Start()
        {
            if(ShowBuildInfo) BuildInfo.text = ((ProductNameOverride != "") ? ProductNameOverride : Application.productName) + " v" + Application.version + "\n(Development Build)"; //dev build since you don't wanna release with this stuff on.
            DebugPanel.GetComponent<Image>().color = BackgroundColor;
            stats.color = TextColor;
            stats.fontSize = TextSize;
            Show(ShowStatsOnBoot);
        }

        /// <summary>
        /// Toggle the visibility of profiling information
        /// </summary>
        /// <param name="status">
        /// true = show
        /// false = hide
        /// </param>
        public void Show(bool status)
        {
            if (status)
            {
                LoadRecorders();
                DebugPanel.SetActive(true);
                if(UpdateDelay > 0) UpdateRoutineRef = StartCoroutine(UpdateRoutine());
            }
            else
            {
                if(UpdateDelay > 0) StopCoroutine(UpdateRoutineRef);
                KillRecorders();
                DebugPanel.SetActive(false);
            }
        }

        private IEnumerator UpdateRoutine()
        {
            while(true)
            {
                yield return new WaitForSecondsRealtime(UpdateDelay);
                InternalUpdate();
            }
        }

        private void Update()
        {
            if (UpdateDelay == 0) InternalUpdate();

            frametime = Time.unscaledDeltaTime;
            framerate = 1.0f / frametime;
            FramerateData.Enqueue(framerate);
            if (FramerateData.Count > AverageFPSFrameRange) FramerateData.Dequeue();
        }

        private void OnApplicationQuit()
        {
            KillRecorders();
        }

        private void LoadRecorders()
        {
            rSetPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            rDrawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            rBatches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            rTriangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            rVertices = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");

            foreach (CustomProfilerRecorder x in CustomRecorders)
                x.ProfilerRecorder = ProfilerRecorder.StartNew(ParseProfilerCategory(x.ProfilerCategory), x.ProfilerMarker);

        }

        /// <summary>
        /// One of the worst ideas I've ever had, maybe you can find a better one...
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private ProfilerCategory ParseProfilerCategory(CustomProfilerCategory category)
        {
            switch (category)
            {
                case CustomProfilerCategory.AI:
                    return ProfilerCategory.Ai;
                case CustomProfilerCategory.Animation:
                    return ProfilerCategory.Animation;
                case CustomProfilerCategory.Audio:
                    return ProfilerCategory.Audio;
                case CustomProfilerCategory.Gui:
                    return ProfilerCategory.Gui;
                case CustomProfilerCategory.Input:
                    return ProfilerCategory.Input;
                case CustomProfilerCategory.Internal:
                    return ProfilerCategory.Internal;
                case CustomProfilerCategory.Lighting:
                    return ProfilerCategory.Lighting;
                case CustomProfilerCategory.Loading:
                    return ProfilerCategory.Loading;
                case CustomProfilerCategory.Memory:
                    return ProfilerCategory.Memory;
                case CustomProfilerCategory.Network:
                    return ProfilerCategory.Network;
                case CustomProfilerCategory.Particles:
                    return ProfilerCategory.Particles;
                case CustomProfilerCategory.Physics:
                    return ProfilerCategory.Physics;
                case CustomProfilerCategory.Render:
                    return ProfilerCategory.Render;
                case CustomProfilerCategory.Scripts:
                    return ProfilerCategory.Scripts;
                case CustomProfilerCategory.Video:
                    return ProfilerCategory.Video;
                case CustomProfilerCategory.VirtualTexturing:
                    return ProfilerCategory.VirtualTexturing;
                case CustomProfilerCategory.VR:
                    return ProfilerCategory.Vr;
                default: return ProfilerCategory.FileIO;
            }
        }

        private void KillRecorders()
        {
            rDrawCalls.Dispose();
            rBatches.Dispose();
            rTriangles.Dispose();
            rVertices.Dispose();
            foreach (CustomProfilerRecorder x in CustomRecorders) x.ProfilerRecorder.Dispose();
        }

        private string GenerateProfilerDataAsString(CustomProfilerRecorder custom_recorder)
        {
            return (custom_recorder.ProfilerRecorder.Valid) ? $"{custom_recorder.ProfilerMarker}: {custom_recorder.ProfilerRecorder.LastValue}" : $"{custom_recorder.ProfilerMarker}: <color=red>recorder invalid</color>";
        }

        private string GenerateProfilerDataAsString(ProfilerRecorder recorder, string marker)
        {
            return (recorder.Valid) ? $"{marker}: {recorder.LastValue}" : $"{marker}: <color=red>recorder invalid</color>";
        }

        private void InternalUpdate()
        {
            string frstat = "";
            string ftstat = "white";

            

            frstat = framerate < 10 ? "red" : framerate < 30 ? "yellow" : framerate < 60 ? "green" : "#00FFFF";

            Stats = "<color=" + frstat + ">" + "Framerate: " + framerate.ToString("0.00") + " FPS</color> [" + ((FramerateData.Count > 0) ? FramerateData.Average() : 0f).ToString("0.00") + "FPS]\n";
            Stats += "<color=" + ftstat + ">" + "Frametime: " + frametime.ToString("0.00") + " ms</color>\n";
            Stats += "TimeScale: " + Time.timeScale.ToString("0.000") + "\n";
            StringBuilder str = new StringBuilder(500);
            if (DrawCalls) str.AppendLine(GenerateProfilerDataAsString(rDrawCalls, "Draw Calls"));
            if (Batches) str.AppendLine(GenerateProfilerDataAsString(rBatches, "Batches"));
            if (SetPassCalls) str.AppendLine(GenerateProfilerDataAsString(rSetPassCalls, "SetPass Calls"));
            if (Triangles) str.AppendLine(GenerateProfilerDataAsString(rTriangles, "Triangles"));
            if (Vertices) str.AppendLine(GenerateProfilerDataAsString(rVertices, "Vertices"));

            foreach (CustomProfilerRecorder x in CustomRecorders) str.AppendLine(GenerateProfilerDataAsString(x));

            Stats += str.ToString();

            stats.text = Stats;

        }
    }
}