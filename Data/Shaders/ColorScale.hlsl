Texture2D<float> Buffer0 : register(t0);
RWTexture2D<float4> BufferOut : register(u0);

cbuffer RenderParams : register(b0)
{
    float4 RenderColor;
    float DataMaximum;
    float Saturation;
    float Threshold;
    float SizeX;
    float SizeY;
    float SizeZ;
    float buffer_1;
    float buffer_2;
}

static const float Zero = 0.0;
static const float One = 1.0;
static const float OneThird = 0.333333;
static const float TwoThirds = 0.666666;
static const float OneQuarter = 0.25;
static const float OneHalf = 0.5;
static const float ThreeQuarters = 0.75;
static const float OneFifth = 0.2;
static const float TwoFifths = 0.4;
static const float ThreeFifths = 0.6;
static const float FourFifths = 0.8;
static const float OneEighth = 0.125;
static const float ThreeEighths = 0.375;
static const float FiveEighths = 0.625;
static const float SevenEighths = 0.875;
static const float OneSeventh = 0.142857;
static const float TwoSevenths = 0.285714;
static const float ThreeSevenths = 0.428571;
static const float FourSevenths = 0.571429;
static const float FiveSevenths = 0.714286;
static const float SixSevenths = 0.857143;

static const float NumStepsThermalWarm = 3.0f;
static const float NumStepsThermalCold = 3.0f;
static const float NumStepsNeon = 4.0f;
static const float NumStepsRetro = 4.0f;
static const float NumStepsGreenWhite = 2.0f;
static const float NumStepsRedWhite = 2.0f;
static const float NumStepsBlueWhite = 2.0f;
static const float NumStepsMagentaWhite = 2.0f;
static const float NumStepsYellowWhite = 2.0f;
static const float NumStepsCyanWhite = 2.0f;
static const float NumStepsRainbow = 7.0f;
static const float NumStepsHeatMap = 3.0f;
static const float NumStepsViridis = 8.0f;
static const float NumStepsRedBlackGreen = 2.0f;
static const float NumStepsRedBlackBlue = 2.0f;
static const float NumStepsGreenBlackBlue = 2.0f;

