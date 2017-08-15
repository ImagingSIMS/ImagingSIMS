import numpy as np

import scipy.ndimage.filters as filters
from skimage import color
from skimage import io

import pywt as pywt

import Helpers as helpers

import colorsys as color
import math
import scipy.misc
import matplotlib as mpl
from matplotlib.mlab import PCA as pca

folder = "D:\\Data\\FusionComparison\\8-15-17\\"
# folder = "C:\\Data\\FusionComparison\\8-15-17\\"

def loadInputImages():

    highres = io.imread(folder + "grid_highres.bmp", as_grey=True)
    lowRes = io.imread(folder + "grid_lowres.bmp")    

    # Scale lowres to [0, 1] -- highres is already in range from imread
    return highres, np.divide(lowRes, 255)

def doWeightedAverageFusion():

    highres, lowres = loadInputImages()

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

    L = helpers.matchHistogram(highres, L)

    meanL = np.mean(L)
    meanHr = np.mean(highres)
    meanSum = meanL + meanHr

    for x in range(hrSizeX):
        for y in range(hrSizeY):
            wa = (L[x,y] * meanL / meanSum) + (highres[x,y] * meanHr / meanSum)
            r,g,b = color.hls_to_rgb(H[x,y], wa, S[x,y])
            fused[x,y,:] = [r,g,b]

    io.imsave(folder + "fused_wa.bmp", fused)

def doHSVFusion():

    highres, lowres = loadInputImages()

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


    V = helpers.matchHistogram(highres, V)

    for x in range(hrSizeX):
        for y in range(hrSizeY):

            r,g,b = color.hsv_to_rgb(H[x,y], S[x,y], V[x,y])
            fused[x,y,:] = [r,g,b]

    io.imsave(folder + "fused_hsv.bmp", fused)

def doHSLFusion():
    # Note: HSL is the typical abbreviation but the colorsys fuction switches the position of s and l (HLS)

    highres, lowres = loadInputImages()

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

    L = helpers.matchHistogram(highres, L)

    for x in range(hrSizeX):
        for y in range(hrSizeY):

            r,g,b = color.hls_to_rgb(H[x,y], L[x,y], S[x,y])
            fused[x,y,:] = [r,g,b]

    io.imsave(folder + "fused_hsl.bmp", fused)

def doADWTFusion():

    highres, lowres = loadInputImages()

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)

    H = np.zeros([hrSizeX, hrSizeY])
    L = np.zeros([hrSizeX, hrSizeY])
    S = np.zeros([hrSizeX, hrSizeY])

    for x in range(hrSizeX):
        for y in range(hrSizeY):            
            H[x,y],L[x,y],S[x,y] = color.rgb_to_hls(lowresResized[x,y,0], lowresResized[x,y,1], lowresResized[x,y,2])

    highres = helpers.matchHistogram(highres, L)


    decompLevels = [1, 2, 3, 4, 5, 6]

    for n in decompLevels:

        decompL = pywt.swt2(L, 'haar', n)
        decompP = pywt.swt2(highres, 'haar', n)

        for x in range(hrSizeX):
            for y in range(hrSizeY):

                decompL[n - 1][1][0][x,y] = (decompL[n - 1][1][0][x,y] + decompP[n - 1][1][0][x,y]) / 2
                decompL[n - 1][1][1][x,y] = (decompL[n - 1][1][1][x,y] + decompP[n - 1][1][1][x,y]) / 2
                decompL[n - 1][1][2][x,y] = (decompL[n - 1][1][2][x,y] + decompP[n - 1][1][2][x,y]) / 2

        L = pywt.iswt2(decompL, 'haar')

        R = np.zeros([hrSizeX, hrSizeY])
        G = np.zeros([hrSizeX, hrSizeY])
        B = np.zeros([hrSizeX, hrSizeY])

        for x in range(hrSizeX):
            for y in range(hrSizeY):
                R[x,y], G[x,y], B[x,y] = color.hls_to_rgb(H[x,y], L[x,y], S[x,y])

        fused = np.empty([hrSizeX,hrSizeY,3])
        fused[:,:,0] = R
        fused[:,:,1] = G
        fused[:,:,2] = B

        io.imsave(folder + "fused_adwt_{0}.bmp".format(n), fused)

