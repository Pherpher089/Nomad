// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios/Aurora"
{
	Properties
	{
		_Brightness("Brightness", Range( 0 , 5)) = 1
		_PannerMask("PannerMask", 2D) = "white" {}
		_BrightnessMask("BrightnessMask", 2D) = "white" {}
		_Color_Up("Color_Up", Color) = (0.2269654,1,0,0)
		_Color_Down("Color_Down", Color) = (0.2269654,1,0,0)
		_BottomColourMask("BottomColourMask", 2D) = "white" {}
		_TopColourMask("TopColourMask", 2D) = "white" {}
		_Speed("Speed", Vector) = (1,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color_Up;
		uniform sampler2D _TopColourMask;
		uniform float4 _TopColourMask_ST;
		uniform sampler2D _BottomColourMask;
		uniform float4 _BottomColourMask_ST;
		uniform float4 _Color_Down;
		uniform sampler2D _PannerMask;
		uniform float2 _Speed;
		uniform float _Brightness;
		uniform sampler2D _BrightnessMask;
		uniform float4 _BrightnessMask_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = float4(1,1,1,0).rgb;
			float2 uv_TopColourMask = i.uv_texcoord * _TopColourMask_ST.xy + _TopColourMask_ST.zw;
			float2 uv_BottomColourMask = i.uv_texcoord * _BottomColourMask_ST.xy + _BottomColourMask_ST.zw;
			float mulTime76 = _Time.y * 0.02;
			float2 panner74 = ( mulTime76 * _Speed + i.uv_texcoord);
			float4 tex2DNode75 = tex2D( _PannerMask, panner74 );
			float2 uv_BrightnessMask = i.uv_texcoord * _BrightnessMask_ST.xy + _BrightnessMask_ST.zw;
			o.Emission = ( ( ( ( _Color_Up * tex2D( _TopColourMask, uv_TopColourMask ) ) + ( tex2D( _BottomColourMask, uv_BottomColourMask ) * _Color_Down ) ) * tex2DNode75 ) * ( _Brightness * tex2D( _BrightnessMask, uv_BrightnessMask ) ) ).rgb;
			o.Alpha = tex2DNode75.a;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18909
-3434;8;2956;1351;1370.072;1085.126;1.430097;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;71;503.8651,-242.1998;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;85;122.1427,-570.1396;Inherit;True;Property;_BottomColourMask;BottomColourMask;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;87;94.14267,-943.1391;Inherit;True;Property;_TopColourMask;TopColourMask;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;84;107.1426,-375.1396;Inherit;False;Property;_Color_Down;Color_Down;4;0;Create;True;0;0;0;False;0;False;0.2269654,1,0,0;0,0.9896681,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;82;105.1427,-752.1395;Inherit;False;Property;_Color_Up;Color_Up;3;0;Create;True;0;0;0;False;0;False;0.2269654,1,0,0;0.5549389,0,0.7075471,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;73;556.8651,-106.1998;Float;False;Property;_Speed;Speed;7;0;Create;True;0;0;0;False;0;False;1,0;0,-80;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;76;548.8651,40.80017;Inherit;False;1;0;FLOAT;0.02;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;521.1426,-717.1396;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;468.1427,-438.1395;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;74;834.8651,-183.1998;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;96;1169.797,-975.5681;Inherit;False;Property;_Brightness;Brightness;0;0;Create;True;0;0;0;False;0;False;1;5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;75;1039.865,-214.1998;Inherit;True;Property;_PannerMask;PannerMask;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;90;717.7605,-676.5403;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;98;1110.087,-700.99;Inherit;True;Property;_BrightnessMask;BrightnessMask;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;1419.761,-674.1526;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;1323.397,-380.5365;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;99;1427.198,-289.9926;Inherit;False;Constant;_Color0;Color 0;8;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;1493.002,-465.3639;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1730.068,-246.0995;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SyntyStudios/Aurora;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;83;0;82;0
WireConnection;83;1;87;0
WireConnection;88;0;85;0
WireConnection;88;1;84;0
WireConnection;74;0;71;0
WireConnection;74;2;73;0
WireConnection;74;1;76;0
WireConnection;75;1;74;0
WireConnection;90;0;83;0
WireConnection;90;1;88;0
WireConnection;97;0;96;0
WireConnection;97;1;98;0
WireConnection;79;0;90;0
WireConnection;79;1;75;0
WireConnection;95;0;79;0
WireConnection;95;1;97;0
WireConnection;0;0;99;0
WireConnection;0;2;95;0
WireConnection;0;9;75;4
ASEEND*/
//CHKSM=4F54C7EDD50A213D8856E41BD2E0C2F32F693BF1