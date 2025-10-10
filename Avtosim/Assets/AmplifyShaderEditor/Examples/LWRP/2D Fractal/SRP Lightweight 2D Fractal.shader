// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/SRP Lightweight/2D Fractal"
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
2310;88;1902;965;919.6686;949.385;1.59128;False;False
Node;AmplifyShaderEditor.CommentaryNode;39;-1200.375,-974.9979;Inherit;False;666.0259;385.254;Animating Zoom;6;16;25;31;26;7;9;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;41;-887.9313,-469.8014;Inherit;False;945.6219;589.0108;Calculating Fractal;9;43;42;3;5;12;6;13;8;4;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;40;102.3927,-356.119;Inherit;False;1269.819;382;Coloring the Fractal;8;38;22;24;23;20;18;19;21;;1,1,1,1;0;0
Node;AmplifyShaderEditor.HSVToRGBNode;23;1129.211,-162.1189;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BreakToComponentsNode;24;857.2116,-146.1189;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;18;697.2116,-130.1189;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;20;535.5114,-259.0188;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;329.2115,-210.1189;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;329.2115,-306.119;Float;False;Constant;_HueOffset;HueOffset;3;0;Create;True;0;0;0;False;0;False;0.9;0.95;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;161.1993,-257.6866;Float;False;Constant;_HueScale;HueScale;2;0;Create;True;0;0;0;False;0;False;0.9;2.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;38;152.3927,-92.86803;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;3;-175.3088,-193.0908;Float;False;float2 r = UV@$int step = 0@$for (int i = 0@ i < MaxIter@ i++) ${$	if (length(r) > Threshold) $		break@$	$	r = mul( float2x2(r.x,-r.y,r.y,r.x) , r) + UV@$	step++@$}$return (float)step/(float)MaxIter@;1;Create;3;True;UV;FLOAT2;0,0;In;;Float;False;True;MaxIter;INT;0;In;;Float;False;True;Threshold;FLOAT;0;In;;Float;False;Mandlebrot;True;False;0;;False;3;0;FLOAT2;0,0;False;1;INT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-382.1088,14.70922;Float;False;Property;_Threshold;Threshold;1;0;Create;True;0;0;0;False;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-268.6803,-350.8014;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;13;-606.1089,-241.2908;Float;False;Property;_Center;Center;4;0;Create;True;0;0;0;False;0;False;-0.412,0.609;-0.766,-0.1009;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-430.1088,-417.2908;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-659.6089,-370.2908;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;2,2;False;1;FLOAT2;-1,-1;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;25;-687.3093,-874.158;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-886.5097,-900.7584;Float;False;Property;_ZoomBase;ZoomBase;5;0;Create;True;0;0;0;False;0;False;0.25;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;31;-904.4308,-792.9454;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;43;-848.379,-251.4149;Float;False;Constant;_Offset;Offset;6;0;Create;True;0;0;0;False;0;False;-1,-1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;42;-850.629,-391.4146;Float;False;Constant;_Tiling;Tiling;6;0;Create;True;0;0;0;False;0;False;2,2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;16;-1148.969,-681.7601;Float;False;Property;_ZoomOffset;ZoomOffset;2;0;Create;True;0;0;0;False;0;False;2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;7;-1141.701,-924.9979;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;9;-1150.375,-758.6487;Float;False;Property;_ZoomScale;ZoomScale;3;0;Create;True;0;0;0;False;0;False;3;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;5;-424.6088,-84.29077;Float;False;Property;_MaxIter;MaxIter;0;0;Create;True;0;0;0;False;0;False;0;250;False;0;1;INT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;45;1494.506,-322.1884;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;ASESampleShaders/SRP Lightweight/2D Fractal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;23;0;24;0
WireConnection;23;1;24;1
WireConnection;23;2;24;2
WireConnection;24;0;18;0
WireConnection;18;0;20;0
WireConnection;18;1;38;0
WireConnection;18;2;38;0
WireConnection;20;0;19;0
WireConnection;20;1;22;0
WireConnection;22;0;21;0
WireConnection;22;1;38;0
WireConnection;38;0;3;0
WireConnection;3;0;12;0
WireConnection;3;1;5;0
WireConnection;3;2;6;0
WireConnection;12;0;8;0
WireConnection;12;1;13;0
WireConnection;8;0;25;0
WireConnection;8;1;4;0
WireConnection;4;0;42;0
WireConnection;4;1;43;0
WireConnection;25;0;26;0
WireConnection;25;1;31;0
WireConnection;31;0;7;3
WireConnection;31;1;9;0
WireConnection;31;2;16;0
ASEEND*/
//CHKSM=2AFF6A0A52CD0E17EFB72E9776A103805AE4F976