static const float4 ThermalWarm[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(1.0f, 0.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)  
};
static const float4 ThermalCold[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 0.0f, 1.0f, 1.0f),
    float4(0.0f, 1.0f, 1.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 Neon[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 1.0f, 1.0f, 1.0f),
    float4(1.0f, 0.0f, 1.0f, 1.0f),
    float4(1.0f, 1.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 Retro[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(1.0f, 0.0f, 1.0f, 1.0f),
    float4(0.0f, 1.0f, 1.0f, 1.0f),
    float4(1.0f, 1.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 RedWhite[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(1.0f, 0.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 GreenWhite[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 1.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 BlueWhite[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 0.0f, 1.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 MagentaWhite[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(1.0f, 0.0f, 1.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 YellowWhite[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 CyanWhite[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 1.0f, 1.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)
};
static const float4 Rainbow[] =
{
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 0.0f, 1.0f, 1.0f),
    float4(0.0f, 1.0f, 1.0f, 1.0f),
    float4(0.0f, 1.0f, 0.0f, 1.0f),
    float4(1.0f, 1.0f, 0.0f, 1.0f),
    float4(1.0f, 0.0f, 0.0f, 1.0f),
    float4(1.0f, 0.0f, 1.0f, 1.0f),
    float4(1.0f, 1.0f, 1.0f, 1.0f)  
};
static const float4 HeatMap[] =
{
    float4(0.0f, 0.0f, 1.0f, 1.0f),
    float4(0.0f, 1.0f, 1.0f, 1.0f),
    float4(1.0f, 1.0f, 0.0f, 1.0f),
    float4(1.0f, 0.0f, 0.0f, 1.0f)
};
static const float4 Viridis[] =
{
    float4(0.267f, 0.003f, 0.329f, 1.0f),
    float4(0.278f, 0.173f, 0.478f, 1.0f),
    float4(0.231f, 0.318f, 0.545f, 1.0f),
    float4(0.173f, 0.443f, 0.557f, 1.0f),
    float4(0.129f, 0.565f, 0.553f, 1.0f),
    float4(0.153f, 0.678f, 0.506f, 1.0f),
    float4(0.388f, 0.794f, 0.388f, 1.0f),
    float4(0.667f, 0.863f, 0.196f, 1.0f),
    float4(0.992f, 0.906f, 0.125f, 1.0f)
};
static const float4 RedBlackGreen[] =
{
    float4(1.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 1.0f, 0.0f, 1.0f)
};
static const float4 RedBlackBlue[] =
{
    float4(1.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 0.0f, 1.0f, 1.0f)
};
static const float4 GreenBlackBlue[] =
{
    float4(0.0f, 1.0f, 0.0f, 1.0f),
    float4(0.0f, 0.0f, 0.0f, 1.0f),
    float4(0.0f, 0.0f, 1.0f, 1.0f)
};

uint PackColor(float4 color)
{
    color *= 255;
    return int(color.r) | int(color.g) << 8 | int(color.b) << 16 | int(color.a) << 24;
}

[numthreads(32, 32, 1)]
void CS_RenderThermalWarmImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsThermalWarm;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = ThermalWarm[index];
    float4 endColor = ThermalWarm[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);

}
[numthreads(32, 32, 1)]
void CS_RenderThermalColdImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsThermalCold;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = ThermalCold[index];
    float4 endColor = ThermalCold[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderNeonImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsNeon;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = Neon[index];
    float4 endColor = Neon[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderRetroImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsRetro;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = Retro[index];
    float4 endColor = Retro[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderGrayImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    BufferOut[loc] = float4(float3(value, value, value), 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderRedWhiteImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsRedWhite;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = RedWhite[index];
    float4 endColor = RedWhite[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderGreenWhiteImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsGreenWhite;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = GreenWhite[index];
    float4 endColor = GreenWhite[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderBlueWhiteImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsBlueWhite;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = BlueWhite[index];
    float4 endColor = BlueWhite[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderMagentaWhiteImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsMagentaWhite;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = MagentaWhite[index];
    float4 endColor = MagentaWhite[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderYellowWhiteImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsYellowWhite;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = YellowWhite[index];
    float4 endColor = YellowWhite[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderCyanWhiteImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsCyanWhite;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = CyanWhite[index];
    float4 endColor = CyanWhite[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderRainbowImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsRainbow;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = Rainbow[index];
    float4 endColor = Rainbow[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderHeatMapImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsHeatMap;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = HeatMap[index];
    float4 endColor = HeatMap[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderViridisImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsViridis;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    uint index = floor(value * numColorSteps);
    float4 startColor = Viridis[index];
    float4 endColor = Viridis[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, value * numColorSteps - index).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderRedBlackGreenImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsRedBlackGreen;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), -1.0f);

    float abs_value = abs(value);
    abs_value = value < 0 ? 1 - abs_value : abs_value;
    uint index = value < 0 ? 0 : 1;

    float4 startColor = RedBlackGreen[index];
    float4 endColor = RedBlackGreen[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, abs_value).rgb, 1.0f);

}
[numthreads(32, 32, 1)]
void CS_RenderRedBlackBlueImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsRedBlackBlue;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), -1.0f);

    float abs_value = abs(value);
    abs_value = value < 0 ? 1 - abs_value : abs_value;
    uint index = value < 0 ? 0 : 1;

    float4 startColor = RedBlackBlue[index];
    float4 endColor = RedBlackBlue[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, abs_value).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderGreenBlackBlueImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float numColorSteps = NumStepsGreenBlackBlue;
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), -1.0f);

    float abs_value = abs(value);
    abs_value = value < 0 ? 1 - abs_value : abs_value;
    uint index = value < 0 ? 0 : 1;

    float4 startColor = GreenBlackBlue[index];
    float4 endColor = GreenBlackBlue[clamp(index + 1, 0, numColorSteps)];

    BufferOut[loc] = float4(lerp(startColor, endColor, abs_value).rgb, 1.0f);
}
[numthreads(32, 32, 1)]
void CS_RenderSolidColorImage(uint3 dispatchThreadID : SV_DispatchThreadID)
{
    float2 loc = float2(dispatchThreadID.x, dispatchThreadID.y);
    float value = Buffer0[loc];

    value = value * step(Threshold, value);
    value /= Saturation;
    value = max(min(value, 1.0f), 0.0f);

    BufferOut[loc] = float4(RenderColor.rgb * value, 1.0f);
}

// "C:\Program Files (x86)\Windows Kits\10\bin\x64\fxc.exe" ComputeShader.hlsl /Fo cs.cso /T cs_5_0 /E CS_RenderImage