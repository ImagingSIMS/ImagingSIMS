Texture2D<float> InputScalarData : register(t0);
RWTexture2D<uint> OutputBuffer : register(u0);

cbuffer RenderParams : register(b0)
{
    float4 RenderColor;
    float DatMaximum;
    float Saturation;
    float Threshold;
    float SizeX;
    float SizeY;
    float empty_1;
    float empty_2;
    float empty_3;
}

uint PackColor(float4 color)
{
    color *= 255;
    return int(color.r) | int(color.g) << 8 | int(color.b) << 16 | int(color.a) << 24;
}

[numthreads(32, 32, 1)]
void CS_RenderSolidColorImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = InputScalarData[loc] / Saturation;
    if(value >= Threshold) 
        OutputBuffer[loc] = PackColor(float4(RenderColor.rgb * value, 1.0f));
    else
        OutputBuffer[loc] = PackColor(float4(0.0f, 0.0f, 0.0f, 1.0f));

}