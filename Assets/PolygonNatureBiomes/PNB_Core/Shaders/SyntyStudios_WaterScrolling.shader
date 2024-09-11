// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios/WaterScrolling"
{
	Properties
	{
		_Opacity("Opacity", Range( 0 , 1)) = 1
		_OpacityFalloff("Opacity Falloff", Float) = 1
		_OpacityMin("Opacity Min", Range( 0 , 1)) = 0.5
		_Specular("Specular", Range( 0 , 1)) = 0.141
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_ReflectionPower("Reflection Power", Range( 0 , 1)) = 0.346
		[Header(Colour)]_ShallowColour("Shallow Colour", Color) = (0.9607843,0.7882353,0.5764706,0)
		_DeepColour("Deep Colour", Color) = (0.04705882,0.3098039,0.1960784,0)
		_VeryDeepColour("Very Deep Colour", Color) = (0.05959199,0.08247829,0.191,0)
		_ShallowFalloff("ShallowFalloff", Float) = 0.4
		_OverallFalloff("OverallFalloff", Range( 0 , 10)) = 0.76
		_Depth("Depth", Float) = 0.28
		[Header(Refraction)]_DistortionMap("Distortion Map", 2D) = "bump" {}
		_DistortionTiling("Distortion Tiling", Float) = 0.33
		_Distortion("Distortion", Range( 0 , 1)) = 0.292
		_DistortionSpeed("Distortion Speed", Range( 0 , 1)) = 0.236
		[Header(Foam)]_FoamSmoothness("Foam Smoothness", Range( 0 , 1)) = 0
		_FoamSpecular("Foam Specular", Range( 0 , 1)) = 0
		_FoamOpacity("FoamOpacity", Range( 0 , 1)) = 1
		_FoamShoreline("Foam Shoreline", Range( 0 , 1)) = 0
		_FoamFalloff("Foam Falloff", Float) = -56
		_Foam_Texture("Foam_Texture", 2D) = "white" {}
		_TilingX("Tiling X", Float) = 1
		_TilingY("Tiling Y", Float) = 1
		_OffsetX("Offset X", Float) = 0
		_OffsetY("Offset Y", Float) = 0
		_ScrollSpeedX("Scroll Speed X", Float) = 0
		_ScrollSpeedY("Scroll Speed Y", Float) = 0.1
		[Header(Normal Map)]_Ripples("Ripples", 2D) = "bump" {}
		_RippleAmount("RippleAmount", Range( 0 , 10)) = 1
		_RippleSpeedMulti("RippleSpeedMulti", Range( 0 , 10)) = 1
		[Header(Glow)]_DepthGlowColour("Depth Glow Colour", Color) = (0,0,0,0)
		_GlowDepth("Glow Depth", Float) = 0.1
		_GlowFalloff("Glow Falloff", Range( 0 , 1)) = 0.1
		_FoamEmitColour("Foam Emit Colour", Color) = (0,0,0,0)
		_FoamGlowMultiplier("Foam Glow Multiplier", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
		};

		uniform sampler2D _Ripples;
		uniform half _ScrollSpeedX;
		uniform half _ScrollSpeedY;
		uniform half _RippleSpeedMulti;
		uniform half _TilingX;
		uniform half _TilingY;
		uniform half _OffsetX;
		uniform half _OffsetY;
		uniform half _RippleAmount;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float _OverallFalloff;
		uniform half _ShallowFalloff;
		uniform float4 _ShallowColour;
		uniform float4 _DeepColour;
		uniform float4 _VeryDeepColour;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform sampler2D _DistortionMap;
		uniform float _DistortionSpeed;
		uniform half _DistortionTiling;
		uniform float _Distortion;
		uniform float _FoamShoreline;
		uniform float _FoamFalloff;
		uniform sampler2D _Foam_Texture;
		uniform half _FoamOpacity;
		uniform half4 _DepthGlowColour;
		uniform half _GlowDepth;
		uniform half _GlowFalloff;
		uniform half _FoamGlowMultiplier;
		uniform half4 _FoamEmitColour;
		uniform float _Specular;
		uniform float _FoamSpecular;
		uniform float _Smoothness;
		uniform float _FoamSmoothness;
		uniform float _ReflectionPower;
		uniform half _OpacityFalloff;
		uniform half _OpacityMin;
		uniform float _Opacity;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			half2 appendResult349 = (half2(_ScrollSpeedX , _ScrollSpeedY));
			half2 appendResult348 = (half2(_TilingX , _TilingY));
			half2 appendResult347 = (half2(_OffsetX , _OffsetY));
			float2 uv_TexCoord26 = i.uv_texcoord * appendResult348 + appendResult347;
			half2 panner362 = ( 1.0 * _Time.y * ( appendResult349 * _RippleSpeedMulti ) + uv_TexCoord26);
			half3 Normals365 = UnpackScaleNormal( tex2D( _Ripples, panner362 ), _RippleAmount );
			o.Normal = Normals365;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			half4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth170 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			half distanceDepth170 = abs( ( screenDepth170 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depth ) );
			half temp_output_99_0 = pow( distanceDepth170 , _OverallFalloff );
			half temp_output_235_0 = ( temp_output_99_0 + _ShallowFalloff );
			half4 lerpResult115 = lerp( _ShallowColour , _DeepColour , temp_output_235_0);
			half4 lerpResult177 = lerp( _DeepColour , _VeryDeepColour , saturate( ( temp_output_99_0 - 1.0 ) ));
			half4 temp_output_175_0 = ( temp_output_235_0 < 1.0 ? lerpResult115 : lerpResult177 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			half4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			half2 temp_cast_0 = (_DistortionSpeed).xx;
			float3 ase_worldPos = i.worldPos;
			half2 appendResult19 = (half2(ase_worldPos.x , ase_worldPos.z));
			half2 panner45 = ( 1.0 * _Time.y * temp_cast_0 + ( appendResult19 * _DistortionTiling ));
			float4 screenColor100 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( (ase_grabScreenPosNorm).xy + (( UnpackNormal( tex2D( _DistortionMap, panner45 ) ) * _Distortion )).xy ));
			half4 Refraction107 = screenColor100;
			half4 lerpResult121 = lerp( temp_output_175_0 , Refraction107 , temp_output_175_0);
			half2 panner37 = ( 1.0 * _Time.y * appendResult349 + uv_TexCoord26);
			half4 tex2DNode39 = tex2D( _Foam_Texture, panner37 );
			half foam62 = saturate( ( ( ( 1.0 - saturate( pow( ( distanceDepth170 + _FoamShoreline ) , _FoamFalloff ) ) ) * ( ( tex2DNode39.r + 2.0 ) / 3.0 ) ) + tex2DNode39.r ) );
			half4 lerpResult141 = lerp( lerpResult121 , float4(1,1,1,0) , ( foam62 * _FoamOpacity ));
			half4 waterAlbedo155 = lerpResult141;
			o.Albedo = waterAlbedo155.rgb;
			float screenDepth333 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			half distanceDepth333 = abs( ( screenDepth333 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _GlowDepth ) );
			half4 lerpResult337 = lerp( _DepthGlowColour , float4( 0,0,0,0 ) , ( 1.0 - pow( distanceDepth333 , _GlowFalloff ) ));
			half4 lerpResult329 = lerp( _FoamEmitColour , float4( 0,0,0,0 ) , ( 1.0 - foam62 ));
			o.Emission = ( lerpResult337 + ( _FoamGlowMultiplier * lerpResult329 ) ).rgb;
			half lerpResult147 = lerp( _Specular , _FoamSpecular , foam62);
			half specular154 = lerpResult147;
			half3 temp_cast_3 = (specular154).xxx;
			o.Specular = temp_cast_3;
			half lerpResult132 = lerp( _Smoothness , _FoamSmoothness , foam62);
			half smoothness156 = ( lerpResult132 * _ReflectionPower );
			o.Smoothness = smoothness156;
			float screenDepth234 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			half distanceDepth234 = abs( ( screenDepth234 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 1.0 ) );
			half waterOpacity218 = ( ( (_OpacityMin + (saturate( ( distanceDepth234 / _OpacityFalloff ) ) - 0.0) * (1.0 - _OpacityMin) / (1.0 - 0.0)) * _Opacity ) + ( foam62 * _FoamOpacity ) );
			o.Alpha = waterOpacity218;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular alpha:fade keepalpha fullforwardshadows 

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
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float4 tSpace0 : TEXCOORD4;
				float4 tSpace1 : TEXCOORD5;
				float4 tSpace2 : TEXCOORD6;
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
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
-3429;13;2956;1357;5008.407;628.4459;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;8;-4169.968,407.3676;Inherit;False;2135.402;752.1004;Foam;24;62;352;350;55;351;356;47;353;38;39;24;23;37;17;349;26;348;341;342;347;344;345;346;343;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;11;-4624.176,-1370.152;Inherit;False;2526.922;883.8828;Refraction;15;107;100;87;72;69;68;66;53;52;45;33;32;21;19;16;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;65;-4224.18,-361.3934;Inherit;False;1231.201;672.2004;Water Depth;12;170;115;176;101;108;111;235;236;179;99;84;78;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;343;-4139.771,889.6037;Inherit;False;Property;_OffsetY;Offset Y;25;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;346;-4141.771,801.6037;Inherit;False;Property;_OffsetX;Offset X;24;0;Create;True;1;__;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;345;-4138.438,591.2704;Inherit;False;Property;_TilingX;Tiling X;22;0;Create;True;1;__;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;344;-4136.438,679.2704;Inherit;False;Property;_TilingY;Tiling Y;23;0;Create;True;0;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;16;-4511.827,-1207.606;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;78;-4162.374,17.06741;Float;False;Property;_Depth;Depth;11;0;Create;True;0;0;0;False;0;False;0.28;0.65;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;348;-3983.438,628.2704;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;342;-3943.771,1066.604;Inherit;False;Property;_ScrollSpeedY;Scroll Speed Y;27;0;Create;True;0;0;0;False;0;False;0.1;-0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;347;-3986.771,838.6037;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;341;-3935.771,981.6037;Inherit;False;Property;_ScrollSpeedX;Scroll Speed X;26;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;26;-3770.768,713.8698;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;349;-3748.771,1006.604;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-3777.567,469.8686;Float;False;Property;_FoamShoreline;Foam Shoreline;19;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;19;-4191.616,-1206.679;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-4212.039,-1031.505;Inherit;False;Property;_DistortionTiling;Distortion Tiling;13;0;Create;True;0;0;0;False;0;False;0.33;0.33;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;170;-3888.993,-13.53784;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-3775.566,554.4685;Float;False;Property;_FoamFalloff;Foam Falloff;20;0;Create;True;0;0;0;False;0;False;-56;26.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-3471.365,451.868;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-4161.707,-828.3553;Float;False;Property;_DistortionSpeed;Distortion Speed;15;0;Create;True;0;0;0;False;0;False;0.236;0.236;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-3994.568,-1128.975;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;37;-3504.566,717.1693;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.01,0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;45;-3777.764,-971.9562;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.04,0.04;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;39;-3298.166,682.169;Inherit;True;Property;_Foam_Texture;Foam_Texture;21;0;Create;True;0;0;0;False;0;False;-1;953aee88aa8b49347a6abd3be0f0eb54;d1ee92b23e8161c498a05646ff127bbf;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;38;-3322.466,451.9684;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;47;-3166.865,452.5684;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;52;-3523.266,-1047.126;Inherit;True;Property;_DistortionMap;Distortion Map;12;1;[Header];Create;True;1;Refraction;0;0;False;0;False;-1;edddba054057d7c49b0669dc00b76644;e4c83886e423af540b2767e694d93e33;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;353;-3070.634,556.1308;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-3566.029,-713.3764;Float;False;Property;_Distortion;Distortion;14;0;Create;True;0;0;0;False;0;False;0.292;0.292;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;351;-3001.852,458.5776;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-3153.62,-773.6414;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;356;-2946.634,551.1308;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-3925.55,114.8916;Float;False;Property;_OverallFalloff;OverallFalloff;10;0;Create;True;0;0;0;False;0;False;0.76;0.55;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;68;-3159.407,-1012.646;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;325;-2042.627,-1075.957;Inherit;False;1558;551;WaterOpacity;13;218;214;161;216;217;220;226;234;227;357;358;359;360;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ComponentMaskNode;69;-2903.727,-995.7393;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2813.865,461.9688;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;72;-2960.526,-775.8404;Inherit;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;99;-3612.844,2.55405;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;227;-1866.838,-828.756;Inherit;False;Property;_OpacityFalloff;Opacity Falloff;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;236;-3555.924,-82.34265;Inherit;False;Property;_ShallowFalloff;ShallowFalloff;9;0;Create;True;0;0;0;False;0;False;0.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;234;-1958.449,-957.7236;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;350;-2577.852,483.5776;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;87;-2675.321,-923.6382;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;179;-3355.729,200.4629;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;101;-3501.157,-266.0128;Float;False;Property;_DeepColour;Deep Colour;7;0;Create;True;0;0;0;False;0;False;0.04705882,0.3098039,0.1960784,0;0.1312871,0.574,0.5159892,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;235;-3313.924,-103.3427;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;111;-3151.482,174.123;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;176;-3406.729,7.462919;Float;False;Property;_VeryDeepColour;Very Deep Colour;8;0;Create;True;0;0;0;False;0;False;0.05959199,0.08247829,0.191,0;0.09000818,0.3001718,0.3867924,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;338;-1517.092,-444.7377;Inherit;False;1064.405;701.74;Glow;14;333;331;332;334;335;336;337;326;327;328;329;330;339;340;Glow;0,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;108;-3755.145,-321.7485;Float;False;Property;_ShallowColour;Shallow Colour;6;1;[Header];Create;True;1;Colour;0;0;False;0;False;0.9607843,0.7882353,0.5764706,0;0.3261999,0.8392157,0.1686273,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;226;-1663.838,-944.756;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;352;-2443.852,501.5776;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;100;-2521.128,-955.6672;Float;False;Global;_ScreenGrab0;Screen Grab 0;-1;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;217;-1593.694,-815.8881;Inherit;False;Property;_OpacityMin;Opacity Min;2;0;Create;True;0;0;0;False;0;False;0.5;0.478;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;220;-1456.693,-923.8881;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;117;-1706.364,749.5282;Inherit;False;1050;443;Smoothness;7;156;148;138;132;130;128;126;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;115;-3216.13,-318.6596;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;62;-2245.634,473.3721;Inherit;False;foam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;364;-4020.625,1317.264;Inherit;False;Property;_RippleSpeedMulti;RippleSpeedMulti;30;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;102;-2739.099,-401.6354;Inherit;False;1148.59;655.2657;WaterAlbedoLayering;6;155;141;125;123;121;113;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;107;-2331.05,-925.6373;Inherit;False;Refraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;177;-2976.729,-9.537081;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;331;-1467.092,-241.7378;Inherit;False;Property;_GlowDepth;Glow Depth;32;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;175;-2901.729,-270.5371;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;363;-3657.625,1292.264;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;357;-1315.747,-675.7733;Inherit;False;62;foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;-2705.278,-146.4805;Inherit;False;107;Refraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;359;-1503.994,-597.9307;Inherit;False;Property;_FoamOpacity;FoamOpacity;18;0;Create;True;0;0;0;False;0;False;1;0.091;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-1630.712,915.0829;Float;False;Property;_FoamSmoothness;Foam Smoothness;16;1;[Header];Create;True;1;Foam;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;216;-1268.693,-924.8881;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;124;-1700.364,288.5282;Inherit;False;956.0001;443;Specular;5;154;147;144;142;133;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DepthFade;333;-1291.092,-265.7377;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;161;-1588.275,-711.0491;Float;False;Property;_Opacity;Opacity;0;0;Create;True;0;0;0;False;0;False;1;0.684;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-1592.723,1081.916;Inherit;False;62;foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-2382.342,162.5208;Inherit;False;62;foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-1609.334,827.0519;Float;False;Property;_Smoothness;Smoothness;4;0;Create;True;0;0;0;False;0;False;1;0.404;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;332;-1269.092,-149.7378;Inherit;False;Property;_GlowFalloff;Glow Falloff;33;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;327;-1313.688,102.071;Inherit;False;62;foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;328;-1138.688,107.071;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;125;-2649.173,49.14542;Float;False;Constant;_Color0;Color 0;-1;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;362;-3493.625,1254.264;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.01,0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;138;-1330.087,1032.311;Float;False;Property;_ReflectionPower;Reflection Power;5;0;Create;True;0;0;0;False;0;False;0.346;0.319;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;367;-3638.524,1444.001;Inherit;False;Property;_RippleAmount;RippleAmount;29;0;Create;True;0;0;0;False;0;False;1;2.19;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;144;-1565.409,602.0158;Inherit;False;62;foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;368;-2158.073,63.94806;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;358;-1150.994,-642.9307;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;132;-1305.898,872.8879;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-1575.787,479.4994;Float;False;Property;_FoamSpecular;Foam Specular;17;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;334;-1009.092,-204.7378;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;133;-1595.371,384.1271;Float;False;Property;_Specular;Specular;3;0;Create;True;0;0;0;False;0;False;0.141;0.039;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;121;-2512.888,-281.4709;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;330;-1212.688,-72.92902;Inherit;False;Property;_FoamEmitColour;Foam Emit Colour;34;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;214;-1045.694,-842.888;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;360;-922.9937,-748.9307;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;329;-969.6872,25.071;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;361;-3287.625,1271.264;Inherit;True;Property;_Ripples;Ripples;28;1;[Header];Create;True;1;Normal Map;0;0;False;0;False;-1;None;bd6f803a7aa2a134693d786c0b77bdaa;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;147;-1149.148,554.225;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-1037.584,852.4713;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;340;-973.0979,-70.69592;Inherit;False;Property;_FoamGlowMultiplier;Foam Glow Multiplier;35;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;336;-994.0917,-394.7377;Inherit;False;Property;_DepthGlowColour;Depth Glow Colour;31;1;[Header];Create;True;1;Glow;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;335;-849.0917,-205.7378;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;141;-2235.325,-197.9272;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;365;-2864.128,1283.558;Inherit;False;Normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;155;-1804.758,-178.1924;Inherit;False;waterAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;337;-675.0917,-322.7377;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;156;-864.2643,863.967;Inherit;False;smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;339;-749.0979,-2.695923;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;154;-976.4514,565.5849;Inherit;False;specular;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;218;-734.6947,-827.8881;Inherit;False;waterOpacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;219;-357.1001,344.6932;Inherit;False;218;waterOpacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;366;-359.9341,19.65555;Inherit;False;365;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;162;-374.9481,249.2891;Inherit;False;156;smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;165;-397.5821,-91.17996;Inherit;False;155;waterAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;326;-589.0692,52.32371;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;-367.9481,164.289;Inherit;False;154;specular;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Half;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;SyntyStudios/WaterScrolling;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;348;0;345;0
WireConnection;348;1;344;0
WireConnection;347;0;346;0
WireConnection;347;1;343;0
WireConnection;26;0;348;0
WireConnection;26;1;347;0
WireConnection;349;0;341;0
WireConnection;349;1;342;0
WireConnection;19;0;16;1
WireConnection;19;1;16;3
WireConnection;170;0;78;0
WireConnection;23;0;170;0
WireConnection;23;1;17;0
WireConnection;32;0;19;0
WireConnection;32;1;21;0
WireConnection;37;0;26;0
WireConnection;37;2;349;0
WireConnection;45;0;32;0
WireConnection;45;2;33;0
WireConnection;39;1;37;0
WireConnection;38;0;23;0
WireConnection;38;1;24;0
WireConnection;47;0;38;0
WireConnection;52;1;45;0
WireConnection;353;0;39;1
WireConnection;351;0;47;0
WireConnection;66;0;52;0
WireConnection;66;1;53;0
WireConnection;356;0;353;0
WireConnection;69;0;68;0
WireConnection;55;0;351;0
WireConnection;55;1;356;0
WireConnection;72;0;66;0
WireConnection;99;0;170;0
WireConnection;99;1;84;0
WireConnection;350;0;55;0
WireConnection;350;1;39;1
WireConnection;87;0;69;0
WireConnection;87;1;72;0
WireConnection;179;0;99;0
WireConnection;235;0;99;0
WireConnection;235;1;236;0
WireConnection;111;0;179;0
WireConnection;226;0;234;0
WireConnection;226;1;227;0
WireConnection;352;0;350;0
WireConnection;100;0;87;0
WireConnection;220;0;226;0
WireConnection;115;0;108;0
WireConnection;115;1;101;0
WireConnection;115;2;235;0
WireConnection;62;0;352;0
WireConnection;107;0;100;0
WireConnection;177;0;101;0
WireConnection;177;1;176;0
WireConnection;177;2;111;0
WireConnection;175;0;235;0
WireConnection;175;2;115;0
WireConnection;175;3;177;0
WireConnection;363;0;349;0
WireConnection;363;1;364;0
WireConnection;216;0;220;0
WireConnection;216;3;217;0
WireConnection;333;0;331;0
WireConnection;328;0;327;0
WireConnection;362;0;26;0
WireConnection;362;2;363;0
WireConnection;368;0;123;0
WireConnection;368;1;359;0
WireConnection;358;0;357;0
WireConnection;358;1;359;0
WireConnection;132;0;130;0
WireConnection;132;1;128;0
WireConnection;132;2;126;0
WireConnection;334;0;333;0
WireConnection;334;1;332;0
WireConnection;121;0;175;0
WireConnection;121;1;113;0
WireConnection;121;2;175;0
WireConnection;214;0;216;0
WireConnection;214;1;161;0
WireConnection;360;0;214;0
WireConnection;360;1;358;0
WireConnection;329;0;330;0
WireConnection;329;2;328;0
WireConnection;361;1;362;0
WireConnection;361;5;367;0
WireConnection;147;0;133;0
WireConnection;147;1;142;0
WireConnection;147;2;144;0
WireConnection;148;0;132;0
WireConnection;148;1;138;0
WireConnection;335;0;334;0
WireConnection;141;0;121;0
WireConnection;141;1;125;0
WireConnection;141;2;368;0
WireConnection;365;0;361;0
WireConnection;155;0;141;0
WireConnection;337;0;336;0
WireConnection;337;2;335;0
WireConnection;156;0;148;0
WireConnection;339;0;340;0
WireConnection;339;1;329;0
WireConnection;154;0;147;0
WireConnection;218;0;360;0
WireConnection;326;0;337;0
WireConnection;326;1;339;0
WireConnection;0;0;165;0
WireConnection;0;1;366;0
WireConnection;0;2;326;0
WireConnection;0;3;158;0
WireConnection;0;4;162;0
WireConnection;0;9;219;0
ASEEND*/
//CHKSM=EB7ECF1CECF1A8456DE103759500180E1E966CCD