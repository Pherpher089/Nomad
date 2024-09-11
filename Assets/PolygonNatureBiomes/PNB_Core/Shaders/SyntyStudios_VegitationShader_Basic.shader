// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios/VegitationShader_Basic"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[Header(Leaves)]_LeafTex("LeafTex", 2D) = "white" {}
		_LeafNormalMap("LeafNormalMap", 2D) = "white" {}
		_LeafMetallic("LeafMetallic", Range( 0 , 1)) = 0.13
		_LeafSmoothness("LeafSmoothness", Range( 0 , 1)) = 0.5
		[Header(Trunk)]_TunkTex("TunkTex", 2D) = "white" {}
		_TrunkNormalMap("TrunkNormalMap", 2D) = "white" {}
		_TrunkMetallic("TrunkMetallic", Range( 0 , 1)) = 0
		_TrunkSmoothness("TrunkSmoothness", Range( 0 , 1)) = 0.2
		[Header(Colour Tinting)]_LeafBaseColour("LeafBaseColour", Color) = (0.07843138,0.02015968,0,0)
		_TrunkBaseColour("TrunkBaseColour", Color) = (0.07843138,0.02015968,0,0)
		[Header(Emissive)]_EmissiveAmount("EmissiveAmount", Range( 0 , 2)) = 0
		_EmissiveMask("EmissiveMask", 2D) = "white" {}
		_EmissiveColour("EmissiveColour", Color) = (0,0,0,0)
		_TrunkEmissiveColour("TrunkEmissiveColour", Color) = (0,0,0,0)
		_TrunkEmissiveMask("TrunkEmissiveMask", 2D) = "white" {}
		[Header(Frosting)]_FrostingColour("FrostingColour", Color) = (1,1,1,0)
		[Toggle]_FrostingSwitch("FrostingSwitch", Float) = 0
		_FrostingHeight("FrostingHeight", Float) = 1
		_FrostingFalloff("FrostingFalloff", Float) = 1
		[ToggleUI]_FrostingWorldObjectSwitch1("FrostingWorldObjectSwitch", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
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
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _LeafNormalMap;
		uniform float4 _LeafNormalMap_ST;
		uniform sampler2D _TrunkNormalMap;
		uniform float4 _TrunkNormalMap_ST;
		uniform float _FrostingSwitch;
		uniform sampler2D _LeafTex;
		uniform float4 _LeafTex_ST;
		uniform float4 _LeafBaseColour;
		uniform sampler2D _TunkTex;
		uniform float4 _TunkTex_ST;
		uniform float4 _TrunkBaseColour;
		uniform float4 _FrostingColour;
		uniform float _FrostingWorldObjectSwitch1;
		uniform float _FrostingHeight;
		uniform float _FrostingFalloff;
		uniform float4 _EmissiveColour;
		uniform sampler2D _EmissiveMask;
		uniform float4 _EmissiveMask_ST;
		uniform float4 _TrunkEmissiveColour;
		uniform sampler2D _TrunkEmissiveMask;
		uniform float4 _TrunkEmissiveMask_ST;
		uniform float _EmissiveAmount;
		uniform float _LeafMetallic;
		uniform float _TrunkMetallic;
		uniform float _LeafSmoothness;
		uniform float _TrunkSmoothness;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_LeafNormalMap = i.uv_texcoord * _LeafNormalMap_ST.xy + _LeafNormalMap_ST.zw;
			float2 uv_TrunkNormalMap = i.uv_texcoord * _TrunkNormalMap_ST.xy + _TrunkNormalMap_ST.zw;
			o.Normal = ( i.vertexColor.b > 0.5 ? tex2D( _LeafNormalMap, uv_LeafNormalMap ) : tex2D( _TrunkNormalMap, uv_TrunkNormalMap ) ).rgb;
			float2 uv_LeafTex = i.uv_texcoord * _LeafTex_ST.xy + _LeafTex_ST.zw;
			float4 tex2DNode70 = tex2D( _LeafTex, uv_LeafTex );
			float4 blendOpSrc73 = tex2DNode70;
			float4 blendOpDest73 = _LeafBaseColour;
			float2 uv_TunkTex = i.uv_texcoord * _TunkTex_ST.xy + _TunkTex_ST.zw;
			float4 tex2DNode550 = tex2D( _TunkTex, uv_TunkTex );
			float4 blendOpSrc574 = tex2DNode550;
			float4 blendOpDest574 = _TrunkBaseColour;
			float4 temp_output_551_0 = ( i.vertexColor.b > 0.5 ? 	max( blendOpSrc73, blendOpDest73 ) : 	max( blendOpSrc574, blendOpDest574 ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float lerpResult582 = lerp( ase_worldNormal.y , ase_vertexNormal.y , _FrostingWorldObjectSwitch1);
			float4 lerpResult461 = lerp( temp_output_551_0 , _FrostingColour , saturate( ( pow( lerpResult582 , _FrostingHeight ) * _FrostingFalloff ) ));
			float4 Albedo78 = (( _FrostingSwitch )?( lerpResult461 ):( temp_output_551_0 ));
			o.Albedo = Albedo78.rgb;
			float2 uv_EmissiveMask = i.uv_texcoord * _EmissiveMask_ST.xy + _EmissiveMask_ST.zw;
			float4 lerpResult417 = lerp( _EmissiveColour , float4( 0,0,0,0 ) , ( 1.0 - tex2D( _EmissiveMask, uv_EmissiveMask ).r ));
			float2 uv_TrunkEmissiveMask = i.uv_texcoord * _TrunkEmissiveMask_ST.xy + _TrunkEmissiveMask_ST.zw;
			float4 lerpResult569 = lerp( _TrunkEmissiveColour , float4( 0,0,0,0 ) , ( 1.0 - tex2D( _TrunkEmissiveMask, uv_TrunkEmissiveMask ).r ));
			float4 Emissive420 = ( ( i.vertexColor.b > 0.5 ? lerpResult417 : lerpResult569 ) * _EmissiveAmount );
			o.Emission = Emissive420.rgb;
			float temp_output_189_0 = ( 1.0 - i.vertexColor.b );
			float lerpResult188 = lerp( _LeafMetallic , 0.0 , temp_output_189_0);
			float lerpResult195 = lerp( _TrunkMetallic , 0.0 , i.vertexColor.b);
			o.Metallic = ( lerpResult188 + lerpResult195 );
			float lerpResult191 = lerp( _LeafSmoothness , 0.0 , temp_output_189_0);
			float lerpResult190 = lerp( _TrunkSmoothness , 0.0 , i.vertexColor.b);
			o.Smoothness = ( lerpResult191 + lerpResult190 );
			o.Alpha = 1;
			float Alpha77 = ( i.vertexColor.b > 0.5 ? tex2DNode70.a : tex2DNode550.a );
			clip( Alpha77 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows dithercrossfade 

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
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
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
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
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
-3423;8;2952;1366;2172.695;203.4297;1;True;True
Node;AmplifyShaderEditor.WorldNormalVector;492;-3345.383,-319.2433;Inherit;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;580;-3398.789,-601.3847;Inherit;False;Property;_FrostingWorldObjectSwitch1;FrostingWorldObjectSwitch;22;1;[ToggleUI];Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;581;-3337.789,-493.3847;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;496;-3302.502,-75.72005;Inherit;False;Property;_FrostingHeight;FrostingHeight;20;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;459;-3103.675,761.2257;Inherit;False;1688.442;867.9663;Emissive;14;420;208;209;565;569;417;563;564;567;419;418;568;200;566;Emmisive;0,0.5021453,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;582;-3093.789,-520.3847;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;63;-4348.655,-114.8565;Inherit;False;Property;_LeafBaseColour;LeafBaseColour;11;1;[Header];Create;True;1;Colour Tinting;0;0;False;0;False;0.07843138,0.02015968,0,0;0.6214569,0.716,0.09665998,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;218;-4272.236,312.3761;Inherit;False;Property;_TrunkBaseColour;TrunkBaseColour;12;0;Create;True;0;0;0;False;0;False;0.07843138,0.02015968,0,0;0.5188679,0.3964281,0.354886,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;70;-4337.806,-354.5892;Inherit;True;Property;_LeafTex;LeafTex;1;1;[Header];Create;True;1;Leaves;0;0;False;0;False;-1;None;830613e727dfa2344816278ade4ac30e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;550;-4355.063,69.98515;Inherit;True;Property;_TunkTex;TunkTex;6;1;[Header];Create;True;1;Trunk;0;0;False;0;False;-1;None;830613e727dfa2344816278ade4ac30e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;574;-3987.24,181.9075;Inherit;False;Lighten;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;553;-3942.684,-53.1058;Inherit;False;Constant;_Float6;Float 6;46;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;552;-3947.684,-219.1051;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;566;-2988.205,1404.061;Inherit;True;Property;_TrunkEmissiveMask;TrunkEmissiveMask;17;0;Create;True;1;Emissive;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;73;-3985.805,41.4267;Inherit;False;Lighten;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;471;-3209.241,31.34607;Inherit;False;Property;_FrostingFalloff;FrostingFalloff;21;0;Create;True;1;Triplanar;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;200;-3030.112,1095.212;Inherit;True;Property;_EmissiveMask;EmissiveMask;14;0;Create;True;1;Emissive;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;495;-3099.502,-199.7198;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;419;-2505.446,1116.566;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;568;-2686.539,1310.416;Inherit;False;Property;_TrunkEmissiveColour;TrunkEmissiveColour;16;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;494;-2948.502,-115.72;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;567;-2465.539,1425.415;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;418;-2726.446,1001.565;Inherit;False;Property;_EmissiveColour;EmissiveColour;15;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Compare;551;-3754.685,-82.1053;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;564;-2314.527,911.4803;Inherit;False;Constant;_Float9;Float 9;46;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;417;-2326.446,1068.566;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;563;-2527.036,852.0372;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;569;-2286.539,1377.416;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;493;-2800.502,-111.72;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;481;-2808.66,-327.0221;Inherit;False;Property;_FrostingColour;FrostingColour;18;1;[Header];Create;True;1;Frosting;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;575;-3384.441,102.2076;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;578;-4008.453,-345.6935;Inherit;False;Constant;_Float7;Float 6;46;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;577;-4013.453,-511.6927;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;461;-2570.914,18.12486;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;576;-3396.14,149.0076;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;187;-1405.013,292.136;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;209;-2092.893,1284.148;Inherit;False;Property;_EmissiveAmount;EmissiveAmount;13;1;[Header];Create;True;1;Emissive;0;0;False;0;False;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;565;-2123.496,998.2015;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;558;-635.664,-407.2;Inherit;False;Constant;_Float8;Float 8;46;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-1832.897,1157.248;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;189;-1211.013,306.136;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;562;-917.9802,-78.95667;Inherit;True;Property;_TrunkNormalMap;TrunkNormalMap;7;0;Create;True;0;0;0;False;0;False;-1;None;817148d55ddfd564bb99d9add95c293e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.5;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;559;-847.1731,-464.6424;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;186;-1052.013,155.136;Inherit;False;Property;_LeafMetallic;LeafMetallic;4;0;Create;True;0;0;0;False;0;False;0.13;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-1038.013,594.136;Inherit;False;Property;_TrunkSmoothness;TrunkSmoothness;10;0;Create;True;0;0;0;False;0;False;0.2;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;194;-1041.513,291.636;Inherit;False;Property;_TrunkMetallic;TrunkMetallic;9;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;81;-912.2821,-271.5256;Inherit;True;Property;_LeafNormalMap;LeafNormalMap;2;0;Create;True;0;0;0;False;0;False;-1;None;817148d55ddfd564bb99d9add95c293e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.5;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;185;-1027.013,464.1361;Inherit;False;Property;_LeafSmoothness;LeafSmoothness;5;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;480;-2394.873,196.7932;Inherit;False;Property;_FrostingSwitch;FrostingSwitch;19;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Compare;579;-3830.454,-391.693;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;191;-744.0126,443.1361;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-3595.427,-377.4442;Inherit;False;Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;190;-743.0126,562.136;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;557;-496.6338,-227.4788;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;195;-738.5123,273.636;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;188;-745.0126,134.136;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;78;-1776.564,236.6218;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;420;-1642.71,1182.965;Inherit;False;Emissive;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;561;-1189.049,-58.71536;Inherit;False;Property;_TrunkNormalAmount;TrunkNormalAmount;8;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;211;-1183.351,-251.2843;Inherit;False;Property;_LeafNormalAmount;LeafNormalAmount;3;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;196;-489.0125,245.136;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;198;-172.867,14.31301;Inherit;False;78;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;560;-262.5151,-17.83606;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;421;-200.4588,209.9659;Inherit;False;420;Emissive;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;193;-537.0126,488.1361;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;199;-313.867,445.3131;Inherit;False;77;Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;41.30931,209.989;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SyntyStudios/VegitationShader_Basic;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;False;TransparentCutout;;Geometry;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;582;0;492;2
WireConnection;582;1;581;2
WireConnection;582;2;580;0
WireConnection;574;0;550;0
WireConnection;574;1;218;0
WireConnection;73;0;70;0
WireConnection;73;1;63;0
WireConnection;495;0;582;0
WireConnection;495;1;496;0
WireConnection;419;0;200;1
WireConnection;494;0;495;0
WireConnection;494;1;471;0
WireConnection;567;0;566;1
WireConnection;551;0;552;3
WireConnection;551;1;553;0
WireConnection;551;2;73;0
WireConnection;551;3;574;0
WireConnection;417;0;418;0
WireConnection;417;2;419;0
WireConnection;569;0;568;0
WireConnection;569;2;567;0
WireConnection;493;0;494;0
WireConnection;575;0;551;0
WireConnection;461;0;575;0
WireConnection;461;1;481;0
WireConnection;461;2;493;0
WireConnection;576;0;551;0
WireConnection;565;0;563;3
WireConnection;565;1;564;0
WireConnection;565;2;417;0
WireConnection;565;3;569;0
WireConnection;208;0;565;0
WireConnection;208;1;209;0
WireConnection;189;0;187;3
WireConnection;480;0;576;0
WireConnection;480;1;461;0
WireConnection;579;0;577;3
WireConnection;579;1;578;0
WireConnection;579;2;70;4
WireConnection;579;3;550;4
WireConnection;191;0;185;0
WireConnection;191;2;189;0
WireConnection;77;0;579;0
WireConnection;190;0;192;0
WireConnection;190;2;187;3
WireConnection;557;0;559;3
WireConnection;557;1;558;0
WireConnection;557;2;81;0
WireConnection;557;3;562;0
WireConnection;195;0;194;0
WireConnection;195;2;187;3
WireConnection;188;0;186;0
WireConnection;188;2;189;0
WireConnection;78;0;480;0
WireConnection;420;0;208;0
WireConnection;196;0;188;0
WireConnection;196;1;195;0
WireConnection;560;0;557;0
WireConnection;193;0;191;0
WireConnection;193;1;190;0
WireConnection;0;0;198;0
WireConnection;0;1;560;0
WireConnection;0;2;421;0
WireConnection;0;3;196;0
WireConnection;0;4;193;0
WireConnection;0;10;199;0
ASEEND*/
//CHKSM=95F84E42CC1BDC7C97129AA71F920ABB02B2890C