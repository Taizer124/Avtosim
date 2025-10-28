using System;
using UnityEngine;
#if VISUAL_EFFECT_GRAPH_INSTALLED
using UnityEngine.VFX;
#endif

namespace Assets.VehicleController
{
    [Serializable]
    public class AntiLagParameters
    {
        public EffectTypeParameters VisualEffect;
        public Transform[] ExhaustsPositionArray;
        public float BackfireDelay = 0.25f;
        [Min(1)]
        public int MinBackfireCount = 2;
        [Min(1)]
        public int MaxBackfireCount = 5;
    }

    [Serializable]
    public class TireSmokeParameters
    {
        public EffectTypeParameters VisualEffect;
        public float VerticalOffset;
    }

    [Serializable]
    public class EffectTypeParameters
    {
        public VisualEffectAssetType.Type VisualEffectType;
        public ParticleSystem ParticleSystem;
#if VISUAL_EFFECT_GRAPH_INSTALLED
        public VisualEffectAsset VFXAsset;
#endif
    }

    [Serializable]
    public class BrakeLightsParameters
    {
        public MeshRenderer[] RearLightMeshes;
        [ColorUsageAttribute(true, true)]
        public Color BrakeColor;
        public bool MaterialsAtSpecificIndex;
        [Min(0)]
        public int[] MaterialIndexArray;
    }

    [Serializable]
    public class BrakeDisksGlowParameters
    {
        public MeshRenderer[] BrakeDisksMeshes;
        [ColorUsageAttribute(true, true)]
        public Color GlowColor;
        public bool MaterialsAtSpecificIndex;
        [Min(0)]
        public int[] MaterialIndexArray;
        [Min(0.1f)]
        public float HeatUpTime = 0.5f;
        [Min(0.2f)]
        public float CoolDownTime = 1f;
    }

    [Serializable]
    public class WingAeroParameters
    {
        public TrailRenderer[] TrailRendererArray;
        [Min(1)]
        public int MinSpeedToDisplay = 20;
        [Range(0, 1f)]
        public float MaxAlpha = 0.5f;
    }

    [Serializable]
    public class TireTrailParameters
    {
        public TrailRenderer TrailRenderer;
        public float VerticalOffset;
    }

    [Serializable]
    public class NitrousParameters
    {
        public EffectTypeParameters VisualEffect;
        public Transform[] ExhaustsPositionArray;
        [GradientUsageAttribute(true), Header("For ParticleSystem, change material properties instead")]
        public Gradient Gradient;
    }

    [Serializable]
    public class EngineSmokeParameters
    {
        public EffectTypeParameters SmokeVisualEffect;
        public Transform EmitPoint;
        public bool PlayOnAwake = false;
    }

    [Serializable]
    public class CollisionParameters
    {
        public EffectTypeParameters ImpactVisualEffect;
        public EffectTypeParameters StayVisualEffect;
    }
}
