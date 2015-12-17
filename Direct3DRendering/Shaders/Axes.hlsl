cbuffer RenderParams : register(b0)
{
	float4x4 WorldProjView;
}

struct AXES_VS_Input
{
	float4 pos		: POSITION;
	float4 col		: COLOR;
};
struct AXES_PS_Input
{
	float4 pos		: SV_POSITION;
	float4 col		: COLOR;
};

AXES_PS_Input AXES_VS(AXES_VS_Input input)
{
	AXES_PS_Input output = (AXES_PS_Input)0;

	output.pos = mul(WorldProjView, input.pos);
	output.col = input.col;

	return output;
}
float4 AXES_PS(AXES_PS_Input input) : SV_Target
{
	return input.col;
}