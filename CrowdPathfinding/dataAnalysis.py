import os

path = "Assets/Logs/"

# get all files in the path
files = os.listdir(path)
print(files)

fileDataList = []

for file in files:
    if (not file.endswith(".txt")):
        continue

    with open(path + file, "r") as f:
            print("Reading file: " + file)
            fileData = f.readlines()
            fileDataList.append(fileData)

patienceDict = {}

for fileData in fileDataList:
    testNumDict = {}    
    
    patience = -1
    testIndex = -1
    
    testData = []
    for line in fileData:
        
        if (line == "\n"):
            # empty lines
            continue
        
        elif (line.startswith("Test")):
            # test headers
            
            # first append the previous test data
            if (testIndex != -1):
                testNumDict[testIndex] = testData
                # print(testIndex, testData[0])
                testData = []
            
            # Test n - Patience: x
            lineStr = line.split(" ")
            testIndex = int(lineStr[1])
            patience = int(lineStr[4])
        
        else:
            # test data
            testData.append(line.rstrip())
    
    patienceDict[patience] = testNumDict
    
testNumDict[testIndex] = testData
print(testIndex, testData[0])

# write file to csv
"""
PatienceDict    -> {patience: TestNumDict}
TestNumDict     -> {testIndex: testData}
testData        -> [testDataLine]
"""

for patience, patienceDataDict in patienceDict.items():
    with open("dataAnalysis/patience_" + str(patience) + ".csv", "w") as fout:
        # write first line as header to csv
        # in the format: Test 0, Test 1...
        numTests = len(patienceDataDict)
        fout.write("Index,")
        for i in range(numTests):
            fout.write("Test " + str(i))
            
            if (i != numTests - 1):
                fout.write(",")
            else:
                fout.write("\n")
        
        formattedData = [[] for i in range(numTests)]
        print(numTests)
        
        # format the data
        for testIndex, testData in patienceDataDict.items():
            numDataLines = len(testData)
            for i in range(numDataLines):
                # print(testIndex)
                formattedData[testIndex-1].append(testData[i])

        # write the data
        for i in range(numDataLines):
            fout.write(str(i+1) + ",")
            for j in range(numTests):
                fout.write(formattedData[j][i])
                
                if (j != numTests - 1):
                    fout.write(",")
                else:
                    fout.write("\n")

print("Done!")