def doSDWTFusion():

    highres, lowres = loadInputImages()

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)

    H = np.zeros([hrSizeX, hrSizeY])
    L = np.zeros([hrSizeX, hrSizeY])
    S = np.zeros([hrSizeX, hrSizeY])

    for x in range(hrSizeX):
        for y in range(hrSizeY):            
            H[x,y],L[x,y],S[x,y] = color.rgb_to_hls(lowresResized[x,y,0], lowresResized[x,y,1], lowresResized[x,y,2])

    highres = helpers.matchHistogram(highres, L)

    decompLevels = [1, 2, 3, 4, 5, 6]

    for n in decompLevels:

        decompL = pywt.swt2(L, 'haar', n)
        decompP = pywt.swt2(highres, 'haar', n)

        for x in range(hrSizeX):
            for y in range(hrSizeY):

                decompL[n - 1][1][0][x,y] = decompP[n - 1][1][0][x,y]
                decompL[n - 1][1][1][x,y] = decompP[n - 1][1][1][x,y]
                decompL[n - 1][1][2][x,y] = decompP[n - 1][1][2][x,y]

        L = pywt.iswt2(decompL, 'haar')

        R = np.zeros([hrSizeX, hrSizeY])
        G = np.zeros([hrSizeX, hrSizeY])
        B = np.zeros([hrSizeX, hrSizeY])

        for x in range(hrSizeX):
            for y in range(hrSizeY):
                R[x,y], G[x,y], B[x,y] = color.hls_to_rgb(H[x,y], L[x,y], S[x,y])

        fused = np.empty([hrSizeX,hrSizeY,3])
        fused[:,:,0] = R
        fused[:,:,1] = G
        fused[:,:,2] = B

        io.imsave(folder + "fused_sdwt_{0}.bmp".format(n), fused)

def doPCAFusion():

    highres, lowres = loadInputImages()

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)
    lowresResized = np.reshape(lowresResized, [hrSizeX * hrSizeY, -1])

    # Mean center each column
    means = np.mean(lowresResized, axis=0)
    means = np.tile(means, [hrSizeX * hrSizeY, 1])
    lowresResized = np.subtract(lowresResized, means)

    highRes = np.reshape(highres, [hrSizeX * hrSizeY])

    pcCount = 3

    fused = np.zeros([hrSizeX, hrSizeY, 3])
    #cov = np.dot(lowresResized.T, lowresResized) / lowresResized.shape[0]
    #eValues, eVectors = np.linalg.eigh(cov)
    #key = np.argsort(eValues)[::-1][:pcCount]
    #eValues, eVectors = eValues[key], eVectors[:, key]
    #u = np.dot(lowresResized, eVectors)

    u,s,v = np.linalg.svd(lowresResized, full_matrices=False)

    # pc1 = u[:,0]
    #us = np.dot(u, np.diag(s))
    #pc1 = us[:,0]
    pc1 = u[:,0]

    minPc1 = np.min(pc1)
    maxPc1 = np.max(pc1)

    highRes = helpers.matchHistogram(highRes, pc1)
    #crossCorrelation = np.correlate(np.reshape(pc1, [-1]), np.reshape(highRes, [-1]))
    #crossCorrelationSign = 1
    #if crossCorrelation < 0:
    #    crossCorrelationSign = -1

    # u[:,0] = highRes * crossCorrelationSign
        
    # us[:,0] = highRes
    u[:,0] = highRes

    # reverted = np.dot(u, eVectors)
    reverted = np.dot(np.dot(u, np.diag(s)), v)
    # Reverse mean centering
    reverted = np.add(reverted, means)
    fused = np.reshape(reverted, [hrSizeX, hrSizeY, 3])

    if np.min(fused) < 0:
        fused = np.subtract(fused, np.min(fused))
    if np.max(fused) > 1:
        fused = np.divide(fused, np.max(fused))

    print("Min {0} Max {1}".format(np.min(fused), np.max(fused)))

    #fused[fused < 0] = 0
    #fused[fused > 1] = 1

    io.imsave(folder + "fused_pca.bmp", fused)

