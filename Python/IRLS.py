import numpy as np

# Iterative reweighted least squares from https://github.com/aehaynes/IRLS/blob/master/irls.py

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