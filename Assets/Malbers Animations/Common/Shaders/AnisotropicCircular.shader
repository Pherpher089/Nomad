// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Anisotropic/Circular"
{
	Properties
	{
		_AlbedoTint("Tint", Color) = (1,1,1,1)
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[NoScaleOffset][SingleLineTexture]_AlbedoRGBOpacityA("Albedo (RGB) Opacity (A)", 2D) = "white" {}
		_AlbedoPower("Albedo Power", Range( 0 , 1)) = 1
		[NoScaleOffset][Normal][SingleLineTexture]_Normal("Normal", 2D) = "bump" {}
		_NormalAmount("Normal Amount", Float) = 1
		_Metallic("Metallic", Range( 0 , 1)) = 0
		[NoScaleOffset][SingleLineTexture]_Specular("Specular", 2D) = "white" {}
		_SpecularTint("Specular Tint", Color) = (1,1,1,1)
		_SpecularAdditive("Specular Additive", Float) = 0
		_SmoothMult("Smooth Mult", Range( 0 , 5)) = 0
		_AnisotropyFalloff("Anisotropy Falloff", Range( 1 , 256)) = 64
		_AnisotropyOffset("Anisotropy Offset", Range( -1 , 1)) = -1
		[Header(Hair Properties)][NoScaleOffset][SingleLineTexture]_FurMap1("Fur Map1 (A Height ID Root)", 2D) = "white" {}
		_ID("ID", Color) = (1,1,1,1)
		_IDPower("ID Power", Range( 0 , 3)) = 0
		_RootColor("Root Color", Color) = (1,1,1,1)
		_TipColor("Tip Color", Color) = (1,1,1,1)
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "DisableBatching" = "LODFading" }
		Cull Off
		ZWrite On
		ZTest LEqual
		Offset  0 , 0
		AlphaToMask On
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
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
			float2 uv_texcoord;
			half ASEIsFrontFacing : VFACE;
			float2 uv2_texcoord2;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		uniform sampler2D _Normal;
		uniform float _NormalAmount;
		uniform float4 _AlbedoTint;
		uniform float4 _ID;
		uniform sampler2D _FurMap1;
		uniform float _IDPower;
		uniform float4 _RootColor;
		uniform float4 _TipColor;
		uniform sampler2D _AlbedoRGBOpacityA;
		uniform float _AlbedoPower;
		uniform float4 _SpecularTint;
		uniform sampler2D _Specular;
		uniform float _AnisotropyOffset;
		uniform float _AnisotropyFalloff;
		uniform float _SpecularAdditive;
		uniform float _Metallic;
		uniform float _SmoothMult;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal45 = i.uv_texcoord;
			float3 NormalMap62 = UnpackScaleNormal( tex2D( _Normal, uv_Normal45, float2( 0,0 ), float2( 0,0 ) ), _NormalAmount );
			float3 switchResult80 = (((i.ASEIsFrontFacing>0)?(NormalMap62):(( 1.0 - NormalMap62 ))));
			o.Normal = switchResult80;
			float2 uv1_FurMap186 = i.uv2_texcoord2;
			float4 tex2DNode86 = tex2D( _FurMap1, uv1_FurMap186 );
			float clampResult90 = clamp( ( ( 1.0 - tex2DNode86.a ) + ( 1.0 - ( tex2DNode86.b * _IDPower ) ) ) , 0.0 , 1.0 );
			float4 lerpResult92 = lerp( _ID , float4( 1,1,1,1 ) , clampResult90);
			float4 lerpResult96 = lerp( _RootColor , _TipColor , tex2DNode86.a);
			float2 uv_AlbedoRGBOpacityA1 = i.uv_texcoord;
			float4 tex2DNode1 = tex2D( _AlbedoRGBOpacityA, uv_AlbedoRGBOpacityA1 );
			float2 uv_Specular4 = i.uv_texcoord;
			float3 PixelNormalWorld52 = normalize( (WorldNormalVector( i , NormalMap62 )) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 LightDirection16 = ase_worldlightDir;
			float3 normalizeResult9 = normalize( ( _WorldSpaceCameraPos - ase_worldPos ) );
			float3 ViewDirection11 = normalizeResult9;
			float3 normalizeResult18 = normalize( ( LightDirection16 + ViewDirection11 ) );
			float3 HalfVector46 = normalizeResult18;
			float dotResult23 = dot( PixelNormalWorld52 , HalfVector46 );
			float nDotH24 = dotResult23;
			float dotResult21 = dot( PixelNormalWorld52 , LightDirection16 );
			float nDotL22 = dotResult21;
			float4 temp_output_53_0 = ( ( ( _SpecularTint * tex2D( _Specular, uv_Specular4 ) ) * pow( max( sin( radians( ( ( _AnisotropyOffset + nDotH24 ) * 180.0 ) ) ) , 0.0 ) , _AnisotropyFalloff ) ) * nDotL22 );
			o.Albedo = ( ( _AlbedoTint * lerpResult92 * lerpResult96 * ( tex2DNode1 + ( 1.0 - _AlbedoPower ) ) ) + ( max( temp_output_53_0 , float4( 0,0,0,0 ) ) * _SpecularAdditive ) ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = ( temp_output_53_0 * _SmoothMult ).r;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows nofog dithercrossfade 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			AlphaToMask Off
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
				float4 customPack1 : TEXCOORD1;
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
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.zw = customInputData.uv2_texcoord2;
				o.customPack1.zw = v.texcoord1;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv2_texcoord2 = IN.customPack1.zw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
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
Version=19202
Node;AmplifyShaderEditor.CommentaryNode;57;-3991.898,642.3001;Inherit;False;891.5006;424.4899;View Direction Vector;4;10;11;9;8;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;10;-3974.112,869.1987;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;59;-3648.292,314.6999;Inherit;False;533.0206;260.4803;Light Direction Vector;2;14;16;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;8;-3699.808,732.3992;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;14;-3628.803,355.3016;Inherit;True;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;9;-3472.11,833.199;Inherit;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;60;-2989.588,457.5991;Inherit;False;661.2201;238.5203;Halfway Vector;3;46;18;17;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-3362.401,353.8022;Float;True;LightDirection;3;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-3304.008,728.8986;Float;False;ViewDirection;4;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;17;-2943.107,530.9026;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;61;-3555,-96;Inherit;False;537.9105;289.5802;Pixel Normal Vector;2;51;52;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;62;-3763.211,-48.7751;Float;False;NormalMap;1;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;18;-2808.305,500.902;Inherit;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;51;-3526,-44;Inherit;True;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;52;-3282,-33;Float;True;PixelNormalWorld;2;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-2570.402,505.2017;Float;False;HalfVector;6;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;23;-2335.101,279.9036;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-2166.802,277.3034;Float;False;nDotH;7;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-2303.304,176.503;Float;False;Property;_AnisotropyOffset;Anisotropy Offset;12;0;Create;True;0;0;0;False;0;False;-1;-1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-1955.204,253.5026;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-1787.703,253.0026;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;180;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;29;-1604.702,253.3027;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-1599.799,508.6237;Float;False;Property;_AnisotropyFalloff;Anisotropy Falloff;11;0;Create;True;0;0;0;False;0;False;64;64;1;256;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;30;-1430.901,254.2025;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;43;-1315.204,-195.798;Float;False;Property;_SpecularTint;Specular Tint;8;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;31;-1251.303,254.8026;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;21;-2942.199,74.0029;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;64;-1145.531,400.0973;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;38;-984.004,252.1033;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-1042.105,-89.49779;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-843.8511,160.7657;Inherit;False;22;nDotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-850.9669,-90.29791;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-595.3027,142.2005;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-353.6139,-199.6209;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-2140.916,-649.0289;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;88;-1931.918,-638.2176;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;89;-1767.509,-776.0026;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;90;-1538.624,-778.5245;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;91;-2122.835,-869.7506;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;92;-1317.57,-916.0865;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,1;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;93;-1774.795,-967.1556;Inherit;False;Property;_ID;ID;14;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;94;-2554.468,-908.6742;Inherit;False;Property;_IDPower;ID Power;15;0;Create;True;0;0;0;False;0;False;0;0;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-1387.158,-9.735434;Inherit;True;Property;_Specular;Specular;7;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;96;-1365.033,-1250.356;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;97;-1667.532,-1505.845;Inherit;False;Property;_RootColor;Root Color;16;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;98;-1669.981,-1329.074;Inherit;False;Property;_TipColor;Tip Color;17;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;84;-644.1923,737.7466;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;69;262.463,-370.0437;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Malbers/Anisotropic/Circular;False;False;False;False;False;False;False;False;False;True;False;False;True;LODFading;True;False;False;False;False;False;False;Off;1;False;;3;False;;True;0;False;;0;False;;False;1;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;4;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;1;False;;10;False;;0;5;False;;10;False;;0;False;;8;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Spherical;False;True;Relative;0;;1;-1;-1;-1;0;True;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;_NormalAmount;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.ColorNode;40;-826.97,-910.7953;Float;False;Property;_AlbedoTint;Tint;0;0;Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1346.8,-585.2773;Inherit;True;Property;_AlbedoRGBOpacityA;Albedo (RGB) Opacity (A);2;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;c83a98c6cb461fa41b0060690a288404;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-440.1932,-740.2839;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-822.9297,-616.7357;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;101;-1062.93,-384.7357;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-1354.93,-369.7357;Inherit;False;Property;_AlbedoPower;Albedo Power;3;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwitchByFaceNode;80;-382.3229,487.6734;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;-914.3653,641.902;Inherit;True;62;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-4306.828,48.32429;Float;False;Property;_NormalAmount;Normal Amount;5;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;45;-4087.228,-52.64075;Inherit;True;Property;_Normal;Normal;4;3;[NoScaleOffset];[Normal];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;34775b640332c3048b208fa36489a4d2;True;0;True;bump;Auto;True;Object;-1;Derivative;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-2679.366,86.38238;Float;False;nDotL;5;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;6;-3967.207,676.8943;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMaxOpNode;54;-254.4419,68.00514;Inherit;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-152.7873,-329.3844;Inherit;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;0;False;0;False;0;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;106;-85.72237,-621.222;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-822.1902,-347.4247;Inherit;False;Property;_SpecularAdditive;Specular Additive;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-715.5737,-178.9113;Inherit;False;Property;_SmoothMult;Smooth Mult;10;0;Create;True;0;0;0;False;0;False;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;-502.236,-479.3135;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;86;-2582.467,-805.4527;Inherit;True;Property;_FurMap1;Fur Map1 (A Height ID Root);13;3;[Header];[NoScaleOffset];[SingleLineTexture];Create;False;1;Hair Properties;0;0;False;0;False;-1;None;None;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;8;0;6;0
WireConnection;8;1;10;0
WireConnection;9;0;8;0
WireConnection;16;0;14;0
WireConnection;11;0;9;0
WireConnection;17;0;16;0
WireConnection;17;1;11;0
WireConnection;62;0;45;0
WireConnection;18;0;17;0
WireConnection;51;0;62;0
WireConnection;52;0;51;0
WireConnection;46;0;18;0
WireConnection;23;0;52;0
WireConnection;23;1;46;0
WireConnection;24;0;23;0
WireConnection;26;0;25;0
WireConnection;26;1;24;0
WireConnection;27;0;26;0
WireConnection;29;0;27;0
WireConnection;30;0;29;0
WireConnection;31;0;30;0
WireConnection;21;0;52;0
WireConnection;21;1;16;0
WireConnection;64;0;3;0
WireConnection;38;0;31;0
WireConnection;38;1;64;0
WireConnection;44;0;43;0
WireConnection;44;1;4;0
WireConnection;39;0;44;0
WireConnection;39;1;38;0
WireConnection;53;0;39;0
WireConnection;53;1;65;0
WireConnection;79;0;53;0
WireConnection;79;1;73;0
WireConnection;87;0;86;3
WireConnection;87;1;94;0
WireConnection;88;0;87;0
WireConnection;89;0;91;0
WireConnection;89;1;88;0
WireConnection;90;0;89;0
WireConnection;91;0;86;4
WireConnection;92;0;93;0
WireConnection;92;2;90;0
WireConnection;96;0;97;0
WireConnection;96;1;98;0
WireConnection;96;2;86;4
WireConnection;84;0;71;0
WireConnection;69;0;106;0
WireConnection;69;1;80;0
WireConnection;69;3;72;0
WireConnection;69;4;79;0
WireConnection;69;10;1;4
WireConnection;41;0;40;0
WireConnection;41;1;92;0
WireConnection;41;2;96;0
WireConnection;41;3;102;0
WireConnection;102;0;1;0
WireConnection;102;1;101;0
WireConnection;101;0;99;0
WireConnection;80;0;71;0
WireConnection;80;1;84;0
WireConnection;45;5;47;0
WireConnection;22;0;21;0
WireConnection;54;0;53;0
WireConnection;106;0;41;0
WireConnection;106;1;105;0
WireConnection;105;0;54;0
WireConnection;105;1;103;0
ASEEND*/
//CHKSM=7BC7083E4651D54DE272809EC957DA7EC9DF4A2A