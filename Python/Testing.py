import numpy as np
from scipy import misc
from skimage import color
import pywt as pywt

def hslFusion(lowRes, highRes):  

    highRes = matchHistogram(highRes, lowRes)

    lowRes = np.divide(lowRes, 255)
    lowRes = color.rgb2hsv(lowRes)

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

    coeffsMs = pywt.wavedec(lowRes, 'db1')
    coeffsPan = pywt.wavedec(highRes, 'db1')

    reconMs = pywt.waverec(coeffsMs, 'db1')
    reconPan = pywt.waverec(coeffsPan, 'db1')

    misc.imsave('D:\\Data\\ReconMs.bmp', reconMs)
    misc.imsave('D:\\Data\\ReconPan.bmp', reconPan)

    print(pywt.wavelist())

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

    highRes = misc.imread('D:\\Data\\1a.bmp', mode='F')
    lowRes = misc.imread('D:\\Data\\4b_m.bmp', mode='RGB')

    lowRes = upscale(lowRes, highRes)

    # fused = hslFusion(lowRes, highRes)
    # fused = pyramidFusion(lowRes, highRes)
    fused = dwtFusion(lowRes, highRes)

    if fused is not None:
        misc.imsave('D:\\Data\\Fused.bmp', fused)