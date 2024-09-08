// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Color4x4v2"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[Header(Albedo (A Gradient))]_Color1("Color 1", Color) = (1,0.1544118,0.1544118,0.291)
		_Color2("Color 2", Color) = (1,0.1544118,0.8017241,0.253)
		_Color3("Color 3", Color) = (0.2535501,0.1544118,1,0.541)
		_Color4("Color 4", Color) = (0.1544118,0.5451319,1,0.253)
		[Space(10)]_Color5("Color 5", Color) = (0.9533468,1,0.1544118,0.553)
		_Color6("Color 6", Color) = (0.2720588,0.1294625,0,0.097)
		_Color7("Color 7", Color) = (0.1544118,0.6151115,1,0.178)
		_Color8("Color 8", Color) = (0.4849697,0.5008695,0.5073529,0.078)
		[Space(10)]_Color9("Color 9", Color) = (0.3164301,0,0.7058823,0.134)
		_Color10("Color 10", Color) = (0.362069,0.4411765,0,0.759)
		_Color11("Color 11", Color) = (0.6691177,0.6691177,0.6691177,0.647)
		_Color12("Color 12", Color) = (0.5073529,0.1574544,0,0.128)
		[Space(10)]_Color13("Color 13", Color) = (1,0.5586207,0,0.272)
		_Color14("Color 14", Color) = (0,0.8025862,0.875,0.047)
		_Color15("Color 15", Color) = (1,0,0,0.391)
		_Color16("Color 16", Color) = (0.4080882,0.75,0.4811866,0.134)
		[Header(Metallic(R) Rough(G) Emmission(B))]_MRE1("MRE 1", Color) = (0,1,0,0)
		_MRE2("MRE 2", Color) = (0,1,0,0)
		_MRE3("MRE 3", Color) = (0,1,0,0)
		_MRE4("MRE 4", Color) = (0,1,0,0)
		[Space(10)]_MRE5("MRE 5", Color) = (0,1,0,0)
		_MRE6("MRE 6", Color) = (0,1,0,0)
		_MRE7("MRE 7", Color) = (0,1,0,0)
		_MRE8("MRE 8", Color) = (0,1,0,0)
		[Space(10)]_MRE9("MRE 9", Color) = (0,1,0,0)
		_MRE10("MRE 10", Color) = (0,1,0,0)
		_MRE11("MRE 11", Color) = (0,1,0,0)
		_MRE12("MRE 12", Color) = (0,1,0,0)
		[Space(10)]_MRE13("MRE 13", Color) = (0,1,0,0)
		_MRE14("MRE 14", Color) = (0,1,0,0)
		_MRE15("MRE 15", Color) = (0,1,0,0)
		_MRE16("MRE 16", Color) = (0,1,0,0)
		[Header(Emmision)]_EmissionPower1("Emission Power", Float) = 1
		[SingleLineTexture][Header(Gradient)]_Gradient("Gradient", 2D) = "white" {}
		_GradientIntensity("Gradient Intensity", Range( 0 , 1)) = 0.75
		_GradientColor("Gradient Color", Color) = (0,0,0,0)
		_GradientScale("Gradient Scale", Float) = 1
		_GradientOffset("Gradient Offset", Float) = 0
		_GradientPower("Gradient Power", Float) = 1

		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		
		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		Cull Off
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 3.0

		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero, One Zero
			ColorMask RGBA
			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999


			#pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON

			#pragma multi_compile _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ _LIGHT_LAYERS
			
			#pragma multi_compile _ _LIGHT_COOKIES
			#pragma multi_compile _ _CLUSTERED_RENDERING

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _Gradient;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord8.xy = v.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				#if defined(LIGHTMAP_ON)
				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
				o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if !defined(LIGHTMAP_ON)
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 texCoord255 = IN.ase_texcoord8.xy * float2( 1,4 ) + float2( 0,0 );
				float4 clampResult234 = clamp( ( ( tex2D( _Gradient, texCoord255 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 saferPower258 = abs( (clampResult234*_GradientScale + _GradientOffset) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g731 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g731 = 1.0;
				float temp_output_7_0_g731 = 4.0;
				float temp_output_9_0_g731 = 4.0;
				float temp_output_8_0_g731 = 4.0;
				float2 texCoord2_g727 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g727 = 2.0;
				float temp_output_7_0_g727 = 4.0;
				float temp_output_9_0_g727 = 4.0;
				float temp_output_8_0_g727 = 4.0;
				float2 texCoord2_g728 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g728 = 3.0;
				float temp_output_7_0_g728 = 4.0;
				float temp_output_9_0_g728 = 4.0;
				float temp_output_8_0_g728 = 4.0;
				float2 texCoord2_g730 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g730 = 4.0;
				float temp_output_7_0_g730 = 4.0;
				float temp_output_9_0_g730 = 4.0;
				float temp_output_8_0_g730 = 4.0;
				float2 texCoord2_g718 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g718 = 1.0;
				float temp_output_7_0_g718 = 4.0;
				float temp_output_9_0_g718 = 3.0;
				float temp_output_8_0_g718 = 4.0;
				float2 texCoord2_g721 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g721 = 2.0;
				float temp_output_7_0_g721 = 4.0;
				float temp_output_9_0_g721 = 3.0;
				float temp_output_8_0_g721 = 4.0;
				float2 texCoord2_g732 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g732 = 3.0;
				float temp_output_7_0_g732 = 4.0;
				float temp_output_9_0_g732 = 3.0;
				float temp_output_8_0_g732 = 4.0;
				float2 texCoord2_g726 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g726 = 4.0;
				float temp_output_7_0_g726 = 4.0;
				float temp_output_9_0_g726 = 3.0;
				float temp_output_8_0_g726 = 4.0;
				float2 texCoord2_g720 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g720 = 1.0;
				float temp_output_7_0_g720 = 4.0;
				float temp_output_9_0_g720 = 2.0;
				float temp_output_8_0_g720 = 4.0;
				float2 texCoord2_g724 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g724 = 2.0;
				float temp_output_7_0_g724 = 4.0;
				float temp_output_9_0_g724 = 2.0;
				float temp_output_8_0_g724 = 4.0;
				float2 texCoord2_g722 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g722 = 3.0;
				float temp_output_7_0_g722 = 4.0;
				float temp_output_9_0_g722 = 2.0;
				float temp_output_8_0_g722 = 4.0;
				float2 texCoord2_g719 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g719 = 4.0;
				float temp_output_7_0_g719 = 4.0;
				float temp_output_9_0_g719 = 2.0;
				float temp_output_8_0_g719 = 4.0;
				float2 texCoord2_g725 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g725 = 1.0;
				float temp_output_7_0_g725 = 4.0;
				float temp_output_9_0_g725 = 1.0;
				float temp_output_8_0_g725 = 4.0;
				float2 texCoord2_g723 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g723 = 2.0;
				float temp_output_7_0_g723 = 4.0;
				float temp_output_9_0_g723 = 1.0;
				float temp_output_8_0_g723 = 4.0;
				float2 texCoord2_g733 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g733 = 3.0;
				float temp_output_7_0_g733 = 4.0;
				float temp_output_9_0_g733 = 1.0;
				float temp_output_8_0_g733 = 4.0;
				float2 texCoord2_g729 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g729 = 4.0;
				float temp_output_7_0_g729 = 4.0;
				float temp_output_9_0_g729 = 1.0;
				float temp_output_8_0_g729 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g731.x , ( ( temp_output_3_0_g731 - 1.0 ) / temp_output_7_0_g731 ) ) ) * ( step( texCoord2_g731.x , ( temp_output_3_0_g731 / temp_output_7_0_g731 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g731.y , ( ( temp_output_9_0_g731 - 1.0 ) / temp_output_8_0_g731 ) ) ) * ( step( texCoord2_g731.y , ( temp_output_9_0_g731 / temp_output_8_0_g731 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g727.x , ( ( temp_output_3_0_g727 - 1.0 ) / temp_output_7_0_g727 ) ) ) * ( step( texCoord2_g727.x , ( temp_output_3_0_g727 / temp_output_7_0_g727 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g727.y , ( ( temp_output_9_0_g727 - 1.0 ) / temp_output_8_0_g727 ) ) ) * ( step( texCoord2_g727.y , ( temp_output_9_0_g727 / temp_output_8_0_g727 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g728.x , ( ( temp_output_3_0_g728 - 1.0 ) / temp_output_7_0_g728 ) ) ) * ( step( texCoord2_g728.x , ( temp_output_3_0_g728 / temp_output_7_0_g728 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g728.y , ( ( temp_output_9_0_g728 - 1.0 ) / temp_output_8_0_g728 ) ) ) * ( step( texCoord2_g728.y , ( temp_output_9_0_g728 / temp_output_8_0_g728 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g730.x , ( ( temp_output_3_0_g730 - 1.0 ) / temp_output_7_0_g730 ) ) ) * ( step( texCoord2_g730.x , ( temp_output_3_0_g730 / temp_output_7_0_g730 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g730.y , ( ( temp_output_9_0_g730 - 1.0 ) / temp_output_8_0_g730 ) ) ) * ( step( texCoord2_g730.y , ( temp_output_9_0_g730 / temp_output_8_0_g730 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g718.x , ( ( temp_output_3_0_g718 - 1.0 ) / temp_output_7_0_g718 ) ) ) * ( step( texCoord2_g718.x , ( temp_output_3_0_g718 / temp_output_7_0_g718 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g718.y , ( ( temp_output_9_0_g718 - 1.0 ) / temp_output_8_0_g718 ) ) ) * ( step( texCoord2_g718.y , ( temp_output_9_0_g718 / temp_output_8_0_g718 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g721.x , ( ( temp_output_3_0_g721 - 1.0 ) / temp_output_7_0_g721 ) ) ) * ( step( texCoord2_g721.x , ( temp_output_3_0_g721 / temp_output_7_0_g721 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g721.y , ( ( temp_output_9_0_g721 - 1.0 ) / temp_output_8_0_g721 ) ) ) * ( step( texCoord2_g721.y , ( temp_output_9_0_g721 / temp_output_8_0_g721 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g732.x , ( ( temp_output_3_0_g732 - 1.0 ) / temp_output_7_0_g732 ) ) ) * ( step( texCoord2_g732.x , ( temp_output_3_0_g732 / temp_output_7_0_g732 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g732.y , ( ( temp_output_9_0_g732 - 1.0 ) / temp_output_8_0_g732 ) ) ) * ( step( texCoord2_g732.y , ( temp_output_9_0_g732 / temp_output_8_0_g732 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g726.x , ( ( temp_output_3_0_g726 - 1.0 ) / temp_output_7_0_g726 ) ) ) * ( step( texCoord2_g726.x , ( temp_output_3_0_g726 / temp_output_7_0_g726 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g726.y , ( ( temp_output_9_0_g726 - 1.0 ) / temp_output_8_0_g726 ) ) ) * ( step( texCoord2_g726.y , ( temp_output_9_0_g726 / temp_output_8_0_g726 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g720.x , ( ( temp_output_3_0_g720 - 1.0 ) / temp_output_7_0_g720 ) ) ) * ( step( texCoord2_g720.x , ( temp_output_3_0_g720 / temp_output_7_0_g720 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g720.y , ( ( temp_output_9_0_g720 - 1.0 ) / temp_output_8_0_g720 ) ) ) * ( step( texCoord2_g720.y , ( temp_output_9_0_g720 / temp_output_8_0_g720 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g724.x , ( ( temp_output_3_0_g724 - 1.0 ) / temp_output_7_0_g724 ) ) ) * ( step( texCoord2_g724.x , ( temp_output_3_0_g724 / temp_output_7_0_g724 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g724.y , ( ( temp_output_9_0_g724 - 1.0 ) / temp_output_8_0_g724 ) ) ) * ( step( texCoord2_g724.y , ( temp_output_9_0_g724 / temp_output_8_0_g724 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( texCoord2_g722.x , ( ( temp_output_3_0_g722 - 1.0 ) / temp_output_7_0_g722 ) ) ) * ( step( texCoord2_g722.x , ( temp_output_3_0_g722 / temp_output_7_0_g722 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g722.y , ( ( temp_output_9_0_g722 - 1.0 ) / temp_output_8_0_g722 ) ) ) * ( step( texCoord2_g722.y , ( temp_output_9_0_g722 / temp_output_8_0_g722 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( texCoord2_g719.x , ( ( temp_output_3_0_g719 - 1.0 ) / temp_output_7_0_g719 ) ) ) * ( step( texCoord2_g719.x , ( temp_output_3_0_g719 / temp_output_7_0_g719 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g719.y , ( ( temp_output_9_0_g719 - 1.0 ) / temp_output_8_0_g719 ) ) ) * ( step( texCoord2_g719.y , ( temp_output_9_0_g719 / temp_output_8_0_g719 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( texCoord2_g725.x , ( ( temp_output_3_0_g725 - 1.0 ) / temp_output_7_0_g725 ) ) ) * ( step( texCoord2_g725.x , ( temp_output_3_0_g725 / temp_output_7_0_g725 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g725.y , ( ( temp_output_9_0_g725 - 1.0 ) / temp_output_8_0_g725 ) ) ) * ( step( texCoord2_g725.y , ( temp_output_9_0_g725 / temp_output_8_0_g725 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( texCoord2_g723.x , ( ( temp_output_3_0_g723 - 1.0 ) / temp_output_7_0_g723 ) ) ) * ( step( texCoord2_g723.x , ( temp_output_3_0_g723 / temp_output_7_0_g723 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g723.y , ( ( temp_output_9_0_g723 - 1.0 ) / temp_output_8_0_g723 ) ) ) * ( step( texCoord2_g723.y , ( temp_output_9_0_g723 / temp_output_8_0_g723 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( texCoord2_g733.x , ( ( temp_output_3_0_g733 - 1.0 ) / temp_output_7_0_g733 ) ) ) * ( step( texCoord2_g733.x , ( temp_output_3_0_g733 / temp_output_7_0_g733 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g733.y , ( ( temp_output_9_0_g733 - 1.0 ) / temp_output_8_0_g733 ) ) ) * ( step( texCoord2_g733.y , ( temp_output_9_0_g733 / temp_output_8_0_g733 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( texCoord2_g729.x , ( ( temp_output_3_0_g729 - 1.0 ) / temp_output_7_0_g729 ) ) ) * ( step( texCoord2_g729.x , ( temp_output_3_0_g729 / temp_output_7_0_g729 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g729.y , ( ( temp_output_9_0_g729 - 1.0 ) / temp_output_8_0_g729 ) ) ) * ( step( texCoord2_g729.y , ( temp_output_9_0_g729 / temp_output_8_0_g729 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult261 = clamp( ( pow( saferPower258 , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float2 texCoord2_g703 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g703 = 1.0;
				float temp_output_7_0_g703 = 4.0;
				float temp_output_9_0_g703 = 4.0;
				float temp_output_8_0_g703 = 4.0;
				float2 texCoord2_g715 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g715 = 2.0;
				float temp_output_7_0_g715 = 4.0;
				float temp_output_9_0_g715 = 4.0;
				float temp_output_8_0_g715 = 4.0;
				float2 texCoord2_g704 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g704 = 3.0;
				float temp_output_7_0_g704 = 4.0;
				float temp_output_9_0_g704 = 4.0;
				float temp_output_8_0_g704 = 4.0;
				float2 texCoord2_g706 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g706 = 4.0;
				float temp_output_7_0_g706 = 4.0;
				float temp_output_9_0_g706 = 4.0;
				float temp_output_8_0_g706 = 4.0;
				float2 texCoord2_g705 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g705 = 1.0;
				float temp_output_7_0_g705 = 4.0;
				float temp_output_9_0_g705 = 3.0;
				float temp_output_8_0_g705 = 4.0;
				float2 texCoord2_g708 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g708 = 2.0;
				float temp_output_7_0_g708 = 4.0;
				float temp_output_9_0_g708 = 3.0;
				float temp_output_8_0_g708 = 4.0;
				float2 texCoord2_g716 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g716 = 3.0;
				float temp_output_7_0_g716 = 4.0;
				float temp_output_9_0_g716 = 3.0;
				float temp_output_8_0_g716 = 4.0;
				float2 texCoord2_g709 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g709 = 4.0;
				float temp_output_7_0_g709 = 4.0;
				float temp_output_9_0_g709 = 3.0;
				float temp_output_8_0_g709 = 4.0;
				float2 texCoord2_g714 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g714 = 1.0;
				float temp_output_7_0_g714 = 4.0;
				float temp_output_9_0_g714 = 2.0;
				float temp_output_8_0_g714 = 4.0;
				float2 texCoord2_g707 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g707 = 2.0;
				float temp_output_7_0_g707 = 4.0;
				float temp_output_9_0_g707 = 2.0;
				float temp_output_8_0_g707 = 4.0;
				float2 texCoord2_g702 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g702 = 3.0;
				float temp_output_7_0_g702 = 4.0;
				float temp_output_9_0_g702 = 2.0;
				float temp_output_8_0_g702 = 4.0;
				float2 texCoord2_g717 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g717 = 4.0;
				float temp_output_7_0_g717 = 4.0;
				float temp_output_9_0_g717 = 2.0;
				float temp_output_8_0_g717 = 4.0;
				float2 texCoord2_g712 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g712 = 1.0;
				float temp_output_7_0_g712 = 4.0;
				float temp_output_9_0_g712 = 1.0;
				float temp_output_8_0_g712 = 4.0;
				float2 texCoord2_g710 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g710 = 2.0;
				float temp_output_7_0_g710 = 4.0;
				float temp_output_9_0_g710 = 1.0;
				float temp_output_8_0_g710 = 4.0;
				float2 texCoord2_g711 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g711 = 3.0;
				float temp_output_7_0_g711 = 4.0;
				float temp_output_9_0_g711 = 1.0;
				float temp_output_8_0_g711 = 4.0;
				float2 texCoord2_g713 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g713 = 4.0;
				float temp_output_7_0_g713 = 4.0;
				float temp_output_9_0_g713 = 1.0;
				float temp_output_8_0_g713 = 4.0;
				float4 temp_output_283_0 = ( ( ( _MRE1 * ( ( ( 1.0 - step( texCoord2_g703.x , ( ( temp_output_3_0_g703 - 1.0 ) / temp_output_7_0_g703 ) ) ) * ( step( texCoord2_g703.x , ( temp_output_3_0_g703 / temp_output_7_0_g703 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g703.y , ( ( temp_output_9_0_g703 - 1.0 ) / temp_output_8_0_g703 ) ) ) * ( step( texCoord2_g703.y , ( temp_output_9_0_g703 / temp_output_8_0_g703 ) ) * 1.0 ) ) ) ) + ( _MRE2 * ( ( ( 1.0 - step( texCoord2_g715.x , ( ( temp_output_3_0_g715 - 1.0 ) / temp_output_7_0_g715 ) ) ) * ( step( texCoord2_g715.x , ( temp_output_3_0_g715 / temp_output_7_0_g715 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g715.y , ( ( temp_output_9_0_g715 - 1.0 ) / temp_output_8_0_g715 ) ) ) * ( step( texCoord2_g715.y , ( temp_output_9_0_g715 / temp_output_8_0_g715 ) ) * 1.0 ) ) ) ) + ( _MRE3 * ( ( ( 1.0 - step( texCoord2_g704.x , ( ( temp_output_3_0_g704 - 1.0 ) / temp_output_7_0_g704 ) ) ) * ( step( texCoord2_g704.x , ( temp_output_3_0_g704 / temp_output_7_0_g704 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g704.y , ( ( temp_output_9_0_g704 - 1.0 ) / temp_output_8_0_g704 ) ) ) * ( step( texCoord2_g704.y , ( temp_output_9_0_g704 / temp_output_8_0_g704 ) ) * 1.0 ) ) ) ) + ( _MRE4 * ( ( ( 1.0 - step( texCoord2_g706.x , ( ( temp_output_3_0_g706 - 1.0 ) / temp_output_7_0_g706 ) ) ) * ( step( texCoord2_g706.x , ( temp_output_3_0_g706 / temp_output_7_0_g706 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g706.y , ( ( temp_output_9_0_g706 - 1.0 ) / temp_output_8_0_g706 ) ) ) * ( step( texCoord2_g706.y , ( temp_output_9_0_g706 / temp_output_8_0_g706 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE5 * ( ( ( 1.0 - step( texCoord2_g705.x , ( ( temp_output_3_0_g705 - 1.0 ) / temp_output_7_0_g705 ) ) ) * ( step( texCoord2_g705.x , ( temp_output_3_0_g705 / temp_output_7_0_g705 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g705.y , ( ( temp_output_9_0_g705 - 1.0 ) / temp_output_8_0_g705 ) ) ) * ( step( texCoord2_g705.y , ( temp_output_9_0_g705 / temp_output_8_0_g705 ) ) * 1.0 ) ) ) ) + ( _MRE6 * ( ( ( 1.0 - step( texCoord2_g708.x , ( ( temp_output_3_0_g708 - 1.0 ) / temp_output_7_0_g708 ) ) ) * ( step( texCoord2_g708.x , ( temp_output_3_0_g708 / temp_output_7_0_g708 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g708.y , ( ( temp_output_9_0_g708 - 1.0 ) / temp_output_8_0_g708 ) ) ) * ( step( texCoord2_g708.y , ( temp_output_9_0_g708 / temp_output_8_0_g708 ) ) * 1.0 ) ) ) ) + ( _MRE7 * ( ( ( 1.0 - step( texCoord2_g716.x , ( ( temp_output_3_0_g716 - 1.0 ) / temp_output_7_0_g716 ) ) ) * ( step( texCoord2_g716.x , ( temp_output_3_0_g716 / temp_output_7_0_g716 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g716.y , ( ( temp_output_9_0_g716 - 1.0 ) / temp_output_8_0_g716 ) ) ) * ( step( texCoord2_g716.y , ( temp_output_9_0_g716 / temp_output_8_0_g716 ) ) * 1.0 ) ) ) ) + ( _MRE8 * ( ( ( 1.0 - step( texCoord2_g709.x , ( ( temp_output_3_0_g709 - 1.0 ) / temp_output_7_0_g709 ) ) ) * ( step( texCoord2_g709.x , ( temp_output_3_0_g709 / temp_output_7_0_g709 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g709.y , ( ( temp_output_9_0_g709 - 1.0 ) / temp_output_8_0_g709 ) ) ) * ( step( texCoord2_g709.y , ( temp_output_9_0_g709 / temp_output_8_0_g709 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE9 * ( ( ( 1.0 - step( texCoord2_g714.x , ( ( temp_output_3_0_g714 - 1.0 ) / temp_output_7_0_g714 ) ) ) * ( step( texCoord2_g714.x , ( temp_output_3_0_g714 / temp_output_7_0_g714 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g714.y , ( ( temp_output_9_0_g714 - 1.0 ) / temp_output_8_0_g714 ) ) ) * ( step( texCoord2_g714.y , ( temp_output_9_0_g714 / temp_output_8_0_g714 ) ) * 1.0 ) ) ) ) + ( _MRE10 * ( ( ( 1.0 - step( texCoord2_g707.x , ( ( temp_output_3_0_g707 - 1.0 ) / temp_output_7_0_g707 ) ) ) * ( step( texCoord2_g707.x , ( temp_output_3_0_g707 / temp_output_7_0_g707 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g707.y , ( ( temp_output_9_0_g707 - 1.0 ) / temp_output_8_0_g707 ) ) ) * ( step( texCoord2_g707.y , ( temp_output_9_0_g707 / temp_output_8_0_g707 ) ) * 1.0 ) ) ) ) + ( _MRE11 * ( ( ( 1.0 - step( texCoord2_g702.x , ( ( temp_output_3_0_g702 - 1.0 ) / temp_output_7_0_g702 ) ) ) * ( step( texCoord2_g702.x , ( temp_output_3_0_g702 / temp_output_7_0_g702 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g702.y , ( ( temp_output_9_0_g702 - 1.0 ) / temp_output_8_0_g702 ) ) ) * ( step( texCoord2_g702.y , ( temp_output_9_0_g702 / temp_output_8_0_g702 ) ) * 1.0 ) ) ) ) + ( _MRE12 * ( ( ( 1.0 - step( texCoord2_g717.x , ( ( temp_output_3_0_g717 - 1.0 ) / temp_output_7_0_g717 ) ) ) * ( step( texCoord2_g717.x , ( temp_output_3_0_g717 / temp_output_7_0_g717 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g717.y , ( ( temp_output_9_0_g717 - 1.0 ) / temp_output_8_0_g717 ) ) ) * ( step( texCoord2_g717.y , ( temp_output_9_0_g717 / temp_output_8_0_g717 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE13 * ( ( ( 1.0 - step( texCoord2_g712.x , ( ( temp_output_3_0_g712 - 1.0 ) / temp_output_7_0_g712 ) ) ) * ( step( texCoord2_g712.x , ( temp_output_3_0_g712 / temp_output_7_0_g712 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g712.y , ( ( temp_output_9_0_g712 - 1.0 ) / temp_output_8_0_g712 ) ) ) * ( step( texCoord2_g712.y , ( temp_output_9_0_g712 / temp_output_8_0_g712 ) ) * 1.0 ) ) ) ) + ( _MRE14 * ( ( ( 1.0 - step( texCoord2_g710.x , ( ( temp_output_3_0_g710 - 1.0 ) / temp_output_7_0_g710 ) ) ) * ( step( texCoord2_g710.x , ( temp_output_3_0_g710 / temp_output_7_0_g710 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g710.y , ( ( temp_output_9_0_g710 - 1.0 ) / temp_output_8_0_g710 ) ) ) * ( step( texCoord2_g710.y , ( temp_output_9_0_g710 / temp_output_8_0_g710 ) ) * 1.0 ) ) ) ) + ( _MRE15 * ( ( ( 1.0 - step( texCoord2_g711.x , ( ( temp_output_3_0_g711 - 1.0 ) / temp_output_7_0_g711 ) ) ) * ( step( texCoord2_g711.x , ( temp_output_3_0_g711 / temp_output_7_0_g711 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g711.y , ( ( temp_output_9_0_g711 - 1.0 ) / temp_output_8_0_g711 ) ) ) * ( step( texCoord2_g711.y , ( temp_output_9_0_g711 / temp_output_8_0_g711 ) ) * 1.0 ) ) ) ) + ( _MRE16 * ( ( ( 1.0 - step( texCoord2_g713.x , ( ( temp_output_3_0_g713 - 1.0 ) / temp_output_7_0_g713 ) ) ) * ( step( texCoord2_g713.x , ( temp_output_3_0_g713 / temp_output_7_0_g713 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g713.y , ( ( temp_output_9_0_g713 - 1.0 ) / temp_output_8_0_g713 ) ) ) * ( step( texCoord2_g713.y , ( temp_output_9_0_g713 / temp_output_8_0_g713 ) ) * 1.0 ) ) ) ) ) );
				
				float3 Albedo = ( clampResult261 * temp_output_155_0 ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( temp_output_155_0 * ( _EmissionPower1 * (temp_output_283_0).b ) ).rgb;
				float3 Specular = 0.5;
				float Metallic = (temp_output_283_0).r;
				float Smoothness = ( 1.0 - (temp_output_283_0).g );
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif
				
				#ifdef _CLEARCOAT
				float CoatMask = 0;
				float CoatSmoothness = 0;
				#endif


				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = ShadowCoords;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif


				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
				inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
				#else
				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#endif

				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
					#endif

					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = Albedo;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;


				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#ifdef _DBUFFER
					ApplyDecalToSurfaceData(IN.clipPos, surfaceData, inputData);
				#endif

				half4 color = UniversalFragmentPBR( inputData, surfaceData);

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999

			
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);


			#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
			#else
				float3 lightDirectionWS = _LightDirection;
			#endif

				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
			
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#endif


				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				return 0;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHONLY
        
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}
		
		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999

			
			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature _ EDITOR_VISUALIZATION

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef EDITOR_VISUALIZATION
				float4 VizUV : TEXCOORD2;
				float4 LightCoord : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _Gradient;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord4.xy = v.texcoord0.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );

			#ifdef EDITOR_VISUALIZATION
				float2 VizUV = 0;
				float4 LightCoord = 0;
				UnityEditorVizData(v.vertex.xyz, v.texcoord0.xy, v.texcoord1.xy, v.texcoord2.xy, VizUV, LightCoord);
				o.VizUV = float4(VizUV, 0, 0);
				o.LightCoord = LightCoord;
			#endif

			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = o.clipPos;
				o.shadowCoord = GetShadowCoord( vertexInput );
			#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord0 = v.texcoord0;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord0 = patch[0].texcoord0 * bary.x + patch[1].texcoord0 * bary.y + patch[2].texcoord0 * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 texCoord255 = IN.ase_texcoord4.xy * float2( 1,4 ) + float2( 0,0 );
				float4 clampResult234 = clamp( ( ( tex2D( _Gradient, texCoord255 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 saferPower258 = abs( (clampResult234*_GradientScale + _GradientOffset) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g731 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g731 = 1.0;
				float temp_output_7_0_g731 = 4.0;
				float temp_output_9_0_g731 = 4.0;
				float temp_output_8_0_g731 = 4.0;
				float2 texCoord2_g727 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g727 = 2.0;
				float temp_output_7_0_g727 = 4.0;
				float temp_output_9_0_g727 = 4.0;
				float temp_output_8_0_g727 = 4.0;
				float2 texCoord2_g728 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g728 = 3.0;
				float temp_output_7_0_g728 = 4.0;
				float temp_output_9_0_g728 = 4.0;
				float temp_output_8_0_g728 = 4.0;
				float2 texCoord2_g730 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g730 = 4.0;
				float temp_output_7_0_g730 = 4.0;
				float temp_output_9_0_g730 = 4.0;
				float temp_output_8_0_g730 = 4.0;
				float2 texCoord2_g718 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g718 = 1.0;
				float temp_output_7_0_g718 = 4.0;
				float temp_output_9_0_g718 = 3.0;
				float temp_output_8_0_g718 = 4.0;
				float2 texCoord2_g721 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g721 = 2.0;
				float temp_output_7_0_g721 = 4.0;
				float temp_output_9_0_g721 = 3.0;
				float temp_output_8_0_g721 = 4.0;
				float2 texCoord2_g732 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g732 = 3.0;
				float temp_output_7_0_g732 = 4.0;
				float temp_output_9_0_g732 = 3.0;
				float temp_output_8_0_g732 = 4.0;
				float2 texCoord2_g726 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g726 = 4.0;
				float temp_output_7_0_g726 = 4.0;
				float temp_output_9_0_g726 = 3.0;
				float temp_output_8_0_g726 = 4.0;
				float2 texCoord2_g720 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g720 = 1.0;
				float temp_output_7_0_g720 = 4.0;
				float temp_output_9_0_g720 = 2.0;
				float temp_output_8_0_g720 = 4.0;
				float2 texCoord2_g724 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g724 = 2.0;
				float temp_output_7_0_g724 = 4.0;
				float temp_output_9_0_g724 = 2.0;
				float temp_output_8_0_g724 = 4.0;
				float2 texCoord2_g722 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g722 = 3.0;
				float temp_output_7_0_g722 = 4.0;
				float temp_output_9_0_g722 = 2.0;
				float temp_output_8_0_g722 = 4.0;
				float2 texCoord2_g719 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g719 = 4.0;
				float temp_output_7_0_g719 = 4.0;
				float temp_output_9_0_g719 = 2.0;
				float temp_output_8_0_g719 = 4.0;
				float2 texCoord2_g725 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g725 = 1.0;
				float temp_output_7_0_g725 = 4.0;
				float temp_output_9_0_g725 = 1.0;
				float temp_output_8_0_g725 = 4.0;
				float2 texCoord2_g723 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g723 = 2.0;
				float temp_output_7_0_g723 = 4.0;
				float temp_output_9_0_g723 = 1.0;
				float temp_output_8_0_g723 = 4.0;
				float2 texCoord2_g733 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g733 = 3.0;
				float temp_output_7_0_g733 = 4.0;
				float temp_output_9_0_g733 = 1.0;
				float temp_output_8_0_g733 = 4.0;
				float2 texCoord2_g729 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g729 = 4.0;
				float temp_output_7_0_g729 = 4.0;
				float temp_output_9_0_g729 = 1.0;
				float temp_output_8_0_g729 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g731.x , ( ( temp_output_3_0_g731 - 1.0 ) / temp_output_7_0_g731 ) ) ) * ( step( texCoord2_g731.x , ( temp_output_3_0_g731 / temp_output_7_0_g731 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g731.y , ( ( temp_output_9_0_g731 - 1.0 ) / temp_output_8_0_g731 ) ) ) * ( step( texCoord2_g731.y , ( temp_output_9_0_g731 / temp_output_8_0_g731 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g727.x , ( ( temp_output_3_0_g727 - 1.0 ) / temp_output_7_0_g727 ) ) ) * ( step( texCoord2_g727.x , ( temp_output_3_0_g727 / temp_output_7_0_g727 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g727.y , ( ( temp_output_9_0_g727 - 1.0 ) / temp_output_8_0_g727 ) ) ) * ( step( texCoord2_g727.y , ( temp_output_9_0_g727 / temp_output_8_0_g727 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g728.x , ( ( temp_output_3_0_g728 - 1.0 ) / temp_output_7_0_g728 ) ) ) * ( step( texCoord2_g728.x , ( temp_output_3_0_g728 / temp_output_7_0_g728 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g728.y , ( ( temp_output_9_0_g728 - 1.0 ) / temp_output_8_0_g728 ) ) ) * ( step( texCoord2_g728.y , ( temp_output_9_0_g728 / temp_output_8_0_g728 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g730.x , ( ( temp_output_3_0_g730 - 1.0 ) / temp_output_7_0_g730 ) ) ) * ( step( texCoord2_g730.x , ( temp_output_3_0_g730 / temp_output_7_0_g730 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g730.y , ( ( temp_output_9_0_g730 - 1.0 ) / temp_output_8_0_g730 ) ) ) * ( step( texCoord2_g730.y , ( temp_output_9_0_g730 / temp_output_8_0_g730 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g718.x , ( ( temp_output_3_0_g718 - 1.0 ) / temp_output_7_0_g718 ) ) ) * ( step( texCoord2_g718.x , ( temp_output_3_0_g718 / temp_output_7_0_g718 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g718.y , ( ( temp_output_9_0_g718 - 1.0 ) / temp_output_8_0_g718 ) ) ) * ( step( texCoord2_g718.y , ( temp_output_9_0_g718 / temp_output_8_0_g718 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g721.x , ( ( temp_output_3_0_g721 - 1.0 ) / temp_output_7_0_g721 ) ) ) * ( step( texCoord2_g721.x , ( temp_output_3_0_g721 / temp_output_7_0_g721 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g721.y , ( ( temp_output_9_0_g721 - 1.0 ) / temp_output_8_0_g721 ) ) ) * ( step( texCoord2_g721.y , ( temp_output_9_0_g721 / temp_output_8_0_g721 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g732.x , ( ( temp_output_3_0_g732 - 1.0 ) / temp_output_7_0_g732 ) ) ) * ( step( texCoord2_g732.x , ( temp_output_3_0_g732 / temp_output_7_0_g732 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g732.y , ( ( temp_output_9_0_g732 - 1.0 ) / temp_output_8_0_g732 ) ) ) * ( step( texCoord2_g732.y , ( temp_output_9_0_g732 / temp_output_8_0_g732 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g726.x , ( ( temp_output_3_0_g726 - 1.0 ) / temp_output_7_0_g726 ) ) ) * ( step( texCoord2_g726.x , ( temp_output_3_0_g726 / temp_output_7_0_g726 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g726.y , ( ( temp_output_9_0_g726 - 1.0 ) / temp_output_8_0_g726 ) ) ) * ( step( texCoord2_g726.y , ( temp_output_9_0_g726 / temp_output_8_0_g726 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g720.x , ( ( temp_output_3_0_g720 - 1.0 ) / temp_output_7_0_g720 ) ) ) * ( step( texCoord2_g720.x , ( temp_output_3_0_g720 / temp_output_7_0_g720 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g720.y , ( ( temp_output_9_0_g720 - 1.0 ) / temp_output_8_0_g720 ) ) ) * ( step( texCoord2_g720.y , ( temp_output_9_0_g720 / temp_output_8_0_g720 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g724.x , ( ( temp_output_3_0_g724 - 1.0 ) / temp_output_7_0_g724 ) ) ) * ( step( texCoord2_g724.x , ( temp_output_3_0_g724 / temp_output_7_0_g724 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g724.y , ( ( temp_output_9_0_g724 - 1.0 ) / temp_output_8_0_g724 ) ) ) * ( step( texCoord2_g724.y , ( temp_output_9_0_g724 / temp_output_8_0_g724 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( texCoord2_g722.x , ( ( temp_output_3_0_g722 - 1.0 ) / temp_output_7_0_g722 ) ) ) * ( step( texCoord2_g722.x , ( temp_output_3_0_g722 / temp_output_7_0_g722 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g722.y , ( ( temp_output_9_0_g722 - 1.0 ) / temp_output_8_0_g722 ) ) ) * ( step( texCoord2_g722.y , ( temp_output_9_0_g722 / temp_output_8_0_g722 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( texCoord2_g719.x , ( ( temp_output_3_0_g719 - 1.0 ) / temp_output_7_0_g719 ) ) ) * ( step( texCoord2_g719.x , ( temp_output_3_0_g719 / temp_output_7_0_g719 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g719.y , ( ( temp_output_9_0_g719 - 1.0 ) / temp_output_8_0_g719 ) ) ) * ( step( texCoord2_g719.y , ( temp_output_9_0_g719 / temp_output_8_0_g719 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( texCoord2_g725.x , ( ( temp_output_3_0_g725 - 1.0 ) / temp_output_7_0_g725 ) ) ) * ( step( texCoord2_g725.x , ( temp_output_3_0_g725 / temp_output_7_0_g725 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g725.y , ( ( temp_output_9_0_g725 - 1.0 ) / temp_output_8_0_g725 ) ) ) * ( step( texCoord2_g725.y , ( temp_output_9_0_g725 / temp_output_8_0_g725 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( texCoord2_g723.x , ( ( temp_output_3_0_g723 - 1.0 ) / temp_output_7_0_g723 ) ) ) * ( step( texCoord2_g723.x , ( temp_output_3_0_g723 / temp_output_7_0_g723 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g723.y , ( ( temp_output_9_0_g723 - 1.0 ) / temp_output_8_0_g723 ) ) ) * ( step( texCoord2_g723.y , ( temp_output_9_0_g723 / temp_output_8_0_g723 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( texCoord2_g733.x , ( ( temp_output_3_0_g733 - 1.0 ) / temp_output_7_0_g733 ) ) ) * ( step( texCoord2_g733.x , ( temp_output_3_0_g733 / temp_output_7_0_g733 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g733.y , ( ( temp_output_9_0_g733 - 1.0 ) / temp_output_8_0_g733 ) ) ) * ( step( texCoord2_g733.y , ( temp_output_9_0_g733 / temp_output_8_0_g733 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( texCoord2_g729.x , ( ( temp_output_3_0_g729 - 1.0 ) / temp_output_7_0_g729 ) ) ) * ( step( texCoord2_g729.x , ( temp_output_3_0_g729 / temp_output_7_0_g729 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g729.y , ( ( temp_output_9_0_g729 - 1.0 ) / temp_output_8_0_g729 ) ) ) * ( step( texCoord2_g729.y , ( temp_output_9_0_g729 / temp_output_8_0_g729 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult261 = clamp( ( pow( saferPower258 , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float2 texCoord2_g703 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g703 = 1.0;
				float temp_output_7_0_g703 = 4.0;
				float temp_output_9_0_g703 = 4.0;
				float temp_output_8_0_g703 = 4.0;
				float2 texCoord2_g715 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g715 = 2.0;
				float temp_output_7_0_g715 = 4.0;
				float temp_output_9_0_g715 = 4.0;
				float temp_output_8_0_g715 = 4.0;
				float2 texCoord2_g704 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g704 = 3.0;
				float temp_output_7_0_g704 = 4.0;
				float temp_output_9_0_g704 = 4.0;
				float temp_output_8_0_g704 = 4.0;
				float2 texCoord2_g706 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g706 = 4.0;
				float temp_output_7_0_g706 = 4.0;
				float temp_output_9_0_g706 = 4.0;
				float temp_output_8_0_g706 = 4.0;
				float2 texCoord2_g705 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g705 = 1.0;
				float temp_output_7_0_g705 = 4.0;
				float temp_output_9_0_g705 = 3.0;
				float temp_output_8_0_g705 = 4.0;
				float2 texCoord2_g708 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g708 = 2.0;
				float temp_output_7_0_g708 = 4.0;
				float temp_output_9_0_g708 = 3.0;
				float temp_output_8_0_g708 = 4.0;
				float2 texCoord2_g716 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g716 = 3.0;
				float temp_output_7_0_g716 = 4.0;
				float temp_output_9_0_g716 = 3.0;
				float temp_output_8_0_g716 = 4.0;
				float2 texCoord2_g709 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g709 = 4.0;
				float temp_output_7_0_g709 = 4.0;
				float temp_output_9_0_g709 = 3.0;
				float temp_output_8_0_g709 = 4.0;
				float2 texCoord2_g714 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g714 = 1.0;
				float temp_output_7_0_g714 = 4.0;
				float temp_output_9_0_g714 = 2.0;
				float temp_output_8_0_g714 = 4.0;
				float2 texCoord2_g707 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g707 = 2.0;
				float temp_output_7_0_g707 = 4.0;
				float temp_output_9_0_g707 = 2.0;
				float temp_output_8_0_g707 = 4.0;
				float2 texCoord2_g702 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g702 = 3.0;
				float temp_output_7_0_g702 = 4.0;
				float temp_output_9_0_g702 = 2.0;
				float temp_output_8_0_g702 = 4.0;
				float2 texCoord2_g717 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g717 = 4.0;
				float temp_output_7_0_g717 = 4.0;
				float temp_output_9_0_g717 = 2.0;
				float temp_output_8_0_g717 = 4.0;
				float2 texCoord2_g712 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g712 = 1.0;
				float temp_output_7_0_g712 = 4.0;
				float temp_output_9_0_g712 = 1.0;
				float temp_output_8_0_g712 = 4.0;
				float2 texCoord2_g710 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g710 = 2.0;
				float temp_output_7_0_g710 = 4.0;
				float temp_output_9_0_g710 = 1.0;
				float temp_output_8_0_g710 = 4.0;
				float2 texCoord2_g711 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g711 = 3.0;
				float temp_output_7_0_g711 = 4.0;
				float temp_output_9_0_g711 = 1.0;
				float temp_output_8_0_g711 = 4.0;
				float2 texCoord2_g713 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g713 = 4.0;
				float temp_output_7_0_g713 = 4.0;
				float temp_output_9_0_g713 = 1.0;
				float temp_output_8_0_g713 = 4.0;
				float4 temp_output_283_0 = ( ( ( _MRE1 * ( ( ( 1.0 - step( texCoord2_g703.x , ( ( temp_output_3_0_g703 - 1.0 ) / temp_output_7_0_g703 ) ) ) * ( step( texCoord2_g703.x , ( temp_output_3_0_g703 / temp_output_7_0_g703 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g703.y , ( ( temp_output_9_0_g703 - 1.0 ) / temp_output_8_0_g703 ) ) ) * ( step( texCoord2_g703.y , ( temp_output_9_0_g703 / temp_output_8_0_g703 ) ) * 1.0 ) ) ) ) + ( _MRE2 * ( ( ( 1.0 - step( texCoord2_g715.x , ( ( temp_output_3_0_g715 - 1.0 ) / temp_output_7_0_g715 ) ) ) * ( step( texCoord2_g715.x , ( temp_output_3_0_g715 / temp_output_7_0_g715 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g715.y , ( ( temp_output_9_0_g715 - 1.0 ) / temp_output_8_0_g715 ) ) ) * ( step( texCoord2_g715.y , ( temp_output_9_0_g715 / temp_output_8_0_g715 ) ) * 1.0 ) ) ) ) + ( _MRE3 * ( ( ( 1.0 - step( texCoord2_g704.x , ( ( temp_output_3_0_g704 - 1.0 ) / temp_output_7_0_g704 ) ) ) * ( step( texCoord2_g704.x , ( temp_output_3_0_g704 / temp_output_7_0_g704 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g704.y , ( ( temp_output_9_0_g704 - 1.0 ) / temp_output_8_0_g704 ) ) ) * ( step( texCoord2_g704.y , ( temp_output_9_0_g704 / temp_output_8_0_g704 ) ) * 1.0 ) ) ) ) + ( _MRE4 * ( ( ( 1.0 - step( texCoord2_g706.x , ( ( temp_output_3_0_g706 - 1.0 ) / temp_output_7_0_g706 ) ) ) * ( step( texCoord2_g706.x , ( temp_output_3_0_g706 / temp_output_7_0_g706 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g706.y , ( ( temp_output_9_0_g706 - 1.0 ) / temp_output_8_0_g706 ) ) ) * ( step( texCoord2_g706.y , ( temp_output_9_0_g706 / temp_output_8_0_g706 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE5 * ( ( ( 1.0 - step( texCoord2_g705.x , ( ( temp_output_3_0_g705 - 1.0 ) / temp_output_7_0_g705 ) ) ) * ( step( texCoord2_g705.x , ( temp_output_3_0_g705 / temp_output_7_0_g705 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g705.y , ( ( temp_output_9_0_g705 - 1.0 ) / temp_output_8_0_g705 ) ) ) * ( step( texCoord2_g705.y , ( temp_output_9_0_g705 / temp_output_8_0_g705 ) ) * 1.0 ) ) ) ) + ( _MRE6 * ( ( ( 1.0 - step( texCoord2_g708.x , ( ( temp_output_3_0_g708 - 1.0 ) / temp_output_7_0_g708 ) ) ) * ( step( texCoord2_g708.x , ( temp_output_3_0_g708 / temp_output_7_0_g708 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g708.y , ( ( temp_output_9_0_g708 - 1.0 ) / temp_output_8_0_g708 ) ) ) * ( step( texCoord2_g708.y , ( temp_output_9_0_g708 / temp_output_8_0_g708 ) ) * 1.0 ) ) ) ) + ( _MRE7 * ( ( ( 1.0 - step( texCoord2_g716.x , ( ( temp_output_3_0_g716 - 1.0 ) / temp_output_7_0_g716 ) ) ) * ( step( texCoord2_g716.x , ( temp_output_3_0_g716 / temp_output_7_0_g716 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g716.y , ( ( temp_output_9_0_g716 - 1.0 ) / temp_output_8_0_g716 ) ) ) * ( step( texCoord2_g716.y , ( temp_output_9_0_g716 / temp_output_8_0_g716 ) ) * 1.0 ) ) ) ) + ( _MRE8 * ( ( ( 1.0 - step( texCoord2_g709.x , ( ( temp_output_3_0_g709 - 1.0 ) / temp_output_7_0_g709 ) ) ) * ( step( texCoord2_g709.x , ( temp_output_3_0_g709 / temp_output_7_0_g709 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g709.y , ( ( temp_output_9_0_g709 - 1.0 ) / temp_output_8_0_g709 ) ) ) * ( step( texCoord2_g709.y , ( temp_output_9_0_g709 / temp_output_8_0_g709 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE9 * ( ( ( 1.0 - step( texCoord2_g714.x , ( ( temp_output_3_0_g714 - 1.0 ) / temp_output_7_0_g714 ) ) ) * ( step( texCoord2_g714.x , ( temp_output_3_0_g714 / temp_output_7_0_g714 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g714.y , ( ( temp_output_9_0_g714 - 1.0 ) / temp_output_8_0_g714 ) ) ) * ( step( texCoord2_g714.y , ( temp_output_9_0_g714 / temp_output_8_0_g714 ) ) * 1.0 ) ) ) ) + ( _MRE10 * ( ( ( 1.0 - step( texCoord2_g707.x , ( ( temp_output_3_0_g707 - 1.0 ) / temp_output_7_0_g707 ) ) ) * ( step( texCoord2_g707.x , ( temp_output_3_0_g707 / temp_output_7_0_g707 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g707.y , ( ( temp_output_9_0_g707 - 1.0 ) / temp_output_8_0_g707 ) ) ) * ( step( texCoord2_g707.y , ( temp_output_9_0_g707 / temp_output_8_0_g707 ) ) * 1.0 ) ) ) ) + ( _MRE11 * ( ( ( 1.0 - step( texCoord2_g702.x , ( ( temp_output_3_0_g702 - 1.0 ) / temp_output_7_0_g702 ) ) ) * ( step( texCoord2_g702.x , ( temp_output_3_0_g702 / temp_output_7_0_g702 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g702.y , ( ( temp_output_9_0_g702 - 1.0 ) / temp_output_8_0_g702 ) ) ) * ( step( texCoord2_g702.y , ( temp_output_9_0_g702 / temp_output_8_0_g702 ) ) * 1.0 ) ) ) ) + ( _MRE12 * ( ( ( 1.0 - step( texCoord2_g717.x , ( ( temp_output_3_0_g717 - 1.0 ) / temp_output_7_0_g717 ) ) ) * ( step( texCoord2_g717.x , ( temp_output_3_0_g717 / temp_output_7_0_g717 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g717.y , ( ( temp_output_9_0_g717 - 1.0 ) / temp_output_8_0_g717 ) ) ) * ( step( texCoord2_g717.y , ( temp_output_9_0_g717 / temp_output_8_0_g717 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE13 * ( ( ( 1.0 - step( texCoord2_g712.x , ( ( temp_output_3_0_g712 - 1.0 ) / temp_output_7_0_g712 ) ) ) * ( step( texCoord2_g712.x , ( temp_output_3_0_g712 / temp_output_7_0_g712 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g712.y , ( ( temp_output_9_0_g712 - 1.0 ) / temp_output_8_0_g712 ) ) ) * ( step( texCoord2_g712.y , ( temp_output_9_0_g712 / temp_output_8_0_g712 ) ) * 1.0 ) ) ) ) + ( _MRE14 * ( ( ( 1.0 - step( texCoord2_g710.x , ( ( temp_output_3_0_g710 - 1.0 ) / temp_output_7_0_g710 ) ) ) * ( step( texCoord2_g710.x , ( temp_output_3_0_g710 / temp_output_7_0_g710 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g710.y , ( ( temp_output_9_0_g710 - 1.0 ) / temp_output_8_0_g710 ) ) ) * ( step( texCoord2_g710.y , ( temp_output_9_0_g710 / temp_output_8_0_g710 ) ) * 1.0 ) ) ) ) + ( _MRE15 * ( ( ( 1.0 - step( texCoord2_g711.x , ( ( temp_output_3_0_g711 - 1.0 ) / temp_output_7_0_g711 ) ) ) * ( step( texCoord2_g711.x , ( temp_output_3_0_g711 / temp_output_7_0_g711 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g711.y , ( ( temp_output_9_0_g711 - 1.0 ) / temp_output_8_0_g711 ) ) ) * ( step( texCoord2_g711.y , ( temp_output_9_0_g711 / temp_output_8_0_g711 ) ) * 1.0 ) ) ) ) + ( _MRE16 * ( ( ( 1.0 - step( texCoord2_g713.x , ( ( temp_output_3_0_g713 - 1.0 ) / temp_output_7_0_g713 ) ) ) * ( step( texCoord2_g713.x , ( temp_output_3_0_g713 / temp_output_7_0_g713 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g713.y , ( ( temp_output_9_0_g713 - 1.0 ) / temp_output_8_0_g713 ) ) ) * ( step( texCoord2_g713.y , ( temp_output_9_0_g713 / temp_output_8_0_g713 ) ) * 1.0 ) ) ) ) ) );
				
				
				float3 Albedo = ( clampResult261 * temp_output_155_0 ).rgb;
				float3 Emission = ( temp_output_155_0 * ( _EmissionPower1 * (temp_output_283_0).b ) ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
			#ifdef EDITOR_VISUALIZATION
				metaInput.VizUV = IN.VizUV.xy;
				metaInput.LightCoord = IN.LightCoord;
			#endif
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero, One Zero
			ColorMask RGBA

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_2D
        
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _Gradient;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 texCoord255 = IN.ase_texcoord2.xy * float2( 1,4 ) + float2( 0,0 );
				float4 clampResult234 = clamp( ( ( tex2D( _Gradient, texCoord255 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 saferPower258 = abs( (clampResult234*_GradientScale + _GradientOffset) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g731 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g731 = 1.0;
				float temp_output_7_0_g731 = 4.0;
				float temp_output_9_0_g731 = 4.0;
				float temp_output_8_0_g731 = 4.0;
				float2 texCoord2_g727 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g727 = 2.0;
				float temp_output_7_0_g727 = 4.0;
				float temp_output_9_0_g727 = 4.0;
				float temp_output_8_0_g727 = 4.0;
				float2 texCoord2_g728 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g728 = 3.0;
				float temp_output_7_0_g728 = 4.0;
				float temp_output_9_0_g728 = 4.0;
				float temp_output_8_0_g728 = 4.0;
				float2 texCoord2_g730 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g730 = 4.0;
				float temp_output_7_0_g730 = 4.0;
				float temp_output_9_0_g730 = 4.0;
				float temp_output_8_0_g730 = 4.0;
				float2 texCoord2_g718 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g718 = 1.0;
				float temp_output_7_0_g718 = 4.0;
				float temp_output_9_0_g718 = 3.0;
				float temp_output_8_0_g718 = 4.0;
				float2 texCoord2_g721 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g721 = 2.0;
				float temp_output_7_0_g721 = 4.0;
				float temp_output_9_0_g721 = 3.0;
				float temp_output_8_0_g721 = 4.0;
				float2 texCoord2_g732 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g732 = 3.0;
				float temp_output_7_0_g732 = 4.0;
				float temp_output_9_0_g732 = 3.0;
				float temp_output_8_0_g732 = 4.0;
				float2 texCoord2_g726 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g726 = 4.0;
				float temp_output_7_0_g726 = 4.0;
				float temp_output_9_0_g726 = 3.0;
				float temp_output_8_0_g726 = 4.0;
				float2 texCoord2_g720 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g720 = 1.0;
				float temp_output_7_0_g720 = 4.0;
				float temp_output_9_0_g720 = 2.0;
				float temp_output_8_0_g720 = 4.0;
				float2 texCoord2_g724 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g724 = 2.0;
				float temp_output_7_0_g724 = 4.0;
				float temp_output_9_0_g724 = 2.0;
				float temp_output_8_0_g724 = 4.0;
				float2 texCoord2_g722 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g722 = 3.0;
				float temp_output_7_0_g722 = 4.0;
				float temp_output_9_0_g722 = 2.0;
				float temp_output_8_0_g722 = 4.0;
				float2 texCoord2_g719 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g719 = 4.0;
				float temp_output_7_0_g719 = 4.0;
				float temp_output_9_0_g719 = 2.0;
				float temp_output_8_0_g719 = 4.0;
				float2 texCoord2_g725 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g725 = 1.0;
				float temp_output_7_0_g725 = 4.0;
				float temp_output_9_0_g725 = 1.0;
				float temp_output_8_0_g725 = 4.0;
				float2 texCoord2_g723 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g723 = 2.0;
				float temp_output_7_0_g723 = 4.0;
				float temp_output_9_0_g723 = 1.0;
				float temp_output_8_0_g723 = 4.0;
				float2 texCoord2_g733 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g733 = 3.0;
				float temp_output_7_0_g733 = 4.0;
				float temp_output_9_0_g733 = 1.0;
				float temp_output_8_0_g733 = 4.0;
				float2 texCoord2_g729 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g729 = 4.0;
				float temp_output_7_0_g729 = 4.0;
				float temp_output_9_0_g729 = 1.0;
				float temp_output_8_0_g729 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g731.x , ( ( temp_output_3_0_g731 - 1.0 ) / temp_output_7_0_g731 ) ) ) * ( step( texCoord2_g731.x , ( temp_output_3_0_g731 / temp_output_7_0_g731 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g731.y , ( ( temp_output_9_0_g731 - 1.0 ) / temp_output_8_0_g731 ) ) ) * ( step( texCoord2_g731.y , ( temp_output_9_0_g731 / temp_output_8_0_g731 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g727.x , ( ( temp_output_3_0_g727 - 1.0 ) / temp_output_7_0_g727 ) ) ) * ( step( texCoord2_g727.x , ( temp_output_3_0_g727 / temp_output_7_0_g727 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g727.y , ( ( temp_output_9_0_g727 - 1.0 ) / temp_output_8_0_g727 ) ) ) * ( step( texCoord2_g727.y , ( temp_output_9_0_g727 / temp_output_8_0_g727 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g728.x , ( ( temp_output_3_0_g728 - 1.0 ) / temp_output_7_0_g728 ) ) ) * ( step( texCoord2_g728.x , ( temp_output_3_0_g728 / temp_output_7_0_g728 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g728.y , ( ( temp_output_9_0_g728 - 1.0 ) / temp_output_8_0_g728 ) ) ) * ( step( texCoord2_g728.y , ( temp_output_9_0_g728 / temp_output_8_0_g728 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g730.x , ( ( temp_output_3_0_g730 - 1.0 ) / temp_output_7_0_g730 ) ) ) * ( step( texCoord2_g730.x , ( temp_output_3_0_g730 / temp_output_7_0_g730 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g730.y , ( ( temp_output_9_0_g730 - 1.0 ) / temp_output_8_0_g730 ) ) ) * ( step( texCoord2_g730.y , ( temp_output_9_0_g730 / temp_output_8_0_g730 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g718.x , ( ( temp_output_3_0_g718 - 1.0 ) / temp_output_7_0_g718 ) ) ) * ( step( texCoord2_g718.x , ( temp_output_3_0_g718 / temp_output_7_0_g718 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g718.y , ( ( temp_output_9_0_g718 - 1.0 ) / temp_output_8_0_g718 ) ) ) * ( step( texCoord2_g718.y , ( temp_output_9_0_g718 / temp_output_8_0_g718 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g721.x , ( ( temp_output_3_0_g721 - 1.0 ) / temp_output_7_0_g721 ) ) ) * ( step( texCoord2_g721.x , ( temp_output_3_0_g721 / temp_output_7_0_g721 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g721.y , ( ( temp_output_9_0_g721 - 1.0 ) / temp_output_8_0_g721 ) ) ) * ( step( texCoord2_g721.y , ( temp_output_9_0_g721 / temp_output_8_0_g721 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g732.x , ( ( temp_output_3_0_g732 - 1.0 ) / temp_output_7_0_g732 ) ) ) * ( step( texCoord2_g732.x , ( temp_output_3_0_g732 / temp_output_7_0_g732 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g732.y , ( ( temp_output_9_0_g732 - 1.0 ) / temp_output_8_0_g732 ) ) ) * ( step( texCoord2_g732.y , ( temp_output_9_0_g732 / temp_output_8_0_g732 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g726.x , ( ( temp_output_3_0_g726 - 1.0 ) / temp_output_7_0_g726 ) ) ) * ( step( texCoord2_g726.x , ( temp_output_3_0_g726 / temp_output_7_0_g726 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g726.y , ( ( temp_output_9_0_g726 - 1.0 ) / temp_output_8_0_g726 ) ) ) * ( step( texCoord2_g726.y , ( temp_output_9_0_g726 / temp_output_8_0_g726 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g720.x , ( ( temp_output_3_0_g720 - 1.0 ) / temp_output_7_0_g720 ) ) ) * ( step( texCoord2_g720.x , ( temp_output_3_0_g720 / temp_output_7_0_g720 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g720.y , ( ( temp_output_9_0_g720 - 1.0 ) / temp_output_8_0_g720 ) ) ) * ( step( texCoord2_g720.y , ( temp_output_9_0_g720 / temp_output_8_0_g720 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g724.x , ( ( temp_output_3_0_g724 - 1.0 ) / temp_output_7_0_g724 ) ) ) * ( step( texCoord2_g724.x , ( temp_output_3_0_g724 / temp_output_7_0_g724 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g724.y , ( ( temp_output_9_0_g724 - 1.0 ) / temp_output_8_0_g724 ) ) ) * ( step( texCoord2_g724.y , ( temp_output_9_0_g724 / temp_output_8_0_g724 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( texCoord2_g722.x , ( ( temp_output_3_0_g722 - 1.0 ) / temp_output_7_0_g722 ) ) ) * ( step( texCoord2_g722.x , ( temp_output_3_0_g722 / temp_output_7_0_g722 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g722.y , ( ( temp_output_9_0_g722 - 1.0 ) / temp_output_8_0_g722 ) ) ) * ( step( texCoord2_g722.y , ( temp_output_9_0_g722 / temp_output_8_0_g722 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( texCoord2_g719.x , ( ( temp_output_3_0_g719 - 1.0 ) / temp_output_7_0_g719 ) ) ) * ( step( texCoord2_g719.x , ( temp_output_3_0_g719 / temp_output_7_0_g719 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g719.y , ( ( temp_output_9_0_g719 - 1.0 ) / temp_output_8_0_g719 ) ) ) * ( step( texCoord2_g719.y , ( temp_output_9_0_g719 / temp_output_8_0_g719 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( texCoord2_g725.x , ( ( temp_output_3_0_g725 - 1.0 ) / temp_output_7_0_g725 ) ) ) * ( step( texCoord2_g725.x , ( temp_output_3_0_g725 / temp_output_7_0_g725 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g725.y , ( ( temp_output_9_0_g725 - 1.0 ) / temp_output_8_0_g725 ) ) ) * ( step( texCoord2_g725.y , ( temp_output_9_0_g725 / temp_output_8_0_g725 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( texCoord2_g723.x , ( ( temp_output_3_0_g723 - 1.0 ) / temp_output_7_0_g723 ) ) ) * ( step( texCoord2_g723.x , ( temp_output_3_0_g723 / temp_output_7_0_g723 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g723.y , ( ( temp_output_9_0_g723 - 1.0 ) / temp_output_8_0_g723 ) ) ) * ( step( texCoord2_g723.y , ( temp_output_9_0_g723 / temp_output_8_0_g723 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( texCoord2_g733.x , ( ( temp_output_3_0_g733 - 1.0 ) / temp_output_7_0_g733 ) ) ) * ( step( texCoord2_g733.x , ( temp_output_3_0_g733 / temp_output_7_0_g733 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g733.y , ( ( temp_output_9_0_g733 - 1.0 ) / temp_output_8_0_g733 ) ) ) * ( step( texCoord2_g733.y , ( temp_output_9_0_g733 / temp_output_8_0_g733 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( texCoord2_g729.x , ( ( temp_output_3_0_g729 - 1.0 ) / temp_output_7_0_g729 ) ) ) * ( step( texCoord2_g729.x , ( temp_output_3_0_g729 / temp_output_7_0_g729 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g729.y , ( ( temp_output_9_0_g729 - 1.0 ) / temp_output_8_0_g729 ) ) ) * ( step( texCoord2_g729.y , ( temp_output_9_0_g729 / temp_output_8_0_g729 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult261 = clamp( ( pow( saferPower258 , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				
				float3 Albedo = ( clampResult261 * temp_output_155_0 ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
            ZTest LEqual
            ZWrite On

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float3 worldNormal : TEXCOORD2;
				float4 worldTangent : TEXCOORD3;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal( v.ase_normal );
				float4 tangentWS = float4(TransformObjectToWorldDir( v.ase_tangent.xyz), v.ase_tangent.w);
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.worldNormal = normalWS;
				o.worldTangent = tangentWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				
				float3 WorldNormal = IN.worldNormal;
				float4 WorldTangent = IN.worldTangent;

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				
				float3 Normal = float3(0, 0, 1);
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif
				
				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(WorldNormal);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					return half4(packedNormalWS, 0.0);
				#else
					
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float crossSign = (WorldTangent.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
							float3 bitangent = crossSign * cross(WorldNormal.xyz, WorldTangent.xyz);
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent.xyz, bitangent, WorldNormal.xyz));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = WorldNormal;
					#endif

					return half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }
			
			Blend One Zero, One Zero
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999

			
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			
			#pragma multi_compile _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile _ _REFLECTION_PROBE_BOX_PROJECTION

			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ _GBUFFER_NORMALS_OCT
			#pragma multi_compile _ _LIGHT_LAYERS
			#pragma multi_compile _ _RENDER_PASS_ENABLED

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"


			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _Gradient;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord8.xy = v.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				#if defined(DYNAMICLIGHTMAP_ON)
				o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			FragmentOutput frag ( VertexOutput IN 
								#ifdef ASE_DEPTH_WRITE_ON
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#else
					ShadowCoords = float4(0, 0, 0, 0);
				#endif


	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 texCoord255 = IN.ase_texcoord8.xy * float2( 1,4 ) + float2( 0,0 );
				float4 clampResult234 = clamp( ( ( tex2D( _Gradient, texCoord255 ) + _GradientColor ) + ( 1.0 - _GradientIntensity ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 saferPower258 = abs( (clampResult234*_GradientScale + _GradientOffset) );
				float4 temp_cast_0 = (_GradientPower).xxxx;
				float2 texCoord2_g731 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g731 = 1.0;
				float temp_output_7_0_g731 = 4.0;
				float temp_output_9_0_g731 = 4.0;
				float temp_output_8_0_g731 = 4.0;
				float2 texCoord2_g727 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g727 = 2.0;
				float temp_output_7_0_g727 = 4.0;
				float temp_output_9_0_g727 = 4.0;
				float temp_output_8_0_g727 = 4.0;
				float2 texCoord2_g728 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g728 = 3.0;
				float temp_output_7_0_g728 = 4.0;
				float temp_output_9_0_g728 = 4.0;
				float temp_output_8_0_g728 = 4.0;
				float2 texCoord2_g730 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g730 = 4.0;
				float temp_output_7_0_g730 = 4.0;
				float temp_output_9_0_g730 = 4.0;
				float temp_output_8_0_g730 = 4.0;
				float2 texCoord2_g718 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g718 = 1.0;
				float temp_output_7_0_g718 = 4.0;
				float temp_output_9_0_g718 = 3.0;
				float temp_output_8_0_g718 = 4.0;
				float2 texCoord2_g721 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g721 = 2.0;
				float temp_output_7_0_g721 = 4.0;
				float temp_output_9_0_g721 = 3.0;
				float temp_output_8_0_g721 = 4.0;
				float2 texCoord2_g732 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g732 = 3.0;
				float temp_output_7_0_g732 = 4.0;
				float temp_output_9_0_g732 = 3.0;
				float temp_output_8_0_g732 = 4.0;
				float2 texCoord2_g726 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g726 = 4.0;
				float temp_output_7_0_g726 = 4.0;
				float temp_output_9_0_g726 = 3.0;
				float temp_output_8_0_g726 = 4.0;
				float2 texCoord2_g720 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g720 = 1.0;
				float temp_output_7_0_g720 = 4.0;
				float temp_output_9_0_g720 = 2.0;
				float temp_output_8_0_g720 = 4.0;
				float2 texCoord2_g724 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g724 = 2.0;
				float temp_output_7_0_g724 = 4.0;
				float temp_output_9_0_g724 = 2.0;
				float temp_output_8_0_g724 = 4.0;
				float2 texCoord2_g722 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g722 = 3.0;
				float temp_output_7_0_g722 = 4.0;
				float temp_output_9_0_g722 = 2.0;
				float temp_output_8_0_g722 = 4.0;
				float2 texCoord2_g719 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g719 = 4.0;
				float temp_output_7_0_g719 = 4.0;
				float temp_output_9_0_g719 = 2.0;
				float temp_output_8_0_g719 = 4.0;
				float2 texCoord2_g725 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g725 = 1.0;
				float temp_output_7_0_g725 = 4.0;
				float temp_output_9_0_g725 = 1.0;
				float temp_output_8_0_g725 = 4.0;
				float2 texCoord2_g723 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g723 = 2.0;
				float temp_output_7_0_g723 = 4.0;
				float temp_output_9_0_g723 = 1.0;
				float temp_output_8_0_g723 = 4.0;
				float2 texCoord2_g733 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g733 = 3.0;
				float temp_output_7_0_g733 = 4.0;
				float temp_output_9_0_g733 = 1.0;
				float temp_output_8_0_g733 = 4.0;
				float2 texCoord2_g729 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g729 = 4.0;
				float temp_output_7_0_g729 = 4.0;
				float temp_output_9_0_g729 = 1.0;
				float temp_output_8_0_g729 = 4.0;
				float4 temp_output_155_0 = ( ( ( _Color1 * ( ( ( 1.0 - step( texCoord2_g731.x , ( ( temp_output_3_0_g731 - 1.0 ) / temp_output_7_0_g731 ) ) ) * ( step( texCoord2_g731.x , ( temp_output_3_0_g731 / temp_output_7_0_g731 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g731.y , ( ( temp_output_9_0_g731 - 1.0 ) / temp_output_8_0_g731 ) ) ) * ( step( texCoord2_g731.y , ( temp_output_9_0_g731 / temp_output_8_0_g731 ) ) * 1.0 ) ) ) ) + ( _Color2 * ( ( ( 1.0 - step( texCoord2_g727.x , ( ( temp_output_3_0_g727 - 1.0 ) / temp_output_7_0_g727 ) ) ) * ( step( texCoord2_g727.x , ( temp_output_3_0_g727 / temp_output_7_0_g727 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g727.y , ( ( temp_output_9_0_g727 - 1.0 ) / temp_output_8_0_g727 ) ) ) * ( step( texCoord2_g727.y , ( temp_output_9_0_g727 / temp_output_8_0_g727 ) ) * 1.0 ) ) ) ) + ( _Color3 * ( ( ( 1.0 - step( texCoord2_g728.x , ( ( temp_output_3_0_g728 - 1.0 ) / temp_output_7_0_g728 ) ) ) * ( step( texCoord2_g728.x , ( temp_output_3_0_g728 / temp_output_7_0_g728 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g728.y , ( ( temp_output_9_0_g728 - 1.0 ) / temp_output_8_0_g728 ) ) ) * ( step( texCoord2_g728.y , ( temp_output_9_0_g728 / temp_output_8_0_g728 ) ) * 1.0 ) ) ) ) + ( _Color4 * ( ( ( 1.0 - step( texCoord2_g730.x , ( ( temp_output_3_0_g730 - 1.0 ) / temp_output_7_0_g730 ) ) ) * ( step( texCoord2_g730.x , ( temp_output_3_0_g730 / temp_output_7_0_g730 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g730.y , ( ( temp_output_9_0_g730 - 1.0 ) / temp_output_8_0_g730 ) ) ) * ( step( texCoord2_g730.y , ( temp_output_9_0_g730 / temp_output_8_0_g730 ) ) * 1.0 ) ) ) ) ) + ( ( _Color5 * ( ( ( 1.0 - step( texCoord2_g718.x , ( ( temp_output_3_0_g718 - 1.0 ) / temp_output_7_0_g718 ) ) ) * ( step( texCoord2_g718.x , ( temp_output_3_0_g718 / temp_output_7_0_g718 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g718.y , ( ( temp_output_9_0_g718 - 1.0 ) / temp_output_8_0_g718 ) ) ) * ( step( texCoord2_g718.y , ( temp_output_9_0_g718 / temp_output_8_0_g718 ) ) * 1.0 ) ) ) ) + ( _Color6 * ( ( ( 1.0 - step( texCoord2_g721.x , ( ( temp_output_3_0_g721 - 1.0 ) / temp_output_7_0_g721 ) ) ) * ( step( texCoord2_g721.x , ( temp_output_3_0_g721 / temp_output_7_0_g721 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g721.y , ( ( temp_output_9_0_g721 - 1.0 ) / temp_output_8_0_g721 ) ) ) * ( step( texCoord2_g721.y , ( temp_output_9_0_g721 / temp_output_8_0_g721 ) ) * 1.0 ) ) ) ) + ( _Color7 * ( ( ( 1.0 - step( texCoord2_g732.x , ( ( temp_output_3_0_g732 - 1.0 ) / temp_output_7_0_g732 ) ) ) * ( step( texCoord2_g732.x , ( temp_output_3_0_g732 / temp_output_7_0_g732 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g732.y , ( ( temp_output_9_0_g732 - 1.0 ) / temp_output_8_0_g732 ) ) ) * ( step( texCoord2_g732.y , ( temp_output_9_0_g732 / temp_output_8_0_g732 ) ) * 1.0 ) ) ) ) + ( _Color8 * ( ( ( 1.0 - step( texCoord2_g726.x , ( ( temp_output_3_0_g726 - 1.0 ) / temp_output_7_0_g726 ) ) ) * ( step( texCoord2_g726.x , ( temp_output_3_0_g726 / temp_output_7_0_g726 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g726.y , ( ( temp_output_9_0_g726 - 1.0 ) / temp_output_8_0_g726 ) ) ) * ( step( texCoord2_g726.y , ( temp_output_9_0_g726 / temp_output_8_0_g726 ) ) * 1.0 ) ) ) ) ) + ( ( _Color9 * ( ( ( 1.0 - step( texCoord2_g720.x , ( ( temp_output_3_0_g720 - 1.0 ) / temp_output_7_0_g720 ) ) ) * ( step( texCoord2_g720.x , ( temp_output_3_0_g720 / temp_output_7_0_g720 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g720.y , ( ( temp_output_9_0_g720 - 1.0 ) / temp_output_8_0_g720 ) ) ) * ( step( texCoord2_g720.y , ( temp_output_9_0_g720 / temp_output_8_0_g720 ) ) * 1.0 ) ) ) ) + ( _Color10 * ( ( ( 1.0 - step( texCoord2_g724.x , ( ( temp_output_3_0_g724 - 1.0 ) / temp_output_7_0_g724 ) ) ) * ( step( texCoord2_g724.x , ( temp_output_3_0_g724 / temp_output_7_0_g724 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g724.y , ( ( temp_output_9_0_g724 - 1.0 ) / temp_output_8_0_g724 ) ) ) * ( step( texCoord2_g724.y , ( temp_output_9_0_g724 / temp_output_8_0_g724 ) ) * 1.0 ) ) ) ) + ( _Color11 * ( ( ( 1.0 - step( texCoord2_g722.x , ( ( temp_output_3_0_g722 - 1.0 ) / temp_output_7_0_g722 ) ) ) * ( step( texCoord2_g722.x , ( temp_output_3_0_g722 / temp_output_7_0_g722 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g722.y , ( ( temp_output_9_0_g722 - 1.0 ) / temp_output_8_0_g722 ) ) ) * ( step( texCoord2_g722.y , ( temp_output_9_0_g722 / temp_output_8_0_g722 ) ) * 1.0 ) ) ) ) + ( _Color12 * ( ( ( 1.0 - step( texCoord2_g719.x , ( ( temp_output_3_0_g719 - 1.0 ) / temp_output_7_0_g719 ) ) ) * ( step( texCoord2_g719.x , ( temp_output_3_0_g719 / temp_output_7_0_g719 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g719.y , ( ( temp_output_9_0_g719 - 1.0 ) / temp_output_8_0_g719 ) ) ) * ( step( texCoord2_g719.y , ( temp_output_9_0_g719 / temp_output_8_0_g719 ) ) * 1.0 ) ) ) ) ) + ( ( _Color13 * ( ( ( 1.0 - step( texCoord2_g725.x , ( ( temp_output_3_0_g725 - 1.0 ) / temp_output_7_0_g725 ) ) ) * ( step( texCoord2_g725.x , ( temp_output_3_0_g725 / temp_output_7_0_g725 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g725.y , ( ( temp_output_9_0_g725 - 1.0 ) / temp_output_8_0_g725 ) ) ) * ( step( texCoord2_g725.y , ( temp_output_9_0_g725 / temp_output_8_0_g725 ) ) * 1.0 ) ) ) ) + ( _Color14 * ( ( ( 1.0 - step( texCoord2_g723.x , ( ( temp_output_3_0_g723 - 1.0 ) / temp_output_7_0_g723 ) ) ) * ( step( texCoord2_g723.x , ( temp_output_3_0_g723 / temp_output_7_0_g723 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g723.y , ( ( temp_output_9_0_g723 - 1.0 ) / temp_output_8_0_g723 ) ) ) * ( step( texCoord2_g723.y , ( temp_output_9_0_g723 / temp_output_8_0_g723 ) ) * 1.0 ) ) ) ) + ( _Color15 * ( ( ( 1.0 - step( texCoord2_g733.x , ( ( temp_output_3_0_g733 - 1.0 ) / temp_output_7_0_g733 ) ) ) * ( step( texCoord2_g733.x , ( temp_output_3_0_g733 / temp_output_7_0_g733 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g733.y , ( ( temp_output_9_0_g733 - 1.0 ) / temp_output_8_0_g733 ) ) ) * ( step( texCoord2_g733.y , ( temp_output_9_0_g733 / temp_output_8_0_g733 ) ) * 1.0 ) ) ) ) + ( _Color16 * ( ( ( 1.0 - step( texCoord2_g729.x , ( ( temp_output_3_0_g729 - 1.0 ) / temp_output_7_0_g729 ) ) ) * ( step( texCoord2_g729.x , ( temp_output_3_0_g729 / temp_output_7_0_g729 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g729.y , ( ( temp_output_9_0_g729 - 1.0 ) / temp_output_8_0_g729 ) ) ) * ( step( texCoord2_g729.y , ( temp_output_9_0_g729 / temp_output_8_0_g729 ) ) * 1.0 ) ) ) ) ) );
				float4 clampResult261 = clamp( ( pow( saferPower258 , temp_cast_0 ) + ( 1.0 - (temp_output_155_0).a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				float2 texCoord2_g703 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g703 = 1.0;
				float temp_output_7_0_g703 = 4.0;
				float temp_output_9_0_g703 = 4.0;
				float temp_output_8_0_g703 = 4.0;
				float2 texCoord2_g715 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g715 = 2.0;
				float temp_output_7_0_g715 = 4.0;
				float temp_output_9_0_g715 = 4.0;
				float temp_output_8_0_g715 = 4.0;
				float2 texCoord2_g704 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g704 = 3.0;
				float temp_output_7_0_g704 = 4.0;
				float temp_output_9_0_g704 = 4.0;
				float temp_output_8_0_g704 = 4.0;
				float2 texCoord2_g706 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g706 = 4.0;
				float temp_output_7_0_g706 = 4.0;
				float temp_output_9_0_g706 = 4.0;
				float temp_output_8_0_g706 = 4.0;
				float2 texCoord2_g705 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g705 = 1.0;
				float temp_output_7_0_g705 = 4.0;
				float temp_output_9_0_g705 = 3.0;
				float temp_output_8_0_g705 = 4.0;
				float2 texCoord2_g708 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g708 = 2.0;
				float temp_output_7_0_g708 = 4.0;
				float temp_output_9_0_g708 = 3.0;
				float temp_output_8_0_g708 = 4.0;
				float2 texCoord2_g716 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g716 = 3.0;
				float temp_output_7_0_g716 = 4.0;
				float temp_output_9_0_g716 = 3.0;
				float temp_output_8_0_g716 = 4.0;
				float2 texCoord2_g709 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g709 = 4.0;
				float temp_output_7_0_g709 = 4.0;
				float temp_output_9_0_g709 = 3.0;
				float temp_output_8_0_g709 = 4.0;
				float2 texCoord2_g714 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g714 = 1.0;
				float temp_output_7_0_g714 = 4.0;
				float temp_output_9_0_g714 = 2.0;
				float temp_output_8_0_g714 = 4.0;
				float2 texCoord2_g707 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g707 = 2.0;
				float temp_output_7_0_g707 = 4.0;
				float temp_output_9_0_g707 = 2.0;
				float temp_output_8_0_g707 = 4.0;
				float2 texCoord2_g702 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g702 = 3.0;
				float temp_output_7_0_g702 = 4.0;
				float temp_output_9_0_g702 = 2.0;
				float temp_output_8_0_g702 = 4.0;
				float2 texCoord2_g717 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g717 = 4.0;
				float temp_output_7_0_g717 = 4.0;
				float temp_output_9_0_g717 = 2.0;
				float temp_output_8_0_g717 = 4.0;
				float2 texCoord2_g712 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g712 = 1.0;
				float temp_output_7_0_g712 = 4.0;
				float temp_output_9_0_g712 = 1.0;
				float temp_output_8_0_g712 = 4.0;
				float2 texCoord2_g710 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g710 = 2.0;
				float temp_output_7_0_g710 = 4.0;
				float temp_output_9_0_g710 = 1.0;
				float temp_output_8_0_g710 = 4.0;
				float2 texCoord2_g711 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g711 = 3.0;
				float temp_output_7_0_g711 = 4.0;
				float temp_output_9_0_g711 = 1.0;
				float temp_output_8_0_g711 = 4.0;
				float2 texCoord2_g713 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g713 = 4.0;
				float temp_output_7_0_g713 = 4.0;
				float temp_output_9_0_g713 = 1.0;
				float temp_output_8_0_g713 = 4.0;
				float4 temp_output_283_0 = ( ( ( _MRE1 * ( ( ( 1.0 - step( texCoord2_g703.x , ( ( temp_output_3_0_g703 - 1.0 ) / temp_output_7_0_g703 ) ) ) * ( step( texCoord2_g703.x , ( temp_output_3_0_g703 / temp_output_7_0_g703 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g703.y , ( ( temp_output_9_0_g703 - 1.0 ) / temp_output_8_0_g703 ) ) ) * ( step( texCoord2_g703.y , ( temp_output_9_0_g703 / temp_output_8_0_g703 ) ) * 1.0 ) ) ) ) + ( _MRE2 * ( ( ( 1.0 - step( texCoord2_g715.x , ( ( temp_output_3_0_g715 - 1.0 ) / temp_output_7_0_g715 ) ) ) * ( step( texCoord2_g715.x , ( temp_output_3_0_g715 / temp_output_7_0_g715 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g715.y , ( ( temp_output_9_0_g715 - 1.0 ) / temp_output_8_0_g715 ) ) ) * ( step( texCoord2_g715.y , ( temp_output_9_0_g715 / temp_output_8_0_g715 ) ) * 1.0 ) ) ) ) + ( _MRE3 * ( ( ( 1.0 - step( texCoord2_g704.x , ( ( temp_output_3_0_g704 - 1.0 ) / temp_output_7_0_g704 ) ) ) * ( step( texCoord2_g704.x , ( temp_output_3_0_g704 / temp_output_7_0_g704 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g704.y , ( ( temp_output_9_0_g704 - 1.0 ) / temp_output_8_0_g704 ) ) ) * ( step( texCoord2_g704.y , ( temp_output_9_0_g704 / temp_output_8_0_g704 ) ) * 1.0 ) ) ) ) + ( _MRE4 * ( ( ( 1.0 - step( texCoord2_g706.x , ( ( temp_output_3_0_g706 - 1.0 ) / temp_output_7_0_g706 ) ) ) * ( step( texCoord2_g706.x , ( temp_output_3_0_g706 / temp_output_7_0_g706 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g706.y , ( ( temp_output_9_0_g706 - 1.0 ) / temp_output_8_0_g706 ) ) ) * ( step( texCoord2_g706.y , ( temp_output_9_0_g706 / temp_output_8_0_g706 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE5 * ( ( ( 1.0 - step( texCoord2_g705.x , ( ( temp_output_3_0_g705 - 1.0 ) / temp_output_7_0_g705 ) ) ) * ( step( texCoord2_g705.x , ( temp_output_3_0_g705 / temp_output_7_0_g705 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g705.y , ( ( temp_output_9_0_g705 - 1.0 ) / temp_output_8_0_g705 ) ) ) * ( step( texCoord2_g705.y , ( temp_output_9_0_g705 / temp_output_8_0_g705 ) ) * 1.0 ) ) ) ) + ( _MRE6 * ( ( ( 1.0 - step( texCoord2_g708.x , ( ( temp_output_3_0_g708 - 1.0 ) / temp_output_7_0_g708 ) ) ) * ( step( texCoord2_g708.x , ( temp_output_3_0_g708 / temp_output_7_0_g708 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g708.y , ( ( temp_output_9_0_g708 - 1.0 ) / temp_output_8_0_g708 ) ) ) * ( step( texCoord2_g708.y , ( temp_output_9_0_g708 / temp_output_8_0_g708 ) ) * 1.0 ) ) ) ) + ( _MRE7 * ( ( ( 1.0 - step( texCoord2_g716.x , ( ( temp_output_3_0_g716 - 1.0 ) / temp_output_7_0_g716 ) ) ) * ( step( texCoord2_g716.x , ( temp_output_3_0_g716 / temp_output_7_0_g716 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g716.y , ( ( temp_output_9_0_g716 - 1.0 ) / temp_output_8_0_g716 ) ) ) * ( step( texCoord2_g716.y , ( temp_output_9_0_g716 / temp_output_8_0_g716 ) ) * 1.0 ) ) ) ) + ( _MRE8 * ( ( ( 1.0 - step( texCoord2_g709.x , ( ( temp_output_3_0_g709 - 1.0 ) / temp_output_7_0_g709 ) ) ) * ( step( texCoord2_g709.x , ( temp_output_3_0_g709 / temp_output_7_0_g709 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g709.y , ( ( temp_output_9_0_g709 - 1.0 ) / temp_output_8_0_g709 ) ) ) * ( step( texCoord2_g709.y , ( temp_output_9_0_g709 / temp_output_8_0_g709 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE9 * ( ( ( 1.0 - step( texCoord2_g714.x , ( ( temp_output_3_0_g714 - 1.0 ) / temp_output_7_0_g714 ) ) ) * ( step( texCoord2_g714.x , ( temp_output_3_0_g714 / temp_output_7_0_g714 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g714.y , ( ( temp_output_9_0_g714 - 1.0 ) / temp_output_8_0_g714 ) ) ) * ( step( texCoord2_g714.y , ( temp_output_9_0_g714 / temp_output_8_0_g714 ) ) * 1.0 ) ) ) ) + ( _MRE10 * ( ( ( 1.0 - step( texCoord2_g707.x , ( ( temp_output_3_0_g707 - 1.0 ) / temp_output_7_0_g707 ) ) ) * ( step( texCoord2_g707.x , ( temp_output_3_0_g707 / temp_output_7_0_g707 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g707.y , ( ( temp_output_9_0_g707 - 1.0 ) / temp_output_8_0_g707 ) ) ) * ( step( texCoord2_g707.y , ( temp_output_9_0_g707 / temp_output_8_0_g707 ) ) * 1.0 ) ) ) ) + ( _MRE11 * ( ( ( 1.0 - step( texCoord2_g702.x , ( ( temp_output_3_0_g702 - 1.0 ) / temp_output_7_0_g702 ) ) ) * ( step( texCoord2_g702.x , ( temp_output_3_0_g702 / temp_output_7_0_g702 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g702.y , ( ( temp_output_9_0_g702 - 1.0 ) / temp_output_8_0_g702 ) ) ) * ( step( texCoord2_g702.y , ( temp_output_9_0_g702 / temp_output_8_0_g702 ) ) * 1.0 ) ) ) ) + ( _MRE12 * ( ( ( 1.0 - step( texCoord2_g717.x , ( ( temp_output_3_0_g717 - 1.0 ) / temp_output_7_0_g717 ) ) ) * ( step( texCoord2_g717.x , ( temp_output_3_0_g717 / temp_output_7_0_g717 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g717.y , ( ( temp_output_9_0_g717 - 1.0 ) / temp_output_8_0_g717 ) ) ) * ( step( texCoord2_g717.y , ( temp_output_9_0_g717 / temp_output_8_0_g717 ) ) * 1.0 ) ) ) ) ) + ( ( _MRE13 * ( ( ( 1.0 - step( texCoord2_g712.x , ( ( temp_output_3_0_g712 - 1.0 ) / temp_output_7_0_g712 ) ) ) * ( step( texCoord2_g712.x , ( temp_output_3_0_g712 / temp_output_7_0_g712 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g712.y , ( ( temp_output_9_0_g712 - 1.0 ) / temp_output_8_0_g712 ) ) ) * ( step( texCoord2_g712.y , ( temp_output_9_0_g712 / temp_output_8_0_g712 ) ) * 1.0 ) ) ) ) + ( _MRE14 * ( ( ( 1.0 - step( texCoord2_g710.x , ( ( temp_output_3_0_g710 - 1.0 ) / temp_output_7_0_g710 ) ) ) * ( step( texCoord2_g710.x , ( temp_output_3_0_g710 / temp_output_7_0_g710 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g710.y , ( ( temp_output_9_0_g710 - 1.0 ) / temp_output_8_0_g710 ) ) ) * ( step( texCoord2_g710.y , ( temp_output_9_0_g710 / temp_output_8_0_g710 ) ) * 1.0 ) ) ) ) + ( _MRE15 * ( ( ( 1.0 - step( texCoord2_g711.x , ( ( temp_output_3_0_g711 - 1.0 ) / temp_output_7_0_g711 ) ) ) * ( step( texCoord2_g711.x , ( temp_output_3_0_g711 / temp_output_7_0_g711 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g711.y , ( ( temp_output_9_0_g711 - 1.0 ) / temp_output_8_0_g711 ) ) ) * ( step( texCoord2_g711.y , ( temp_output_9_0_g711 / temp_output_8_0_g711 ) ) * 1.0 ) ) ) ) + ( _MRE16 * ( ( ( 1.0 - step( texCoord2_g713.x , ( ( temp_output_3_0_g713 - 1.0 ) / temp_output_7_0_g713 ) ) ) * ( step( texCoord2_g713.x , ( temp_output_3_0_g713 / temp_output_7_0_g713 ) ) * 1.0 ) ) * ( ( 1.0 - step( texCoord2_g713.y , ( ( temp_output_9_0_g713 - 1.0 ) / temp_output_8_0_g713 ) ) ) * ( step( texCoord2_g713.y , ( temp_output_9_0_g713 / temp_output_8_0_g713 ) ) * 1.0 ) ) ) ) ) );
				
				float3 Albedo = ( clampResult261 * temp_output_155_0 ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( temp_output_155_0 * ( _EmissionPower1 * (temp_output_283_0).b ) ).rgb;
				float3 Specular = 0.5;
				float Metallic = (temp_output_283_0).r;
				float Smoothness = ( 1.0 - (temp_output_283_0).g );
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.positionCS = IN.clipPos;
				inputData.shadowCoord = ShadowCoords;



				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
				#else
					inputData.normalWS = WorldNormal;
				#endif
					
				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize( WorldViewDirection );



				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(WorldPosition, 1.0),  IN.fogFactorAndVertexLight.x);
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				

				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#else
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
					#else
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
					#endif
				#endif

				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
						#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				#ifdef _DBUFFER
					ApplyDecal(IN.clipPos,
						Albedo,
						Specular,
						inputData.normalWS,
						Metallic,
						Occlusion,
						Smoothness);
				#endif

				BRDFData brdfData;
				InitializeBRDFData
				(Albedo, Metallic, Specular, Smoothness, Alpha, brdfData);

				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
				half4 color;
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
				color.rgb = GlobalIllumination(brdfData, inputData.bakedGI, Occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
				color.a = Alpha;
				
				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif
				
				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				
				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb);
			}

			ENDHLSL
		}

		
        Pass
        {
			
            Name "SceneSelectionPass"
            Tags { "LightMode"="SceneSelectionPass" }
        
			Cull Off

			HLSLPROGRAM
        
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999

        
			#pragma only_renderers d3d11 glcore gles gles3 
			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif
			
			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				
				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;


				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}

			ENDHLSL
        }

		
        Pass
        {
			
            Name "ScenePickingPass"
            Tags { "LightMode"="Picking" }
        
			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define ASE_SRP_VERSION 999999


			#pragma only_renderers d3d11 glcore gles gles3 
			#pragma vertex vert
			#pragma fragment frag

        
			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY
			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _GradientColor;
			float4 _Color16;
			float4 _MRE1;
			float4 _MRE2;
			float4 _MRE3;
			float4 _MRE4;
			float4 _MRE5;
			float4 _MRE6;
			float4 _MRE7;
			float4 _MRE8;
			float4 _MRE9;
			float4 _MRE10;
			float4 _MRE11;
			float4 _MRE12;
			float4 _MRE13;
			float4 _MRE14;
			float4 _Color15;
			float4 _MRE15;
			float4 _Color14;
			float4 _Color12;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color13;
			float4 _Color5;
			float4 _Color6;
			float4 _Color4;
			float4 _Color8;
			float4 _Color9;
			float4 _Color10;
			float4 _Color11;
			float4 _Color7;
			float4 _MRE16;
			float _EmissionPower1;
			float _GradientPower;
			float _GradientOffset;
			float _GradientScale;
			float _GradientIntensity;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
        
			float4 _SelectionID;

        
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				
				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;


				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;
				
				return outColor;
			}
        
			ENDHLSL
        }
		
	}
	
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18935
-31;112;1399;633;-1283.725;495.4097;1.624516;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;255;615.0826,-371.7701;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,4;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;157;-182.3802,1181.25;Float;False;Property;_Color7;Color 7;6;0;Create;True;0;0;0;False;0;False;0.1544118,0.6151115,1,0.178;0.9099331,0.9264706,0.6267301,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;159;-187.9672,688.0273;Float;False;Property;_Color5;Color 5;4;0;Create;True;0;0;0;False;1;Space(10);False;0.9533468,1,0.1544118,0.553;0.2669382,0.3207545,0.0226949,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;213;-234.6901,2683.007;Float;False;Property;_Color13;Color 13;12;0;Create;True;0;0;0;False;1;Space(10);False;1,0.5586207,0,0.272;1,0.5586207,0,0.272;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;230;859.2263,-398.1579;Inherit;True;Property;_Gradient;Gradient;33;1;[SingleLineTexture];Create;True;0;0;0;False;1;Header(Gradient);False;-1;0f424a347039ef447a763d3d4b4782b0;0f424a347039ef447a763d3d4b4782b0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;214;-242.6307,2942.365;Float;False;Property;_Color14;Color 14;13;0;Create;True;0;0;0;False;0;False;0,0.8025862,0.875,0.047;0,0.8025862,0.875,0.047;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;152;-194.2135,166.9271;Float;False;Property;_Color3;Color 3;2;0;Create;True;0;0;0;False;0;False;0.2535501,0.1544118,1,0.541;0.2535499,0.1544116,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;158;-183.7895,1424.406;Float;False;Property;_Color8;Color 8;7;0;Create;True;0;0;0;False;0;False;0.4849697,0.5008695,0.5073529,0.078;0.1544116,0.1602432,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;181;-218.8154,2174.284;Float;False;Property;_Color11;Color 11;10;0;Create;True;0;0;0;False;0;False;0.6691177,0.6691177,0.6691177,0.647;1,0.1544117,0.3818459,0.316;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;229;874.0561,-170.8387;Float;False;Property;_GradientColor;Gradient Color;35;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-199.8005,-326.2955;Float;False;Property;_Color1;Color 1;0;0;Create;True;0;0;0;False;1;Header(Albedo (A Gradient));False;1,0.1544118,0.1544118,0.291;1,0.1544116,0.1544116,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;218;-229.103,3176.23;Float;False;Property;_Color15;Color 15;14;0;Create;True;0;0;0;False;0;False;1,0,0,0.391;1,0,0,0.391;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;180;-232.3431,1940.419;Float;False;Property;_Color10;Color 10;9;0;Create;True;0;0;0;False;0;False;0.362069,0.4411765,0,0.759;0.1544117,0.1602433,1,0.341;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;188;91.31517,1685.902;Inherit;True;ColorShartSlot;-1;;720;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;182;-220.2247,2417.44;Float;False;Property;_Color12;Color 12;11;0;Create;True;0;0;0;False;0;False;0.5073529,0.1574544,0,0.128;0.02270761,0.1632712,0.2205881,0.484;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;228;845.7399,12.46821;Float;False;Property;_GradientIntensity;Gradient Intensity;34;0;Create;True;0;0;0;False;0;False;0.75;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;154;-195.6228,411.2479;Float;False;Property;_Color4;Color 4;3;0;Create;True;0;0;0;False;0;False;0.1544118,0.5451319,1,0.253;0.9533468,1,0.1544116,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;156;-195.9079,947.3851;Float;False;Property;_Color6;Color 6;5;0;Create;True;0;0;0;False;0;False;0.2720588,0.1294625,0,0.097;1,0.4519259,0.152941,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;217;-264.3738,3419.386;Float;False;Property;_Color16;Color 16;15;0;Create;True;0;0;0;False;0;False;0.4080882,0.75,0.4811866,0.134;0.4080881,0.75,0.4811865,0.134;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;183;-224.4024,1681.061;Float;False;Property;_Color9;Color 9;8;0;Create;True;0;0;0;False;1;Space(10);False;0.3164301,0,0.7058823,0.134;0.152941,0.9929401,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;265;928.9007,2379.895;Float;False;Property;_MRE2;MRE 2;17;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;185;97.41646,2422.281;Inherit;True;ColorShartSlot;-1;;719;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;160;119.8096,952.2258;Inherit;True;ColorShartSlot;-1;;721;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;186;96.90227,2179.125;Inherit;True;ColorShartSlot;-1;;722;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;223;73.08682,2945.046;Inherit;True;ColorShartSlot;-1;;723;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;688.9302,993.4156;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;258;2237.497,-354.0456;Inherit;True;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;224;86.61465,3181.071;Inherit;True;ColorShartSlot;-1;;733;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;161;133.3375,1186.091;Inherit;True;ColorShartSlot;-1;;732;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;145;115.9171,-321.4549;Inherit;True;ColorShartSlot;-1;;731;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;231;1182.122,-372.6908;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;153;122.0185,414.924;Inherit;True;ColorShartSlot;-1;;730;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;222;87.12894,3424.227;Inherit;True;ColorShartSlot;-1;;729;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;151;121.5042,171.7677;Inherit;True;ColorShartSlot;-1;;728;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;3;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;149;107.9764,-62.09709;Inherit;True;ColorShartSlot;-1;;727;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;162;133.8517,1429.247;Inherit;True;ColorShartSlot;-1;;726;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;4;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;163;127.7504,692.868;Inherit;True;ColorShartSlot;-1;;718;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;216;81.02762,2687.848;Inherit;True;ColorShartSlot;-1;;725;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;287;879.1714,4698.762;Float;False;Property;_MRE12;MRE 12;27;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;293;888.8267,5554.006;Float;False;Property;_MRE15;MRE 15;30;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;233;1367.421,-383.9108;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;225;683.3512,1524.765;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;292;886.7413,5342.351;Float;False;Property;_MRE14;MRE 14;29;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;187;83.37437,1945.26;Inherit;True;ColorShartSlot;-1;;724;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;0.7843138,0.3137255,0,0;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;184;686.7443,1260.558;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;294;884.4691,5770.134;Float;False;Property;_MRE16;MRE 16;31;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;288;881.2148,4482.635;Float;False;Property;_MRE11;MRE 11;26;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;146;688.2412,727.387;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;232;1156.605,-68.40891;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;262;888.3417,3297.014;Float;False;Property;_MRE6;MRE 6;21;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;150;-207.7412,-66.93771;Float;False;Property;_Color2;Color 2;1;0;Create;True;0;0;0;False;0;False;1,0.1544118,0.8017241,0.253;1,0.1544116,0.8017241,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;269;876.3038,4037.208;Float;False;Property;_MRE9;MRE 9;24;0;Create;True;0;0;0;False;1;Space(10);False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;298;1185.632,5764.37;Inherit;True;ColorShartSlot;-1;;713;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;263;924.805,2591.167;Float;False;Property;_MRE3;MRE 3;18;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;290;881.4436,4270.979;Float;False;Property;_MRE10;MRE 10;25;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;267;883.4818,3508.669;Float;False;Property;_MRE7;MRE 7;22;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;270;881.4384,3724.796;Float;False;Property;_MRE8;MRE 8;23;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;268;920.3644,2166.243;Float;False;Property;_MRE1;MRE 1;16;0;Create;True;0;0;0;False;1;Header(Metallic(R) Rough(G) Emmission(B));False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;266;927.7203,2799.667;Float;False;Property;_MRE4;MRE 4;19;0;Create;True;0;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;295;1178.695,5106.292;Inherit;True;ColorShartSlot;-1;;712;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;297;1194.631,5541.872;Inherit;True;ColorShartSlot;-1;;711;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;296;1192.958,5326.958;Inherit;True;ColorShartSlot;-1;;710;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;1;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;291;881.6017,5108.58;Float;False;Property;_MRE13;MRE 13;28;0;Create;True;0;0;0;False;1;Space(10);False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;276;1182.602,3719.032;Inherit;True;ColorShartSlot;-1;;709;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;289;1187.66,4255.586;Inherit;True;ColorShartSlot;-1;;707;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;277;1200.345,2802.812;Inherit;True;ColorShartSlot;-1;;706;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;278;1189.738,3073.549;Inherit;True;ColorShartSlot;-1;;705;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;274;1200.378,2592.315;Inherit;True;ColorShartSlot;-1;;704;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;275;1204.517,2165.975;Inherit;True;ColorShartSlot;-1;;703;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;238;1648.681,-135.2692;Float;False;Property;_GradientScale;Gradient Scale;36;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;239;1655.499,-42.52599;Float;False;Property;_GradientOffset;Gradient Offset;37;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;285;1189.334,4470.5;Inherit;True;ColorShartSlot;-1;;702;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;234;1652.17,-392.4709;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;1016.686,1030.498;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;273;1187.369,3281.62;Inherit;True;ColorShartSlot;-1;;708;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;272;1173.398,4034.921;Inherit;True;ColorShartSlot;-1;;714;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;1;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;264;887.6243,3070.586;Float;False;Property;_MRE5;MRE 5;20;0;Create;True;0;0;0;False;1;Space(10);False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;305;2944.843,925.5689;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;286;1180.335,4692.998;Inherit;True;ColorShartSlot;-1;;717;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;4;False;9;FLOAT;2;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;279;1189.043,3496.534;Inherit;True;ColorShartSlot;-1;;716;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;3;False;9;FLOAT;3;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;299;1571.928,5274.19;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;280;1566.631,4202.819;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;281;1569.768,2500.448;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;271;1202.304,2381.9;Inherit;True;ColorShartSlot;-1;;715;231fe18505db4a84b9c478d379c9247d;0;5;38;COLOR;1,1,1,1;False;3;FLOAT;2;False;9;FLOAT;4;False;7;FLOAT;4;False;8;FLOAT;4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;282;1553.285,3234.033;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;256;2060.061,-74.53971;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;237;1929.951,-353.3528;Inherit;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;259;2282.192,-66.26985;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;245;1659.99,53.90989;Float;False;Property;_GradientPower;Gradient Power;38;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;302;2524.654,1230.847;Inherit;False;Property;_EmissionPower1;Emission Power;32;0;Create;True;0;0;0;False;1;Header(Emmision);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;283;1960.203,2821.543;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;236;3433.932,229.384;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;284;2918.832,2649.558;Inherit;True;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;304;3114.618,1152.562;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;307;2570.26,731.6305;Inherit;True;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;306;2576.238,929.5774;Inherit;True;False;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;261;2796.672,-358.3472;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;260;2530.138,-355.3468;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;301;2514.004,1365.87;Inherit;True;False;False;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;303;2828.549,1217.506;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;315;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;317;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;308;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;309;4040.206,769.1205;Float;False;True;-1;2;ASEMaterialInspector;0;2;Malbers/Color4x4v2;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;19;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;1;LightMode=UniversalForward;False;False;0;;0;0;Standard;40;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;0;637780420231502105;Fragment Normal Space,InvertActionOnDeselection;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,-1;0;Translucency;0;0;  Translucency Strength;1,False,-1;0;  Normal Distortion;0.5,False,-1;0;  Scattering;2,False,-1;0;  Direct;0.9,False,-1;0;  Ambient;0.1,False,-1;0;  Shadow;0.5,False,-1;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;1;0;Debug Display;0;0;Clear Coat;0;0;0;10;False;True;True;True;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;310;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;311;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;312;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;313;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;314;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;316;4040.206,769.1205;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;230;1;255;0
WireConnection;188;38;183;0
WireConnection;185;38;182;0
WireConnection;160;38;156;0
WireConnection;186;38;181;0
WireConnection;223;38;214;0
WireConnection;164;0;163;0
WireConnection;164;1;160;0
WireConnection;164;2;161;0
WireConnection;164;3;162;0
WireConnection;258;0;237;0
WireConnection;258;1;245;0
WireConnection;224;38;218;0
WireConnection;161;38;157;0
WireConnection;145;38;23;0
WireConnection;231;0;230;0
WireConnection;231;1;229;0
WireConnection;153;38;154;0
WireConnection;222;38;217;0
WireConnection;151;38;152;0
WireConnection;149;38;150;0
WireConnection;162;38;158;0
WireConnection;163;38;159;0
WireConnection;216;38;213;0
WireConnection;233;0;231;0
WireConnection;233;1;232;0
WireConnection;225;0;216;0
WireConnection;225;1;223;0
WireConnection;225;2;224;0
WireConnection;225;3;222;0
WireConnection;187;38;180;0
WireConnection;184;0;188;0
WireConnection;184;1;187;0
WireConnection;184;2;186;0
WireConnection;184;3;185;0
WireConnection;146;0;145;0
WireConnection;146;1;149;0
WireConnection;146;2;151;0
WireConnection;146;3;153;0
WireConnection;232;0;228;0
WireConnection;298;38;294;0
WireConnection;295;38;291;0
WireConnection;297;38;293;0
WireConnection;296;38;292;0
WireConnection;276;38;270;0
WireConnection;289;38;290;0
WireConnection;277;38;266;0
WireConnection;278;38;264;0
WireConnection;274;38;263;0
WireConnection;275;38;268;0
WireConnection;285;38;288;0
WireConnection;234;0;233;0
WireConnection;155;0;146;0
WireConnection;155;1;164;0
WireConnection;155;2;184;0
WireConnection;155;3;225;0
WireConnection;273;38;262;0
WireConnection;272;38;269;0
WireConnection;305;0;306;0
WireConnection;286;38;287;0
WireConnection;279;38;267;0
WireConnection;299;0;295;0
WireConnection;299;1;296;0
WireConnection;299;2;297;0
WireConnection;299;3;298;0
WireConnection;280;0;272;0
WireConnection;280;1;289;0
WireConnection;280;2;285;0
WireConnection;280;3;286;0
WireConnection;281;0;275;0
WireConnection;281;1;271;0
WireConnection;281;2;274;0
WireConnection;281;3;277;0
WireConnection;271;38;265;0
WireConnection;282;0;278;0
WireConnection;282;1;273;0
WireConnection;282;2;279;0
WireConnection;282;3;276;0
WireConnection;256;0;155;0
WireConnection;237;0;234;0
WireConnection;237;1;238;0
WireConnection;237;2;239;0
WireConnection;259;0;256;0
WireConnection;283;0;281;0
WireConnection;283;1;282;0
WireConnection;283;2;280;0
WireConnection;283;3;299;0
WireConnection;236;0;261;0
WireConnection;236;1;155;0
WireConnection;284;0;283;0
WireConnection;304;0;155;0
WireConnection;304;1;303;0
WireConnection;307;0;283;0
WireConnection;306;0;283;0
WireConnection;261;0;260;0
WireConnection;260;0;258;0
WireConnection;260;1;259;0
WireConnection;301;0;283;0
WireConnection;303;0;302;0
WireConnection;303;1;301;0
WireConnection;309;0;236;0
WireConnection;309;2;304;0
WireConnection;309;3;307;0
WireConnection;309;4;305;0
ASEEND*/
//CHKSM=49B79ABC53ADAD0D852AA09F559636AB8B8FA1FE