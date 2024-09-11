// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios/GlacierTriplanar"
{
	Properties
	{
		_FallOff("FallOff", Range( 0 , 20)) = 10
		_Tiling("Tiling", Range( 0 , 10)) = 1
		[Header(Albedo)]_Top("Top", 2D) = "white" {}
		_Sides("Sides", 2D) = "white" {}
		_ShallowColoradd("Shallow Color (add)", Color) = (0.148938,0.5404209,0.723,0)
		_DeepColormulti("Deep Color (multi)", Color) = (0.07505883,0.1448601,0.3411765,0)
		_ColourFalloff("Colour Falloff", Float) = 1
		[Header(Normal)][Normal]_TopNormal("TopNormal", 2D) = "bump" {}
		[Normal]_SidesNormal("SidesNormal", 2D) = "bump" {}
		[Header(Emissive)]_TopEmissive("TopEmissive", 2D) = "black" {}
		_SidesEmissive("SidesEmissive", 2D) = "black" {}
		_TopMetallic("TopMetallic", Range( 0 , 1)) = 0
		_SideMetallic("SideMetallic", Range( 0 , 1)) = 0
		_TopSmoothness("TopSmoothness", Range( 0 , 1)) = 0.2
		_SideSmoothness("SideSmoothness", Range( 0 , 1)) = 0.2
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Overlay"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		uniform sampler2D _TopNormal;
		uniform sampler2D _SidesNormal;
		uniform float _Tiling;
		uniform float _FallOff;
		uniform sampler2D _Top;
		uniform sampler2D _Sides;
		uniform float4 _ShallowColoradd;
		uniform float4 _DeepColormulti;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _ColourFalloff;
		uniform sampler2D _TopEmissive;
		uniform sampler2D _SidesEmissive;
		uniform float _SideMetallic;
		uniform float _TopMetallic;
		uniform float _SideSmoothness;
		uniform float _TopSmoothness;


		inline float4 TriplanarSampling8( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling5( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling21( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float Tiling15 = _Tiling;
			float Falloff14 = _FallOff;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar8 = TriplanarSampling8( _TopNormal, _SidesNormal, _SidesNormal, ase_worldPos, ase_worldNormal, Falloff14, Tiling15, float3( 1,1,1 ), float3(0,0,0) );
			o.Normal = UnpackNormal( triplanar8 );
			float4 triplanar5 = TriplanarSampling5( _Top, _Sides, _Sides, ase_worldPos, ase_worldNormal, Falloff14, Tiling15, float3( 1,1,1 ), float3(0,0,0) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth58 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth58 = abs( ( screenDepth58 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 1.0 ) );
			float4 lerpResult57 = lerp( ( triplanar5 + _ShallowColoradd ) , ( triplanar5 * _DeepColormulti ) , saturate( ( distanceDepth58 / _ColourFalloff ) ));
			float temp_output_43_0 = saturate( ( pow( ase_worldNormal.y , 10.0 ) * Falloff14 ) );
			float top61 = temp_output_43_0;
			float4 lerpResult60 = lerp( lerpResult57 , triplanar5 , top61);
			o.Albedo = lerpResult60.xyz;
			float4 triplanar21 = TriplanarSampling21( _TopEmissive, _SidesEmissive, _SidesEmissive, ase_worldPos, ase_worldNormal, Falloff14, Tiling15, float3( 1,1,1 ), float3(0,0,0) );
			o.Emission = triplanar21.xyz;
			float lerpResult44 = lerp( _SideMetallic , _TopMetallic , temp_output_43_0);
			o.Metallic = lerpResult44;
			float lerpResult49 = lerp( _SideSmoothness , _TopSmoothness , temp_output_43_0);
			o.Smoothness = lerpResult49;
			o.Alpha = 1;
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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 screenPos : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
-3429;11;2956;1359;3555.806;839.571;1.667148;True;True
Node;AmplifyShaderEditor.RangedFloatNode;1;-1953.097,372.8673;Float;False;Property;_FallOff;FallOff;1;0;Create;True;0;0;0;False;0;False;10;10;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-1953.333,472.0004;Float;False;Property;_Tiling;Tiling;2;0;Create;True;0;0;0;False;0;False;1;6.24;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;37;-1175.168,1574.084;Inherit;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-1617.333,371.0004;Inherit;False;Falloff;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1132.286,1817.608;Inherit;False;Constant;_Height;Height;11;0;Create;True;0;0;0;False;0;False;10;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;-1622.333,470.0004;Inherit;False;Tiling;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;40;-929.2867,1693.608;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-956.0477,1892.131;Inherit;False;14;Falloff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;19;-890.833,189.0004;Inherit;False;15;Tiling;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;18;-881.833,271.0004;Inherit;False;14;Falloff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;3;-1151.096,62.62511;Float;True;Property;_Sides;Sides;4;0;Create;True;0;0;0;True;0;False;None;9e88b336bd16b1e4b99de75f486126c1;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.DepthFade;58;-487.4905,-635.4455;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-888.0966,-100.1325;Float;True;Property;_Top;Top;3;1;[Header];Create;True;1;Albedo;0;0;True;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-778.2867,1777.608;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-444.5012,-505.6285;Inherit;False;Property;_ColourFalloff;Colour Falloff;7;0;Create;True;0;0;0;False;0;False;1;9.39;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;63;-393.1145,-352.8102;Float;False;Property;_ShallowColoradd;Shallow Color (add);5;0;Create;True;0;0;0;False;0;False;0.148938,0.5404209,0.723,0;0,0.8088232,0.8088235,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;5;-595.0159,84.01523;Inherit;True;Cylindrical;World;False;Top Texture 0;_TopTexture0;white;2;None;Mid Texture 0;_MidTexture0;white;1;None;Bot Texture 0;_BotTexture0;white;3;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT3;1,1,1;False;3;FLOAT;1;False;4;FLOAT;100;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;54;-400.2246,-172.2021;Float;False;Property;_DeepColormulti;Deep Color (multi);6;0;Create;True;0;0;0;False;0;False;0.07505883,0.1448601,0.3411765,0;0,0.04310164,0.2499982,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;43;-630.2868,1781.608;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;53;-226.5011,-534.6285;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;16;-901.333,709.0004;Inherit;False;14;Falloff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;17;-910.333,627.0004;Inherit;False;15;Tiling;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-81.1145,-289.8102;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TexturePropertyNode;9;-1187.072,562.5331;Float;True;Property;_SidesNormal;SidesNormal;9;1;[Normal];Create;True;0;0;0;True;0;False;None;None;False;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;10;-1184.704,362.9987;Float;True;Property;_TopNormal;TopNormal;8;2;[Header];[Normal];Create;True;1;Normal;0;0;True;0;False;None;None;False;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-79.82053,-145.6099;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;-114.8436,1797.384;Inherit;False;top;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;56;-65.5013,-522.6285;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;23;-940.8419,1260.819;Inherit;False;14;Falloff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;22;-1210.105,1052.443;Float;True;Property;_SidesEmissive;SidesEmissive;11;0;Create;True;0;0;0;True;0;False;None;None;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;45;-670.8475,1689.031;Inherit;False;Property;_TopMetallic;TopMetallic;12;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-662.9474,1601.631;Inherit;False;Property;_SideMetallic;SideMetallic;13;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;57;148.5353,-171.442;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-694.7139,2022.951;Inherit;False;Property;_TopSmoothness;TopSmoothness;14;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;90.29571,176.2771;Inherit;False;61;top;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;24;-949.8419,1178.819;Inherit;False;15;Tiling;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;8;-592.0234,440.2465;Inherit;True;Cylindrical;World;False;Top Texture 1;_TopTexture1;white;2;None;Mid Texture 1;_MidTexture1;white;1;None;Bot Texture 1;_BotTexture1;white;3;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT3;1,1,1;False;3;FLOAT;1;False;4;FLOAT;100;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;20;-947.1055,889.6854;Float;True;Property;_TopEmissive;TopEmissive;10;1;[Header];Create;True;1;Emissive;0;0;True;0;False;None;None;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;51;-696.4782,1936.886;Inherit;False;Property;_SideSmoothness;SideSmoothness;15;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;44;-364.8983,1696.354;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;60;305.4433,72.87328;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TriplanarNode;21;-654.0248,1073.833;Inherit;True;Cylindrical;World;False;Top Texture 2;_TopTexture2;white;2;None;Mid Texture 2;_MidTexture2;white;1;None;Bot Texture 2;_BotTexture2;white;3;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT3;1,1,1;False;3;FLOAT;1;False;4;FLOAT;100;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.UnpackScaleNormalNode;12;-150.3328,477.0004;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;49;-368.1173,1957.89;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;531.1,77.5;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SyntyStudios/GlacierTriplanar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;False;Overlay;;Transparent;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;1;0
WireConnection;15;0;13;0
WireConnection;40;0;37;2
WireConnection;40;1;38;0
WireConnection;41;0;40;0
WireConnection;41;1;48;0
WireConnection;5;0;2;0
WireConnection;5;1;3;0
WireConnection;5;2;3;0
WireConnection;5;3;19;0
WireConnection;5;4;18;0
WireConnection;43;0;41;0
WireConnection;53;0;58;0
WireConnection;53;1;52;0
WireConnection;65;0;5;0
WireConnection;65;1;63;0
WireConnection;59;0;5;0
WireConnection;59;1;54;0
WireConnection;61;0;43;0
WireConnection;56;0;53;0
WireConnection;57;0;65;0
WireConnection;57;1;59;0
WireConnection;57;2;56;0
WireConnection;8;0;10;0
WireConnection;8;1;9;0
WireConnection;8;2;9;0
WireConnection;8;3;17;0
WireConnection;8;4;16;0
WireConnection;44;0;46;0
WireConnection;44;1;45;0
WireConnection;44;2;43;0
WireConnection;60;0;57;0
WireConnection;60;1;5;0
WireConnection;60;2;62;0
WireConnection;21;0;20;0
WireConnection;21;1;22;0
WireConnection;21;2;22;0
WireConnection;21;3;24;0
WireConnection;21;4;23;0
WireConnection;12;0;8;0
WireConnection;49;0;51;0
WireConnection;49;1;50;0
WireConnection;49;2;43;0
WireConnection;0;0;60;0
WireConnection;0;1;12;0
WireConnection;0;2;21;0
WireConnection;0;3;44;0
WireConnection;0;4;49;0
ASEEND*/
//CHKSM=5F8423C294A8621356399892A992A22A1D1EEAE4