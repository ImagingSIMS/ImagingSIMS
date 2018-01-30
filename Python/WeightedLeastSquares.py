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