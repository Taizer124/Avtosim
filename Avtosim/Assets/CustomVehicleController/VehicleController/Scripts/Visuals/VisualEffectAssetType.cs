namespace Assets.VehicleController
{
    public struct VisualEffectAssetType
    {
        public enum Type
        {
            ParticleSystem,
#if VISUAL_EFFECT_GRAPH_INSTALLED
            VisualEffect,
#endif
        }
    }
}
