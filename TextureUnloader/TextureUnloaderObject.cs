﻿using System.Collections.Generic;
using UnityEngine;
using System;
using EVEManager;
using Utils;
using System.Reflection;

namespace TextureUnloader
{

    [ConfigName("partUrl")]
    public class TextureUnloaderObject : IEVEObject
    {
        
        public void Apply()
        {

            ConfigNode moduleNode = new ConfigNode("MODULE");
            moduleNode.SetValue("name", typeof(TextureUnloaderPartModule).Name, true);

            foreach (AvailablePart ap in PartLoader.LoadedPartsList)
            {
                if (ap.partUrl != null && ap.partUrl != "")
                {
                    TextureUnloaderManager.Log("Found: " + ap.partUrl);
                    Part part = ap.partPrefab;
                    TextureUnloaderPartModule module = (TextureUnloaderPartModule)part.AddModule(typeof(TextureUnloaderPartModule).Name);
                    MethodInfo mI = typeof(PartModule).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
                    mI.Invoke(module, null);
                    module.Load(moduleNode);
                    module.Remove();
                }
            }

            Part[] parts = GameObject.FindObjectsOfType<Part>();
            foreach (Part part in parts)
            {
                if (part.partInfo.partUrl != null && part.partInfo.partUrl != "")
                {
                    TextureUnloaderPartModule module = (TextureUnloaderPartModule)part.AddModule(typeof(TextureUnloaderPartModule).Name);
                    MethodInfo mI = typeof(PartModule).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
                    mI.Invoke(module, null);
                    module.Load(moduleNode);
                    module.Remove();
                }
            }
        }

        public void LoadConfigNode(ConfigNode node)
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            foreach (AvailablePart ap in PartLoader.LoadedPartsList)
            {
                if (ap.partUrl != null && ap.partUrl != "")
                {
                    Part part = ap.partPrefab;
                    TextureUnloaderPartModule module = part.FindModuleImplementing<TextureUnloaderPartModule>();
                    module.Remove();
                    part.RemoveModule(module);
                }
            }
        }
    }

    public class TextureUnloaderPartModule : PartModule
    {
        protected class TexRefCnt
        {
            public static Dictionary<string, TexRefCnt> textureDictionary = new Dictionary<string, TexRefCnt>();

            public static void LoadFromRenderer(MeshRenderer renderer)
            {
                foreach (Material material in renderer.materials)
                {
                    LoadFromMaterial(material);
                }
                foreach (Material material in renderer.sharedMaterials)
                {
                    LoadFromMaterial(material);
                }
            }

            public static void LoadFromMaterial(Material material)
            {
                LoadFromTexture(material, ShaderProperties._MainTex_PROPERTY);
                LoadFromTexture(material, ShaderProperties._Emissive_PROPERTY);
                LoadFromTexture(material, ShaderProperties._BumpMap_PROPERTY);
            }

            public static void LoadFromTexture(Material material, int id)
            {
                Texture2D texture = (Texture2D)material.GetTexture(id);
                if (texture != null)
                {
                    TexRefCnt texRef;
                    if(textureDictionary.ContainsKey(texture.name))
                    {
                        texRef = textureDictionary[texture.name];
                    }
                    else
                    {
                        texRef = new TexRefCnt(texture);
                        textureDictionary[texture.name] = texRef;
                    }
                    texRef.count++;
                    if (texRef.count == 1)
                    {
                        TextureConverter.Reload(texRef.texInfo);
                    }
                    material.SetTexture(id, texRef.texInfo.texture);
                }
            }

            public static void UnLoadFromRenderer(MeshRenderer renderer)
            {
                foreach (Material material in renderer.materials)
                {
                    UnLoadFromMaterial(material);
                }
                foreach (Material material in renderer.sharedMaterials)
                {
                    UnLoadFromMaterial(material);
                }
            }

            public static void UnLoadFromMaterial(Material material)
            {
                UnLoadFromTexture(material, ShaderProperties._MainTex_PROPERTY);
                UnLoadFromTexture(material, ShaderProperties._Emissive_PROPERTY);
                UnLoadFromTexture(material, ShaderProperties._BumpMap_PROPERTY);
            }

            public static void UnLoadFromTexture(Material material, int id)
            {
                Texture2D texture = (Texture2D)material.GetTexture(id);
                if (texture != null && textureDictionary.ContainsKey(texture.name))
                {

                    TexRefCnt texRef = textureDictionary[texture.name];
                    if (texRef.count > 0)
                    {
                        texRef.count--;
                    }
                    if (texRef.count <= 0)
                    {
                        TextureConverter.Minimize(texRef.texInfo);
                    }
                }
                else if(texture != null)
                {
                    TextureConverter.Minimize(texture);
                    textureDictionary[texture.name] = new TexRefCnt(texture);
                }
            }

            int count = 0;
            GameDatabase.TextureInfo texInfo;
            public TexRefCnt(Texture2D tex)
            {
                texInfo = GameDatabase.Instance.GetTextureInfo(tex.name);
            }
        }

        
         
        public override void OnStart(StartState state)
        {
            Add();
        }

        public override void OnInactive()
        {
            Remove();
        }

        private void Add()
        {
            foreach (MeshRenderer mr in part.FindModelComponents<MeshRenderer>())
            {
                TexRefCnt.LoadFromRenderer(mr);
            }
        }

        public void Remove()
        {
            foreach (MeshRenderer mr in part.FindModelComponents<MeshRenderer>())
            {
                TexRefCnt.UnLoadFromRenderer(mr);
            }
        }
    }
}