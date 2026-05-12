print("test2")
testnumber = 123
testBool = true
testfloat = 1.23
teststring = "hello world"

function testfunc()
    print("no parameters")
end

function testfunc2(a)
    print("with parameters")
    return a+1
end

function testfunc3(a)
    print("multiple return values")
    return a,a+1,2
end

--变长参数
function testfunc4(a,...)
    print("vararg parameters")
    print(a)
    local args = {...}
    for i,v in ipairs(args) do
        print(v)
    end
    
end