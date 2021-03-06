using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Object = System.Object;
using UnityEditor;
using PackageInfo = UnityEditor.PackageInfo;

namespace Packages.FxEditor
{
    public class SceneConfig : MonoBehaviour
    {
        public enum JsonTypes
        {
            [InspectorName("没有")] None,
            [InspectorName("特效/webp特效")] Fx,
            [InspectorName("转场")] Translator,
            [InspectorName("滤镜")] Filtr,
            [InspectorName("主题")] Theme,
            [InspectorName("Collage")] Collage,
            [InspectorName("卡点")] Beats,
            [InspectorName("字幕")] Subtitle
        }


        private Exporter _exporter = null; //new Exporter();
        private bool isSaved = false;
        private UIRenderer _uiRenderer = new UIRenderer();

        public bool showCanvasUI = true;
        public int frameRate = 120;
        public float duration = 2;
        public string outputPath = null;


        public static Camera currentCamera = null;

        public bool forExport = false;

        [Header("纹理数据设置")] [Tooltip("决定是否外置纹理数据")]
        public bool isExternalTexture = true;

        [Tooltip("0--100,数值越小质量越差")] public int jpegCompressQuality = 75;

        [Tooltip("节点内容自动刷新开关")] public bool autoRefreshNode = true;


        public JsonTypes jsonType = JsonTypes.None;


        private Timeline _timeline = null;

        private void Start()
        {
            //isExporting = true;

            //outputPath="/Volumes/Workspace/Projects/HLVideoFx/source/PlatformsApp/testdata/fx/test.videofx";
            Prepare();

            //AddFrame();
            //_exporter=new Exporter();
        }

        
        
        
        
        public void Prepare()
        {
            GlobalUtility.UpdateCanvasNodeOrder();
            currentCamera = Camera.main;
            Time.captureFramerate = frameRate;

            _timeline = UnityEngine.Object.FindObjectOfType<Timeline>();
            if (_timeline != null)
            {
                currentCamera = _timeline.clips[0].camera;
            }

            _exporter = new Exporter();
            _exporter._Timeline = _timeline;
            //outputPath="/Volumes/Workspace/Projects/HLVideoFx/source/PlatformsApp/testdata/fx/test.videofx";
        }


        private void Update()
        {
            GlobalUtility.time = Time.time;
            AddFrame(false);
        }

        public void AddFrame(bool isQ)
        {
            
            if (_timeline != null)
            {
                _timeline.MyUpdate();
            }

            if (!forExport && !isQ) return;

            if (Time.time > duration && !isQ)
            {
                if (Application.isPlaying || outputPath == null || outputPath == "")
                {
                    //if(!isSaved)_exporter.SaveToFile("/Volumes/TmpSpace/testdata/test.videofx");
                    //if (!isSaved) _exporter.SaveToFile(outputPath);
                    if (!isSaved) SaveTotFile();
                    isSaved = true;
                    GlobalConfig.isPlaying = false;
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                }
            }
            else
            {
                if (_exporter == null)
                {
                    ExportMode mode = ExportMode.Generic;
                    if (_timeline != null) mode = ExportMode.Timeline;
                    _exporter = new Exporter(mode);
                    _exporter._Timeline = _timeline;
                }

                _exporter.AddFrame();
            }
        }

        public void SaveTotFile()
        {
            _exporter.SaveToFile(outputPath);
            //-------------------json-----------
            var jb = UnityEngine.Object.FindObjectOfType<FxJsonDataBase>();
            if (jb != null)
            {
                FileInfo info = new FileInfo(outputPath);
                //string jsonfile = outputPath.Replace("videofx", "json");
                string jsonfile = info.Directory.FullName + "/config.json";
                jb.SaveTo(jsonfile);
            }
        }


        private void UpdpateJsonData()
        {
            var jsonData = GetComponent<FxJsonDataBase>();
        }

        private void OnDrawGizmos()
        {
            UpdpateJsonData();

            //forExport = false;

            if (_uiRenderer == null) _uiRenderer = new UIRenderer();
            //------timeline ui---
            {
                var tl = FindObjectOfType<Timeline>();
                if (tl != null)
                {
                    foreach (var c in tl.clips)
                    {
                        if (c.camera == null) continue;
                        _uiRenderer.DrawCameraBound(c.camera, Color.white);
                    }
                }
            }
            //return;
            //if (Application.isPlaying) return;
            //
            // if(autoRefreshNode) 
            //     GlobalUtility.UpdateCanvasNodeOrder();
            if (showCanvasUI) _uiRenderer.DrawCanvasUIS();
            //_uiRenderer.DrawString(Camera.main.transform.position,"hello" );
        }
    }
}