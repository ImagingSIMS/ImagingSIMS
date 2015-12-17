#include "DataTransform.h"

using namespace std;

DataTransform::DataTransform()
{
	
}


DataTransform::~DataTransform()
{
}

int DataTransform::InitializeTransform(char* filePath){

	return loadArguments(filePath);
}
int DataTransform::loadArguments(char* filePath)
{
	// [0]:     (int)TransformType
	// [1]:     (int)Moving (1) or fixed (0) image
	// [2]:     (float)Translation X
	// [3]:     (float)Translation Y
	// [4]:     (float)Angle
	// [5]:     (float)Rotation center X
	// [6]:     (float)Rotation center Y
	// [7]:     (float)Scale
	// [8]:     (int)Data width
	// [9]:     (int)Data height
	// [10-n]:  (float)Data

	dataFilePath = filePath;
	ifstream file;
	float* rawData;

	try
	{
		file.open(filePath, ios::in | ios::binary);

		if (!file.is_open()){

			cout << "Could not open data file." << endl;
			return 0;
		}

		int transformTypeInt;
		file.read((char*)&transformTypeInt, sizeof(transformTypeInt));
		transformType = static_cast<ImageRegistrationTypes>(transformTypeInt);
		
		file.read((char*)&isMovingImage, sizeof(isMovingImage));
		
		file.read((char*)&translationX, sizeof(translationX));
		file.read((char*)&translationY, sizeof(translationY));
		file.read((char*)&angle, sizeof(angle));
		file.read((char*)&rotationCenterX, sizeof(rotationCenterX));
		file.read((char*)&rotationCenterY, sizeof(rotationCenterY));
		file.read((char*)&scale, sizeof(scale));
		file.read((char*)&dataWidth, sizeof(dataWidth));
		file.read((char*)&dataHeight, sizeof(dataHeight));

		cout << "Parameters read from pointer." << endl;
		cout << "Transform type: " << transformType << endl;
		cout << "IsMoving: " << isMovingImage << " Translation X: " << translationX << " Y: " << translationY << endl;
		cout << "Rotation Center X: " << rotationCenterX << " Y: " << rotationCenterY << " Angle: " << angle << endl;
		cout << "Scale: " << scale << endl;
		cout << "Data width:  " << dataWidth << " height: " << dataHeight << endl;


		int arraySize = dataWidth*dataHeight;
		rawData = new float[arraySize];
		
		for (int i = 0; i < arraySize; i++){

			file.read((char*)&rawData[i], sizeof(float));
		}

		file.close();
	}
	catch (exception ex)
	{
		cout << "Exception caught while converting arguments: " << ex.what() << endl;
		if (file.is_open())
		{
			file.close();
		}

		return 0;
	}

	cout << "Creating DataArray." << endl;
	dataArray = DataArrayType::New();
	cout << "DataArray created." << endl;

	cout << "Initializing DataArray." << endl;
	DataArrayType::SizeType imageSize;
	DataArrayType::RegionType region;
	DataArrayType::IndexType start;

	start[0] = 0;
	start[1] = 0;

	imageSize[0] = dataWidth;
	imageSize[1] = dataHeight;

	region.SetSize(imageSize);
	region.SetIndex(start);

	cout << "DataArray initialized." << endl;

	try
	{
		dataArray->SetRegions(region);
		dataArray->Allocate();
	}
	catch (exception ex)
	{
		cout << "Exception caught while allocating dataArray: " << ex.what() << endl;

		return 0;
	}

	try
	{
		for (unsigned int x = 0; x < imageSize[0]; x++){
			for (unsigned int y = 0; y < imageSize[1]; y++){

				DataArrayType::IndexType pixelIndex;
				pixelIndex[0] = x;
				pixelIndex[1] = y;

				int arrayIndex = (x*dataHeight) + y;
				dataArray->SetPixel(pixelIndex, rawData[arrayIndex]);
			}
		}
	}
	catch (exception ex)
	{
		cout << "Exception caught while populating dataArray: " << ex.what() << endl;

		return 0;
	}
	
	delete[] rawData;

	return 1;
}

