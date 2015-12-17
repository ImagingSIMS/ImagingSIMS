#include "Registration.h"

Registration::Registration()
{
}


Registration::~Registration()
{
}

void Registration::InitializeRegistration(RegistrationParameters registrationParams){
	
	regParams = registrationParams;

	fixedImageReader = RGBImageReaderType::New();
	movingImageReader = RGBImageReaderType::New();
	fixedImageWriter = RGBImageWriterType::New();
	movingImageWriter = RGBImageWriterType::New();

	fixedImageReader->SetFileName(regParams.FixedImagePath);
	movingImageReader->SetFileName(regParams.MovingImagePath);
	fixedImageWriter->SetFileName(regParams.OutputFixedImagePath);
	movingImageWriter->SetFileName(regParams.OutputMovingImagePath);

	fixedImageReader->Update();
	movingImageReader->Update();

	cout << "Input images loaded." << endl;

	colorFixedImage = fixedImageReader->GetOutput();
	colorMovingImage = movingImageReader->GetOutput();


	if (regParams.DenoiseImages)
	{
		denoiseImages(regParams.DenoiseMethod);
	}
	else
	{
		grayFixedImage = RGBToGray(colorFixedImage);
		grayMovingImage = RGBToGray(colorMovingImage);
	}

	observer = CommandIterationUpdate::New();
}

void Registration::FinalizeRegistration()
{
	fixedImageWriter->SetInput(finalFixedImage);
	movingImageWriter->SetInput(finalMovingImage);
	fixedImageWriter->Update();
	movingImageWriter->Update();

	cout << "Output images saved." << endl;
}

GrayImageType::Pointer Registration::RGBToGray(RGBImageType::Pointer rgb)
{
	GrayImageType::Pointer gray = GrayImageType::New();

	RGBImageType::RegionType region = rgb->GetLargestPossibleRegion();
	RGBImageType::SizeType size = region.GetSize();

	int width = size[0];
	int height = size[1];

	gray->SetRegions(region);
	gray->Allocate();

	for (int x = 0; x < width; x++)
	{
		for (int y = 0; y < height; y++)
		{
			RGBImageType::IndexType indexRGB;
			indexRGB[0] = x;
			indexRGB[1] = y;

			GrayImageType::IndexType indexGray;
			indexGray[0] = x;
			indexGray[1] = y;

			unsigned char r = rgb->GetPixel(indexRGB).GetRed();
			unsigned char g = rgb->GetPixel(indexRGB).GetGreen();
			unsigned char b = rgb->GetPixel(indexRGB).GetBlue();

			unsigned char grayValue = (r + b + g) / 3.;

			gray->SetPixel(indexGray, grayValue);
		}
	}

	return gray;
}
RGBImageType::Pointer Registration::GrayToRGB(GrayImageType::Pointer gray)
{
	RGBImageType::Pointer rgb = RGBImageType::New();

	GrayImageType::RegionType region = gray->GetLargestPossibleRegion();
	GrayImageType::SizeType size = region.GetSize();

	int width = size[0];
	int height = size[1];

	rgb->SetRegions(region);
	rgb->Allocate();

	for (size_t x = 0; x < width; x++)
	{
		for (size_t y = 0; y < height; y++)
		{
			RGBImageType::IndexType indexRGB;
			indexRGB[0] = x;
			indexRGB[1] = y;

			GrayImageType::IndexType indexGray;
			indexGray[0] = x;
			indexGray[1] = y;

			unsigned char grayValue = gray->GetPixel(indexGray);

			RGBPixelType rgbPixel;
			rgbPixel.SetRed(grayValue);
			rgbPixel.SetGreen(grayValue);
			rgbPixel.SetBlue(grayValue);

			rgb->SetPixel(indexRGB, rgbPixel);			
		}
	}

	return rgb;
}
GrayImageType::Pointer Registration::CreateBlankImageMask(RGBImageType::SizeType imageSize)
{
	GrayImageType::Pointer image = GrayImageType::New();
	GrayImageType::RegionType region;
	GrayImageType::IndexType start;
	start[0] = 0;
	start[1] = 0;

	region.SetSize(imageSize);
	region.SetIndex(start);

	image->SetRegions(region);
	image->Allocate();

	for (unsigned int x = 0; x < imageSize[0]; x++){
		for (unsigned int y = 0; y < imageSize[1]; y++){
			GrayImageType::IndexType pixelIndex;
			pixelIndex[0] = x;
			pixelIndex[1] = y;

			image->SetPixel(pixelIndex, 1);
		}
	}

	return image;
}
void Registration::MultiplyRGBImages(RGBImageType::Pointer image, GrayImageType::Pointer mask)
{
	GrayImageType::SizeType size = mask->GetLargestPossibleRegion().GetSize();

	for (int x = 0; x < size[0]; x++){
		for (int y = 0; y < size[1]; y++){
			RGBImageType::IndexType pixelIndex;
			pixelIndex[0] = x;
			pixelIndex[1] = y;

			RGBPixelType imagePixel = image->GetPixel(pixelIndex);
			unsigned char maskValue = mask->GetPixel(pixelIndex);
			RGBPixelType finalPixel;

			finalPixel.SetRed(imagePixel.GetRed() * maskValue);
			finalPixel.SetGreen(imagePixel.GetGreen() * maskValue);
			finalPixel.SetBlue(imagePixel.GetBlue() * maskValue);

			image->SetPixel(pixelIndex, finalPixel);
		}
	}
}

