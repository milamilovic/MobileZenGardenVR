Shader "ZenGarden/SandRealistic"
{
    Properties
    {
        [Header(Sand Textures)]
        _MainTex ("Sand Albedo", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        
        [Header(Sand Colors)]
        _SandColor ("Sand Tint", Color) = (0.8943396, 0.8908541, 0.8825275, 1)
        _ShadowColor ("Shadow Tint", Color) = (1, 0.9243297, 0.7603773, 1)
        _TrailColor ("Trail Color (Disturbed Sand)", Color) = (0.6, 0.55, 0.45, 1)
        
        [Header(Surface Properties)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.15
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        _TextureScale ("Texture Scale", Float) = 1.0
        
        [Header(Deformation)]
        _HeightMap ("Height Map (Deformation)", 2D) = "gray" {}
        _DisplacementStrength ("Displacement Strength", Range(0, 0.5)) = 0.05
        
        [Header(Trail Settings)]
        _DarknessMap ("Darkness Map (Trails)", 2D) = "black" {}
        _TrailIntensity ("Trail Darkness Intensity", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.5
        
        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _RoughnessMap;
        sampler2D _HeightMap;
        sampler2D _DarknessMap;
        
        float4 _SandColor;
        float4 _ShadowColor;
        float4 _TrailColor;
        float _Smoothness;
        float _NormalStrength;
        float _TextureScale;
        float _DisplacementStrength;
        float _TrailIntensity;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_HeightMap;
            float3 worldPos;
        };
        
        void vert(inout appdata_full v)
        {
            // Deformation based on height map
            float height = tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0)).r;
            v.vertex.y += (height - 0.5) * _DisplacementStrength;
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Scale texture
            float2 scaledUV = IN.uv_MainTex * _TextureScale;
            
            // Base sand albedo
            fixed4 sandTex = tex2D(_MainTex, scaledUV);
            float3 baseColor = sandTex.rgb * _SandColor.rgb;
            
            // Normal map for detail
            fixed3 normal = UnpackNormal(tex2D(_BumpMap, scaledUV));
            normal.xy *= _NormalStrength;
            o.Normal = normalize(normal);
            
            // Roughness
            float roughness = tex2D(_RoughnessMap, scaledUV).r;
            o.Smoothness = (1 - roughness) * _Smoothness;
            
            // Add slight color variation based on height deformation
            float deformation = tex2D(_HeightMap, IN.uv_HeightMap).r;
            float3 shadowTint = lerp(_ShadowColor.rgb, float3(1,1,1), deformation);
            baseColor *= shadowTint;
            
            // Apply trails where rake has passed
            float darkness = tex2D(_DarknessMap, IN.uv_HeightMap).r;
            
            // Blend between base sand color and darker trail color
            float3 finalColor = lerp(baseColor, _TrailColor.rgb, darkness * _TrailIntensity);
            
            o.Albedo = finalColor;
            o.Metallic = 0;
        }
        ENDCG
    }
    
    FallBack "Standard"
}