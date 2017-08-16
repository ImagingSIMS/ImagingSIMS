import numpy as np
from skimage import io

folder = "D:\\Data\\FusionComparison\\8-15-17\\"

def standardDeviation(fusedImage):

    gray = (fusedImage[:,:,0] + fusedImage[:,:,1] + fusedImage[:,:,2]) / 3
    gray = np.divide(gray, np.max(gray))
    hist, bins = np.histogram(np.reshape(gray, [-1]), bins=100, density=True)
    bins = bins[:-1]

    iBar = np.sum(bins * hist)
    stDev = np.sqrt(np.sum((bins - iBar) * hist))

    return stDev

def informationEntropy(fusedImage):

    gray = (fusedImage[:,:,0] + fusedImage[:,:,1] + fusedImage[:,:,2]) / 3
    gray = np.divide(gray, np.max(gray))
    hist, bins = np.histogram(np.reshape(gray, [-1]), bins=100, density=True)
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

    grayL = (lowresImage[:,:,0] + lowresImage[:,:,1] + lowresImage[:,:,2]) / 3
    grayL = np.divide(grayL, np.max(grayL))
    histL, binsL = np.histogram(np.reshape(grayL, [-1]), bins=100, density=True)

    grayH = (highresImage[:,:,0] + highresImage[:,:,1] + highresImage[:,:,2]) / 3
    grayH = np.divide(grayH, np.max(grayH))
    histH, binsH = np.histogram(np.reshape(grayH, [-1]), bins=100, density=True)

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

    jdfFL, binsFLx, binsFLy = np.histogram2d(grayF, grayL, bins=100, normed=True)
    jdfFH, binsFHx, binsFHy = np.histogram2d(grayF, grayH, bins=100, normed=True)

    pdfF, binsF = np.histogram(grayF, bins=100, density=True)
    pdfL, binsL = np.histogram(grayL, bins=100, density=True)
    pdfH, binsH = np.histogram(grayH, bins=100, density=True)

    bins = binsF[:-1]

    # Add small amout to avoid nan
    jdfFL = jdfFL + 1e-10
    jdfFH = jdfFH + 1e-10
    pdfF = pdfF + 1e-10
    pdfL = pdfL + 1e-10
    pdfH = pdfH + 1e-10

    iFL = np.sum(jdfFL * np.log2(jdfFL / np.dot(pdfF, pdfL)))
    iFH = np.sum(jdfFH * np.log2(jdfFH / np.dot(pdfF, pdfH)))

    return iFL + iFH

if __name__ == "__main__":

    lowresImage = io.imread(folder + "grid_lowres.bmp")
    highresImage = io.imread(folder + "grid_highres.bmp")
    fusedImage = io.imread(folder + "fused_hsv.bmp")

    stdev = standardDeviation(fusedImage)
    infEntropy = informationEntropy(fusedImage)
    crossEntropy = crossEntropy(highresImage, lowresImage, fusedImage)
    mutualInf = mutualInformation(highresImage, lowresImage, fusedImage)

    print("Standard deviation: {0}".format(stdev))
    print("Information entropy: {0}".format(infEntropy))
    print("Cross entropy: {0}".format(crossEntropy))
    print("Mutual information: {0}".format(mutualInf))

    i = 0