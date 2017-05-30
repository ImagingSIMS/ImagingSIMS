import numpy as np

from skimage import color
from skimage import io

import pywt as pywt

import Helpers as helpers

import colorsys as color
import math
import scipy.misc

def loadInputImages():

    highres = io.imread("C:\\Data\\Fusion Particles\\Fusion Comparison\\highres.bmp", as_grey=True)
    lowRes = io.imread("C:\\Data\\Fusion Particles\\Fusion Comparison\\lowres.bmp")    

    return highres, lowRes

def doHSVFusion():

    highres, lowres = loadInputImages()

    # Scale lowres to [0, 1] -- highres is already in range from imread
    # lowres = np.divide(lowres, 255)

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)
    # Scale lowres to [0, 1] -- highres is already in range from imread
    lowresResized = np.divide(lowresResized, 255)

    fused = np.zeros([hrSizeX, hrSizeY, 3])
    H = np.zeros([hrSizeX, hrSizeY])
    S = np.zeros([hrSizeX, hrSizeY])
    V = np.zeros([hrSizeX, hrSizeY])

    for x in range(hrSizeX):
        for y in range(hrSizeY):            
            H[x,y],S[x,y],V[x,y] = color.rgb_to_hsv(lowresResized[x,y,0], lowresResized[x,y,1], lowresResized[x,y,2])


    V = helpers.matchHistogram(highres, V)

    for x in range(hrSizeX):
        for y in range(hrSizeY):

            r,g,b = color.hsv_to_rgb(H[x,y], S[x,y], V[x,y])
            fused[x,y,:] = [r,g,b]

    io.imsave("C:\\Data\\Fusion Particles\\Fusion Comparison\\fused_hsv.bmp", fused)

def doHSLFusion():
    # Note: HSL is the typical abbreviation but the colorsys fuction switches the position of s and l (HLS)

    highres, lowres = loadInputImages()

    # Scale lowres to [0, 1] -- highres is already in range from imread
    # lowres = np.divide(lowres, 255)

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)
    # Scale lowres to [0, 1] -- highres is already in range from imread
    lowresResized = np.divide(lowresResized, 255)

    fused = np.zeros([hrSizeX, hrSizeY, 3])
    H = np.zeros([hrSizeX, hrSizeY])
    L = np.zeros([hrSizeX, hrSizeY])
    S = np.zeros([hrSizeX, hrSizeY])

    for x in range(hrSizeX):
        for y in range(hrSizeY):            
            H[x,y],L[x,y],S[x,y] = color.rgb_to_hls(lowresResized[x,y,0], lowresResized[x,y,1], lowresResized[x,y,2])

    L = helpers.matchHistogram(highres, L)

    for x in range(hrSizeX):
        for y in range(hrSizeY):

            r,g,b = color.hls_to_rgb(H[x,y], L[x,y], S[x,y])
            fused[x,y,:] = [r,g,b]

    io.imsave("C:\\Data\\Fusion Particles\\Fusion Comparison\\fused_hsl.bmp", fused)

def doADWTFusion():

    highres, lowres = loadInputImages()

    # Scale lowres to [0, 1] -- highres is already in range from imread
    # lowres = np.divide(lowres, 255)

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)
    # Scale lowres to [0, 1] -- highres is already in range from imread
    lowresResized = np.divide(lowresResized, 255)

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

    highres = helpers.matchHistogram(highres, I)
    
    io.imsave("C:\\Data\\Fusion Particles\\Fusion Comparison\\panmatched.bmp", highres)
    io.imsave("C:\\Data\\Fusion Particles\\Fusion Comparison\\intensity.bmp", I)
    io.imsave("C:\\Data\\Fusion Particles\\Fusion Comparison\\v1.bmp", v1)
    io.imsave("C:\\Data\\Fusion Particles\\Fusion Comparison\\v2.bmp", v2)

    decomp1 = pywt.wavedec2(highres, 'haar', level=1)
    decomp2 = pywt.wavedec2(highres, 'haar', level=2)

    y1 = helpers.upscaleImage(decomp1[0], highres)
    y2 = helpers.upscaleImage(decomp2[0], highres)

    iprime = I + y1 + y2
    iprime = np.divide(iprime, np.max(iprime))
    for x in range(hrSizeX):
        for y in range(hrSizeY):
            fused[x,y,:] = helpers.ivv2rgb([iprime[x,y], v1[x,y], v2[x,y]])

    fusedmax = np.max(fused)
    fusedmin = np.min(fused)

    io.imsave("C:\\Data\\Fusion Particles\\Fusion Comparison\\fused_adwt.bmp", fused)

def doSDWTFusion():

    highres, lowres = loadInputImages()

    # Scale lowres to [0, 1] -- highres is already in range from imread
    # lowres = np.divide(lowres, 255)

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)
    # Scale lowres to [0, 1] -- highres is already in range from imread
    lowresResized = np.divide(lowresResized, 255)

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

    highres = helpers.matchHistogram(highres, I)

    pdecomp1 = pywt.wavedec2(highres, 'haar', level=1)
    idecomp1 = pywt.wavedec2(I, 'haar', level=1)
    pdecomp2 = pywt.wavedec2(highres, 'haar', level=2)
    idecomp2 = pywt.wavedec2(I, 'haar', level=2)

    py1 = helpers.upscaleImage(pdecomp1[0], highres)
    iy1 = helpers.upscaleImage(idecomp1[0], highres)
    py2 = helpers.upscaleImage(pdecomp2[0], highres)
    iy2 = helpers.upscaleImage(idecomp2[0], highres)

    iprime = iy2 + py1 + py2
    iprime = np.divide(iprime, np.max(iprime))
    for x in range(hrSizeX):
        for y in range(hrSizeY):
            fused[x,y,:] = helpers.ivv2rgb([iprime[x,y], v1[x,y], v2[x,y]])

    fusedmax = np.max(fused)
    fusedmin = np.min(fused)

    io.imsave("C:\\Data\\Fusion Particles\\Fusion Comparison\\fused_sdwt.bmp", fused)

def doPCAFusion():

    highres, lowres = loadInputImages()

    # Scale lowres to [0, 1] -- highres is already in range from imread
    # lowres = np.divide(lowres, 255)

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)
    # Scale lowres to [0, 1] -- highres is already in range from imread
    lowresResized = np.divide(lowresResized, 255)

    highRes = np.reshape(highres, [hrSizeX * hrSizeY])

    fused = np.zeros([hrSizeX, hrSizeY, 3])
    # ->Scores, ..., Loadings [m x m]
    u, sigma, coeff = scipy.sparse.linalg.svds(array, 3, which='LM')    
    scores = np.multiply(u, np.matlib.repmat(sigma.transpose(), sizeX * sizeY, 1))

    # 0: i, 1: loadings, 2: scores, 3: coeffs
    pcs = [(i, np.abs(sigma[i]), scores[:,i], coeff[i]) for i in range(len(sigma))]
    pcs.sort(key=lambda x: x[1], reverse=True)

    pc1 = pcs[0][2]
    highRes = helpers.matchHistogram(highRes, pc1)
    pcs[0][2] = highRes

    

if __name__ == "__main__":
    
    # doHSVFusion()
    # doHSLFusion()
    # doADWTFusion()
    # doSDWTFusion()
    doPCAFusion()

    #ivv = helpers.rgb2ivv([0.5, 0.25, 1.0])
    #print(ivv)

    #rgb = [0.25, 0.5, 1.0]
    #print(rgb)
    #ivv = helpers.rgb2ivv(rgb)
    #print(ivv)
    #reverted = helpers.ivv2rgb(ivv)
    #print(reverted)