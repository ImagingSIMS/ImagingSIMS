#pragma once
#include "IntensityRegistration.h"
#include "itkCenteredSimilarity2DTransform.h"

class SimilarityStd :
	public IntensityRegistration
{
public:
	SimilarityStd();
	~SimilarityStd();

	int DoRegistration();
};

