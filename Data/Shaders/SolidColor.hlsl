Texture2D<float>    Buffer0     : register(t0);
RWTexture2D<uint>   BufferOut   : register(u0);

cbuffer RenderParams            : register(b0)
{
    float4 RenderColor;
    float DataMaximum;
    float Saturation;
    float Threshold;
    float SizeX;
    float SizeY;
    float padding_1;
    float padding_2;
    float padding_3;
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

    float value = Buffer0[loc] / Saturation;
    if(value >=  Threshold)
    {
        //BufferOut[loc] = PackColor(float4(RenderColor.rgb * value, 1.0f));
        BufferOut[loc] = PackColor(float4(loc.x / SizeX, 0.0f, loc.y / SizeY, 1.0f));
    }
    else
    {
        BufferOut[loc] = PackColor(float4(0.0f, 0.0f, 0.0f, 1.0f));
    }
}