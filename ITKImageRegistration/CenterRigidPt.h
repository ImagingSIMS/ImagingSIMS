#pragma once
#include "PointRegistration.h"
#include "itkCenteredRigid2DTransform.h"

class CenterRigidPt :
	public PointRegistration
{
public:
	CenterRigidPt();
	~CenterRigidPt();

	int DoRegistration();
};

