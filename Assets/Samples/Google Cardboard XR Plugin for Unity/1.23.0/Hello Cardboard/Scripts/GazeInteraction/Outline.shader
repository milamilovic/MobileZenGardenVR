Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 1, 1, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.03
    }
    
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            float _OutlineWidth;
            float4 _OutlineColor;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // Expand vertices along normals in object space
                float3 normal = normalize(v.normal);
                float3 expandedPos = v.vertex.xyz + normal * _OutlineWidth;
                
                o.pos = UnityObjectToClipPos(float4(expandedPos, 1.0));
                
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            
            ENDCG
        }
    }
    
    Fallback Off
}
