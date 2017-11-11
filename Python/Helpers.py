import numpy as np

import scipy.misc as misc

import sys
import math

def writeMessage(message):
    sys.stdout.write('\r' + message + ' ' * 50)
    sys.stdout.flush()

def upscaleImage(lowRes, highRes):

    hrWidth = highRes.shape[0]
    hrHeight = highRes.shape[1]
    lrWidth = lowRes.shape[0]
    lrHeight = lowRes.shape[1]

    resized = misc.imresize(lowRes, [hrWidth, hrHeight], 'bilinear')
    return np.divide(resized, 255)


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

def rgb2ivv(rgb):

    mat = np.array(
        [
            [1/3,              1/3,             1/3],
            [-math.sqrt(2)/6, -math.sqrt(2)/6,  2*math.sqrt(2)/6],
            [1/math.sqrt(2),  -1/math.sqrt(2),  0]
         ])

    return np.dot(mat, rgb)

def ivv2rgb(ivv):

    mat = np.array(
        [
            [1, -1/math.sqrt(2), 1/math.sqrt(2)],
            [1, -1/math.sqrt(2), -1/math.sqrt(2)],
            [1, math.sqrt(2), 0]
        ])

    return np.dot(mat, ivv)

def rgb2ihs(rgb):

    ivv = rgb2ivv(rgb)

    h = math.atan(ivv[2] / ivv[1])
    s = math.sqrt(ivv[1] * ivv[1] + ivv[2] * ivv[2])

    return np.array([ivv[0], h, s])

def emptyUpscale(image):

    sizeX = image.shape[0]
    sizeY = image.shape[1]

    upscaled = np.zeros([sizeX * 2, sizeY * 2])

    for x in range(sizeX):
        for y in range(sizeY):
            upscaled[x*2,y*2] = image[x,y]

    return upscaled

def normalizeComponent(matrix):

    temp = matrix

    #min = np.min(matrix)

    #if min < 0:
    #    temp = np.subtract(temp, min)
    temp[temp < 0] = 0

    max = np.max(matrix)

    #if max > 1:
    temp = np.divide(temp, max)

    return temp