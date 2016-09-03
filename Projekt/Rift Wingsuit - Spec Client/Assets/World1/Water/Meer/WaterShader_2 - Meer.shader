Shader "Rift-Suit/Water/Normal" {
	Properties {
		_SunPositionX("Sonne: X-Position", Float) = 3200
		_SunPositionZ("Sonne: Z-Position", Float) = 3200
		_WaterSpeed ("Wassergeschwindigkeit", Range (0.01, 1)) = 1
		_SpecColor ("Lichtfarbe", Color) = (0.5,0.5,0.5,1)
		_Shininess ("Lichthelligkeit", Range (0.01, 0.95)) = 0.078125
		_DepthColor ("Wasserfarbe (nah)", Color) = (0.01,0.02,0.02,0.8)
		_ReflectColor ("Wasserfarbe (fern)", Color) = (1,1,1,0.5)
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_WaterReflex("Wasser-Reflexionsstärke", Float) = 2.0
		_WaterLumi("Wasser Helligkeit", Range(0.01, 2)) = 1.0
		_Opaque("Sichtdurchlässigkeit", Range(0.00,1.00)) = 0.95
	}

	SubShader 
	{	
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}
		LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		Offset -1, -1

		GrabPass 
		{ 
			
		}
		
		CGPROGRAM

		#pragma surface surf BlinnPhong alpha
		#pragma target 3.0

		sampler2D _GrabTexture : register(s0);
		sampler2D _BumpMap : register(s2);

		sampler2D _CameraDepthTexture; // : register(s4);

		float _SunPositionX;
		float _SunPositionZ;
		float _WaterSpeed;
		float4 _ReflectColor;
		float4 _DepthColor;
		float _Shininess;
		float _WaterReflex;
		float _WaterLumi;
		float _R0 = 0.55;
		float _Opaque;

		float4 _GrabTexture_TexelSize;
		float4 _CameraDepthTexture_TexelSize;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldRefl; 
			float4 screenPos;
			float3 viewDir;
			INTERNAL_DATA
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			// shore blending
			float z1 = tex2Dproj(_CameraDepthTexture,  IN.screenPos); 
			z1 =  LinearEyeDepth(z1);	
			float z2 = (IN.screenPos.z);
			
			//Specular
			o.Gloss = _SpecColor.a;
			o.Specular = (1.0 - _Shininess);
			
			float waterSpeed = _WaterSpeed * 175;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap - waterSpeed * _Time.y * _GrabTexture_TexelSize.xy ));
			o.Normal += UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap - (waterSpeed*3) * _Time.y * _GrabTexture_TexelSize.xy));
			o.Normal *= 0.5;
			
			//Seethrough
			o.Alpha = _Opaque;

			//Sonnenposition
			IN.screenPos.xy = (_SunPositionX, _SunPositionZ);	
			
			half4 reflcol = (0.75,0.75,1,1) * _ReflectColor;
			
			//float3 refrColor = tex2Dproj(_GrabTexture, IN.screenPos);
			float3 refrColor = _DepthColor;
			//refrColor = refrColor * ( 1.0 -  depthAlpha ) + _DepthColor * depthAlpha;
			
			
			//Freshel realisation
			half fresnel = saturate( 1.0 - dot(o.Normal, normalize(IN.viewDir)) );
			fresnel = pow(fresnel, _WaterReflex);
			fresnel =  _R0 + (1.0 - _R0) * fresnel;
			
			half4 resCol = reflcol * fresnel + half4( refrColor.xyz ,1.0) * ( 1.0 - fresnel);	
			//half4 resCol = reflcol;	
			o.Emission = resCol * 0.8;
			
			o.Albedo = resCol * _WaterLumi;
			


		}
		ENDCG
	}
}