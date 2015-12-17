#include "itkImageRegistrationMethod.h"
#include "itkMeanSquaresImageToImageMetric.h"
#include "itkGradientDescentOptimizer.h"
#include "itkCenteredTransformInitializer.h"
#include "itkImageFileReader.h"
#include "itkImageFileWriter.h"
#include "itkCropImageFilter.h"
#include "itkRegionOfInterestImageFilter.h"
#include "itkExtractImageFilter.h"
#include "itkResampleImageFilter.h"
#include "itkVectorResampleImageFilter.h"
#include "itkMutualInformationImageToImageMetric.h"
#include "itkGradientDescentOptimizer.h"
#include "itkNormalizeImageFilter.h"
#include "itkDiscreteGaussianImageFilter.h"
#include "itkMattesMutualInformationImageToImageMetric.h"
#include "itkCastImageFilter.h"
#include "itkRescaleIntensityImageFilter.h"
#include "itkSubtractImageFilter.h"
#include "itkMatchCardinalityImageToImageMetric.h"

#include "itkCurvatureFlowImageFilter.h"
#include "itkDiscreteGaussianImageFilter.h"
#include "itkGradientAnisotropicDiffusionImageFilter.h"

#include "itkMath.h"

#include "CommandIterationUpdate.h"
#include "RegistrationParameters.h"

#include <stdbool.h>

using namespace std;

#pragma once
const unsigned int													Dimension = 2;
//typedef unsigned char												GrayPixelType;
typedef float														GrayPixelType;
typedef itk::RGBPixel<unsigned char>								RGBPixelType;
typedef itk::Image<GrayPixelType, Dimension>						GrayImageType;
typedef itk::Image<RGBPixelType, Dimension>							RGBImageType;
typedef itk::Image<float>											DataArrayType;

class Registration
{
public:
	// typedefs for itk types
	typedef itk::ImageFileReader<RGBImageType>							RGBImageReaderType;
	typedef itk::ImageFileWriter<RGBImageType>							RGBImageWriterType;
	typedef itk::ImageFileWriter<GrayImageType>							GrayImageWriterType;

	typedef itk::ResampleImageFilter<RGBImageType, RGBImageType>		ResampleColorFilterType;
	typedef itk::ResampleImageFilter<GrayImageType, GrayImageType>		ResampleGrayFilterType;

	Registration();
	~Registration();

	virtual void	InitializeRegistration(RegistrationParameters registrationParams);
	virtual int		DoRegistration() { return 0; }
	virtual void	FinalizeRegistration();

	double*			GetScales(){
		return new double[3]{translationScale, rotationScale, scaleScale};
	};

protected:
	
	// Member variables
	const char* ResultsLineStarter = "Registration results: ";

	const double scaleScale = 1.0;
	const double rotationScale = 1.0;
	const double translationScale = 1.0 / 100.0;

	RegistrationParameters			regParams;
	RGBImageReaderType::Pointer		fixedImageReader;
	RGBImageReaderType::Pointer		movingImageReader;
	RGBImageWriterType::Pointer		fixedImageWriter;
	RGBImageWriterType::Pointer		movingImageWriter;

	GrayImageType::Pointer			grayFixedImage;
	GrayImageType::Pointer			grayMovingImage;
	RGBImageType::Pointer			colorFixedImage;
	RGBImageType::Pointer			colorMovingImage;

	RGBImageType::Pointer			finalFixedImage;
	RGBImageType::Pointer			finalMovingImage;

	// Function prototypes
	GrayImageType::Pointer			RGBToGray(RGBImageType::Pointer rgb);
	RGBImageType::Pointer			GrayToRGB(GrayImageType::Pointer gray);
	GrayImageType::Pointer			CreateBlankImageMask(RGBImageType::SizeType imageSize);
	void							MultiplyRGBImages(RGBImageType::Pointer image, GrayImageType::Pointer mask);
	void							TransformImages(const itk::ResampleImageFilter<GrayImageType, GrayImageType, double, double>::TransformType *finalTransform,
											RGBImageType::Pointer colorMovingImage, RGBImageType::Pointer colorFixedImage);
	bool							hasROIDefined(RegistrationParameters* parameters, GrayImageType::Pointer inputImage);

	CommandIterationUpdate::Pointer	observer;

private:

	// Denoising function prototypes
	void							denoiseImages(DenoiseMethodTypes denoiseMethod);
	GrayImageType::Pointer			denoiseCurvatureFlow(GrayImageType::Pointer input);
	GrayImageType::Pointer			denoiseDiscreteGaussian(GrayImageType::Pointer input);
	GrayImageType::Pointer			denoiseGradientAnisotropicDiffusion(GrayImageType::Pointer input);
};