void Registration::TransformImages(const itk::ResampleImageFilter<GrayImageType, GrayImageType, double, double>::TransformType *finalTransform,
	RGBImageType::Pointer colorMovingImage, RGBImageType::Pointer colorFixedImage)
{
	if (!finalFixedImage)finalFixedImage = RGBImageType::New();
	if (!finalMovingImage)finalMovingImage = RGBImageType::New();

	// Resample moving image to the fixed image space
	ResampleColorFilterType::Pointer resamplerMoving = ResampleColorFilterType::New();
	resamplerMoving->SetInput(colorMovingImage);
	resamplerMoving->SetSize(colorMovingImage->GetLargestPossibleRegion().GetSize());
	resamplerMoving->SetOutputOrigin(colorMovingImage->GetOrigin());
	resamplerMoving->SetOutputSpacing(colorMovingImage->GetSpacing());
	resamplerMoving->SetOutputDirection(colorMovingImage->GetDirection());
	resamplerMoving->SetTransform(finalTransform);
	resamplerMoving->Update();
	finalMovingImage = resamplerMoving->GetOutput();

	// Since there are pixels in the fixed image that won't map to the space of the
	// resampled moving image, I create a blank image and pass it through the resampler.
	// The result is an image that pixels that do map are 1 and those that don't are 0
	// which is multiplied against the fixed image. Pixels that don't map are thus
	// black in the final fixed image.
	GrayImageType::Pointer fixedMask = CreateBlankImageMask(colorFixedImage->GetLargestPossibleRegion().GetSize());
	ResampleGrayFilterType::Pointer resamplerMask = ResampleGrayFilterType::New();
	resamplerMask->SetInput(fixedMask);
	resamplerMask->SetSize(fixedMask->GetLargestPossibleRegion().GetSize());
	resamplerMask->SetOutputOrigin(fixedMask->GetOrigin());
	resamplerMask->SetOutputSpacing(fixedMask->GetSpacing());
	resamplerMask->SetOutputDirection(fixedMask->GetDirection());
	resamplerMask->SetTransform(finalTransform);
	resamplerMask->Update();
	MultiplyRGBImages(colorFixedImage, resamplerMask->GetOutput());
	finalFixedImage = colorFixedImage;
}

bool Registration::hasROIDefined(RegistrationParameters* parameters, GrayImageType::Pointer inputImage)
{
	GrayImageType::SizeType imageSize = inputImage->GetLargestPossibleRegion().GetSize();
	double width = imageSize[0];
	double height = imageSize[1];

	double left = parameters->ROIFixedLeft;
	double right = parameters->ROIFixedRight;
	double top = parameters->ROIFixedTop;
	double bottom = parameters->ROIFixedBottom;

	if (left == 0 && top == 0 &&
		right == width && bottom == height)
		return false;

	else return true;
}

