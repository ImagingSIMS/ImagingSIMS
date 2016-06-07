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
	float4		MinClipCoords;				//16 x 1 = 16
	float4		MaxClipCoords;				//16 x 1 = 16
}

cbuffer LightingParams : register(b1)
{
	float4		AmbientLightColor;			//16 x 1 =  16
	float		AmbientLightIntensity;		// 4 x 1 =   4
	float		EnableAmbientLighting;		// 4 x 1 =   4
	float		EnablePointLighting;		// 4 x 1 =   4
	float		EnableSpecularLighting;		// 4 x 1 =   4
	float4		PointEnabled[8];			//16 x 8 = 128
	float4		PointLocation[8];			//16 x 8 = 128
	float4		PointColor[8];				//16 x 8 = 128	
	float4		PointIntensity[8];			//16 x 8 = 128
}

cbuffer HeightMapParams : register(b2)
{
	float		DepthScale;					// 4 x 1 =  4
	float		h_padding1;					// 4 x 1 =  4
	float		h_padding2;					// 4 x 1 =  4
	float		h_padding3;					// 4 x 1 =  4
}

static const uint numPointLights = 8;
static const float matEmissive = 0.1f;
static const float matAmbient = 0.3f;
static const float matDiffuse = 0.6f;

struct HEIGHTMAP_VS_Input
{
	float4 pos		: POSITION;
	float4 col		: COLOR;
};
struct HEIGHTMAP_PS_Input
{
	float4 pos		: SV_POSITION;
	float4 col		: COLOR;
};

HEIGHTMAP_PS_Input HEIGHTMAP_VS(HEIGHTMAP_VS_Input input)
{
	HEIGHTMAP_PS_Input output = (HEIGHTMAP_PS_Input)0;

	output.pos = mul(WorldProjView, input.pos);
	
	float4 materialColor = input.col;

	// http://www.3dgep.com/texturing-lighting-directx-11/
	// Color = emissive + ambient + sum(ambient[i] + diffuse[i] + specular[i])
	// No emissive
	// Settings for ambient (1) and diffused (8)

	float4 emissiveColor = materialColor * matEmissive;

	float4 sumDiffuseColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
	if (EnablePointLighting == 1.0f) {
		for (uint i = 0; i < numPointLights; i++)
		{
			if (PointEnabled[i].x == 1.0f) {
				float4 lightLocation = mul(WorldProjView, PointLocation[i]);

				float l = normalize(lightLocation - output.pos);
				// TODO: Include normal in VS input definition
				//float4 diffuse = max(0, dot(l, output.nor)) * PointColor[i] * PointIntensity[i].x * matDiffuse * materialColor;
				//sumDiffuseColor += diffuse;
			}
		}
	}

	// Ambient color
	float4 ambientColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
	if (EnableAmbientLighting == 1.0f) {
		ambientColor = matAmbient * materialColor * AmbientLightIntensity * AmbientLightColor;
	}

	output.col = float4((emissiveColor + ambientColor + sumDiffuseColor).rgb, materialColor.a);

	return output;
}
float4 HEIGHTMAP_PS(HEIGHTMAP_PS_Input input) : SV_Target
{
	return float4(input.col);
}