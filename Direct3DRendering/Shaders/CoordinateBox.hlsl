cbuffer RenderParams : register(b0)
{
	float4x4	WorldProjView;
	float		Transparency;
	float		r_padding1;
	float		r_padding2;
	float		r_padding3;
}

struct CBOX_VS_Input
{
	float4 pos		: POSITION;
	float4 col		: COLOR;
};
struct CBOX_PS_Input
{
	float4 pos		: SV_POSITION;
	float4 col		: COLOR;
};

CBOX_PS_Input CBOX_VS(CBOX_VS_Input input)
{
	CBOX_PS_Input output = (CBOX_PS_Input)0;

	output.pos = mul(WorldProjView, input.pos);
	output.col = input.col;

	return output;
}
float4 CBOX_PS(CBOX_PS_Input input) : SV_Target
{
	return float4(input.col.rgb, Transparency);
}