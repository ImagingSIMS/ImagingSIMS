#include <stdlib.h>

enum ImageRegistrationTypes { NoRegistration = 0, Translation = 1, CenterRigid2D = 2, CenterSimilarity = 3, Affine = 4};
enum DenoiseMethodTypes { NoDenoise = 0, CurvatureFlow = 1, DiscreteGaussian = 2, GradientAnisotropicDiffusion = 3 };

struct RegistrationParameters{
	ImageRegistrationTypes	RegType;
	char*					FixedImagePath;
	char*					MovingImagePath;
	char*					OutputFixedImagePath;
	char*					OutputMovingImagePath;
	char*					FixedPointListPath;
	char*					MovingPointListPath;
	char*					DenoisedFixedPath;
	char*					DenoisedMovingPath;
	int						MaximumIterations;
	double					Angle;
	double					Scale;
	int						NumberBins;
	int						NumberSamples;
	double					MaxStepLength;
	double					MinStepLength;
	int						MultiModal;
	int						UseAllPixels;
	double					RelaxationFactor;
	int						PointBased;
	double					GradientTolerance;
	double					ValueTolerance;
	double					EpsilonFunction;
	double					TranslationX;
	double					TranslationY;
	double					ROIFixedLeft;
	double					ROIFixedTop;
	double					ROIFixedRight;
	double					ROIFixedBottom;
	double					ROIMovingLeft;
	double					ROIMovingTop;
	double					ROIMovingRight;
	double					ROIMovingBottom;
	int						DenoiseImages;
	DenoiseMethodTypes		DenoiseMethod;
	int						UseCenterForRotation;

	RegistrationParameters(){

	}

	//Argument layout:
	//[0]: (char*)	Executable name
	//[1]: (int)	Regsitration method enum
	//[2]: (char*)	Fixed image file name
	//[3]: (char*)	Moving image file name
	//[4]: (char*)	Output fixed image file name
	//[5]: (char*)	Output moving image file name
	//[6]: (char*)	Fixed point list file name
	//[7]: (char*)  Moving point list file name
	//[8]: (char*)	Denoised fixed image path
	//[9]: (char*)	Denoised moving image path
	//[10]:(int)	Maximum number of iterations
	//[11]:(double)	Rotational registration angle guess
	//[12]:(double) Scaled registration scale guess
	//[13]:(int)	Multi modal registration
	//[14]:(int)	Number bins for multi modal
	//[15]:(int)	Number samples for multi modal
	//[16]:(double) Max step length
	//[17]:(double) Min step length
	//[18]:(int)	Use all pixels for multi modal registrations
	//[19]:(double) Relaxation factor
	//[20]:(int)	Point based registration
	//[21]:(double) Gradient tolerance
	//[22]:(double) Value tolerance
	//[23]:(double) Epsilon function
	//[24]:(double) Initial translation X guess
	//[25]:(double) Initial translation Y guess
	//[26]:(double) Fixed image ROI left
	//[27]:(double)					top
	//[28]:(double)					right
	//[29]:(double)					bottom
	//[30]:(double)	Moving image ROI left
	//[31]:(double)					 top
	//[32]:(double)					 right
	//[33]:(double)					 bottom
	//[34]:(int)	Denoise images before registration
	//[35]:(int)	Denoise method
	//[36]:(int)	Use center of image for rotation center

	RegistrationParameters(char* args[]){
		RegType = static_cast<ImageRegistrationTypes>(atoi(args[1]));
		FixedImagePath = args[2];
		MovingImagePath = args[3];
		OutputFixedImagePath = args[4];
		OutputMovingImagePath = args[5];
		FixedPointListPath = args[6];
		MovingPointListPath = args[7];
		DenoisedFixedPath = args[8];
		DenoisedMovingPath = args[9];
		MaximumIterations = atoi(args[10]);
		Angle = atof(args[11]);
		Scale = atof(args[12]);
		MultiModal = atoi(args[13]);
		NumberBins = atoi(args[14]);
		NumberSamples = atoi(args[15]);
		MaxStepLength = atof(args[16]);
		MinStepLength = atof(args[17]);
		UseAllPixels = atoi(args[18]);
		RelaxationFactor = atof(args[19]);
		PointBased = atoi(args[20]);
		GradientTolerance = atof(args[21]);
		ValueTolerance = atof(args[22]);
		EpsilonFunction = atof(args[23]);
		TranslationX = atof(args[24]);
		TranslationY = atof(args[25]);
		ROIFixedLeft = atof(args[26]);
		ROIFixedTop = atof(args[27]);
		ROIFixedRight = atof(args[28]);
		ROIFixedBottom = atof(args[29]);
		ROIMovingLeft = atof(args[30]);
		ROIMovingTop = atof(args[31]);
		ROIMovingRight = atof(args[32]);
		ROIMovingBottom = atof(args[33]);
		DenoiseImages = atoi(args[34]);
		DenoiseMethod = static_cast<DenoiseMethodTypes>(atoi(args[35]));
		UseCenterForRotation = atoi(args[36]);
	}
};