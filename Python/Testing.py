import numpy as np
from scipy import misc
from skimage import color
from skimage import io
import pywt as pywt

import matplotlib as mpl

import Helpers as helpers
def hslFusion(lowRes, highRes):  

    lowRes = np.divide(lowRes, 255)
    lowRes = color.rgb2hsv(lowRes)

    # Histogram match pan wrt intensity image
    highRes = matchHistogram(highRes, lowRes[:,:,2])

    highRes = np.divide(highRes, np.max(highRes))

    hrMean = np.mean(highRes)
    hrStDev = np.std(highRes)
    lrMean = np.mean(lowRes[:,:,2])
    lrStDev = np.std(lowRes[:,:,2])

    temp = np.sqrt((lrStDev/hrStDev)*(np.square(highRes)-hrMean+hrStDev)+lrMean-lrStDev)
    temp[np.isnan(temp)] = 0

    lowRes[:,:,2] = temp
    lowRes = color.hsv2rgb(lowRes)

    return lowRes

def pyramidFusion(lowRes, highRes):

    numLevels = 4

    inputSize = lowRes.shape

    pyramidWidth = 0
    pyramidHeight = 0
    for j in range(numLevels):

        pyramidWidth += int(inputSize[0] / (2**j))
        pyramidHeight += int(inputSize[1] / (2**j))

    msPyramid = np.empty([pyramidWidth, pyramidHeight, 3])
    panPyramid = np.empty([pyramidWidth, pyramidHeight])

    startX = 0
    startY = 0

    for j in range(numLevels):

        levelWidth = int(inputSize[0] / (2**j))
        levelHeight = int(inputSize[1] / (2**j))

        dsPan = misc.imresize(highRes, np.divide(inputSize, 2**j).astype(np.int))
        dsMs = misc.imresize(lowRes, np.divide(inputSize, 2**j).astype(np.int))

        panPyramid[startX : startX + levelWidth, startY : startY + levelHeight] = dsPan
        msPyramid[startX : startX + levelWidth, startY : startY + levelHeight, :] = dsMs

        startX += levelWidth
        startY += levelHeight
    
    misc.imsave('D:\\Data\\PyramidPan.bmp', panPyramid)
    misc.imsave('D:\\Data\\PyramidMs.bmp', msPyramid)     
    
def dwtFusion(lowRes, highRes):

    # Convert ms to hsv color
    ms = np.divide(lowRes, 255)
    ms = color.rgb2hsv(lowRes)

    # Histogram match pan wrt intensity image
    highRes = matchHistogram(highRes, lowRes[:,:,2]) 
    
    # Decompose pan
    numLevels = 1
    decompPan = pywt.wavedec2(highRes, 'haar', level=numLevels)
    decompInt = pywt.wavedec2(ms[:,:,2], 'haar', level=numLevels)

    decompInt[0] += decompPan[0]
    #for l in range(1, numLevels + 1):
    #    for y in range(3):
    #        decompInt[l][y] += decompPan[l][y]

    ms[:,:,2] = pywt.waverec2(decompInt, 'haar')
    ms = color.hsv2rgb(ms)

    return ms
    #decompPan[0],      ll
    #decompPan[1][0],   lh
    #decompPan[1][1],   hl
    #decompPan[1][2],   hh
    #decompPan[2][0],   lh
    #decompPan[2][1],   hl
    #decompPan[2][2]    hh

    #for x in range(len(decompPan)):
    #    decomp = decompPan[x]
    #    if not len(decomp) == 3:
    #        misc.imsave("D:\\Data\decomp{0}.bmp".format(x), decomp)
    #    else:
    #        for y in range(len(decomp)):
    #            misc.imsave("D:\\Data\decomp{0}-{1}.bmp".format(x, y), decomp[y])

def upscale(lowRes, highRes):

    hrWidth = highRes.shape[0]
    hrHeight = highRes.shape[1]
    lrWidth = lowRes.shape[0]
    lrHeight = lowRes.shape[1]

    return misc.imresize(lowRes, [hrWidth, hrHeight])

