// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Grass"
{
	Properties
	{
		_TintColor1("Tint Color 1", Color) = (1,1,1,1)
		_TintColor2("Tint Color 2", Color) = (1,1,1,1)
		_ColorHeight("Color Height",  Range(0, 1)) = .3
		_CutoutThresh("Cutout Threshold", Range(0.0,1.0)) = 0.2
		_Speed("Speed", Range(0.0,5.0)) = 0.5
		_Amount("Amount", Range(-1.0,1.0)) = 1
		_NoiseScale("Noise Scale",  Range(0, .5)) = .1
		_Height("Grass Height",  Range(0, 2)) = 1

		_TestSlider("Test Slider",  Range(0, 2)) = 1.

		_CollisionBending("Collision Bending (instanced)", Color) = (0,0,0,0)
	}

		SubShader
	{
		//Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }
		LOD 100

		ZWrite On



		//Tags{ "LightMode" = "ForwardBase" }
		//#pragma multi_compile_fwdbase



		//Blend SrcAlpha OneMinusSrcAlpha
		Cull Back

		Pass
	{
	CGPROGRAM

	#pragma vertex vert
	#pragma fragment frag
	#pragma target 3.0
	#pragma multi_compile_instancing
	//#pragma instancing_options assumeuniformscaling nolightmap // might save performance

	#include "AutoLight.cginc"

	#include "UnityCG.cginc"



	float hash(float n)
	{
		return frac(sin(n)*43758.5453);
	}

	float noise(float3 x)
	{
		// The noise function returns a value in the range -1.0f -> 1.0f

		float3 p = floor(x);
		float3 f = frac(x);

		f = f*f*(3.0 - 2.0*f);
		float n = p.x + p.y*57.0 + 113.0*p.z;

		return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
			lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
			lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
				lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
	}

	struct Input
	{
		//float3 worldPos; geht nicht ?!?! mit semantic "TEXCOORD0" ist es 0 ? 
		float4 vertex : POSITION;
		float localPos : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
		float localPos : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		SHADOW_COORDS(1)
	};

	float4 _TintColor1;
	float4 _TintColor2;
	float _CutoutThresh;
	float _Distance;
	float _Amplitude;
	float _Speed;
	float _Amount;
	float _NoiseScale;
	float _Height;
	float _ColorHeight;

	float _TestSlider;

	UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(fixed4, _CollisionBending)
	UNITY_INSTANCING_BUFFER_END(Props)

	v2f vert(Input input)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(input);
		//UNITY_TRANSFER_INSTANCE_ID(input, o); brauche ich im fragment shader nicht, also nicht transferen


		TRANSFER_SHADOW(o)


		fixed4 collisionBend = UNITY_ACCESS_INSTANCED_PROP(Props, _CollisionBending);

		input.vertex.y *= collisionBend.y;

		float3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
		o.localPos = input.vertex.y;

		float3 worldPosxz = worldPos;
		worldPosxz.y = 0;
		float heightNoise = (noise(worldPosxz * _NoiseScale) + 1. );
		float worldNoise = noise(worldPosxz * _NoiseScale + fixed3(1, 0, 0) * _Time.y) - .5;
		float worldNoise2 = noise(worldPosxz * _NoiseScale + fixed3(1, 0, 0) * _Time.y + fixed3(100,100,100)) - .5;
		//worldNoise = _Amount;
		input.vertex.y = input.vertex.y * heightNoise * _Height;
		float yy = input.vertex.y * input.vertex.y * _Amount;
		input.vertex.x += (worldNoise + collisionBend.x * 2.) * yy;
		input.vertex.z += (worldNoise2 + collisionBend.z * 2.) * yy;
		input.vertex.y -= input.vertex.y * (abs(collisionBend.x) + abs(collisionBend.z)) * .7;//(abs(collisionBend.x) + abs(collisionBend.y)) * yy;

		
		o.vertex = UnityObjectToClipPos(input.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = lerp(_TintColor1, _TintColor2, dot(_ColorHeight, i.localPos.x)); // local x pos is height

		float atten = SHADOW_ATTENUATION(i);

		//col.r = atten;
		//col.g = atten;
		//col.b = atten;

		col.a = 1;
		return col;
	}
		ENDCG
	}
	}
	FallBack "Diffuse"
}