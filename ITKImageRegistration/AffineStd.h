#pragma once
#include "IntensityRegistration.h"
#include "itkAffineTransform.h"

class AffineStd :
	public IntensityRegistration
{
public:
	AffineStd();
	~AffineStd();

	int DoRegistration();
};

