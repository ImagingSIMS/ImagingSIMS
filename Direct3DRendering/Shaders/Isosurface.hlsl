// https://developer.amd.com/wordpress/media/2012/10/Chapter9-Real-Time_Isosurface_Extraction.pdf

static const uint maxNumVolumes = 8;

Texture3D<float>		txVolumes[maxNumVolumes];

cbuffer RenderParams : register(b0)
{
	float4x4	WorldProjView;				//64 x 1 =  64
	float2		InvWindowSize;				// 8 x 1 =   8
	float		Brightness;					// 4 x 1 =   4
	float		ClipDistance;				// 4 x 1 =   4
	float3		CameraPosition;				//12 x 1 =  12
	float		r_padding0;					// 4 x 1 =   4
	float3		CameraDirection;			//12 x 1 =  12
	float		r_padding1;					// 4 x 1 =   4
	float3		CameraUp;					//12 x 1 =  12
	float		r_padding2;					// 4 x 1 =   4
	float4		NearClipPlane;				//16 x 1 =  16
	float4		FarClipPlane;				//16 x 1 =  16
	float4		MinClipCoords;				//16 x 1 =  16
	float4		MaxClipCoords;				//16 x 1 =  16
}

cbuffer IsosurfaceParams : register(b1)
{
	float4		IsosurfaceColor[8];			//16 x 8 = 128
	float		NumIsosurfaces;				// 4 x 1 =   4
	float		i_padding0;					// 4 x 1 =   4
	float		i_padding1;					// 4 x 1 =   4
	float		i_padding2;					// 4 x 1 =   4
}

struct ISOSURFACE_VS_Input{

};

struct ISOSURFACE_VS_Output {

};

ISOSURFACE_VS_Output ISOSURFACE_VS(ISOSURFACE_VS_Input input) {
	ISOSURFACE_VS_Output output = (ISOSURFACE_VS_Output)0;

}