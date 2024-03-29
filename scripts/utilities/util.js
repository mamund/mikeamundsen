/*
    $Date$
    $Rev$
    $Author$
*/

function Utilities()
{
    var that = this;
    
    this.typeOf = function(value)
    {
        var s = typeof value;
        if (s === 'object')
        {
            if (value)
            {
                if (typeof value.length === 'number' &&
                    !(value.propertyIsEnumerable('length')) &&
                    typeof value.splice === 'function')
                {
                    s = 'array';
                }
            }
            else
            {
                s = 'null';
            }
        }
        return s;
    };
    
    this.isEmpty = function(o)
    {
        var i, v;
        
        if(that.typeOf(o) === 'object')
        {
            for (i in o)
            {
                v = o[i];
                if( v!== undefined && typeOf(v) !== 'function')
                {
                    return false;
                }
            }
        }
        return true;
    };
}

// extend string object
String.prototype.trim = function()
{
    return this.replace(/^\s+/,'').replace(/\s*$/, '');
};

String.prototype.entityify = function()
{
    return this.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;");
};

String.prototype.supplant = function(o)
{
    return this.replace(/{([^{}]*)}/g,
        function(a,b) {
            var r = o[b];
            return (typeof r === 'string' || typeof r === 'number' ? r : a);
        }
    );
};

String.prototype.quote = function () {
    var c, i, l = this.length, o = '"';
    for (i = 0; i < l; i += 1) {
        c = this.charAt(i);
        if (c >= ' ') {
            if (c === '\\' || c === '"') {
                o += '\\';
            }
            o += c;
        } else {
            switch (c) {
            case '\b':
                o += '\\b';
                break;
            case '\f':
                o += '\\f';
                break;
            case '\n':
                o += '\\n';
                break;
            case '\r':
                o += '\\r';
                break;
            case '\t':
                o += '\\t';
                break;
            default:
                c = c.charCodeAt();
                o += '\\u00' + Math.floor(c / 16).toString(16) +
                    (c % 16).toString(16);
            }
        }
    }
    return o + '"';
};

// extend Object to ease prototypal inheritance
Object.prototype.begetObject = function() {
    function F() {}
    F.prototype = this;
    return new F();
};