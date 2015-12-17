#pragma once
#include "IntensityRegistration.h"
#include "itkTranslationTransform.h"

class TranslationStd :
	public IntensityRegistration
{
public:
	TranslationStd();
	~TranslationStd();

	int DoRegistration();
};

