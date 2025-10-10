// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/SRP Lightweight/GlintSparkle"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			half filler;
		};

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18934
2310;88;1902;965;1518.819;3175.704;5.863735;False;False
Node;AmplifyShaderEditor.CommentaryNode;569;1344.167,-453.8903;Inherit;False;842.9095;339.4828;Normals;4;560;568;567;561;;0.2573529,0.3546652,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;570;2400,-656;Inherit;False;2555;885;;28;510;355;354;555;554;366;360;511;359;365;508;507;515;524;557;525;556;363;364;367;369;346;347;536;368;564;571;419;Sparkles Effect;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;429;1352.417,-1965.659;Inherit;False;3595.187;815.0569;;31;488;495;492;505;499;494;440;497;447;466;491;439;504;489;442;478;470;463;468;477;484;483;473;424;486;487;401;431;400;390;562;Glint Effect;0.9986145,1,0.4103774,1;0;0
Node;AmplifyShaderEditor.StaticSwitch;494;2064,-1840;Float;False;Property;_InvertDirection;Invert Direction;4;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;X;Y;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;440;2112,-1680;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;447;2112,-1600;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;499;1920,-1632;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;495;1696,-1840;Float;False;Property;_Direction;Direction;3;0;Create;True;0;0;0;False;0;False;0;1;1;True;;KeywordEnum;3;X;Y;Z;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;488;1440,-1856;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;505;1440,-1664;Float;False;Property;_SizeSpeedInterval;Size Speed Interval;5;0;Create;True;0;0;0;False;0;False;1,1,1;0.5,10,5;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NegateNode;492;1936,-1776;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;510;2720,-336;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;511;2656,-32;Float;False;Property;_SpakleSpeed;Spakle Speed;15;0;Create;True;0;0;0;False;0;False;0;0.002;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;504;3008,-1696;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;355;2704,-176;Float;False;Property;_Frequency;Frequency;11;0;Create;True;0;0;0;False;0;False;20;4;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;524;2464,-608;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;515;2448,-416;Float;True;Property;_Noise;Noise;10;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;None;bdbe94d7623ec3940947b62544306f1c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;556;3024,-544;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;439;2528,-1760;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.27;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;360;2960,-32;Inherit;False;1;0;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;497;2352,-1792;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;525;2768,-608;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;478;3200,-1600;Inherit;False;2;0;FLOAT;-1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;463;3216,-1760;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;557;2672,-480;Float;False;Property;_ScreenContribution;Screen Contribution;16;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;489;3008,-1568;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;442;2752,-1760;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;555;3024,-160;Float;False;Constant;_Float4;Float 4;19;0;Create;True;0;0;0;False;0;False;1.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;354;3024,-256;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;359;3440,-272;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;477;3504,-1760;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;560;1394.167,-403.8903;Inherit;True;Property;_Normals;Normals;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;365;3440,-128;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NegateNode;366;3232,-32;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;554;3216,-192;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;468;3504,-1536;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;470;3200,-1472;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;346;4816,-256;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;507;3632,-368;Inherit;True;Property;_TextureSample0;Texture Sample 0;14;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;491;2272,-1600;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;364;4224,-240;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.8;False;2;FLOAT;0.85;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;473;3840,-1664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;568;1732.077,-293.4075;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;508;3632,-160;Inherit;True;Property;_TextureSample1;Texture Sample 1;13;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;483;3648,-1760;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;484;3648,-1536;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;369;3648,112;Float;False;Property;_Range;Range;13;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;486;3984,-1664;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;564;4218.197,-93.65095;Inherit;False;567;worldNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;567;1944.077,-295.4075;Float;False;worldNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;572;4264.248,588.1351;Float;False;Property;_MainGlowFresnel;Fresnel Bias, Scale, Power;19;0;Create;False;0;0;0;False;0;False;0,0,0;0.02,1,5;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;563;4328.248,508.1352;Inherit;False;567;worldNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;363;3984,-240;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;562;3808,-1424;Inherit;False;567;worldNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;424;3808,-1344;Float;False;Property;_GlintFresnel;Fresnel Bias, Scale, Power;6;0;Create;False;0;0;0;False;0;False;0,0,0;0.01,3,4;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;368;4048,0;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;466;2656,-1456;Float;False;Property;_TailHeadFalloff;Tail Head Falloff;7;0;Create;True;0;0;0;False;0;False;0.5;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;390;4765.775,-1686.948;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;536;4528,-48;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.01;False;2;FLOAT;0.8;False;3;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;571;4208,0;Float;False;Property;_SparkleFresnel;Fresnel Bias, Scale, Power;17;0;Create;False;0;0;0;False;0;False;0,0,0;0.02,0.8,2;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;550;4672,382.1486;Float;False;Property;_BodyGlow;Color;18;0;Create;False;0;0;0;False;1;Header(Body Glow);False;0,0,0,0;0.6323529,0.4794762,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;347;4528,-384;Float;False;Property;_SparkleColor;Color;9;0;Create;False;0;0;0;False;2;Space(10);Header(Sparkles);False;0,0,0,0;1,0.7779999,0.2971669,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;370;4584.248,572.1351;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.02;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;431;4480,-1360;Float;False;Property;_Brightness;Brightness;8;0;Create;True;0;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;401;4432,-1824;Float;False;Property;_GlintColor;Color;2;0;Create;False;0;0;0;False;1;Header(Glint Effect);False;0,0,0,0;1,0.9154145,0.4292426,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;419;4576,-160;Float;False;Property;_SparklesBrightness;Brightness;14;0;Create;False;0;0;0;False;0;False;2;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;487;4176,-1664;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;400;4187.048,-1422.668;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.01;False;2;FLOAT;1;False;3;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;345;5257.424,-157.0255;Float;False;Constant;_Float12;Float 12;20;0;Create;True;0;0;0;False;0;False;0.9;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;374;5200,-288;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;561;1718.788,-401.7002;Float;False;tangentNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;565;5203.372,-430.7056;Inherit;False;561;tangentNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;292;5204.548,-643.8364;Float;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.7647059,0.5730754,0.1124562,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;371;4992,384;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;367;3648,32;Float;False;Property;_Threshold;Threshold;12;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;577;5576.469,-401.7877;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;ASESampleShaders/SRP Lightweight/GlintSparkle;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;494;1;495;0
WireConnection;494;0;492;0
WireConnection;440;0;499;0
WireConnection;447;0;499;0
WireConnection;499;0;505;1
WireConnection;499;1;505;2
WireConnection;495;1;488;1
WireConnection;495;0;488;2
WireConnection;495;2;488;3
WireConnection;492;0;495;0
WireConnection;510;2;515;0
WireConnection;504;0;466;0
WireConnection;556;0;525;0
WireConnection;556;1;557;0
WireConnection;439;0;497;0
WireConnection;439;1;440;0
WireConnection;360;0;511;0
WireConnection;497;0;494;0
WireConnection;497;1;505;1
WireConnection;525;0;524;0
WireConnection;478;1;489;0
WireConnection;463;0;442;0
WireConnection;463;1;504;0
WireConnection;489;0;466;0
WireConnection;442;0;439;0
WireConnection;442;1;491;0
WireConnection;354;0;510;0
WireConnection;354;1;355;0
WireConnection;359;0;556;0
WireConnection;359;1;354;0
WireConnection;359;2;360;0
WireConnection;477;0;463;0
WireConnection;477;1;478;0
WireConnection;365;0;554;0
WireConnection;365;1;366;0
WireConnection;366;0;360;0
WireConnection;554;0;354;0
WireConnection;554;1;555;0
WireConnection;468;0;463;0
WireConnection;468;1;470;0
WireConnection;470;1;466;0
WireConnection;346;0;347;0
WireConnection;346;1;364;0
WireConnection;346;2;419;0
WireConnection;346;3;536;0
WireConnection;507;0;515;0
WireConnection;507;1;359;0
WireConnection;491;0;447;0
WireConnection;491;1;505;3
WireConnection;364;0;363;0
WireConnection;364;1;367;0
WireConnection;364;2;368;0
WireConnection;473;0;483;0
WireConnection;473;1;484;0
WireConnection;568;0;560;0
WireConnection;508;0;515;0
WireConnection;508;1;365;0
WireConnection;483;0;477;0
WireConnection;484;0;468;0
WireConnection;486;0;473;0
WireConnection;567;0;568;0
WireConnection;363;0;507;2
WireConnection;363;1;508;2
WireConnection;368;0;367;0
WireConnection;368;1;369;0
WireConnection;390;0;401;0
WireConnection;390;1;487;0
WireConnection;390;2;400;0
WireConnection;390;3;431;0
WireConnection;536;0;564;0
WireConnection;536;1;571;1
WireConnection;536;2;571;2
WireConnection;536;3;571;3
WireConnection;370;0;563;0
WireConnection;370;1;572;1
WireConnection;370;2;572;2
WireConnection;370;3;572;3
WireConnection;487;0;486;0
WireConnection;400;0;562;0
WireConnection;400;1;424;1
WireConnection;400;2;424;2
WireConnection;400;3;424;3
WireConnection;374;0;390;0
WireConnection;374;1;346;0
WireConnection;374;2;371;0
WireConnection;561;0;560;0
WireConnection;371;0;550;0
WireConnection;371;1;370;0
ASEEND*/
//CHKSM=4B23125DFE5C32844D458DB25F395152AF831E7D