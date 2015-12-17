#pragma once
#include "IntensityRegistration.h"
#include "itkCenteredRigid2DTransform.h"

class CenterRigidStd
	: public IntensityRegistration
{
public:
	CenterRigidStd();
	~CenterRigidStd();

	int DoRegistration();
};

