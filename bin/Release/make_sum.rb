fnm = "gep.exe"
data = open(fnm, "rb") {|f|f.read}
sum=1653
data.each_byte {|x| sum += x}
sum = sum % 65536
p sum
while ((d = 65536 - sum)>0) 
    x = (d > 256) ? 256 : d+1
    t = rand(x)
    data << t
    sum += t
end
open("GraphEditPlus.exe","wb") {|f| f.write(data) }
