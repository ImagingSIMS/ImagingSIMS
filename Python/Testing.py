import numpy as np
from scipy import misc
from skimage import color
from skimage import io
import pywt as pywt

import matplotlib as mpl

import Helpers as helpers

def loadInputImages():

    highres = io.imread("D:\\Data\\FusionComparison\\highres.bmp", as_grey=True)
    lowRes = io.imread("D:\\Data\\FusionComparison\\lowres.bmp")    

    # Scale lowres to [0, 1] -- highres is already in range from imread
    return highres, np.divide(lowRes, 255)

sims = np.genfromtxt("D:\\Data\\58.txt", delimiter=',')

decomp = pywt.swt2(sims, 'bior2.2', 2)

decomp[1][0][:,:] = pywt.threshold(decomp[1][0][:,:], 5, 'greater')

reconstructed = pywt.iswt2(decomp, 'bior2.2')

reconstructed[reconstructed < 0] = 0
reconstructed[reconstructed > 1] = 1

io.imsave("D:\\Data\\FusionComparison\\bior.bmp", reconstructed)