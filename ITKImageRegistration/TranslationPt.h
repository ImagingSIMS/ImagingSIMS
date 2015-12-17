#pragma once
#include "PointRegistration.h"
#include "itkTranslationTransform.h"

class TranslationPt :
	public PointRegistration
{
public:
	TranslationPt();
	~TranslationPt();

	int DoRegistration();
};

