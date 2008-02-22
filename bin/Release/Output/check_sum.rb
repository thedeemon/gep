fnm = "GraphEditPlus.exe"
data = open(fnm, "rb") {|f|f.read}
sum=1653
data.each_byte {|x| sum += x}
sum = sum % 65536
p sum
