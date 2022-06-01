
/* 这个是一个类 */

function xx(num, str) {
    //说明
    var a = num;
    this.aa = a;
    this.bb = function() { alert(str); }
    this.cc = function() {
        for (var i = 0; i < 10; i++) {
            document.title = i;
        }
    }
    this.yy = new yy();

    function xxf() {
        alert("xxf");
        if ((/\{\d+\}/).test("asdf{2}12_d"))
            alert("{\\d} is match!");
    }
}

xx.prototype.dd = function() {
    alert("dd");
    a.yy.ll();
    var fnx = function(i) {
        this.index = i;
        this.aa = function() {
            alert(this.index);
        }
    }
    var f1 = new fnx(12);
    f1.aa();
}

function yy() {
    alert('yy');
}
yy.prototype.ll = function() {
    alert("yyll");
}

var a = new xx(100, "hello"), b = new xx(0, "ttyp");
eval("a.aa=20");
a.bb();
b.dd();
alert(a.aa);

var k = 9;
function kk() {
    var k = 0;
    alert(k);
}
kk();
alert(k);
//show:"yy"->"yy"->"hello"->"dd"->"yyll"->"12"->"20"->"0"->"9"
