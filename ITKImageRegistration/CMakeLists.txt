cmake_minimum_required(VERSION 2.8 FATAL_ERROR)
project(ITKImageRegistration)

find_package(ITK REQUIRED)
include(${ITK_USE_FILE})

add_executable(ITKImageRegistration main.cxx RegistrationParameters.h CommandIterationUpdate.h CommandIterationUpdate.cpp Registration.h Registration.cpp IntensityRegistration.h IntensityRegistration.cpp PointRegistration.h PointRegistration.cpp DataTransform.h DataTransform.cpp AffineStd.h AffineStd.cpp CenterRigidMM.h CenterRigidMM.cpp CenterRigidStd.h CenterRigidStd.cpp CenterRigidPt.h CenterRigidPt.cpp SimilarityMM.h SimilarityMM.cpp SimilarityStd.h SimilarityStd.cpp SimilarityPt.h SimilarityPt.cpp TranslationMM.h TranslationMM.cpp TranslationStd.h TranslationStd.cpp TranslationPt.h TranslationPt.cpp AffineStd.h AffineStd.cpp AffineMM.h AffineMM.cpp AffinePt.h AffinePt.cpp)

target_link_libraries(ITKImageRegistration ${ITK_LIBRARIES})