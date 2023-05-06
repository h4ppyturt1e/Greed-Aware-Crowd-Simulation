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
        
for file in fileDataList:
    patience = file[1].rstrip().split(" ")[4]
    outFileName = "dataAnalysis/mass_" + patience + ".csv"
    
    with open(outFileName, "w") as fout:
        for line in file:
            if (line == "\n") or (line.startswith("Test")):
                # empty lines and test headers
                continue

            else:
                # test data
                fout.write(line.rstrip() + "\n")