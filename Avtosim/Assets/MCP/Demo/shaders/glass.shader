Shader "URP/Glass Reflective"
{
    Properties
    {
        _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
        _Cube ("Reflection Cubemap", CUBE) = "" {}
        _Shininess ("Shininess", Range(0.01, 1)) = 0.078125
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 300
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldRefl : TEXCOORD0;
                float fogCoord : TEXCOORD1;
            };

            samplerCUBE _Cube;
            float4 _ReflectColor;
            float _Shininess;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(IN.normalOS);

                OUT.worldRefl = reflect(normalize(worldPos - GetCameraPositionWS()), normalize(worldNormal));
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);

                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float4 reflColor = texCUBE(_Cube, IN.worldRefl);
                float3 finalColor = reflColor.rgb * _ReflectColor.rgb;
                float alpha = reflColor.a * _ReflectColor.a;

                finalColor = MixFog(finalColor, IN.fogCoord);

                return float4(finalColor, alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
