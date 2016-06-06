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
	float4		AmbientLightColor;			//16 x 1 = 16
	float		AmbientLightIntensity;		// 4 x 1 =  4
	float		EnableAmbientLighting;		// 4 x 1 =  4
	float		EnableDirectionalLighting;	// 4 x 1 =  4
	float		EnableSpecularLighting;		// 4 x 1 =  4
}

cbuffer HeightMapParams : register(b2)
{
	float		DepthScale;					// 4 x 1 =  4
	float		h_padding1;					// 4 x 1 =  4
	float		h_padding2;					// 4 x 1 =  4
	float		h_padding3;					// 4 x 1 =  4
}

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
	output.col = input.col;

	return output;
}
float4 HEIGHTMAP_PS(HEIGHTMAP_PS_Input input) : SV_Target
{
	//return float4(input.col.rgb * Brightness, input.col.a);
	return float4(input.col);
}