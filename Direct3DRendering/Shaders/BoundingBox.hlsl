cbuffer RenderParams : register(b0)
{
	float4x4 WorldProjView;
}

struct BBOX_VS_Input
{
	float4 pos		: POSITION;
	float4 col		: COLOR;
};
struct BBOX_PS_Input
{
	float4 pos		: SV_POSITION;
	float4 col		: COLOR;
};

BBOX_PS_Input BBOX_VS(BBOX_VS_Input input)
{
	BBOX_PS_Input output = (BBOX_PS_Input)0;

	output.pos = mul(WorldProjView, input.pos);
	output.col = input.col;

	return output;
}
float4 BBOX_PS(BBOX_PS_Input input) : SV_Target
{
	return input.col;
}