import numpy as np
from skimage import io

folder = "D:\\Data\\FusionComparison\\8-15-17\\"

def standardDeviation(fusedImage):

    gray = (fusedImage[:,:,0] + fusedImage[:,:,1] + fusedImage[:,:,2]) / 3
    gray = np.divide(gray, np.max(gray))
    hist, bins = np.histogram(np.reshape(gray, [-1]), bins=100, density=True)
    hist = np.divide(hist, np.sum(hist))
    bins = bins[:-1]

    iBar = np.sum(bins * hist)
    stDev = np.sqrt(np.sum(np.power((bins - iBar), 2) * hist))

    return stDev

def informationEntropy(fusedImage):

    gray = (fusedImage[:,:,0] + fusedImage[:,:,1] + fusedImage[:,:,2]) / 3
    gray = np.divide(gray, np.max(gray))
    hist, bins = np.histogram(np.reshape(gray, [-1]), bins=100, density=True)
    hist = np.divide(hist, np.sum(hist))
    bins = bins[:-1]

    # Add small amout to avoid nan
    hist = hist + 1e-10

    logHist = np.log2(hist)
    infEntropy = -np.sum(hist * logHist)

    return infEntropy

def crossEntropy(highresImage, lowresImage, fusedImage):

    grayF = (fusedImage[:,:,0] + fusedImage[:,:,1] + fusedImage[:,:,2]) / 3
    grayF = np.divide(grayF, np.max(grayF))
    histF, binsF = np.histogram(np.reshape(grayF, [-1]), bins=100, density=True)
    histF = np.divide(histF, np.sum(histF))

    grayL = (lowresImage[:,:,0] + lowresImage[:,:,1] + lowresImage[:,:,2]) / 3
    grayL = np.divide(grayL, np.max(grayL))
    histL, binsL = np.histogram(np.reshape(grayL, [-1]), bins=100, density=True)
    histL = np.divide(histL, np.sum(histL))

    grayH = (highresImage[:,:,0] + highresImage[:,:,1] + highresImage[:,:,2]) / 3
    grayH = np.divide(grayH, np.max(grayH))
    histH, binsH = np.histogram(np.reshape(grayH, [-1]), bins=100, density=True)
    histH = np.divide(histH, np.sum(histH))

    bins = binsF[:-1]

    # Add small amout to avoid nan
    histF = histF + 1e-10
    histL = histL + 1e-10
    histH = histH + 1e-10

    relativeEntropyL = np.sum(histL * np.log2(histL / histF))
    relativeEntropyH = np.sum(histH * np.log2(histH / histF))

    return (relativeEntropyH + relativeEntropyL) / 2

def mutualInformation(highresImage, lowresImage, fusedImage):

    grayF = np.reshape((fusedImage[:,:,0] + fusedImage[:,:,1] + fusedImage[:,:,2]) / 3, [-1])
    grayF = np.divide(grayF, np.max(grayF))
    grayL = np.reshape((lowresImage[:,:,0] + lowresImage[:,:,1] + lowresImage[:,:,2]) / 3, [-1])
    grayL = np.divide(grayL, np.max(grayL))
    grayH = np.reshape((highresImage[:,:,0] + highresImage[:,:,1] + highresImage[:,:,2]) / 3, [-1])
    grayL = np.divide(grayH, np.max(grayH))

    jdfFL, binsFLx, binsFLy = np.histogram2d(grayF, grayL, bins=(100, 100), normed=False)
    jdfFL = np.divide(jdfFL, np.sum(jdfFL))
    jdfFH, binsFHx, binsFHy = np.histogram2d(grayF, grayH, bins=(100, 100), normed=False)
    jdfFH = np.divide(jdfFH, np.sum(jdfFH))

    pdfF, binsF = np.histogram(grayF, bins=100, density=True)
    pdfF = np.divide(pdfF, np.sum(pdfF))
    pdfL, binsL = np.histogram(grayL, bins=100, density=True)
    pdfL = np.divide(pdfL, np.sum(pdfL))
    pdfH, binsH = np.histogram(grayH, bins=100, density=True)
    pdfH = np.divide(pdfH, np.sum(pdfH))

    bins = binsF[:-1]

    # Add small amout to avoid nan
    jdfFL = jdfFL + 1e-10
    jdfFH = jdfFH + 1e-10
    pdfF = pdfF + 1e-10
    pdfL = pdfL + 1e-10
    pdfH = pdfH + 1e-10

    #iFL = np.sum(jdfFL * np.log2(jdfFL / np.dot(pdfF, pdfL)))
    #iFH = np.sum(jdfFH * np.log2(jdfFH / np.dot(pdfF, pdfH)))

    sumFH = 0
    sumFL = 0

    for f in range(len(bins)):
        for h in range(len(bins)):
            sumFH = sumFH + jdfFH[f, h] * np.log2(jdfFH[f, h] / (pdfF[f] * pdfH[h]))

    for f in range(len(bins)):
        for l in range(len(bins)):
            sumFL = sumFL + jdfFL[f, l] * np.log2(jdfFL[f, l] / (pdfF[f] * pdfL[l]))            

    # return iFL + iFH
    return sumFH + sumFL

def crossCorrelation(lowresImage, fusedImage):

    rL = np.reshape(lowresImage[:,:,0], [-1])
    gL = np.reshape(lowresImage[:,:,1], [-1])
    bL = np.reshape(lowresImage[:,:,2], [-1])

    rF = np.reshape(fusedImage[:,:,0], [-1])
    gF = np.reshape(fusedImage[:,:,1], [-1])
    bF = np.reshape(fusedImage[:,:,2], [-1])

    rMeanL = np.mean(rL)
    gMeanL = np.mean(gL)
    bMeanL = np.mean(bL)

    rMeanF = np.mean(rF)
    gMeanF = np.mean(gF)
    bMeanF = np.mean(bF)

    cc = np.zeros([3])

    cc[0] = np.sum((rF - rMeanF) * (rL - rMeanL)) / np.sqrt(np.sum(np.square(rF - rMeanF)) * np.sum(np.square(rL - rMeanL)))
    cc[1] = np.sum((gF - gMeanF) * (gL - gMeanL)) / np.sqrt(np.sum(np.square(gF - gMeanF)) * np.sum(np.square(gL - gMeanL)))
    cc[2] = np.sum((bF - bMeanF) * (bL - bMeanL)) / np.sqrt(np.sum(np.square(bF - bMeanF)) * np.sum(np.square(bL - bMeanL)))

    return np.sum(cc) / 3

if __name__ == "__main__":

    lowresImage = io.imread(folder + "grid_lowres.bmp")
    highresImage = io.imread(folder + "grid_highres.bmp")
    fusedImage = io.imread(folder + "fused_hsv.bmp")

    stdev = standardDeviation(fusedImage)
    infEntropy = informationEntropy(fusedImage)
    crossEntropy = crossEntropy(highresImage, lowresImage, fusedImage)
    mutualInf = mutualInformation(highresImage, lowresImage, fusedImage)
    cc = crossCorrelation(lowresImage, fusedImage)

    print("Standard deviation: {0}".format(stdev))
    print("Information entropy: {0}".format(infEntropy))
    print("Cross entropy: {0}".format(crossEntropy))
    print("Mutual information: {0}".format(mutualInf))
    print("Cross correlation: {0}".format(cc))

    i = 0