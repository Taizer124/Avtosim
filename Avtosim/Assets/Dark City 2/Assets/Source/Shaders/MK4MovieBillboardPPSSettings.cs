// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
#if UNITY_POST_PROCESSING_STACK_V2
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess( typeof( MK4MovieBillboardPPSRenderer ), PostProcessEvent.AfterStack, "MK4MovieBillboard", true )]
public sealed class MK4MovieBillboardPPSSettings : PostProcessEffectSettings
{
	[Tooltip( "Main texture" )]
	public TextureParameter _Maintexture = new TextureParameter {  };
	[Tooltip( "Masks" )]
	public TextureParameter _Masks = new TextureParameter {  };
	[Tooltip( "Color" )]
	public ColorParameter _Color = new ColorParameter { value = new Color(0.5807741f,0.7100198f,0.9632353f,0f) };
	[Tooltip( "Animation" )]
	public TextureParameter _Animation = new TextureParameter {  };
	[Tooltip( "Columns" )]
	public FloatParameter _Columns = new FloatParameter { value = 0f };
	[Tooltip( "Rows" )]
	public FloatParameter _Rows = new FloatParameter { value = 16f };
	[Tooltip( "Movie Speed" )]
	public FloatParameter _MovieSpeed = new FloatParameter { value = 1f };
	[Tooltip( "Specular" )]
	public FloatParameter _Specular = new FloatParameter { value = 0f };
	[Tooltip( "Smoothness" )]
	public FloatParameter _Smoothness = new FloatParameter { value = 0.5f };
	[Tooltip( "LED" )]
	public TextureParameter _LED = new TextureParameter {  };
	[Tooltip( "Emission Albedo" )]
	public FloatParameter _EmissionAlbedo = new FloatParameter { value = 1f };
	[Tooltip( "Emission LED" )]
	public FloatParameter _EmissionLED = new FloatParameter { value = 1f };
	[Tooltip( "Texture 0" )]
	public TextureParameter _Texture0 = new TextureParameter {  };
	[Tooltip( "Distort" )]
	public FloatParameter _Distort = new FloatParameter { value = 0.1f };
	[Tooltip( "Distort Speed" )]
	public FloatParameter _DistortSpeed = new FloatParameter { value = 0f };
}

public sealed class MK4MovieBillboardPPSRenderer : PostProcessEffectRenderer<MK4MovieBillboardPPSSettings>
{
	public override void Render( PostProcessRenderContext context )
	{
		var sheet = context.propertySheets.Get( Shader.Find( "MK4/Movie Billboard" ) );
		if(settings._Maintexture.value != null) sheet.properties.SetTexture( "_Maintexture", settings._Maintexture );
		if(settings._Masks.value != null) sheet.properties.SetTexture( "_Masks", settings._Masks );
		sheet.properties.SetColor( "_Color", settings._Color );
		if(settings._Animation.value != null) sheet.properties.SetTexture( "_Animation", settings._Animation );
		sheet.properties.SetFloat( "_Columns", settings._Columns );
		sheet.properties.SetFloat( "_Rows", settings._Rows );
		sheet.properties.SetFloat( "_MovieSpeed", settings._MovieSpeed );
		sheet.properties.SetFloat( "_Specular", settings._Specular );
		sheet.properties.SetFloat( "_Smoothness", settings._Smoothness );
		if(settings._LED.value != null) sheet.properties.SetTexture( "_LED", settings._LED );
		sheet.properties.SetFloat( "_EmissionAlbedo", settings._EmissionAlbedo );
		sheet.properties.SetFloat( "_EmissionLED", settings._EmissionLED );
		if(settings._Texture0.value != null) sheet.properties.SetTexture( "_Texture0", settings._Texture0 );
		sheet.properties.SetFloat( "_Distort", settings._Distort );
		sheet.properties.SetFloat( "_DistortSpeed", settings._DistortSpeed );
		context.command.BlitFullscreenTriangle( context.source, context.destination, sheet, 0 );
	}
}
#endif
