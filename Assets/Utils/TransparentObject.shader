Shader "Unlit/TransparentObject"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}
        _Hide("Hide", Float) = 0.1
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Name "MAPHEIGHT"
            Tags{ "LightMode" = "ForwardBase" }
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog    
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "AutoLight.cginc"            
            
            float4 _Color;
            float _Hide;
            //sampler2D _MainTex;
            
            struct v2f {
                half2 uv : TEXCOORD3;
                float4 pos : SV_POSITION;
            };
            
            
            v2f vert(float4 vertex : POSITION, float3 normal : NORMAL,float2 uv : TEXCOORD0)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.uv = uv;
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                TRANSFER_SHADOW(o);
                return o;
            }
            
            sampler2D _MainTex;
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv)* _Color;
                if(_Hide)
                c.a = 0;
                return c;
            }
            
            ENDCG
        }
        // shadow casting support
        // shadow caster rendering pass, implemented manually
        Pass
        {
            Name "SHADOWCASTER"
            Tags{ "LightMode" = "ShadowCaster" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            
            struct v2f {
                V2F_SHADOW_CASTER;
            };
            
            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }//end subshader
}//end shader