def matchHistogram(target, reference):

    shape = target.shape
    target = target.ravel()
    reference = reference.ravel()

    valuesTarget, binIndex, countsTarget = np.unique(target, return_inverse=True, return_counts=True)
    valuesRef, countsRef = np.unique(reference, return_counts=True)

    qTarget = np.cumsum(countsTarget).astype(np.float32)
    qTarget /= qTarget[-1]
    qReference = np.cumsum(countsRef).astype(np.float32)
    qReference /= qReference[-1]

    interpolated = np.interp(qTarget, qReference, valuesRef)

    return interpolated[binIndex].reshape(shape)

if __name__ == "__main__":

    #highRes = misc.imread('D:\\Data\\1a.bmp', mode='F')
    #lowRes = misc.imread('D:\\Data\\4b_m.bmp', mode='RGB')

    #lowRes = upscale(lowRes, highRes)

    #fused = hslFusion(lowRes, highRes)

    #if fused is not None:
    #    misc.imsave('D:\\Data\\FusedHSL.bmp', fused)

    #fused = dwtFusion(lowRes, highRes)

    #if fused is not None:
    #    misc.imsave('D:\\Data\\FusedDWT.bmp', fused)

    #for r in range(255):
    #    for g in range(255):
    #        for b in range(255):
    #            rgb = [r,g,b]
    #            rgb = np.divide(rgb, 255)
    #            ivv = helpers.rgb2ivv(rgb)
    #            rev = helpers.ivv2rgb(ivv)
    #            rev = np.multiply(rev, 255)
    #            rgb = np.multiply(rgb, 255)
    #            assert abs(rgb[0] - rev [0]) < 1 and abs(rgb[1] - rev[1]) < 1 and abs(rgb[2] - rev[2]) < 1, "rgb {0} {1} {2} not converted correctly".format(rgb[0], rgb[1], rgb[2])

    #src = np.random.rand(516,516)
    #dest = np.empty((512,512))

    #for x in range(512):
    #    for y in range(512):

    #        adjX = x + 2
    #        adjY = y + 2
    #        subset = src[adjX-2:adjX+3, adjY-2:adjY+3]
    #        s = subset.shape
    #        assert s[0] == 5 and s[1] == 5, "Array shape is [0,1] at {2},{3}".format(s[0],s[1],x,y)
    #        j=0;

    #highres = io.imread("D:\\Data\\FusionComparison\\highres.bmp", as_grey=True)
    #lowRes = io.imread("D:\\Data\\FusionComparison\\lowres.bmp")  

    #lowRes = np.divide(lowRes, 255)

    #ivv = np.zeros_like(lowRes)
    #rev = np.zeros_like(lowRes, dtype=int)

    #for x in range(lowRes.shape[0]):
    #    for y in range(lowRes.shape[1]):
    #        ivv[x,y,:] = helpers.rgb2ivv(lowRes[x,y,:])
    #        temp = helpers.ivv2rgb(ivv[x,y,:])
    #        rev[x,y,:] = np.multiply(temp, 255)

    #io.imsave("D:\\Data\\FusionComparison\\ivv.bmp", ivv)
    #io.imsave("D:\\Data\\FusionComparison\\rev.bmp", rev)

    #decomp1 = pywt.wavedec2(highres, 'haar', level=1)
    #io.imsave("D:\\Data\\FusionComparison\\decomp-1-a.bmp", np.clip(decomp1[0], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-1-h1.bmp", np.clip(decomp1[1][0], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-1-v1.bmp", np.clip(decomp1[1][1], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-1-d1.bmp", np.clip(decomp1[1][2], -1, 1))

    #decomp2 = pywt.wavedec2(highres, 'haar', level=2)
    #io.imsave("D:\\Data\\FusionComparison\\decomp-2-a.bmp", np.clip(decomp2[0], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-2-h1.bmp", np.clip(decomp2[1][0], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-2-v1.bmp", np.clip(decomp2[1][1], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-2-d1.bmp", np.clip(decomp2[1][2], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-2-h2.bmp", np.clip(decomp2[2][0], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-2-v2.bmp", np.clip(decomp2[2][1], -1, 1))
    #io.imsave("D:\\Data\\FusionComparison\\decomp-2-d2.bmp", np.clip(decomp2[2][2], -1, 1))

    mat = np.array([[1, 2, 3], [4, 5, 6], [7, 8, 9], [10, 11, 12]])
    means = np.mean(mat, axis=0)
    means = np.repeat(means, 4, axis=1)

    print(means)