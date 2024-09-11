// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios/GlacierIce"
{
	Properties
	{
		_ShallowColor("Shallow Color", Color) = (0.148938,0.5404209,0.723,0)
		_DeepColor("Deep Color", Color) = (0.07505883,0.1448601,0.3411765,0)
		_ColourFalloff("Colour Falloff", Float) = 1
		_OpacityFalloff("Opacity Falloff", Float) = 1
		_OpacityMin("Opacity Min", Range( 0 , 1)) = 0.5
		_OpacityMulti("Opacity Multi", Range( 0 , 2)) = 1
		_Metallic("Metallic", Range( 0 , 1)) = 0.5
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.7
		[Header(Frosting)]_Snow("Snow", Color) = (1,1,1,0)
		[ToggleUI]_SnowToggle("SnowToggle", Float) = 1
		_SnowHeight("SnowHeight", Float) = 1
		_SnowFalloff("SnowFalloff", Float) = 1
		_MetallicSnow("MetallicSnow", Range( 0 , 1)) = 0.5
		_SmoothnessSnow("SmoothnessSnow", Range( 0 , 1)) = 0.7
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Overlay"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float4 screenPos;
			float3 worldNormal;
		};

		uniform float4 _ShallowColor;
		uniform float4 _DeepColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _ColourFalloff;
		uniform float4 _Snow;
		uniform float _SnowHeight;
		uniform float _SnowFalloff;
		uniform float _SnowToggle;
		uniform float _Metallic;
		uniform float _MetallicSnow;
		uniform float _Smoothness;
		uniform float _SmoothnessSnow;
		uniform float _OpacityFalloff;
		uniform float _OpacityMin;
		uniform float _OpacityMulti;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth25 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth25 = abs( ( screenDepth25 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 1.0 ) );
			float4 lerpResult19 = lerp( _ShallowColor , _DeepColor , saturate( ( distanceDepth25 / _ColourFalloff ) ));
			float3 ase_worldNormal = i.worldNormal;
			float temp_output_41_0 = saturate( ( pow( ase_worldNormal.y , _SnowHeight ) * _SnowFalloff ) );
			float4 lerpResult43 = lerp( lerpResult19 , _Snow , temp_output_41_0);
			float4 lerpResult49 = lerp( lerpResult19 , lerpResult43 , _SnowToggle);
			o.Albedo = lerpResult49.rgb;
			float lerpResult44 = lerp( _Metallic , _MetallicSnow , temp_output_41_0);
			o.Metallic = lerpResult44;
			float lerpResult45 = lerp( _Smoothness , _SmoothnessSnow , temp_output_41_0);
			o.Smoothness = lerpResult45;
			o.Alpha = ( (_OpacityMin + (saturate( ( distanceDepth25 / _OpacityFalloff ) ) - 0.0) * (1.0 - _OpacityMin) / (1.0 - 0.0)) * _OpacityMulti );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldNormal = IN.worldNormal;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18909
-3434;8;2956;1351;2121.162;814.604;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;34;-1355.162,164.3961;Inherit;False;Property;_ColourFalloff;Colour Falloff;3;0;Create;True;0;0;0;False;0;False;1;9.39;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;37;-962.0321,-809.726;Inherit;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;36;-919.1509,-566.2019;Inherit;False;Property;_SnowHeight;SnowHeight;11;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;25;-1453.189,788.4273;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-1361.577,917.3949;Inherit;False;Property;_OpacityFalloff;Opacity Falloff;4;0;Create;True;0;0;0;False;0;False;1;9.39;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-825.89,-459.1359;Inherit;False;Property;_SnowFalloff;SnowFalloff;12;0;Create;True;1;Triplanar;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;38;-716.1509,-690.2019;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;33;-1137.162,135.3961;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-1031.328,-59.41629;Float;False;Property;_DeepColor;Deep Color;2;0;Create;True;0;0;0;False;0;False;0.07505883,0.1448601,0.3411765,0;0,0.04310164,0.2499982,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-1030.927,-228.2155;Float;False;Property;_ShallowColor;Shallow Color;1;0;Create;True;0;0;0;False;0;False;0.148938,0.5404209,0.723,0;0,0.8088232,0.8088235,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;35;-976.1621,147.3961;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;27;-1158.577,801.3949;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-565.1509,-606.2019;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;-619.3278,1.384697;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;42;-425.3086,-817.5048;Inherit;False;Property;_Snow;Snow;9;1;[Header];Create;True;1;Frosting;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;29;-1088.433,930.2628;Inherit;False;Property;_OpacityMin;Opacity Min;5;0;Create;True;0;0;0;False;0;False;0.5;0.32;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;28;-951.433,822.2628;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;41;-417.1509,-602.2019;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;43;-176.4625,-328.6559;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-671.1384,233.9096;Float;False;Property;_Metallic;Metallic;7;0;Create;True;0;0;0;False;0;False;0.5;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1082.015,1035.102;Float;False;Property;_OpacityMulti;Opacity Multi;6;0;Create;True;0;0;0;False;0;False;1;0.9;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-667.9393,409.2061;Float;False;Property;_Smoothness;Smoothness;8;0;Create;True;0;0;0;False;0;False;0.7;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-673.1621,315.3961;Float;False;Property;_MetallicSnow;MetallicSnow;13;0;Create;True;0;0;0;False;0;False;0.5;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-669.1621,499.3961;Float;False;Property;_SmoothnessSnow;SmoothnessSnow;14;0;Create;True;0;0;0;False;0;False;0.7;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;31;-763.4327,821.2628;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-208.1621,-47.604;Inherit;False;Property;_SnowToggle;SnowToggle;10;1;[ToggleUI];Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;45;-237.1621,298.3961;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-519.4327,900.2628;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;44;-238.1621,101.3961;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;49;-16.16211,-156.604;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;166,3;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SyntyStudios/GlacierIce;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Overlay;;Transparent;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;38;0;37;2
WireConnection;38;1;36;0
WireConnection;33;0;25;0
WireConnection;33;1;34;0
WireConnection;35;0;33;0
WireConnection;27;0;25;0
WireConnection;27;1;26;0
WireConnection;40;0;38;0
WireConnection;40;1;39;0
WireConnection;19;0;15;0
WireConnection;19;1;13;0
WireConnection;19;2;35;0
WireConnection;28;0;27;0
WireConnection;41;0;40;0
WireConnection;43;0;19;0
WireConnection;43;1;42;0
WireConnection;43;2;41;0
WireConnection;31;0;28;0
WireConnection;31;3;29;0
WireConnection;45;0;22;0
WireConnection;45;1;47;0
WireConnection;45;2;41;0
WireConnection;32;0;31;0
WireConnection;32;1;30;0
WireConnection;44;0;21;0
WireConnection;44;1;46;0
WireConnection;44;2;41;0
WireConnection;49;0;19;0
WireConnection;49;1;43;0
WireConnection;49;2;48;0
WireConnection;0;0;49;0
WireConnection;0;3;44;0
WireConnection;0;4;45;0
WireConnection;0;9;32;0
ASEEND*/
//CHKSM=388D556D5C1FCD5B109E68D39FE48AACB8D063C2