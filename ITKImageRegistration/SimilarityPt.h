#pragma once
#include "PointRegistration.h"
#include "itkCenteredSimilarity2DTransform.h"

class SimilarityPt :
	public PointRegistration
{
public:
	SimilarityPt();
	~SimilarityPt();

	virtual int DoRegistration();
};

