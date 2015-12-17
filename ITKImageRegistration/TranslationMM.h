#pragma once
#include "IntensityRegistration.h"
#include "itkTranslationTransform.h"

class TranslationMM :
	public IntensityRegistration
{
public:
	TranslationMM();
	~TranslationMM();

	int DoRegistration();
};

