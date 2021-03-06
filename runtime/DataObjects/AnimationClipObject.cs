using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Packages.FxEditor
{
    public class AnimationClipProperty
    {
        public Int64 objectID = 0;
        public string name = "";
        public int type = ShaderParameter.ParameterTypeFloat;

        public AnimationClipProperty(string _name, int typ)
        {
            name = _name;
            type = typ;
        }
    }

    public class AnimationClipObject : DataObjectBase
    {
        public const int AnimationTypeUnknown = 0;
        public const int AnimationTypeCurve = 1;
        public const int AnimationTypeSampler = 2;

        private AnimationClip animationClip = null;
        private Shader shader = null;
        private string matrix = "";
        private Vector4 staticColor=new Vector4(1,1,1,1);


        MaterialPropertyBlock block = new MaterialPropertyBlock();


        //------------data block----------------
        public int animationType = AnimationTypeSampler;
        public float duration = 0.0f;
        public float frameRate = 240.0f;
        public int framesCount = 0;
        public List<AnimationClipProperty> animationDataProperies = new List<AnimationClipProperty>();
        //-------------------------
        public List<Matrix4x4> glyphMatrix=new List<Matrix4x4>();
        public List<Vector4> _Color=new List<Vector4>();
        //------------------------------------------------------

        public AnimationClipObject(AnimationClip clip)
        {
            ObjectType = ObjectTypeAnimationClip;
            //--------------------------
            animationClip = clip;
            duration = clip.length;
            framesCount = (int) Math.Floor(duration * frameRate);
            SamplerData();
        }


        public void SamplerData()
        {
            
            
            var binds = AnimationUtility.GetCurveBindings(animationClip);
            
            
            GameObject gameObject=new GameObject("TMP");
            gameObject.tag = "EditorOnly";
            
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material=new Material(Shader.Find("HLFx/TextureColorMask"));
            renderer.GetPropertyBlock(block);
            
            for (int i = 0; i <= framesCount; i++)
            {
                
                float time = duration * i / framesCount;
                animationClip.SampleAnimation(gameObject,time);
                renderer.GetPropertyBlock(block);
                
                glyphMatrix.Add(gameObject.transform.localToWorldMatrix);
                
                
                var color = block.GetColor("_Color");
                var colorV = new Vector4(color.r, color.g, color.b, color.a);
                if (colorV != Vector4.zero)
                {
                    _Color.Add(colorV);
                }

                
                Debug.Log("color:"+color);
            }
            
            animationDataProperies.Add(new AnimationClipProperty("animationMatrix",
                ShaderParameter.ParameterTypeMatrix4x4));
            if (_Color.Count > 0)
            {
                animationDataProperies.Add(new AnimationClipProperty("_Color", ShaderParameter.ParameterTypeFloat4));    
            }
            

            Object.DestroyImmediate(gameObject);
        }

        public override void Write(Stream stream)
        {
            
            Write(stream, duration);
            Write(stream, frameRate);
            Write(stream, animationType);
            Write(stream, framesCount);
            
            
            //-------animation data--------
            Write(stream, animationDataProperies.Count);
            {
                var p = animationDataProperies[0];
                Write(stream,p.objectID);
                Write(stream,p.name);
                Write(stream,p.type);
            }
            
            if(_Color.Count>0)
            {
                var p = animationDataProperies[1];
                Write(stream,p.objectID);
                Write(stream,p.name);
                Write(stream,p.type);
            }
                
            
            Write(stream,glyphMatrix.ToArray());
            
            if(_Color.Count>0)
            Write(stream,_Color.ToArray());
        }
    }
}