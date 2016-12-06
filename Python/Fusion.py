import matplotlib.pyplot as plt
import numpy as np
import skimage.measure

from skimage import color
from scipy import misc

import sys

# Iterative reweighted least squares from https://github.com/aehaynes/IRLS/blob/master/irls.py

def writeMessage(message):
    sys.stdout.write('\r' + message + ' ' * 50)
    sys.stdout.flush()

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

def calculateB(x, y, W):

    bx = x.T.dot(W).dot(x)
    by = x.T.dot(W).dot(y)

    # If bx is nonivertible, calculate generalized inverse instead
    if not np.linalg.det(bx) == 0:
        bx = np.linalg.inv(bx)
    else:
        bx = np.linalg.pinv(bx)

    B = np.dot(bx, by)

    return B

def IRLS(y, x, maxIter, wInit = 1, d = 0.0001, tolerance = 0.001):
    # assert type(x) is np.array, "x is not a numpy array"
    # assert type(y) is np.array, "y is not a numpy array"

    n, p = x.shape
    delta = np.array(np.repeat(d, n)).reshape(1, n)
    w = np.repeat(1, n)
    W = np.diag(w)

    B = calculateB(x, y, W)

    for iter in range(maxIter):

        _B = B
        _w = abs(y - x.dot(B)).T
        w = float(1) / np.maximum(delta, _w)
        W = np.diag(w[0])
        B = calculateB(x, y, W)
        tol = np.sum(abs(B - _B))
        # print("Iteration {0}: tolerance: {1}".format(iter, tol))
        if(tol < tolerance):
            a = np.sum(np.subtract(np.multiply(B, np.mean(x)), np.mean(y)))
            return B, a, False

    a = np.sum(np.subtract(np.multiply(B, np.mean(x)), np.mean(y)))
    return B, a, True

def testWLS():
    #Test Example: Fit the following data under Least Absolute Deviations regression
    # first line = "p n" where p is the number of predictors and n number of observations
    # following lines are the data lines for predictor x and response variable y
    #	 "<pred_1> ... <pred_p> y"
    # next line win "n" gives the number n of test cases to expect
    # following lines are the test cases with predictors and expected response
    input_str = '''2 7
    0.18 0.89 109.85
    1.0 0.26 155.72
    0.92 0.11 137.66
    0.07 0.37 76.17
    0.85 0.16 139.75
    0.99 0.41 162.6
    0.87 0.47 151.77
    4
    0.49 0.18 105.22
    0.57 0.83 142.68
    0.56 0.64 132.94
    0.76 0.18 129.71
    '''

    input_list = input_str.split('\n')

    p,n = [int(i) for i in input_list.pop(0).split() ]
    x = np.empty([n, p+1])
    x[:,0] = np.repeat(1, n)
    y = np.empty([n,1])
    for i in range(n):
        l = [float(i) for i in input_list.pop(0).split()]
        x[i, 1:] = np.array(l[0:p])
        y[i] = np.array(l[p])

    n = [int(i) for i in input_list.pop(0).split()][0]
    x_new = np.empty([n, p+1])
    x_new[:,0] = np.repeat(1, n)
    y_new = np.empty([n, 1])
    for i in range(n):
        l = [float(i) for i in input_list.pop(0).split()]
        x_new[i, 1:] = np.array(l[0:p])
        y_new[i] = np.array(l[p])

    B, a = IRLS(y, x, 20)
    abs_error = abs(y_new - x_new.dot(B))
    print("Absolute error: {0}".format(abs_error))

def fuseColorImage():

    pan = misc.imread("D:\\Data\\1a.bmp")
    ms = misc.imread("D:\\Data\\4b_r.bmp")

    lrX = ms.shape[0]
    lrY = ms.shape[1]
    hrX = pan.shape[0]
    hrY = pan.shape[1]
    numSpecBands = ms.shape[2]

    pan = color.rgb2gray(pan)
    ms = upscale(ms, pan)

    # ihs = color.rgb2hsv(ms)
    # pan = matchHistogram(pan, ihs[:,:,2])
    # pan = skimage.measure.block_reduce(pan, (2,2))

    # x = np.reshape(ms, [hrX * hrY, 3])
    # y = np.reshape(pan, [hrX * hrY, 1])

    panNew = np.empty([hrX, hrY])

    windowSize = 5
    halfWindowSize = int(windowSize / 2)

    paddedPan = np.pad(pan, (halfWindowSize,halfWindowSize), 'reflect')
    paddedMs = np.pad(ms, ((halfWindowSize,halfWindowSize),(halfWindowSize,halfWindowSize),(0,0)) , 'reflect')

    coeffs = np.empty_like(ms, dtype="float64")
    alpha = np.empty_like(pan, dtype="float64")
    bools = np.empty_like(pan, dtype="float64")

    for xx in range(int(hrX)):
        for yy in range(int(hrY)):

            writeMessage("Calculating pixel ({0},{1})".format(xx, yy))

            adjX = xx + halfWindowSize
            adjY = yy + halfWindowSize

            windowPan = paddedPan[adjX-halfWindowSize:adjX+halfWindowSize+1,adjY-halfWindowSize:adjY+halfWindowSize+1]
            windowMs = paddedMs[adjX-halfWindowSize:adjX+halfWindowSize+1,adjY-halfWindowSize:adjY+halfWindowSize+1,:]

            x = np.reshape(windowMs, [5 * 5, 3])
            y = np.reshape(windowPan, [5 * 5, 1])

            B, a, maxedOut = IRLS(y, x, 100)

            alpha[xx,yy] = a
            coeffs[xx,yy,:] = np.reshape(B, 3)

            panNew[xx,yy] = np.sum(np.multiply(B, paddedMs[adjX,adjY,:])) + a

            bools[xx,yy] = float(maxedOut)

    max = panNew.max()

    #if panNew.min() < 0:
    #    panNew += -panNew.min()

    #newMax = panNew.max()

    #panNew *= (256 / panNew.max())

    cMax = coeffs.max()
    cMin = coeffs.min()
    rMax = np.max(coeffs[:,:,0])
    rMin = np.min(coeffs[:,:,0])
    gMax = np.max(coeffs[:,:,1])
    gMin = np.min(coeffs[:,:,1])
    bMax = np.max(coeffs[:,:,2])
    bMin = np.min(coeffs[:,:,2])
    aMax = alpha.max()
    aMin = alpha.min()

    fused = np.empty_like(ms)
    for i in range(numSpecBands):
        fused[:,:,i] = ms[:,:,i] + panNew[:,:]

    #misc.imsave("D:\\Data\\1-4-fused.bmp", fused)
    #misc.imsave("D:\\Data\\1a_new.bmp", panNew)
    #misc.imsave("D:\\Data\\1-4-a.bmp", alpha)
    #misc.imsave("D:\\Data\\1-4-r.bmp", coeffs[:,:,0])
    #misc.imsave("D:\\Data\\1-4-g.bmp", coeffs[:,:,1])
    #misc.imsave("D:\\Data\\1-4-b.bmp", coeffs[:,:,2])
    misc.imsave("D:\\Data\\1-4-bools.bmp", bools)
    # B = IRLS(y, x, 100)

    i = 0

if __name__ == "__main__":
    
    # testWLS()
    fuseColorImage()