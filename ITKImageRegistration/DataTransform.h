#pragma once
#include "Registration.h"

#include "itkTranslationTransform.h"
#include "itkCenteredRigid2DTransform.h"
#include "itkCenteredSimilarity2DTransform.h"
#include "itkAffineTransform.h"

#include <stdio.h>


class DataTransform
{
public:
	DataTransform();
	~DataTransform();

	typedef itk::LinearInterpolateImageFunction<DataArrayType, float>						InterpolatorType;
	typedef itk::ResampleImageFilter<DataArrayType, DataArrayType, float, float>			ResamplerType;

	typedef itk::TranslationTransform<float, 2>												TranslationTransformType;
	typedef itk::CenteredRigid2DTransform<float>											CenteredRigid2DTransformType;
	typedef itk::CenteredSimilarity2DTransform<float>										CenteredSimilarityTransformType;
	typedef itk::AffineTransform<float, 2>													AffineTransformType;

	int		InitializeTransform(char* filePath);
	int		DoTransform();
	int		FinalizeTransform();

protected:
	const int				dataOffset = 10;

	char*					dataFilePath;

	int						transformType;
	int						isMovingImage;
	float					translationX;
	float					translationY;
	float					angle;
	float					rotationCenterX;
	float					rotationCenterY;
	float					scale;
	unsigned int			dataWidth;
	unsigned int			dataHeight;

	DataArrayType::Pointer	dataArray;
	DataArrayType::Pointer	transformed;

	int loadArguments(char* filePath);

	DataArrayType::Pointer createBlankImageMask(DataArrayType::SizeType imageSize);
	void multiplyImageMask(DataArrayType::Pointer image, DataArrayType::Pointer mask);
};

