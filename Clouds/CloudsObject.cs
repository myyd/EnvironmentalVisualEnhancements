﻿using EVEManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;

namespace Atmosphere
{
    public class CloudsMaterial : MaterialManager
    {
#pragma warning disable 0169
#pragma warning disable 0414
        [Persistent, Tooltip("Color to be applied to clouds.")]
        Color _Color = 256*Color.white;
        [Persistent, Tooltip("Main texture used with clouds.")]
        Texture2D _MainTex;
        [Persistent]
        Texture2D _DetailTex;
        [Persistent]
        float _DetailScale = 100f;
        [Persistent, InverseScaled]
        float _DetailDist = 0.000002f;
        [Persistent, InverseScaled]
        float _DistFade = 1.0f;
        [Persistent, InverseScaled]
        float _DistFadeVert = 0.00004f;
    }

    public class CloudsObject : MonoBehaviour, IEVEObject
    {
        public String Name { get { return name; } set { name = node.name = value; } }
        public ConfigNode ConfigNode { get { return node; } }
        public String Body { get { return body; } }
        private new String name;
        private ConfigNode node;
        private String body;

        
        [Persistent]
        float altitude = 1000f;
        [Persistent]
        Vector3 speed = new Vector3(0, 30, 0);
        [Persistent]
        Vector3 detailSpeed = new Vector3(0,5,0);
        [Persistent]
        Vector3 offset = new Vector3(0, 0, 0);
        [Persistent, Tooltip("Settings for the cloud rendering")]
        CloudsMaterial settings = null;
        
        [Persistent, Optional]
        CloudsVolume layerVolume = null;
        [Persistent, Optional]
        Clouds2D layer2D = null;

        private CloudsPQS cloudsPQS = null;
        private CelestialBody celestialBody;
        private Transform scaledCelestialTransform;
        public void LoadConfigNode(ConfigNode node, String body)
        {
            ConfigHelper.LoadObjectFromConfig(this, node);
            this.node = node;
            this.body = body;
            name = node.name;
        }

        public void Apply()
        {
            celestialBody = Tools.GetCelestialBody(body);
            scaledCelestialTransform = Tools.GetScaledTransform(body);
            
            GameObject go = new GameObject();
            cloudsPQS = go.AddComponent<CloudsPQS>();
            go.name = this.name;
            cloudsPQS.Apply(body, settings, layer2D, layerVolume, altitude, speed, detailSpeed, offset);
        }

        public void Remove()
        {
            cloudsPQS.Remove();
            GameObject go = cloudsPQS.gameObject;
            

            GameObject.DestroyImmediate(cloudsPQS);
            GameObject.DestroyImmediate(go);

        }

    }
}