def doLaplaceFusion():
    
    highres, lowres = loadInputImages()

    hrSizeX = highres.shape[0]
    hrSizeY = highres.shape[1]
    lrSizeX = lowres.shape[0]
    lrSizeY = lowres.shape[1]

    lowresResized = helpers.upscaleImage(lowres, highres)

    fused = np.zeros([hrSizeX, hrSizeY, 3])
    H = np.zeros([lrSizeX, lrSizeY])
    L = np.zeros([lrSizeX, lrSizeY])
    S = np.zeros([lrSizeX, lrSizeY])

    for x in range(lrSizeX):
        for y in range(lrSizeY):            
            H[x,y],L[x,y],S[x,y] = color.rgb_to_hls(lowres[x,y,0], lowres[x,y,1], lowres[x,y,2])

    numLevels = int(np.log2(max(hrSizeX / lrSizeX, hrSizeY / lrSizeY))) + 1

    fusionLevel = numLevels - 1

    sHr = [None] * numLevels
    gHr = [None] * numLevels
    lHr = [None] * numLevels

    for i in range(numLevels):

        if i == 0:
            sHr[i] = highres
        else:
            sHr[i] = sHr[i - 1][::2,::2]

        gHr[i] = filters.gaussian_filter(sHr[i], 1.0)
        lHr[i] = sHr[i] - gHr[i]

    matched = helpers.matchHistogram(L, lHr[fusionLevel])

    sRec = [None] * numLevels
    gRec = [None] * numLevels
    lRec = [None] * numLevels

    i = fusionLevel
    while i >= 0:

        if i == fusionLevel:
            sRec[i] = matched + lHr[i]
            # sRec[i] = gHr[i] + lHr[i]
        else:
            sRec[i] = gRec[i] + lHr[i]
        
        if i > 0:
            gRec[i - 1] = filters.gaussian_filter(helpers.emptyUpscale(sRec[i]), 1.0) * 4

        i = i - 1

    H = helpers.upscaleImage(H, sRec[0])
    S = helpers.upscaleImage(S, sRec[0])
    L = sRec[0]

    L = helpers.normalizeComponent(L)

    lmin = np.min(L)
    lmax = np.max(L)

    for x in range(hrSizeX):
        for y in range(hrSizeY):

            red,green,blue = color.hls_to_rgb(H[x,y], L[x,y], S[x,y])
            fused[x,y,:] = [red,green,blue]

    # print("Min {0} Max {1}".format(np.min(fused), np.max(fused)))

    #if np.min(fused) < 0:
    #    fused = np.subtract(fused, np.min(fused))
    #if np.max(fused) > 1:
    #    fused = np.divide(fused, np.max(fused))

    #fused[fused < 0] = 0
    #fused[fused > 1] = 1

    #numLevels = [1, 2, 3, 4, 5, 6]
    #for n in numLevels:

    #    fusionLevel = n - 1

    #    s = [None] * n
    #    g = [None] * n
    #    l = [None] * n

    #    for i in range(n):

    #        if i == 0:
    #            s[i] = L
    #        else:
    #            s[i] = s[i - 1][::2,::2]

    #        g[i] = filters.gaussian_filter(s[i], 1.0)
    #        l[i] = s[i] - g[i]

    
    #    matched = helpers.matchHistogram(highres, l[fusionLevel])

    #    for i in range(fusionLevel):
    #        matched = matched[::2,::2]

    #    r = [None] * n
    #    i = fusionLevel
    #    while i >= 0:

    #        if i == fusionLevel:
    #            r[i] = matched + l[i]
    #        else:
    #            r[i] = filters.gaussian_filter(helpers.emptyUpscale(r[i + 1]), 1.0) + l[i]

    #        i = i - 1

    #    H = np.divide(helpers.upscaleImage(H, r[0]), 255)
    #    S = np.divide(helpers.upscaleImage(S, r[0]), 255)

    #    lmin = np.min(r[0])
    #    lmax = np.max(r[0])

    #    if np.min(r[0]) < 0:
    #        r[0] = np.subtract(r[0], np.min(r[0]))
    #    if np.max(r[0]) > 0:
    #        r[0] = np.divide(r[0], np.max(r[0]))

    #    z = 0

    #    for x in range(hrSizeX):
    #        for y in range(hrSizeY):

    #            red,green,blue = color.hls_to_rgb(H[x,y], r[0][x,y], S[x,y])
    #            fused[x,y,:] = [red,green,blue]

    #    fusedMin = np.min(fused)
    #    fusedMax = np.max(fused)

    #    print("Min {0} Max {1}".format(np.min(fused), np.max(fused)))

    #    if np.min(fused) < 0:
    #        fused = np.subtract(fused, np.min(fused))
    #    if np.max(fused) > 1:
    #        fused = np.divide(fused, np.max(fused))

    #    #fused[fused < 0] = 0
    #    #fused[fused > 1] = 1

    #    io.imsave("D:\\Data\\FusionComparison\\fused_lpf_{0}.bmp".format(n), fused)
    io.imsave(folder + "fused_lpf.bmp", fused)

if __name__ == "__main__":
    
    # doWeightedAverageFusion()
    # doHSVFusion()
    # doHSLFusion()
    # doADWTFusion()
    # doSDWTFusion()
    doPCAFusion()
    # doLaplaceFusion()