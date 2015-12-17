#pragma once
#include "PointRegistration.h"
#include "itkAffineTransform.h"

class AffinePt :
	public PointRegistration
{
public:
	AffinePt();
	~AffinePt();

	int DoRegistration();
};

