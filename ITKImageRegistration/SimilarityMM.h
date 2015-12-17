#pragma once
#include "IntensityRegistration.h"
#include "itkCenteredSimilarity2DTransform.h"

class SimilarityMM :
	public IntensityRegistration
{
public:
	SimilarityMM();
	~SimilarityMM();

	int DoRegistration();
};

