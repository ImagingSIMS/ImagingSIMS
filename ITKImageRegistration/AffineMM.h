#pragma once
#include "IntensityRegistration.h"
#include "itkAffineTransform.h"

class AffineMM :
	public IntensityRegistration
{
public:
	AffineMM();
	~AffineMM();

	int DoRegistration();
};

