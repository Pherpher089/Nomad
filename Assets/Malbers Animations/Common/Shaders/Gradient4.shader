// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Malbers/Gradient4"
{
	Properties
	{
		_Tint("Tint", Color) = (1,1,1,1)
		[Header(Color 1)]_Color1Top("Top (A: Rough)", Color) = (1,0,0,1)
		_Color1Bottom("Bottom (A: Metallic)", Color) = (0,0.1124103,0.4056604,0)
		[HDR]_Color1Emmission("Emmission", Color) = (0,0.1124103,0.4056604,0)
		_Power1("Power", Float) = 0
		[Header(Color 2)]_Color2Top("Top (A: Rough)", Color) = (1,0,0,1)
		_Color2Bottom("Bottom (A: Metallic)", Color) = (0,0.1124103,0.4056604,0)
		[HDR]_Color2Emmission("Emmission", Color) = (0,0.1124103,0.4056604,0)
		_Power2("Power", Float) = 0
		[Header(Color 2)]_Color3Top("Top (A: Rough)", Color) = (1,0,0,1)
		_Color3Bottom("Bottom (A: Metallic)", Color) = (0,0.1124103,0.4056604,0)
		[HDR]_Color3Emmission("Emmission", Color) = (0,0.1124103,0.4056604,0)
		_Power3("Power", Float) = 0
		[Header(Color 2)]_Color4Top("Top (A: Rough)", Color) = (1,0,0,1)
		_Color4Bottom("Bottom (A: Metallic)", Color) = (0,0.1124103,0.4056604,0)
		[HDR]_Color4Emmission("Emmission", Color) = (0,0.1124103,0.4056604,0)
		_Power4("Power", Float) = 0
		_Lit("Lit", Range( 0 , 1)) = 0
		[HDR][Header(Extras)]_EmissionColor("Emission Color", Color) = (0,0,0,0)
		_MaskTexture("Mask Texture", 2D) = "white" {}
		_MaskTint("Mask Tint", Color) = (1,1,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv2_texcoord2;
			float2 uv_texcoord;
		};

		uniform float4 _MaskTint;
		uniform sampler2D _MaskTexture;
		uniform float4 _MaskTexture_ST;
		uniform float4 _Tint;
		uniform float4 _Color1Bottom;
		uniform float4 _Color1Top;
		uniform float _Power1;
		uniform float4 _Color2Bottom;
		uniform float4 _Color2Top;
		uniform float _Power2;
		uniform float4 _Color3Bottom;
		uniform float4 _Color3Top;
		uniform float _Power3;
		uniform float4 _Color4Bottom;
		uniform float4 _Color4Top;
		uniform float _Power4;
		uniform float _Lit;
		uniform float4 _Color1Emmission;
		uniform float4 _Color2Emmission;
		uniform float4 _Color3Emmission;
		uniform float4 _Color4Emmission;
		uniform float4 _EmissionColor;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv1_MaskTexture = i.uv2_texcoord2 * _MaskTexture_ST.xy + _MaskTexture_ST.zw;
			float4 lerpResult162 = lerp( float4( 1,1,1,0 ) , _MaskTint , tex2D( _MaskTexture, uv1_MaskTexture ));
			float DefaultGradient26 = i.uv_texcoord.y;
			float saferPower30 = abs( DefaultGradient26 );
			float4 lerpResult9 = lerp( _Color1Bottom , _Color1Top , pow( saferPower30 , _Power1 ));
			float temp_output_11_0 = step( i.uv_texcoord.x , 0.25 );
			float Gradient125 = temp_output_11_0;
			float saferPower125 = abs( DefaultGradient26 );
			float4 lerpResult121 = lerp( _Color2Bottom , _Color2Top , pow( saferPower125 , _Power2 ));
			float Gradient223 = ( step( i.uv_texcoord.x , 0.5 ) * ( 1.0 - temp_output_11_0 ) );
			float saferPower133 = abs( DefaultGradient26 );
			float4 lerpResult132 = lerp( _Color3Bottom , _Color3Top , pow( saferPower133 , _Power3 ));
			float temp_output_18_0 = step( 0.75 , i.uv_texcoord.x );
			float Gradient322 = ( step( 0.5 , i.uv_texcoord.x ) * ( 1.0 - temp_output_18_0 ) );
			float saferPower146 = abs( DefaultGradient26 );
			float4 lerpResult145 = lerp( _Color4Bottom , _Color4Top , pow( saferPower146 , _Power4 ));
			float Gradient424 = temp_output_18_0;
			float4 temp_output_78_0 = ( lerpResult162 * ( _Tint * ( ( lerpResult9 * Gradient125 ) + ( lerpResult121 * Gradient223 ) + ( lerpResult132 * Gradient322 ) + ( lerpResult145 * Gradient424 ) ) ) );
			o.Albedo = temp_output_78_0.rgb;
			o.Emission = ( ( _Lit * temp_output_78_0 ) + ( ( ( _Color1Emmission * Gradient125 ) + ( _Color2Emmission * Gradient223 ) + ( _Color3Emmission * Gradient322 ) + ( _Color4Emmission * Gradient424 ) ) + _EmissionColor ) ).rgb;
			o.Metallic = ( ( _Color1Bottom.a * Gradient125 ) + ( _Color2Bottom.a * Gradient223 ) + ( _Color3Bottom.a * Gradient322 ) + ( _Color4Bottom.a * Gradient424 ) );
			o.Smoothness = ( 1.0 - ( ( _Color1Top.a * Gradient125 ) + ( _Color2Top.a * Gradient223 ) + ( _Color3Top.a * Gradient322 ) + ( _Color4Top.a * Gradient424 ) ) );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.CommentaryNode;166;-22.27451,-1482.911;Inherit;False;706.571;453.4742;Mask;3;162;88;77;;0.8396226,0.3128782,0.3128782,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;130;-1495.185,-811.1176;Inherit;False;1098.129;929.6774;Comment;12;118;119;120;121;122;123;124;125;126;127;128;129;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;37;-1487.616,-1753.258;Inherit;False;1123.305;887.5463;Gradient1;12;113;109;97;14;30;35;112;6;9;28;27;5;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;36;-2988.322,-821.5596;Inherit;False;1139.196;1023.971;Gradients;14;8;11;26;16;15;20;17;19;18;21;22;23;24;25;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-2937.471,-504.743;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;11;-2586.956,-750.9186;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-2938.323,-221.4015;Inherit;False;DefaultGradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;16;-2379.68,-656.4557;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;15;-2581.481,-454.2695;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;20;-2563.708,-354.5058;Inherit;False;2;0;FLOAT;0.5;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-2348.604,-509.3107;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;19;-2577.903,-226.8012;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;18;-2540.874,-51.5888;Inherit;True;2;0;FLOAT;0.75;False;1;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-2360.918,-284.2387;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-2073.126,-277.9415;Inherit;False;Gradient3;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;-2114.823,-494.6176;Inherit;False;Gradient2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-2247.455,-59.38372;Inherit;False;Gradient4;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-2297.105,-771.5597;Inherit;False;Gradient1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-1436.751,-1703.258;Inherit;False;Property;_Color1Top;Top (A: Rough);1;1;[Header];Create;False;1;Color 1;0;0;False;0;False;1,0,0,1;1,0.8431372,0.6352941,0.7450981;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;27;-1451.253,-1117.223;Inherit;False;26;DefaultGradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;-1045.103,-1702.313;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;6;-1432.872,-1533.384;Inherit;False;Property;_Color1Bottom;Bottom (A: Metallic);2;0;Create;False;0;0;0;False;0;False;0,0.1124103,0.4056604,0;0.5647059,0.1921565,0.1529408,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;35;-892.4625,-1444.333;Inherit;False;25;Gradient1;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-638.1231,-1708.737;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;-642.7473,-1499.295;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-644.5492,-1290.136;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-649.4342,-1083.059;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;112;-1436.971,-1335.117;Inherit;False;Property;_Color1Emmission;Emmission;3;1;[HDR];Create;False;0;0;0;False;0;False;0,0.1124103,0.4056604,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;28;-1408.678,-1038.686;Inherit;False;Property;_Power1;Power;4;0;Create;False;0;0;0;False;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;121;-1039.037,-754.6931;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;-632.0571,-761.1176;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;-636.6812,-551.6754;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;128;-638.4832,-342.5165;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-1445.185,-169.6033;Inherit;False;26;DefaultGradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;124;-886.3971,-496.7138;Inherit;False;23;Gradient2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;122;-1426.804,-585.7639;Inherit;False;Property;_Color2Bottom;Bottom (A: Metallic);6;0;Create;False;0;0;0;False;0;False;0,0.1124103,0.4056604,0;0.1226411,0.1036306,0.09545169,0.2901961;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;123;-1430.903,-387.4976;Inherit;False;Property;_Color2Emmission;Emmission;7;1;[HDR];Create;False;0;0;0;False;0;False;0,0.1124103,0.4056604,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;120;-1402.61,-91.06691;Inherit;False;Property;_Power2;Power;8;0;Create;False;0;0;0;False;0;False;0;3.17;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;118;-1430.683,-755.638;Inherit;False;Property;_Color2Top;Top (A: Rough);5;1;[Header];Create;False;1;Color 2;0;0;False;0;False;1,0,0,1;0.8301887,0.482462,0.3328582,0.5803922;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;131;-1490.59,151.6976;Inherit;False;1098.129;929.6774;Comment;12;143;142;141;140;139;138;136;133;132;137;135;134;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;53;119.0074,-847.8591;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;98;270.9904,-608.2259;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;111;329.1559,-363.4786;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;114;340.0198,-127.1017;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;132;-1034.441,208.122;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;136;-633.8868,620.2991;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;138;-1440.59,793.2125;Inherit;False;26;DefaultGradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;144;-1502.35,1156.053;Inherit;False;1098.129;929.6774;Comment;12;156;155;154;153;152;151;150;149;148;147;146;145;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PowerNode;146;-1162.902,1626.474;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;-1452.351,1797.567;Inherit;False;26;DefaultGradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;155;-1409.775,1876.104;Inherit;False;Property;_Power4;Power;16;0;Create;False;0;0;0;False;0;False;0;1.27;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;137;-618.6122,849.3589;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;133;-1212.904,836.2346;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;125;-1192.794,-110.1114;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;30;-1211.213,-1082.436;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;-627.5634,388.5325;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-622.1853,172.5636;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;129;-651.6569,-105.794;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-881.8006,466.1017;Inherit;False;22;Gradient3;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;145;-1046.203,1212.478;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;-639.223,1206.053;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-643.8471,1415.497;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;149;-645.6494,1624.653;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;150;-650.5342,1831.73;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-893.5621,1470.457;Inherit;False;24;Gradient4;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;143;-1426.087,207.1772;Inherit;False;Property;_Color3Top;Top (A: Rough);9;1;[Header];Create;False;1;Color 2;0;0;False;0;False;1,0,0,1;0.4216799,0.6421142,0.8679245,0.7215686;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;140;-1422.209,375.0301;Inherit;False;Property;_Color3Bottom;Bottom (A: Metallic);10;0;Create;False;0;0;0;False;0;False;0,0.1124103,0.4056604,0;0.07843097,0.1063231,0.1999996,0.282353;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;141;-1428.329,575.3181;Inherit;False;Property;_Color3Emmission;Emmission;11;1;[HDR];Create;False;0;0;0;False;0;False;0,0.1124103,0.4056604,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;142;-1398.014,871.7491;Inherit;False;Property;_Power3;Power;12;0;Create;False;0;0;0;False;0;False;0;1.61;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;156;-1437.848,1211.533;Inherit;False;Property;_Color4Top;Top (A: Rough);13;1;[Header];Create;False;1;Color 2;0;0;False;0;False;1,0,0,1;1,1,1,0.7215686;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;153;-1433.97,1381.407;Inherit;False;Property;_Color4Bottom;Bottom (A: Metallic);14;0;Create;False;0;0;0;False;0;False;0,0.1124103,0.4056604,0;0,0,0,0.7294118;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;154;-1438.069,1579.672;Inherit;False;Property;_Color4Emmission;Emmission;15;1;[HDR];Create;False;0;0;0;False;0;False;0,0.1124103,0.4056604,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;161;749.2261,-55.12042;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;702.947,-878.5511;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;162;419.2965,-1350.132;Inherit;True;3;0;COLOR;1,1,1,0;False;1;COLOR;1,1,1,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;77;27.72549,-1259.437;Inherit;True;Property;_MaskTexture;Mask Texture;19;0;Create;True;0;0;0;False;0;False;-1;None;None;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;88;34.2602,-1432.911;Inherit;False;Property;_MaskTint;Mask Tint;20;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;168;423.5345,-882.4908;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;167;126.4495,-1026.664;Inherit;False;Property;_Tint;Tint;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1290.25,-563.0848;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Malbers/Gradient4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0.01;0,0,0,0;VertexScale;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SimpleAddOpNode;169;1143.946,-392.455;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;171;1003.898,-387.0684;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;158;742.8075,-558.8237;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;170;641.0871,-470.9787;Inherit;False;Property;_Lit;Lit;17;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;160;381.1844,105.9753;Inherit;False;Property;_EmissionColor;Emission Color;18;2;[HDR];[Header];Create;False;1;Extras;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;11;0;8;1
WireConnection;26;0;8;2
WireConnection;16;0;11;0
WireConnection;15;0;8;1
WireConnection;20;1;8;1
WireConnection;17;0;15;0
WireConnection;17;1;16;0
WireConnection;19;0;18;0
WireConnection;18;1;8;1
WireConnection;21;0;20;0
WireConnection;21;1;19;0
WireConnection;22;0;21;0
WireConnection;23;0;17;0
WireConnection;24;0;18;0
WireConnection;25;0;11;0
WireConnection;9;0;6;0
WireConnection;9;1;5;0
WireConnection;9;2;30;0
WireConnection;14;0;9;0
WireConnection;14;1;35;0
WireConnection;97;0;5;4
WireConnection;97;1;35;0
WireConnection;109;0;6;4
WireConnection;109;1;35;0
WireConnection;113;0;112;0
WireConnection;113;1;35;0
WireConnection;121;0;122;0
WireConnection;121;1;118;0
WireConnection;121;2;125;0
WireConnection;126;0;121;0
WireConnection;126;1;124;0
WireConnection;127;0;118;4
WireConnection;127;1;124;0
WireConnection;128;0;122;4
WireConnection;128;1;124;0
WireConnection;53;0;14;0
WireConnection;53;1;126;0
WireConnection;53;2;134;0
WireConnection;53;3;147;0
WireConnection;98;0;97;0
WireConnection;98;1;127;0
WireConnection;98;2;135;0
WireConnection;98;3;148;0
WireConnection;111;0;109;0
WireConnection;111;1;128;0
WireConnection;111;2;136;0
WireConnection;111;3;149;0
WireConnection;114;0;113;0
WireConnection;114;1;129;0
WireConnection;114;2;137;0
WireConnection;114;3;150;0
WireConnection;132;0;140;0
WireConnection;132;1;143;0
WireConnection;132;2;133;0
WireConnection;136;0;140;4
WireConnection;136;1;139;0
WireConnection;146;0;151;0
WireConnection;146;1;155;0
WireConnection;137;0;141;0
WireConnection;137;1;139;0
WireConnection;133;0;138;0
WireConnection;133;1;142;0
WireConnection;125;0;119;0
WireConnection;125;1;120;0
WireConnection;30;0;27;0
WireConnection;30;1;28;0
WireConnection;135;0;143;4
WireConnection;135;1;139;0
WireConnection;134;0;132;0
WireConnection;134;1;139;0
WireConnection;129;0;123;0
WireConnection;129;1;124;0
WireConnection;145;0;153;0
WireConnection;145;1;156;0
WireConnection;145;2;146;0
WireConnection;147;0;145;0
WireConnection;147;1;152;0
WireConnection;148;0;156;4
WireConnection;148;1;152;0
WireConnection;149;0;153;4
WireConnection;149;1;152;0
WireConnection;150;0;154;0
WireConnection;150;1;152;0
WireConnection;161;0;114;0
WireConnection;161;1;160;0
WireConnection;78;0;162;0
WireConnection;78;1;168;0
WireConnection;162;1;88;0
WireConnection;162;2;77;0
WireConnection;168;0;167;0
WireConnection;168;1;53;0
WireConnection;0;0;78;0
WireConnection;0;2;169;0
WireConnection;0;3;111;0
WireConnection;0;4;158;0
WireConnection;169;0;171;0
WireConnection;169;1;161;0
WireConnection;171;0;170;0
WireConnection;171;1;78;0
WireConnection;158;0;98;0
ASEEND*/
//CHKSM=F93102D0539631DE636B4447A831838EAA271BDA