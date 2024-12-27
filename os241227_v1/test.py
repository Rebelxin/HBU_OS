file_name = "OO"
extension_name = "Z"
fcb = (
    (file_name if len(file_name) >= 3 else file_name + "À" * (3 - len(file_name)))
    + (
        extension_name
        if len(extension_name) >= 2
        else extension_name + "À" * (2 - len(extension_name))
    )
    + "W"
    + "3"
    + "1"
)
print(fcb)

teststr = "0123456789"
# teststr[2:2+3] = "abc"
teststr = teststr[:2] + "abc" + teststr[2 + 3 :]
print(teststr)

teststr2 = "A" + "B" + "C"
print(teststr2, type(teststr2))

print(ord("Ã") - 192)
