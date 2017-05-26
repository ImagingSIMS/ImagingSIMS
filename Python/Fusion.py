import numpy as np

from skimage import color
from skimage import io

import pywt

import Helpers as helpers

import colorsys as color
import math

def loadInputImages():

    highres = io.imread("D:\\Data\\FusionComparison\\highres.bmp", as_grey=True)
    lowRes = io.imread("D:\\Data\\FusionComparison\\lowres.bmp")    

    return highres, lowRes

def doHSVFusion():

    highres, lowres = loadInputImages()

    # Scale lowres to [0, 1] -- highres is already in range from imread
    lowres = np.divide(lowres, 255)

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)

    fused = np.zeros([hrSizeX, hrSizeY, 3])
    H = np.zeros([hrSizeX, hrSizeY])
    S = np.zeros([hrSizeX, hrSizeY])
    V = np.zeros([hrSizeX, hrSizeY])

    for x in range(hrSizeX):
        for y in range(hrSizeY):            
            H[x,y],S[x,y],V[x,y] = color.rgb_to_hsv(lowresResized[x,y,0], lowresResized[x,y,1], lowresResized[x,y,2])


    V = helpers.matchHistogram(highres, np.divide(V, 255))

    for x in range(hrSizeX):
        for y in range(hrSizeY):

            r,g,b = color.hsv_to_rgb(H[x,y], S[x,y], V[x,y])
            fused[x,y,:] = [r,g,b]

    io.imsave("D:\\Data\\FusionComparison\\fused_hsv.bmp", fused)

def doHSLFusion():
    # Note: HSL is the typical abbreviation but the colorsys fuction switches the position of s and l (HLS)

    highres, lowres = loadInputImages()

    # Scale lowres to [0, 1] -- highres is already in range from imread
    lowres = np.divide(lowres, 255)

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)

    fused = np.zeros([hrSizeX, hrSizeY, 3])
    H = np.zeros([hrSizeX, hrSizeY])
    L = np.zeros([hrSizeX, hrSizeY])
    S = np.zeros([hrSizeX, hrSizeY])

    for x in range(hrSizeX):
        for y in range(hrSizeY):            
            H[x,y],L[x,y],S[x,y] = color.rgb_to_hls(lowresResized[x,y,0], lowresResized[x,y,1], lowresResized[x,y,2])

    meanL = np.mean(L)
    L = helpers.matchHistogram(highres, np.divide(L, 255))
    meanML = np.mean(L)

    for x in range(hrSizeX):
        for y in range(hrSizeY):

            r,g,b = color.hsv_to_rgb(H[x,y], L[x,y], S[x,y])
            fused[x,y,:] = [r,g,b]

    io.imsave("D:\\Data\\FusionComparison\\fused_hsl.bmp", fused)

def doADWTFusion():

    highres, lowres = loadInputImages()

    # Scale lowres to [0, 1] -- highres is already in range from imread
    lowres = np.divide(lowres, 255)

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)

    fused = np.zeros([hrSizeX, hrSizeY, 3])
    I = np.zeros([hrSizeX, hrSizeY])
    v1 = np.zeros([hrSizeX, hrSizeY])
    v2 = np.zeros([hrSizeX, hrSizeY])

    for x in range(hrSizeX):
        for y in range(hrSizeY):
            ivv = helpers.rgb2ivv(lowresResized[x,y,:])
            I[x,y] = ivv[0]
            I[x,y] = ivv[1]
            I[x,y] = ivv[2]



if __name__ == "__main__":
    
    # doHSVFusion()
    # doHSLFusion()
    # doADWTFusion()

    #ivv = helpers.rgb2ivv([0.5, 0.25, 1.0])
    #print(ivv)

    rgb = [0.25, 0.5, 1.0]
    print(rgb)
    ivv = helpers.rgb2ivv(rgb)
    print(ivv)
    reverted = helpers.ivv2rgb(ivv)
    print(reverted)