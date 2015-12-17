#pragma once
#include "IntensityRegistration.h"
#include "itkCenteredRigid2DTransform.h"

class CenterRigidMM :
	public IntensityRegistration
{
public:
	CenterRigidMM();
	~CenterRigidMM();

	int DoRegistration();
};

