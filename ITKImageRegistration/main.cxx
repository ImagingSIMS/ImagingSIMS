#include "TranslationStd.h"
#include "CenterRigidStd.h"
#include "SimilarityStd.h"
#include "TranslationMM.h"
#include "CenterRigidMM.h"
#include "SimilarityMM.h"
#include "TranslationPt.h"
#include "CenterRigidPt.h"
#include "SimilarityPt.h"
#include "AffineStd.h"
#include "AffineMM.h"
#include "AffinePt.h"
#include "DataTransform.h"

using namespace std;

//Function prototypes
int doRegistration(int argc, char* argv[]);
int doTransform(int argc, char* argv[]);
char* getImageRegistrationType(ImageRegistrationTypes regType);
const char* getRegistrationParamsString(RegistrationParameters *params);

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

int main(int argc, char *argv[])
{	
	cout << "Registration started." << endl;
	cout << "Number of parameters passed: " << argc << endl;

	if (argc == 2){

		return doTransform(argc, argv);
	}

	if (argc == 37){
		
		return doRegistration(argc, argv);
	}

	else{
		cout << "Invalid number of parameters." << endl;
		return -1;
	}


}

int doRegistration(int argc, char* argv[]){

	RegistrationParameters params = RegistrationParameters(argv);
	// Write out parameter values to ensure proper passing of values via command line arguments
	cout << getRegistrationParamsString(&params);

	cout << endl;

	// Declare base Registration variable
	Registration* reg;

	// Determine via RegistrationParameters the desired registration type

	// Check if point based
	if (params.PointBased)
	{
		switch (params.RegType)
		{
		case Translation:
			reg = new TranslationPt();
			break;
		case CenterRigid2D:
			reg = new CenterRigidPt();
			break;
		case CenterSimilarity:
			reg = new SimilarityPt();
			break;
		case Affine:
			reg = new AffinePt();
			// Affine transform not implemented yet
			return -10;
		default:
			cout << "Invalid registration type." << endl;
			return -10;
		}
	}
	// If not point based, it's intensity based
	else
	{
		// Check for multimodal registration
		if (params.MultiModal)
		{
			switch (params.RegType)
			{
			case Translation:
				reg = new TranslationMM();
				break;
			case CenterRigid2D:
				reg = new CenterRigidMM();
				break;
			case CenterSimilarity:
				reg = new SimilarityMM();
				break;
			case Affine:
				reg = new AffineMM();
				// Affine transform not implemented yet
				return -10;
			default:
				cout << "Invalid registration type." << endl;
				return -10;
			}
		}
		// If not, it's standard registration
		else
		{
			switch (params.RegType)
			{
			case Translation:
				reg = new TranslationStd();
				break;
			case CenterRigid2D:
				reg = new CenterRigidStd();
				break;
			case CenterSimilarity:
				reg = new SimilarityStd();
				break;
			case Affine:
				reg = new AffineStd();
				// Affine transform not implemented yet
				return -10;
			default:
				cout << "Invalid registration type." << endl;
				return -10;
			}
		}
	}

	// Write out scales
	// [0]: Translation
	// [1]: Rotation
	// [2]: Scaling
	double* scales = reg->GetScales();
	cout << "Translation scale: " << scales[0] << " Rotation scale: " << scales[1] << " Scaling scale: " << scales[2] << endl;

	// Initialize and perform registration
	reg->InitializeRegistration(params);

	// Do registration and check to see if it succeeded
	if (reg->DoRegistration())
	{
		// If succeeded, finalize registration
		reg->FinalizeRegistration();
		return 0;
	}
	// If registration failed, return exit code for failure
	else return -5;
}

int doTransform(int argc, char* argv[]){

	cout << "Starting transform function." << endl;

	cout << "Creating DataTransform object." << endl;
	DataTransform* dataTransform = new DataTransform();
	cout << "DataTransform object created." << endl;

	cout << "Initializing transform." << endl;
	if (!dataTransform->InitializeTransform(argv[1]))
	{
		return -15;
	}

	cout << "Performing transform." << endl;
	if (!dataTransform->DoTransform())
	{
		return -16;
	}
	cout << "Transform complete." << endl;

	cout << "Finalizing transform" << endl;
	if (!dataTransform->FinalizeTransform())
	{
		return -17;
	}

	return 0;
}

char* getImageRegistrationType(ImageRegistrationTypes regType)
{
	switch (regType){
	case NoRegistration:
		return "No Registration";
	case Translation:
		return "Translation";
	case CenterRigid2D:
		return "CenterRigid2D";
	case CenterSimilarity:
		return "CenterSimilarity";
	case Affine:
		return "Affine";
	default:
		return "empty";
	}
}
const char* getRegistrationParamsString(RegistrationParameters *params)
{
	const string newLine = "\n";

	string value;

	value += "Registration type: ";
	value += getImageRegistrationType(params->RegType);
	value += newLine;

	value += "Max Iterations: ";
	value += params->MaximumIterations;
	value += " Max Step Length: ";
	value += params->MaxStepLength;
	value += " Min Step Length: ";
	value += params->MinStepLength;
	value += newLine;

	value += "Point Based: ";
	if (params->PointBased)
	{
		value += " Value Tolerance: ";
		value += params->ValueTolerance;
		value += " Gradient Tolerance: ";
		value += params->GradientTolerance;
		value += " Epsilon Function: ";
		value += params->EpsilonFunction;
	}
	value += newLine;

	if (params->MultiModal)
	{
		value += " Bins: ";
		value += params->NumberBins;
		value += " Samples: ";
		value += params->NumberSamples;
		value += " Use All Pixels: ";
		value += params->UseAllPixels;
	}
	value += newLine;

	value += "Translation: (";
	value += params->TranslationX;
	value += ", ";
	value += params->TranslationY;
	value += ")";
	value += newLine;

	if (params->RegType == CenterSimilarity || params->RegType == CenterRigid2D || params->RegType == Affine)
	{
		value += "Rotation: ";
		value += params->Angle;
	}
	if (params->RegType == CenterSimilarity || params->RegType == Affine)
	{
		value += "Scale: ";
		value += params->Scale;
		value += newLine;
	}

	value += "Relaxation Factor: ";
	value += params->RelaxationFactor;
	value += newLine;
	
	return value.c_str();
}