int DataTransform::DoTransform()
{
	ResamplerType::Pointer resampleFilter = ResamplerType::New();

	InterpolatorType::Pointer interpolator = InterpolatorType::New();
	resampleFilter->SetInterpolator(interpolator);

	if (transformType == Translation){

		TranslationTransformType::Pointer transform = TranslationTransformType::New();

		TranslationTransformType::ParametersType translation = transform->GetParameters();
		translation[0] = translationX;
		translation[1] = translationY;

		transform->SetParameters(translation);

		resampleFilter->SetTransform(transform);
	}
	else if (transformType == CenterRigid2D){

		CenteredRigid2DTransformType::Pointer transform = CenteredRigid2DTransformType::New();

		CenteredRigid2DTransformType::OutputVectorType translation;
		translation[0] = translationX;
		translation[1] = translationY;
		transform->SetTranslation(translation);

		CenteredRigid2DTransformType::InputVectorType center;
		center[0] = rotationCenterX;
		center[1] = rotationCenterY;
		transform->SetCenter(center);

		transform->SetAngle(angle);

		resampleFilter->SetTransform(transform);
	}
	else if (transformType == CenterSimilarity){

		CenteredSimilarityTransformType::Pointer transform = CenteredSimilarityTransformType::New();

		CenteredSimilarityTransformType::OutputVectorType translation;
		translation[0] = translationX;
		translation[1] = translationY;
		transform->SetTranslation(translation);

		CenteredSimilarityTransformType::InputVectorType center;
		center[0] = rotationCenterX;
		center[1] = rotationCenterY;
		transform->SetCenter(center);

		transform->SetAngle(angle);

		transform->SetScale(scale);
		
		resampleFilter->SetTransform(transform);

	}

	// WARNING: Affine transform not implemented yet
	else if (transformType == Affine){

		AffineTransformType::Pointer transform = AffineTransformType::New();

		AffineTransformType::OutputVectorType translation;
		translation[0] = translationX;
		translation[1] = translationY;
		transform->SetTranslation(translation);

		AffineTransformType::InputVectorType center;
		center[0] = rotationCenterX;
		center[1] = rotationCenterY;
		transform->SetCenter(center);

		//transform->SetAngle(angle);

		//transform->SetScale(scale);

		resampleFilter->SetTransform(transform);
	}
	else if (transformType == NoRegistration){

		cout << "Invalid transform type (NoRegistration)." << endl;
		return 0;
	}
	else{

		cout << "Invalid transform type." << endl;
		return 0;
	}

	if (isMovingImage){
		
		resampleFilter->SetInput(dataArray);

		resampleFilter->SetSize(dataArray->GetLargestPossibleRegion().GetSize());
		resampleFilter->SetOutputSpacing(dataArray->GetSpacing());
		resampleFilter->SetOutputOrigin(dataArray->GetOrigin());
		resampleFilter->SetOutputDirection(dataArray->GetDirection());

		resampleFilter->Update();
		transformed = resampleFilter->GetOutput();
	}

	else{

		DataArrayType::Pointer mask = createBlankImageMask(dataArray->GetLargestPossibleRegion().GetSize());

		resampleFilter->SetInput(mask);

		resampleFilter->SetSize(mask->GetLargestPossibleRegion().GetSize());
		resampleFilter->SetOutputOrigin(mask->GetOrigin());
		resampleFilter->SetOutputSpacing(mask->GetSpacing());
		resampleFilter->SetOutputDirection(mask->GetDirection());

		resampleFilter->Update();
		transformed = dataArray;
		multiplyImageMask(transformed, resampleFilter->GetOutput());
	}

	return 1;
}

int DataTransform::FinalizeTransform()
{
	// Replace data in array passed from C# with transformed data

	ofstream file;
	file.open(dataFilePath, ios::out | ios::binary);

	if (!file.is_open()){

		cout << "Could not open data file to save." << endl;
		return 0;
	}

	try
	{
		int transformTypeInt = static_cast<int>(transformType);

		cout << "Writing function parameters to file." << endl;
		file.write((char*)&transformType, sizeof(transformTypeInt));
		file.write((char*)&isMovingImage, sizeof(isMovingImage));

		cout << "Writing transform parameters to file." << endl;
		file.write((char*)&translationX, sizeof(translationX));
		file.write((char*)&translationY, sizeof(translationY));
		file.write((char*)&angle, sizeof(translationY));
		file.write((char*)&rotationCenterX, sizeof(translationY));
		file.write((char*)&rotationCenterY, sizeof(translationY));
		file.write((char*)&scale, sizeof(translationY));

		cout << "Writing data information to file." << endl;
		file.write((char*)&dataWidth, sizeof(translationY));
		file.write((char*)&dataHeight, sizeof(translationY));

		cout << "Writing transformed data to file." << endl;
		for (int x = 0; x < dataWidth; x++){
			for (int y = 0; y < dataHeight; y++){

				DataArrayType::IndexType pixelIndex;
				pixelIndex[0] = x;
				pixelIndex[1] = y;

				float value = transformed->GetPixel(pixelIndex);
				file.write((char*)&value, sizeof(value));
			}
		}

		cout << "Data written to file." << endl;
		file.close();
	}
	catch (exception ex)
	{
		cout << "Could not write to file. " << ex.what() << endl;

		if (file.is_open())
		{
			file.close();
		}

		return 0;
	}
	
	return 1;
}

DataArrayType::Pointer DataTransform::createBlankImageMask(DataArrayType::SizeType imageSize)
{
	DataArrayType::Pointer image = DataArrayType::New();
	DataArrayType::RegionType region;
	DataArrayType::IndexType start;
	start[0] = 0;
	start[1] = 0;

	region.SetSize(imageSize);
	region.SetIndex(start);

	image->SetRegions(region);
	image->Allocate();

	for (unsigned int x = 0; x < imageSize[0]; x++){
		for (unsigned int y = 0; y < imageSize[1]; y++){

			DataArrayType::IndexType pixelIndex;
			pixelIndex[0] = x;
			pixelIndex[1] = y;

			image->SetPixel(pixelIndex, 1);
		}
	}

	return image;
}

void DataTransform::multiplyImageMask(DataArrayType::Pointer image, DataArrayType::Pointer mask)
{
	DataArrayType::SizeType size = mask->GetLargestPossibleRegion().GetSize();

	for (int x = 0; x < size[0]; x++){
		for (int y = 0; y < size[1]; y++){

			DataArrayType::IndexType pixelIndex;
			pixelIndex[0] = x;
			pixelIndex[1] = y;

			float value = image->GetPixel(pixelIndex) * mask->GetPixel(pixelIndex);
			image->SetPixel(pixelIndex, value);
		}
	}
}
