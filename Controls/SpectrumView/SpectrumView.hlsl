Texture2D<float>		txSpectrum : register(t0);

cbuffer Spectrum : register(b0)
{
	float	StartMass;
	float	EndMass;
	float	StartTime;
	float	EndTime;
	float	MassSlope;
	float	MassInt;
}