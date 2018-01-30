import numpy as np
import numpy.matlib
import os
import scipy.misc
import matplotlib.pyplot as plt
from matplotlib.colors import LinearSegmentedColormap

#from mpl_toolkits.mplot3d import Axes3D
#from mpl_toolkits.mplot3d import proj3d

#np.random.seed(25641261)

#mu_vec1 = np.array([0,0,0])
#cov_mat1 = np.array([[1,0,0],[0,1,0],[0,0,1]])
#class1_sample = np.random.multivariate_normal(mu_vec1, cov_mat1, 20).T
#assert class1_sample.shape == (3,20), "The matrix does not have dimensions 3 x 20"

#mu_vec2 = np.array([0,0,0])
#cov_mat2 = np.array([[1,0,0],[0,1,0],[0,0,1]])
#class2_sample = np.random.multivariate_normal(mu_vec2, cov_mat2, 20).T
#assert class1_sample.shape == (3,20), "The matrix does not have dimensions 3 x 20"

##fig = plt.figure(figsize=(8,8))
##ax = fig.add_subplot(111, projection='3d')
##plt.rcParams['legend.fontsize'] = 10
##ax.plot(class1_sample[0,:], class1_sample[1,:], class1_sample[2,:], 'o', markersize=8, color='blue', alpha=0.5, label='class1')
##ax.plot(class2_sample[0,:], class2_sample[1,:], class2_sample[2,:], '^', markersize=8, color='red', alpha=0.5, label='class2')

##plt.title('Samples for class 1 and class 2')
##ax.legend(loc='upper right')

##plt.show()

#all_samples = np.concatenate((class1_sample,class2_sample), axis=1)
#assert all_samples.shape == (3,40), "The matrix does not have dimensions 3 x 40"

#mean_x = np.mean(all_samples[0,:])
#mean_y = np.mean(all_samples[1,:])
#mean_z = np.mean(all_samples[2,:])

#mean_vector = np.array([[mean_x],[mean_y],[mean_z]])
#print('Mean vector:\n', mean_vector)

#cov_mat = np.cov([all_samples[0,:], all_samples[1,:], all_samples[2,:]])
#print('Covariance matrix:\n', cov_mat)

#eig_val, eig_vec = np.linalg.eig(cov_mat)

#for i in range(len(eig_val)):
#    eigv = eig_vec[:,i].reshape(1,3).T
#    print('Eigenvector {}: {}'.format(i+1, eigv))
#    print('Eigenvalue {}: {}\n'.format(i+1, eig_val[i]))

#    np.testing.assert_array_almost_equal(1.0, np.linalg.norm(eigv))

## Could extend this to include additional value for ID (mass) of species
#eig_pairs = [(np.abs(eig_val[i]), eig_vec[:,i]) for i in range(len(eig_val))]

#eig_pairs.sort(key=lambda x: x[0], reverse=True)

#for i in eig_pairs:
#    print(i[0])                              

def createScoreImage(matrix):

    shape = np.shape(matrix)

    posScores = np.copy(matrix)
    posScores[posScores < 0] = 0
    maxPosScore = np.max(posScores)

    if maxPosScore == 0:
        maxPosScore = 1
    
    normScores = np.divide(posScores, maxPosScore)

    negScores = np.copy(matrix)
    negScores[negScores > 0] = 0
    negScores *= -1
    minNegScore = np.max(negScores)

    if minNegScore == 0:
        minNegScore = 1

    normNegScores = np.divide(negScores, minNegScore)
    normScores = np.subtract(normScores, normNegScores)

    return normScores

def loadSpecFile():

    fileNames = []
    fileNames.append("D:\\Data\\Al-Li Alloy\\5.dat")
    fileNames.append("D:\\Data\\Al-Li Alloy\\6.0151.dat")
    fileNames.append("D:\\Data\\Al-Li Alloy\\7.016.dat")
    fileNames.append("D:\\Data\\Al-Li Alloy\\23.985.dat")
    fileNames.append("D:\\Data\\Al-Li Alloy\\27.9769.dat")
    fileNames.append("D:\\Data\\Al-Li Alloy\\53.9631.dat")
    fileNames.append("D:\\Data\\Al-Li Alloy\\55.9349.dat")
    fileNames.append("D:\\Data\\Al-Li Alloy\\62.9296.dat")

    numMassChannels = len(fileNames)
    sizeX = 256
    sizeY = 256

    masses = np.empty([numMassChannels])
    array = np.empty([sizeX * sizeY, numMassChannels])

    for i in range(numMassChannels):

        mass = os.path.splitext(os.path.split(fileNames[i])[1])[0]

        masses[i] = float(mass)
        array[:, i] = np.fromfile(fileNames[i], dtype=float)

    means = np.mean(array, axis=0)
    array = np.subtract(array, means)

    newMeans = np.mean(array, axis=0);
    assert newMeans.all() == 0, 'Rescaled columns not centered at zero'

    cov = np.cov(array, rowvar=0)


    # ->Scores, ..., Loadings [m x m]
    u, sigma, coeff = scipy.sparse.linalg.svds(array, 7, which='LM')    
    scores = np.multiply(u, np.matlib.repmat(sigma.transpose(), sizeX * sizeY, 1))

    # 0: i, 1: loadings, 2: scores, 3: coeffs
    pcs = [(i, np.abs(sigma[i]), scores[:,i], coeff[i]) for i in range(len(sigma))]
    pcs.sort(key=lambda x: x[1], reverse=True)

    redBlackGreen = LinearSegmentedColormap.from_list('redblackgreen', 
                                                  [(1, 0, 0), (0, 0, 0), (0, 1, 0)],
                                                  64)
    plt.register_cmap('redblackgreen', redBlackGreen)
    plt.axis('off')
    plt.set_cmap(redBlackGreen)

    rescaled = np.zeros_like(array)

    for i in range(len(pcs)):
        pcScores = createScoreImage(np.reshape(pcs[i][2], (sizeX, sizeY)).transpose())
        scoresPlot = plt.imshow(pcScores)
        plt.savefig("D:\\Data\\Al-Li Alloy\\{0}-pca.jpg".format(i + 1), bbox_inches='tight', pad_inches=0)

    j = 0

loadSpecFile()