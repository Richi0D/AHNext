using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntennaHelperNext
{
    public static class Lib
    {
        static int fast_float_seed = 1;
        /// <summary>
        /// return random float in [-1,+1] range
        /// - it is less random than the c# RNG, but is way faster
        /// - the seed is meant to overflow! (turn off arithmetic overflow/underflow exceptions)
        /// </summary>
        public static float FastRandomFloat()
        {
            fast_float_seed *= 16807;
            return fast_float_seed * 4.6566129e-010f;
        }
        
        private static string GetShaderPath()
        {
            string platform = "windows";
            if (Application.platform == RuntimePlatform.LinuxPlayer) platform = "linux";
            else if (Application.platform == RuntimePlatform.OSXPlayer) platform = "osx";

            int version = Versioning.version_major * 100 + Versioning.version_minor;

            string shadersFolder;
            switch (version)
            {
                // should it ever be necessary...
                //case 105: // 1.5
                //case 106: // 1.6
                //case 107: // 1.7
                //case 108: // 1.8
                //case 109: // 1.9
                //	shadersFolder = "15";
                //	break;
                //case 110: // 1.10
                //	shadersFolder = "110";
                //	break;
                default:
                    shadersFolder = "15";
                    break;
            }

            return KSPUtil.ApplicationRootPath + "GameData/AntennaHelperNext/Shaders/" + shadersFolder + "/_" + platform;
        }
        
        public static Dictionary<string, Material> shaders;
        public static Material GetShader( string name )
        {
            if (shaders == null)
            {
                shaders = new Dictionary<string, Material>();
#pragma warning disable CS0618 // WWW is obsolete
                using (WWW www = new WWW("file://" + GetShaderPath()))
#pragma warning restore CS0618
                {
                    AssetBundle bundle = www.assetBundle;
                    Shader[] pre_shaders = bundle.LoadAllAssets<Shader>();
                    foreach (Shader shader in pre_shaders)
                    {
                        string key = shader.name.Replace("Custom/", string.Empty);
                        if (shaders.ContainsKey(key))
                            shaders.Remove(key);
                        shaders.Add(key, new Material(shader));
                    }
                    bundle.Unload(false);
                    www.Dispose();
                }
            }

            Material mat;
            if (!shaders.TryGetValue( name, out mat ))
            {
                throw new Exception( "shader " + name + " not found" );
            }
            return mat;
        }
        
    }
}