void Registration::denoiseImages(DenoiseMethodTypes denoiseMethod)
{
	GrayImageType::Pointer grayFixedInput = RGBToGray(colorFixedImage);
	GrayImageType::Pointer grayMovingInput = RGBToGray(colorMovingImage);

	switch (denoiseMethod)
	{
	case CurvatureFlow:
		grayFixedImage = denoiseCurvatureFlow(grayFixedInput);
		grayMovingImage = denoiseCurvatureFlow(grayMovingInput);
		break;
	case DiscreteGaussian:
		grayFixedImage = denoiseDiscreteGaussian(grayFixedInput);
		grayMovingImage = denoiseDiscreteGaussian(grayMovingInput);
		break;
	case GradientAnisotropicDiffusion:
		grayFixedImage = denoiseGradientAnisotropicDiffusion(grayFixedInput);
		grayMovingImage = denoiseGradientAnisotropicDiffusion(grayMovingInput);
		break;
	case NoDenoise:
		cout << "WARNING: No denoise method specified. Defaulting to non-denoised preprocessing." << endl;
		grayFixedImage = grayFixedInput;
		grayMovingImage = grayMovingInput;
		break;
	default:
		cout << "WARNING: No denoise method specified. Defaulting to non-denoised preprocessing." << endl;
		grayFixedImage = grayFixedInput;
		grayMovingImage = grayMovingInput;
		break;
	}

	RGBImageWriterType::Pointer grayFixedWriter = RGBImageWriterType::New();
	grayFixedWriter->SetFileName(regParams.DenoisedFixedPath);
	grayFixedWriter->SetInput(GrayToRGB(grayFixedImage));
	grayFixedWriter->Update();

	RGBImageWriterType::Pointer grayMovingWriter = RGBImageWriterType::New();
	grayMovingWriter->SetFileName(regParams.DenoisedMovingPath);
	grayMovingWriter->SetInput(GrayToRGB(grayMovingImage));
	grayMovingWriter->Update();
}

GrayImageType::Pointer Registration::denoiseCurvatureFlow(GrayImageType::Pointer input)
{
	typedef itk::CurvatureFlowImageFilter<GrayImageType, GrayImageType> FilterType;
	FilterType::Pointer smoothing = FilterType::New();

	const int numIterations = 5;
	const double timeStep = 0.125;

	smoothing->SetInput(input);
	smoothing->SetNumberOfIterations(numIterations);
	smoothing->SetTimeStep(timeStep);
	smoothing->Update();

	return smoothing->GetOutput();
}

GrayImageType::Pointer Registration::denoiseDiscreteGaussian(GrayImageType::Pointer input)
{
	typedef itk::DiscreteGaussianImageFilter<GrayImageType, GrayImageType> FilterType;
	FilterType::Pointer smoothing = FilterType::New();

	// Default values from http://math.nist.gov/mcsd/savg/software/filters/smooth/
	const double variance = 30;
	const double maxKernel = 0.5;

	smoothing->SetInput(input);
	smoothing->SetVariance(variance);
	smoothing->SetMaximumKernelWidth(maxKernel);
	smoothing->Update();

	return smoothing->GetOutput();
}
GrayImageType::Pointer Registration::denoiseGradientAnisotropicDiffusion(GrayImageType::Pointer input)
{
	typedef itk::GradientAnisotropicDiffusionImageFilter<GrayImageType, GrayImageType> FilterType;
	FilterType::Pointer smoothing = FilterType::New();

	// Default valuse from http://math.nist.gov/mcsd/savg/software/filters/smooth/
	const double conductance = 1.0;
	const int numIterations = 5;
	const double timeStep = 0.125;

	smoothing->SetInput(input);
	smoothing->SetNumberOfIterations(numIterations);
	smoothing->SetTimeStep(timeStep);
	smoothing->SetConductanceParameter(conductance);
	smoothing->Update();

	return smoothing->GetOutput();
}