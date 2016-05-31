Texture2D<float4>	txPositionFront : register(t0);
Texture2D<float4>	txPositionBack : register(t1);

Texture3D<float>	txActVoxels : register(t2);

Texture3D<float>	txVolume1 : register(t3);
Texture3D<float>	txVolume2 : register(t4);
Texture3D<float>	txVolume3 : register(t5);
Texture3D<float>	txVolume4 : register(t6);
Texture3D<float>	txVolume5 : register(t7);
Texture3D<float>	txVolume6 : register(t8);
Texture3D<float>	txVolume7 : register(t9);
Texture3D<float>	txVolume8 : register(t10);

SamplerState		samplerLinear : register(s0);

cbuffer RenderParams : register(b0)
{
	float4x4	WorldProjView;				//64 x 1 = 64
	float2		InvWindowSize;				// 8 x 1 =  8
	float		Brightness;					// 4 x 1 =  4
	float		ClipDistance;				// 4 x 1 =  4
	float3		CameraPosition;				//12 x 1 = 12
	float		r_padding0;					// 4 x 1 =  4
	float3		CameraDirection;			//12 x 1 = 12
	float		r_padding1;					// 4 x 1 =  4
	float3		CameraUp;					//12 x 1 = 12
	float		r_padding2;					// 4 x 1 =  4
	float4		NearClipPlane;				//16 x 1 = 16
	float4		FarClipPlane;				//16 x 1 = 16
}

cbuffer VolumeParams : register(b1)
{
	float4		VolumeScaleStart;			//16 x 1 =   4
	float4		VolumeColor[8];				//16 x 8 = 128
	float4		VolumeScale;				//16 x 1 =	16
	float4		VolumeScaleDenominator;		//16 x 1 =  16
	uint		NumVolumes;					// 4 x 1 =   4
	float		v_padding0;					// 4 x 1 =   4
	float		v_padding1;					// 4 x 1 =   4
	float		v_padding2;					// 4 x 1 =   4
}

cbuffer IsosurfaceParams : register(b2)
{
	float4		IsosurfaceColor[8];			//16 x 8 = 128
	float		NumIsosurfaces;				// 4 x 1 =   4
	float		i_padding0;					// 4 x 1 =   4
	float		i_padding1;					// 4 x 1 =   4
	float		i_padding2;					// 4 x 1 =   4
}

static const uint maxVolumes = 8;
static const uint maxIterations = 256;
static const float stepSize = sqrt(3.f) / maxIterations;

//Struct definitions

struct MODEL_VS_Input
{
	float4 pos : POSITION;
};
struct MODEL_PS_Input
{
	float4 pos : SV_POSITION;
	float4 tex : TEXCOORD0;
};

struct RAYCAST_VS_Input
{
	float4 pos : POSITION;
};
struct RAYCAST_PS_Input
{
	float4 pos : SV_POSITION;
};

struct ISOSURFACE_VS_Input
{
	float4 pos : POSITION;
	float4 nor : NORMAL;
};
struct ISOSURFACE_PS_Input
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float4 nor : NORMAL;
	float id : SURFACEID;
};

//Functions
float3 ScaleVector(float3 inputVector)
{
	float x = (inputVector.x - VolumeScaleStart.x) / VolumeScaleDenominator.x;
	float y = (inputVector.y - VolumeScaleStart.y) / VolumeScaleDenominator.y;
	float z = (inputVector.z - VolumeScaleStart.z) / VolumeScaleDenominator.z;

	return float3(x, y, z) * VolumeScale.xyz;
}

//MODEL
MODEL_PS_Input MODEL_VS(MODEL_VS_Input input)
{
	MODEL_PS_Input output = (MODEL_PS_Input)0;
	output.pos = mul(WorldProjView, input.pos);

	output.tex = 0.5 * (input.pos + 1);

	return output;
}
float4 MODEL_PS(MODEL_PS_Input input)		: SV_Target
{
	float4 pos = input.pos / 3;

	return input.tex;
}

