Shader "URP/Reflective Transparent Diffuse"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ReflectColor ("Reflection Color", Color) = (1,1,1,1)
        _MainTex ("Base Texture (RGB)", 2D) = "white" {}
        _Cube ("Reflection Cubemap", CUBE) = "_Skybox" {}
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        LOD 200
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

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
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            sampler2D _MainTex;
            samplerCUBE _Cube;

            float4 _Color;
            float4 _ReflectColor;
            float4 _MainTex_ST;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(IN.normalOS);

                OUT.worldPos = worldPos;
                OUT.worldNormal = normalize(worldNormal);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);

                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                // Base texture
                float4 tex = tex2D(_MainTex, IN.uv);
                float4 baseColor = tex * _Color;

                // Lighting
                float3 normalWS = normalize(IN.worldNormal);
                Light light = GetMainLight();
                float3 lightDir = normalize(light.direction);
                float NdotL = saturate(dot(normalWS, lightDir));
                float3 diffuse = baseColor.rgb * light.color * NdotL;

                // Reflection
                float3 viewDir = normalize(GetCameraPositionWS() - IN.worldPos);
                float3 reflVec = reflect(-viewDir, normalWS);
                float3 reflection = texCUBE(_Cube, reflVec).rgb;

                // Final color
                float3 finalColor = diffuse + reflection * _ReflectColor.rgb * tex.a;
                finalColor = MixFog(finalColor, IN.fogCoord);

                return float4(finalColor, baseColor.a);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