//RAYCAST
RAYCAST_PS_Input RAYCAST_VS(RAYCAST_VS_Input input)
{
	RAYCAST_PS_Input output = (RAYCAST_PS_Input)0;
	output.pos = mul(WorldProjView, input.pos);

	return output;
}

float4 RAYCAST_PS(RAYCAST_PS_Input input) : SV_TARGET
{
	//Current pixel location on screen
	float2 tex = input.pos.xy * InvWindowSize;

	//Read cube front and back face positions from texture
	float3 pos_front = txPositionFront.Sample(samplerLinear, tex).xyz;
	float3 pos_back = txPositionBack.Sample(samplerLinear, tex).xyz;

	//Direction vector
	float3 dir = normalize(pos_back - pos_front);

	//Single step
	float3 step = stepSize * dir;

	//Current position
	float3 v = pos_front;

	//Accumulate result: value and transparency
	float4 accumulatedColor = float4(0, 0, 0, 0);

	for (uint i = 0; i < maxIterations; i++)
	{
		float3 scaledVector = ScaleVector(v);
		for (uint j = 0; j < maxVolumes; j++)
		{
			float intensity = 0;

			if (j >= NumVolumes) break;

			if (j == 0)intensity = txVolume1.Sample(samplerLinear, scaledVector).r;
			if (j == 1)intensity = txVolume2.Sample(samplerLinear, scaledVector).r;
			if (j == 2)intensity = txVolume3.Sample(samplerLinear, scaledVector).r;
			if (j == 3)intensity = txVolume4.Sample(samplerLinear, scaledVector).r;
			if (j == 4)intensity = txVolume5.Sample(samplerLinear, scaledVector).r;
			if (j == 5)intensity = txVolume6.Sample(samplerLinear, scaledVector).r;
			if (j == 6)intensity = txVolume7.Sample(samplerLinear, scaledVector).r;
			if (j == 7)intensity = txVolume8.Sample(samplerLinear, scaledVector).r;

			intensity *= Brightness;
			intensity *= VolumeColor[j].a;

			float4 sampledColor = float4(VolumeColor[j].rgb * intensity, intensity);

			if (intensity > 0 && accumulatedColor.a <= 0.95)
			{
				float oneMinusAlpha = 1.0 - accumulatedColor.a;

				accumulatedColor.r += oneMinusAlpha * sampledColor.r;
				accumulatedColor.g += oneMinusAlpha * sampledColor.g;
				accumulatedColor.b += oneMinusAlpha * sampledColor.b;
				accumulatedColor.a += oneMinusAlpha * sampledColor.a;
			}
		}
		v += step;
	}

	return accumulatedColor;
}

//ISOSURFACE
ISOSURFACE_PS_Input ISOSURFACE_VS(ISOSURFACE_VS_Input input)
{
	ISOSURFACE_PS_Input output = (ISOSURFACE_PS_Input)0;
	output.pos = mul(WorldProjView, input.pos);	

	output.id = input.nor.w;

	output.col = IsosurfaceColor[(int)output.id];

	output.nor = input.nor;

	return output;
}
float4 ISOSURFACE_PS(ISOSURFACE_PS_Input input) : SV_TARGET
{
	// TODO: Lighting calculation with input.nor
	
	return float4(input.col.rgb * Brightness, input.col.a);
	//return float4(input.col.rgb, 1.0f);
	/*if (input.id == 0.0f)
	{
		return float4(1.0f, 0.0f, 0.0f, 1.0f);
	}
	else if (input.id == 1.0f) {
		return float4(0.0f, 1.0f, 0.0f, 1.0f);
	}
	else if (input.id == 2.0f) {
		return float4(0.0f, 0.0f, 1.0f, 1.0f);
	}
	else if (input.id == 3.0f) {
		return float4(1.0f, 0.0f, 1.0f, 1.0f);
	}
	else return float4(1.0f, 1.0f, 1.0f, 1.0f);*/
	//return float4(0.25f, 0.5f, 0.75f, 1.0f);
}